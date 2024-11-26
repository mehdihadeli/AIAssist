#!/bin/bash

# Create tree-sitter folder structure
mkdir -p tree-sitter/grammars/bins
mkdir -p tree-sitter/bins

# Default value
mode="Debug"
dotnet_version="net8.0"

# Check if the first argument is passed
if [ -n "$1" ]; then
  mode="$1"
fi

# Check if the second argument is provided, if so, use it as dotnet_version
if [ -n "$2" ]; then
    dotnet_version="$2"
fi

# Get the OS type
OS=$(uname -s)

# Define paths for the source files and output binary
lib_src="tree-sitter/tree-sitter/lib/src/lib.c"
treesitter_bin_output="tree-sitter/bins"
test_bin_path="tests/UnitTests/TreeSitter.Bindings.UnitTests/bin/${mode}/$dotnet_version"
ai_assist_integration_test_bin_path="tests/IntegrationTests/AIAssistant.IntegrationTests/bin/${mode}/$dotnet_version"
app_bin_path="src/AIAssist/bin/${mode}/$dotnet_version"

# Create the directory if it doesn't exist
mkdir -p "${test_bin_path}"
mkdir -p "${ai_assist_integration_test_bin_path}"
mkdir -p "${app_bin_path}"

# Check for Linux
if [[ "$OS" == "Linux" ]]; then
    echo "Detected Linux OS. Compiling for Linux..."

    # Array of output files
    linux_tree_sitter_output_files=("${treesitter_bin_output}/tree-sitter.so" "${app_bin_path}/tree-sitter.so" "${test_bin_path}/tree-sitter.so" "${ai_assist_integration_test_bin_path}/tree-sitter.so")

    # Iterate over the arrays
    for i in "${!linux_tree_sitter_output_files[@]}"; do
        echo "Compiling ${linux_tree_sitter_output_files[$i]}..."
        gcc -o "${linux_tree_sitter_output_files[$i]}" -shared "$lib_src" -I./tree-sitter/tree-sitter/lib/src -I./tree-sitter/tree-sitter/lib/include -fPIC
        if [ $? -eq 0 ]; then
            echo "Successfully compiled tree-sitter.so"
        else
            echo "Error during compilation of tree-sitter.so"
            exit 1
        fi
    done
# Check for Windows (MINGW or MSYS environments)
elif [[ "$OS" == "MINGW"* || "$OS" == "MSYS"* ]]; then
    echo "Detected Windows OS. Compiling for Windows..."

    # Array of output files
    windows_tree_sitter_output_files=("${treesitter_bin_output}/tree-sitter.dll" "${app_bin_path}/tree-sitter.dll" "${test_bin_path}/tree-sitter.dll" "${ai_assist_integration_test_bin_path}/tree-sitter.dll")

    # Iterate over the arrays
    for i in "${!windows_tree_sitter_output_files[@]}"; do
        echo "Compiling ${windows_tree_sitter_output_files[$i]}..."
        gcc -o "${windows_tree_sitter_output_files[$i]}" -shared "$lib_src" -I./tree-sitter/tree-sitter/lib/src -I./tree-sitter/tree-sitter/lib/include
        if [ $? -eq 0 ]; then
            echo "Successfully compiled tree-sitter.dll"
        else
            echo "Error during compilation of tree-sitter.dll"
            exit 1
        fi
    done
# Unsupported OS
else
    echo "Unsupported operating system: $OS"
    exit 1
fi

# File containing the list of URLs
grammar_file="tree-sitter/tree-sitter-grammar.txt"

# Check if the grammar file exists
if [ ! -f "$grammar_file" ]; then
    echo "Error: File $grammar_file not found!"
    exit 1
fi

# Read the list of URLs from the file into an array, skipping empty lines and stopping at the first empty line
repos=()
while IFS= read -r line; do
    # Trim leading/trailing whitespace and check if the line is empty
    trimmed_line=$(echo "$line" | xargs)

    if [ -z "$trimmed_line" ]; then
        # Stop parsing when an empty line is encountered
        break
    fi

    # Add the non-empty line to the repos array
    repos+=("$trimmed_line")
done < "$grammar_file"


# Iterate over each URL in the list and perform operations
for repo in "${repos[@]}"; do
    echo "Processing repository: $repo"

    # Get the repository name from the URL by extracting everything after the last /
    repo_name=$(basename "$repo")

    # Trim any trailing or leading whitespace (including newlines) from repo_name
    repo_name=$(echo "$repo_name" | tr -d '[:space:]')

    echo "Repository name (trimmed): $repo_name"

    # Define paths for the source files and output binary
    scanner_src="tree-sitter/grammars/$repo_name/src/scanner.c"
    parser_src="tree-sitter/grammars/$repo_name/src/parser.c"
    grammar_bin_output="tree-sitter/grammars/bins"

    # Debugging: print paths to ensure correctness
    echo "Scanner source path: $scanner_src"
    echo "Parser source path: $parser_src"

    # Check if source files exist
    if [[ ! -f "$scanner_src" && ! -f "$parser_src" ]]; then
        echo "Error: both scanner.c and parser.c files not found for $repo_name!"
        continue
    fi

    # Check if parser.c exists (it should always exist)
    if [ ! -f "$parser_src" ]; then
        echo "Error: parser.c not found for $repo_name!"
        continue
    fi

    # Prepare the GCC command based on available files
    if [ -f "$scanner_src" ]; then
        inputs="$scanner_src $parser_src"
    else
        inputs="$parser_src"
    fi

    # Run the appropriate gcc command based on the OS
    if [[ "$OS" == "Linux" ]]; then
        echo "Detected Linux OS. Compiling for Linux..."

        # Array of output files
        linux_grammar_output_files=("${grammar_bin_output}/${repo_name}.so" "${app_bin_path}/${repo_name}.so" "${test_bin_path}/${repo_name}.so" "${ai_assist_integration_test_bin_path}/${repo_name}.so")

        # Iterate over the arrays
        for i in "${!linux_grammar_output_files[@]}"; do
            echo "Compiling ${linux_grammar_output_files[$i]}..."
            gcc -o "${linux_grammar_output_files[$i]}"  -shared $inputs -fPIC
            if [ $? -eq 0 ]; then
                echo "Successfully compiled ${repo_name}.so"
            else
                echo "Error during compilation of ${repo_name}.so"
                exit 1
            fi
        done
    elif [[ "$OS" == "MINGW"* || "$OS" == "MSYS"* ]]; then
        echo "Detected Windows OS. Compiling for Windows..."

        # Array of output files
        windows_grammar_output_files=("${grammar_bin_output}/${repo_name}.dll" "${app_bin_path}/${repo_name}.dll" "${test_bin_path}/${repo_name}.dll" "${ai_assist_integration_test_bin_path}/${repo_name}.dll")

        # Iterate over the arrays
        for i in "${!windows_grammar_output_files[@]}"; do
            echo "Compiling ${windows_grammar_output_files[$i]}..."
            gcc -o "${windows_grammar_output_files[$i]}"  -shared $inputs
            if [ $? -eq 0 ]; then
                echo "Successfully compiled ${repo_name}.dll"
            else
                echo "Error during compilation of ${repo_name}.dll"
                exit 1
            fi
        done
    else
        echo "Unsupported operating system: $OS"
        exit 1
    fi

    echo "Compilation complete for: $repo_name"
done

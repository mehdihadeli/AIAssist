#!/bin/bash

# Create tree-sitter folder structure
mkdir -p tree-sitter/grammars/bins
mkdir -p tree-sitter/bins

# Get the OS type
OS=$(uname -s)

# Check for Linux
if [[ "$OS" == "Linux" ]]; then
    echo "Detected Linux OS. Compiling for Linux..."
    gcc -o tree-sitter/bin/tree-sitter.so -shared tree-sitter/tree-sitter/src/lib.c -I./tree-sitter/tree-sitter/src -I./tree-sitter/tree-sitter/include -fPIC
    if [ $? -eq 0 ]; then
        echo "Successfully compiled tree-sitter.so"
    else
        echo "Error during compilation"
        exit 1
    fi
# Check for Windows (MINGW or MSYS environments)
elif [[ "$OS" == "MINGW"* || "$OS" == "MSYS"* ]]; then
    echo "Detected Windows OS. Compiling for Windows..."
    gcc -o tree-sitter/bin/tree-sitter.dll -shared tree-sitter/tree-sitter/src/lib.c -I./tree-sitter/tree-sitter/src -I./tree-sitter/tree-sitter/include
    if [ $? -eq 0 ]; then
        echo "Successfully compiled tree-sitter.dll"
    else
        echo "Error during compilation"
        exit 1
    fi
# Unsupported OS
else
    echo "Unsupported operating system: $OS"
    exit 1
fi


# File containing the list of URLs
grammar_file="../tree-sitter/tree-sitter-grammar.txt"

# Check if the grammar file exists
if [ ! -f "$grammar_file" ]; then
    echo "Error: File $grammar_file not found!"
    exit 1
fi

# Read the list of URLs from the file into an array
repos=()
while IFS= read -r line; do
    repos+=("$line")
done < "$grammar_file"


# Iterate over each URL in the list and perform operations
for repo in "${repos[@]}"; do
    echo "Processing repository: $repo"

    # Get the repository name from the URL
    repo_name=$(basename "$repo")
    echo "Repository name: $repo_name"

    # Define paths for the source files and output binary
    scanner_src="../tree-sitter/grammars/${repo_name}/src/scanner.c"
    parser_src="../tree-sitter/grammars/${repo_name}/src/parser.c"
    bin_output="../tree-sitter/grammars/bins/${repo_name}"

    # Check if source files exist
    if [ ! -f "$scanner_src" ] || [ ! -f "$parser_src" ]; then
        echo "Error: Source files not found for $repo_name!"
        continue
    fi

    # Run the appropriate gcc command based on the OS
    if [[ "$OS" == "Linux" ]]; then
        echo "Detected Linux OS. Compiling for Linux..."

        gcc -o "${bin_output}.so" -shared "$scanner_src" "$parser_src" -I../tree-sitter/grammars/${repo_name}/src -I../tree-sitter/grammars/${repo_name}/include -fPIC
        if [ $? -eq 0 ]; then
            echo "Successfully compiled ${repo_name}.so"
        else
            echo "Error during compilation of ${repo_name}.so"
            exit 1
        fi
    elif [[ "$OS" == "MINGW"* || "$OS" == "MSYS"* ]]; then
        echo "Detected Windows OS. Compiling for Windows..."

        gcc -o "${bin_output}.dll" -shared "$scanner_src" "$parser_src" -I../tree-sitter/grammars/${repo_name}/src -I../tree-sitter/grammars/${repo_name}/include
        if [ $? -eq 0 ]; then
            echo "Successfully compiled ${repo_name}.dll"
        else
            echo "Error during compilation of ${repo_name}.dll"
            exit 1
        fi
    else
        echo "Unsupported operating system: $OS"
        exit 1
    fi

    echo "Compilation complete for: $repo_name"
done

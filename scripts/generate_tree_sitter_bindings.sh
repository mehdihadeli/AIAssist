#!/bin/bash

cd tree-sitter || exit 1

tree_sitter_command="ClangSharpPInvokeGenerator \"@TreeSitterGenerator.rsp\""
echo "Running command: $tree_sitter_command"
eval "$tree_sitter_command"  

cd grammars || exit 1

# Define an array with the command generators
generators=(
    "ClangSharpPInvokeGenerator \"@CsharpTreeSitterGenerator.rsp\""
    "ClangSharpPInvokeGenerator \"@GoTreeSitterGenerator.rsp\""
    "ClangSharpPInvokeGenerator \"@JavaTreeSitterGenerator.rsp\""
    "ClangSharpPInvokeGenerator \"@PythonTreeSitterGenerator.rsp\""
    "ClangSharpPInvokeGenerator \"@JavascriptTreeSitterGenerator.rsp\""
    "ClangSharpPInvokeGenerator \"@TypescriptTreeSitterGenerator.rsp\""
)

# Iterate over each generator in the array
for generator in "${generators[@]}"; do
    echo "Generate binding for: '$generator; started."
    eval "$generator"  # Use eval to run the command
	echo "Binding for: '$generator; completed."
done

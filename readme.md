# Code Assist

## Installation

By default, `AIAssist` uses OpenAI's API and GPT modeles and You'll need an API key.

> [!TIP]
> Alternatively, you can use ollama and its models like `llama 3.2` locally. To use local models, you will need to run ollama [Ollama](https://github.com/ollama/ollama) first, personally I perefere to use ollama docker container.
>
> **❗️Note that AIAssist is not optimized for local models and may not work as expected.**

## Getting started

```bash
# go to `ollama container` and install corresponding llama3.1 model with using `ollama` command inside of `ollama` container
docker exe -it ollama ollama run llama3.1

# create a alias for connecting to `ollama` container and run `ollama` command in bash
alias ollama="docker exec -it ollama ollama"
```

```bash
dotnet pack -c Release
# go to release folder
dotnet tool install --global --add-source ./ AIAssistant --version 1.0.0

# Change directory into a git repo
cd /to/your/git/repo

export LLAMA_BASE_ADDRESS=llama address
aiassist --model ollama
```

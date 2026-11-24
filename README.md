# Llama on Windows .NET Gate

A one-file C# check that a local model behind Llama on Windows really calls a tool. The tool returns a one-time code, so a model can't pass by repeating the prompt.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 10+](https://img.shields.io/badge/.NET_SDK-10%2B-512BD4)](https://dotnet.microsoft.com/)
[![Stars](https://img.shields.io/github/stars/mikhailpetrusheuski/llama-windows-dotnet-gate?style=social)](https://github.com/mikhailpetrusheuski/llama-windows-dotnet-gate/stargazers)

## Run

```bash
winget install 9PPPSM6Q21Q6 --source msstore
dotnet run local-agent-check.cs
dotnet run local-agent-check.cs -- <model-id>
```

| Exit code | Meaning |
| --- | --- |
| 0 | Tool called and the code returned |
| 1 | Server unavailable, invalid model list, or model not found |
| 2 | Model answered without calling the tool or returning the code |
| 3 | Request failed or timed out |

## Test without a model

```bash
python tests/fake_server.py 18100 liar
LLAMA_ENDPOINT=http://127.0.0.1:18100/v1 dotnet run local-agent-check.cs
```

Modes: `tools` (0), `empty` (1), `liar` (2), `error` (3). Run the two commands in separate terminals. All four were run with SDK 10.0.402. The gate was not run against a real Llama model.

## Source

[Run local models in your .NET apps with Llama on Windows](https://devblogs.microsoft.com/dotnet/local-models-dotnet-llama-windows/)

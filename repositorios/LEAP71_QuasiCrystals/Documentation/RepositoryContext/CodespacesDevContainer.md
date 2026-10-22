# GitHub Codespaces development environment

To work directly in the cloud without installing native dependencies on a local machine, create a development container for the repository with .NET 8, C# support, and PicoGK support.

## .devcontainer/devcontainer.json

Create this file at the repository root so GitHub Codespaces prepares the environment automatically:

```json
{
  "name": "LEAP71 QuasiCrystals Dev Environment",
  "image": "mcr.microsoft.com/devcontainers/dotnet:1-8.0-jammy",
  "customizations": {
    "vscode": {
      "settings": {
        "dotnet.defaultSolution": "*.sln",
        "editor.formatOnSave": true
      },
      "extensions": [
        "ms-dotnettools.csharp",
        "ms-dotnettools.csdevkit",
        "GitHub.copilot",
        "GitHub.copilot-chat"
      ]
    }
  },
  "postCreateCommand": "dotnet restore"
}
```

> Note on PicoGK: PicoGK requires native voxel-processing libraries such as OpenVDB and C++ support. The Ubuntu 22.04-based container allows compiling and running volumetric functions in the background.

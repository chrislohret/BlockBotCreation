# BlockEnvironmentCopilot — Dataverse C# Plugin

A Microsoft Dataverse (Dynamics 365 / Power Platform) plug-in that blocks
Copilot Studio agent creation in the environment where it is registered.

## Project layout

```
BlockEnvironmentCopilot.sln
src/
  BlockEnvironmentCopilot/
    BlockEnvironmentCopilot.csproj   # SDK-style project targeting .NET Framework 4.6.2
    BlockEnvironmentCopilot.snk      # Strong-name key (required for registration)
    PluginBase.cs                    # Base class handling context/service boilerplate
    LocalPluginContext.cs            # Wrapper exposing common Dataverse services
    BlockBotCreation.cs              # Blocks Copilot agent (bot) creation
```

## Requirements

- Dataverse plugins **must** target **.NET Framework 4.6.2**.
- The assembly **must be strong-named** (already configured via `MyPlugin.snk`).
- Keep third-party dependencies to a minimum so the assembly can run in
  **sandbox (isolation) mode**. If you must add dependencies, merge them with
  [ILRepack](https://github.com/gluck/il-repack) or `ILMerge`.

## Build

```powershell
dotnet build -c Release
```

The signed assembly is produced at `src/BlockEnvironmentCopilot/bin/Release/BlockEnvironmentCopilot.dll`.

## Register the plugin

Use one of the following:

- **Plugin Registration Tool** (`pac tool prt`) from the Power Platform CLI.
- **pac plugin push** from the Power Platform CLI.

Registration for the included `BlockBotCreation` plugin (register in any Environment to block Copilot agent creation there):

| Setting          | Value                     |
| ---------------- | ------------------------- |
| Message          | `Create`                  |
| Primary Entity   | `bot`                     |
| Stage            | `PreValidation`           |
| Execution Mode   | `Synchronous`             |

## Develop

1. Put your logic in a class that derives from `PluginBase` and override
   `ExecuteDataversePlugin(ILocalPluginContext)`.
2. Use `localContext.InitiatingUserService` / `SystemUserService` for data
   operations, `localContext.Trace(...)` for logging.
3. Throw `InvalidPluginExecutionException` to surface business errors to the user.

## Useful CLI commands

```powershell
# Install the Power Platform CLI (if needed)
dotnet tool install --global Microsoft.PowerApps.CLI.Tool

# Launch the Plugin Registration Tool
pac tool prt
```
# Original Idea - https://www.linkedin.com/pulse/how-block-copilot-studio-default-environment-using-simple-ananthula-62qaf/

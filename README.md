# BlockEnvironmentCopilot — Dataverse C# Plugin

A Microsoft Dataverse (Dynamics 365 / Power Platform) plug-in that blocks
Copilot Studio agent creation in the environment where it is registered.

## Project layout

```
BlockEnvironmentCopilot.sln
src/
  BlockEnvironmentCopilot/
    BlockEnvironmentCopilot.csproj   # SDK-style project targeting .NET Framework 4.6.2
    BlockEnvironmentCopilot.snk      # Strong-name key (NOT in source control — supply your own)
    PluginBase.cs                    # Base class handling context/service boilerplate
    LocalPluginContext.cs            # Wrapper exposing common Dataverse services
    BlockBotCreation.cs              # Blocks Copilot agent (bot) creation
solution/                           # Exported unmanaged Power Platform solution (.zip)
```

## Requirements

- Dataverse plugins **must** target **.NET Framework 4.6.2**.
- The assembly **must be strong-named**. The strong-name key
  (`src/BlockEnvironmentCopilot/BlockEnvironmentCopilot.snk`) is **not included
  in this repository** and is excluded via `.gitignore`. Before building from
  source, generate your own key in that location:

  ```powershell
  sn -k src/BlockEnvironmentCopilot/BlockEnvironmentCopilot.snk
  ```
- Keep third-party dependencies to a minimum so the assembly can run in
  **sandbox (isolation) mode**. If you must add dependencies, merge them with
  [ILRepack](https://github.com/gluck/il-repack) or `ILMerge`.

## Build

```powershell
dotnet build -c Release
```

The signed assembly is produced at `src/BlockEnvironmentCopilot/bin/Release/BlockEnvironmentCopilot.dll`.

## Install (recommended)

The easiest way to deploy this control is to **import the packaged solution** — no
manual plugin registration required. The solution already contains the plugin
assembly and its registered step.

1. Download the solution zip from the [`solution/`](solution) folder:
   [`BlockBotCreation_1_0_0_2.zip`](solution/BlockBotCreation_1_0_0_2.zip).
2. In the [Power Platform admin center](https://make.powerapps.com) select the
   target environment, then **Solutions → Import solution**.
3. Choose the downloaded zip and complete the import.

Once imported, the plugin step is registered automatically. Blocking is
**off by default** — you must turn it on via the `bbc_BlockOn` environment
variable (see below).

## Environment variables

The plugin's behavior is controlled by two Dataverse environment variables:

| Schema name         | Purpose                                                        | Default / behavior                                                                 |
| ------------------- | -------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| `bbc_BlockOn`       | Master switch that turns blocking on or off.                   | **Off by default.** Blocking occurs only when the value is `Yes` (case-insensitive). Any other value (including `No`, empty, or unset) allows agent creation. |
| `bbc_MakerUIMessage`| Custom message shown to makers when creation is blocked.       | Optional. Falls back to a built-in default message when unset.                     |

To block Copilot Studio agent creation in an environment, set `bbc_BlockOn` to
`Yes`. To re-enable creation, set it to `No` (or clear the value).

## Register the plugin manually (developers only)

If you're building from source instead of importing the solution, register the
step yourself using one of the following:

- **Plugin Registration Tool** (`pac tool prt`) from the Power Platform CLI.
- **pac plugin push** from the Power Platform CLI.

Registration for the included `BlockBotCreation` plugin (register in any Environment to block Copilot agent creation there):

| Setting          | Value                     |
| ---------------- | ------------------------- |
| Message          | `Create`                  |
| Primary Entity   | `bot`                     |
| Stage            | `PreValidation`           |
| Execution Mode   | `Synchronous`             |

## Solution (download & deploy)

The [`solution/`](solution) folder holds the exported **unmanaged** Power Platform
solution `.zip`.

1. **Download** the unmanaged solution into a **development environment**.
2. **Update** it there as needed (plugin step, environment variable, or other
   components).
3. **Re-export** the updated unmanaged solution back into `solution/` to keep
   source control current.
4. **Deploy** to downstream environments as a **managed** solution using your
   normal ALM process (pipeline, `pac solution import`, or managed export/import).

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

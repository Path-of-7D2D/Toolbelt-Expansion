# Contributor Guide

This repository contains both the deployable mod folder and the C# source used
to build `ToolbeltExpansion.dll`.

## Project layout

```text
1A-12SlotToolbeltExpansion/         deployable mod folder
  ModInfo.xml                       mod manifest
  Config/XUi_InGame/windows.xml     toolbelt layout patch
  Config/Localization.csv           slot 11/12 control labels
  ToolbeltExpansion.dll             built Harmony mod assembly

src/ToolbeltExpansion/              C# Harmony project
  ToolbeltExpansionModApi.cs        IModApi entry point and Harmony bootstrap
  ToolbeltSlotPatches.cs            Inventory.PUBLIC_SLOTS patches
  ToolbeltHotkeyPatches.cs          slot 11/12 control patches
  ToolbeltExpansion.csproj
```

The `1A-` prefix controls load order. 7 Days to Die loads mod folders
alphabetically.

## Why the mod needs a DLL

The toolbelt's functional size is not controlled by XML. In V3.0, the normal
play toolbelt count comes from `Inventory.PUBLIC_SLOTS`, backed by hardcoded
game values:

| Context       | Vanilla value |
| ------------- | ------------- |
| Normal play   | 10            |
| Prefab editor | 20            |

The XUi patch only changes how the belt is drawn. The Harmony patch is required
so the extra slots are real inventory slots instead of empty UI cells.

## Build requirements

- .NET SDK
- Local 7 Days to Die V3.0 install
- Game reference assemblies from `7DaysToDie_Data/Managed`
- Harmony from `Mods/0_TFP_Harmony/0Harmony.dll`

Build from the repository root:

```sh
dotnet build src/ToolbeltExpansion/ToolbeltExpansion.csproj -c Release
```

The build copies `ToolbeltExpansion.dll` into
`1A-12SlotToolbeltExpansion/`.

If the game is installed at the default Steam path, the build also refreshes the
live game install at:

```text
7 Days To Die/Mods/1A-12SlotToolbeltExpansion/
```

Disable live install refresh with:

```sh
dotnet build src/ToolbeltExpansion/ToolbeltExpansion.csproj -c Release -p:InstallToGame=false
```

If your game is not at the default Steam path, override it:

```sh
dotnet build src/ToolbeltExpansion/ToolbeltExpansion.csproj -c Release -p:Game7D2D="D:\SteamLibrary\steamapps\common\7 Days To Die"
```

You can also set `GAME_7D2D` in the environment.

## Changing the slot count

Keep the functional slot count and UI layout in sync.

Update:

- `ExpandedSlots` in `src/ToolbeltExpansion/ToolbeltSlotPatches.cs`
- Toolbelt hotkey handling in `src/ToolbeltExpansion/ToolbeltHotkeyPatches.cs`
- `cols`, `width`, and `pos` in
  `1A-12SlotToolbeltExpansion/Config/XUi_InGame/windows.xml`
- Control labels in `1A-12SlotToolbeltExpansion/Config/Localization.csv`
- User-facing docs in `README.md`

After changing the slot count, test that items survive in every added slot and
that the toolbelt stays in one row during normal play.

## Release process

Releases are created by the manual GitHub Actions workflow in
`.github/workflows/release.yml`.

The workflow:

- Accepts a manual `version_tag`, such as `0.1.0`.
- Zips `1A-12SlotToolbeltExpansion`.
- Creates or updates the GitHub Release for that tag.
- Generates release notes through `Path-of-7D2D/Changelog-Generator`.
- Includes commits since the latest previous release.
- Groups changelog entries by conventional commit type.

Use conventional commit subjects for useful release notes:

```text
feat: add slot 11 and 12 hotkeys
fix: prevent second toolbelt row in normal play
docs: clarify multiplayer install requirements
```

## Validation checklist

Before publishing a release:

- Build in `Release` configuration.
- Confirm `1A-12SlotToolbeltExpansion/ToolbeltExpansion.dll` is current.
- Start the game with EasyAntiCheat off.
- Confirm the toolbelt shows one row with 12 slots.
- Confirm slots 11 and 12 keep items after closing and reopening inventory.
- Confirm `-` selects slot 11 and `=` selects slot 12.
- Confirm the controls can be rebound in the Controls menu.
- If testing multiplayer changes, test with both server and client installed.

# Toolbelt Expansion

Expands the 7 Days to Die player toolbelt from **10** slots to **12**.

Built for the **V3.0 "Dead Hot Summer"** experimental branch.

---

## Why this needs a DLL (and isn't XML-only)

On V3.0 the toolbelt's *functional* size is `Inventory.PUBLIC_SLOTS`, a hardcoded
property in `Assembly-CSharp.dll`:

| Context        | Vanilla value |
| -------------- | ------------- |
| Normal play    | 10 (`PUBLIC_SLOTS_PLAYMODE`) |
| Prefab editor  | 20 (`PUBLIC_SLOTS_PREFABEDITOR`) |

There is **no XML, cvar, or GamePref** that changes it. Editing only the XUi grid
would draw extra cells that can't actually hold items. So this mod is two parts:

1. **A Harmony patch** (`ToolbeltExpansion.dll`) raises the play-mode count to 12.
   The prefab editor's 20-slot belt is left alone.
2. **An XUi patch** (`Config/XUi_InGame/windows.xml`) widens the on-screen belt to a
   single row of 12 cells.

## Contents

```
1A-12SlotToolbeltExpansion/         # the deployable mod - copy this folder into Mods/
  ModInfo.xml                       #   mod manifest (V3.0 format)
  Config/XUi_InGame/windows.xml     #   toolbelt layout: single row of 12
  ToolbeltExpansion.dll             #   built output, deployed here (git-ignored)
src/ToolbeltExpansion/              # C# Harmony project (source)
  ToolbeltExpansionModApi.cs        #   IModApi entry point -> Harmony bootstrap
  ToolbeltSlotPatches.cs            #   the PUBLIC_SLOTS patches
  ToolbeltExpansion.csproj
```

The `1A-` prefix sets the load order (7DTD loads mod folders alphabetically).

## Building

Requires the .NET SDK and a local 7 Days to Die V3.0 install (for the reference
assemblies). Harmony is provided by the game's `Mods/0_TFP_Harmony/0Harmony.dll`.

```sh
dotnet build src/ToolbeltExpansion/ToolbeltExpansion.csproj -c Release
```

The build copies `ToolbeltExpansion.dll` into `1A-12SlotToolbeltExpansion/`, completing
that folder as a ready-to-install modlet. If the game is installed, it **also refreshes the
live install** at `7 Days To Die/Mods/1A-12SlotToolbeltExpansion/` (disable with
`-p:InstallToGame=false`).

> **Important:** the DLL is git-ignored (it's a build artifact). A plain file/Git copy of
> this folder will **not** include it, and the mod will then load its XML (you'll see 12
> cells) but silently drop items in slots 11-12 because the Harmony patch isn't running.
> Always `dotnet build` to produce/refresh the DLL.

If your game is not at the default Steam path, override it:

```sh
dotnet build src/ToolbeltExpansion/ToolbeltExpansion.csproj \
  -p:Game7D2D="D:\SteamLibrary\steamapps\common\7 Days To Die"
```

## Installing

Copy the `1A-12SlotToolbeltExpansion` folder into:

```
7 Days To Die/Mods/
```

so that `Mods/1A-12SlotToolbeltExpansion/ModInfo.xml` exists.

- **EasyAntiCheat must be OFF** (this mod uses Harmony). `ModInfo.xml` sets
  `SkipWithAntiCheat`, so with EAC on the game simply skips the mod instead of erroring.
- **Multiplayer:** the slot count is part of the networked inventory, so the mod must be
  installed on the **server and every client**. Mismatched installs will desync the belt.

## Configuring the slot count

Change `ExpandedSlots` in
[`ToolbeltSlotPatches.cs`](src/ToolbeltExpansion/ToolbeltSlotPatches.cs) and, to keep the
HUD in sync, the `cols` / `width` / `pos` values in
[`windows.xml`](1A-12SlotToolbeltExpansion/Config/XUi_InGame/windows.xml). Then rebuild.

## First-run check (the one thing to verify in-game)

The single-row-of-12 layout assumes the toolbelt window only shows its built-in second
row inside the prefab editor. If, on your first test, the belt instead appears as **two
rows (10 + 2)** or shows a phantom empty second row, enable the commented-out
`HasSecondRow_Patch` at the bottom of `ToolbeltSlotPatches.cs` and rebuild.

**Known limitation:** the default number-key bindings (`1`–`0`) only reach the first 10
slots. Slots 11 and 12 are reachable with the mouse wheel; binding extra hotkeys is out
of scope for this version.

## Compatibility

Conflicts with any other mod that patches `Inventory.PUBLIC_SLOTS` or replaces the
`windowToolbelt` XUi window.

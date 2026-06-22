# Toolbelt Expansion

Toolbelt Expansion gives the 7 Days to Die player toolbelt **12 usable slots**
instead of the vanilla 10.

It includes the UI changes and the Harmony patch needed to make slots 11 and 12
actually hold items.

Built for **7 Days to Die V3.0 "Dead Hot Summer"**.

## Features

- Expands the normal player toolbelt from 10 slots to 12.
- Keeps the prefab editor's larger toolbelt behavior intact.
- Adds rebindable controls for slot 11 and slot 12.
- Keeps the belt in one clean row of 12 slots.
- Skips loading when EasyAntiCheat is enabled, instead of crashing the game.

## Download

Download the latest `ToolbeltExpansion-*.zip` from the
[GitHub Releases](https://github.com/Path-of-7D2D/Toolbelt-Expansion/releases)
page.

Do not install the repository source zip unless you are building the mod
yourself. Use the release zip so the required DLL is included.

## Installation

1. Turn off EasyAntiCheat before launching the game.
2. Extract the release zip.
3. Copy the `1A-12SlotToolbeltExpansion` folder into your `Mods` folder:

```text
7 Days To Die/Mods/1A-12SlotToolbeltExpansion/
```

The folder is installed correctly when this file exists:

```text
7 Days To Die/Mods/1A-12SlotToolbeltExpansion/ModInfo.xml
```

## Controls

Vanilla number keys still select slots 1 through 10.

This mod adds:

| Slot | Default key |
| ---- | ----------- |
| 11   | `-`         |
| 12   | `=`         |

The new controls appear in the game's Controls menu under the **Toolbelt**
group as `Slot 11` and `Slot 12`, so you can rebind them.

The mouse wheel cycles through all 12 slots. Vanilla's hidden `Shift+1` and
`Shift+2` behavior can also reach slots 11 and 12 once the belt is expanded.

## Multiplayer

This mod changes the player's inventory slot count. For multiplayer, install it
on the **server and every client**.

Mismatched installs can cause inventory desyncs or missing items in the extra
slots.

## EasyAntiCheat

This mod uses Harmony, so EasyAntiCheat must be off.

The mod is marked with `SkipWithAntiCheat`, which means the game should skip it
cleanly when EAC is enabled.

## Compatibility

This mod can conflict with other mods that:

- Patch `Inventory.PUBLIC_SLOTS`.
- Replace or heavily modify the `windowToolbelt` XUi window.
- Change toolbelt slot selection controls.

## Troubleshooting

If you see 12 slots but slots 11 and 12 do not keep items, the DLL is missing or
did not load. Reinstall from the release zip and make sure EasyAntiCheat is off.

If the belt appears as two rows, report the issue with your game version and any
other UI mods you are using.

## Contributing

Source, build, release, and implementation details live in
[CONTRIBUTOR.md](CONTRIBUTOR.md).

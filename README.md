# Speedometer

A BepInEx client mod for Mycopunk that displays your current movement speed on the HUD.

## Features

- **Live speed display** — Shows your current movement speed in m/s
- **Toggleable HUD** — Enable or disable the speedometer from the config file
- **Configurable position** — Adjust X/Y anchor position to place the HUD wherever you like
- **Configurable color** — Customize the speed value color (default: sky blue)
- **Hot reload** — Config changes are picked up automatically while the game is running
- **Optional HUD repositioning** — Registers with HudRepositionAPI when available for in-game dragging

## Dependencies

- Mycopunk
- [BepInEx](https://github.com/BepInEx/BepInEx) 5.4.2403 or compatible (BepInExPack_Mycopunk)
- [SparrohUILib](https://thunderstore.io/c/mycopunk/p/Sparroh/SparrohUILib/) 1.2.0 or compatible

## Installation

**Via Thunderstore (recommended)**

1. Install with a Thunderstore-compatible mod manager (e.g. r2modman or Thunderstore Mod Manager).
2. Required dependencies are installed automatically.

**Manual installation**

1. Install BepInEx and SparrohUILib for Mycopunk.
2. Place `Speedometer.dll` in `<Mycopunk Directory>/BepInEx/plugins/`.

The mod loads automatically through BepInEx when the game starts. Check the BepInEx log for a successful load message.

## Building

1. Clone this repository.
2. Open the solution in Visual Studio, Rider, or another C# IDE.
3. Ensure game and dependency assembly paths in the project file match your install.
4. Build in Release mode:

```bash
dotnet build --configuration Release
```

The output DLL is written to `bin/Release/netstandard2.1/Speedometer.dll`.

## Configuration

Settings are stored at:

`<Mycopunk Directory>/BepInEx/config/sparroh.speedometer.cfg`

| Section         | Key                | Default  | Description                                   |
|-----------------|--------------------|----------|-----------------------------------------------|
| General         | Enable Speedometer | `true`   | Toggles the speedometer HUD on or off         |
| HUD Positioning | Speedometer X      | `~0.064` | Horizontal anchor (0–1)                       |
| HUD Positioning | Speedometer Y      | `~0.230` | Vertical anchor (0–1)                         |
| Colors          | Speed Color        | sky blue | Rich-text value color (`RRGGBB` or `#RRGGBB`) |

## Troubleshooting

- **Mod not loading?** Confirm BepInEx and SparrohUILib are installed, then check the BepInEx log for errors.
- **Speed not updating?** Make sure you are in-game with a local player loaded.
- **HUD position wrong?** Adjust Speedometer X / Speedometer Y in the config, or use a HUD reposition mod if available.
- **No speed detected?** The mod tries several player velocity sources; if none report motion you will see "No Speed
  Detected".

## Authors

- Sparroh

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

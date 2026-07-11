# Speedometer

A BepInEx mod for MycoPunk that displays your current movement speed on the HUD.

## Features

- **Live Speed Display**: Shows your current movement speed in m/s on a configurable HUD element
- **Toggleable HUD**: Enable or disable the speedometer from the config file
- **Configurable Position**: Adjust X/Y anchor position to place the HUD wherever you like
- **Hot Reload**: Config changes are picked up automatically while the game is running

## Getting Started

### Dependencies

* MycoPunk (base game)
* [BepInEx](https://github.com/BepInEx/BepInEx) - Version 5.4.2403 or compatible
* .NET Framework 4.8
* [HarmonyLib](https://github.com/pardeike/Harmony) (included via NuGet)

### Building/Compiling

1. Clone this repository
2. Open the solution file in Visual Studio, Rider, or your preferred C# IDE
3. Build the project in Release mode to generate the .dll file

Alternatively, use dotnet CLI:
```bash
dotnet build --configuration Release
```

### Installing

**Via Thunderstore (Recommended)**:
1. Download and install via Thunderstore Mod Manager
2. The mod will be automatically installed to the correct directory

**Manual Installation**:
1. Place the built `Speedometer.dll` in your `<MycoPunk Directory>/BepInEx/plugins/` folder

### Executing program

The mod loads automatically through BepInEx when the game starts. Check the BepInEx console for loading confirmation messages.

## Configuration

Access mod settings through the BepInEx configuration file at `<MycoPunk Directory>/BepInEx/config/sparroh.speedometer.cfg`. Key options include:

- **EnableSpeedometerHUD**: Toggle the speedometer display on/off (default: true)
- **SpeedometerAnchorX**: Horizontal anchor position (0–1, default: 0.15)
- **SpeedometerAnchorY**: Vertical anchor position (0–1, default: 0.86)

## Help

* **Mod not loading?** Verify BepInEx is installed correctly and check console logs for errors
* **Speed not updating?** Ensure you are in-game with a local player loaded
* **HUD position wrong?** Adjust `SpeedometerAnchorX` / `SpeedometerAnchorY` in the config file

## Authors

- Sparroh

## License

This project is licensed under the MIT License - see the LICENSE file for details

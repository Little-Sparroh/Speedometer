# Speedometer

A BepInEx mod for MycoPunk that displays real-time player speed on-screen as a HUD element.

## Description

This client-side mod adds a non-intrusive speedometer HUD to your game interface, showing your current velocity in meters per second (m/s). Perfect for speedrunners, racers, or players who want precise movement feedback. The mod intelligently detects player velocity through multiple sources to ensure accurate readings across different movement types and scenarios.

The HUD is positioned near the damage meter for easy viewing and can be toggled on/off with the F6 key. The speed display uses an attractive sky blue color for the numerical values and automatically positions itself to avoid overlapping with other UI elements. When no speed is detected, it gracefully displays a "No Speed Detected" message.

## Getting Started

### Dependencies

* MycoPunk (base game)
* [BepInEx](https://github.com/BepInEx/BepInEx) - Version 5.4.2403 or compatible
* .NET Framework 4.8

### Building/Compiling

1. Clone this repository
2. Open the solution file in Visual Studio, Rider, or your preferred C# IDE
3. Build the project in Release mode

Alternatively, use dotnet CLI:
```bash
dotnet build --configuration Release
```

### Installing

**Option 1: Via Thunderstore (Recommended)**
1. Download and install using the Thunderstore Mod Manager
2. Search for "Speedometer" under MycoPunk community
3. Install and enable the mod

**Option 2: Manual Installation**
1. Ensure BepInEx is installed for MycoPunk
2. Copy `Speedometer.dll` from the build folder
3. Place it in `<MycoPunk Game Directory>/BepInEx/plugins/`
4. Launch the game

### Executing program

Once installed, the mod works automatically during gameplay:

**HUD Features:**
- **Real-time Speed Display:** Updates every frame with current velocity
- **Multiple Detection Methods:** Velocity from Player velocity field/property, Rigidbody, currentMoveSpeed, or moveVelocity
- **Smart Positioning:** Automatically repositions when damage meter is visible
- **Color Coded:** Speed values in attractive sky blue color
- **Toggle Control:** Press F6 to show/hide the speedometer HUD
- **Non-intrusive:** Small, clean display that doesn't interfere with gameplay

**Interface Behavior:**
- HUD appears in top-center area near reticle
- Numbers formatted to 1 decimal place for precision
- Graceful fallback displays when velocity detection fails
- Maintains proper layering with other UI elements

## Help

* **HUD not showing?** Press F6 to toggle visibility - it starts hidden by default
* **Wrong position?** The HUD positions relative to the reticle and damage meter - should be in top area
* **No speed displayed?** Mod tries multiple velocity sources - if none work, check logs for detection method
* **Performance impact?** Minimal - only monitors velocity fields that already exist
* **Conflicts with other mods?** Shouldn't conflict unless other mods modify Player velocity fields
* **BepInEx logs?** Check console for which velocity detection method was found during startup
* **Disable temporarily?** Uninstall the mod if you want to disable permanently instead of toggling

## Authors

* Sparroh
* funlennysub (original mod template)
* [@DomPizzie](https://twitter.com/dompizzie) (README template)

## License

* This project is licensed under the MIT License - see the LICENSE.md file for details

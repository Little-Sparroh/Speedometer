# Changelog

## 1.2.2

### Changed

- Refactored config into `ConfigManager` with debounced hot-reload (aligned with other Sparroh HUD mods)
- Extracted player speed reflection into `PlayerSpeedReader`
- Use SparrohUILib `HudAnchors` / `HudHandle.EnableReposition` instead of a local HudRepositionClient
- Raised SparrohUILib dependency to 1.2.0

## 1.2.1

### Fixed

- Reload the speedometer UI after returning to the main menu

## 1.2.0

### Added

- Initial standalone release of Speedometer (split from ExpandedHUD)
- Configurable HUD enable toggle and anchor position
- Configurable speed value color
- Live player speed display in m/s
- Optional integration with HudRepositionAPI for in-game HUD repositioning
- Dependency on SparrohUILib for HUD building

## 1.1.0

### Changed

- Enhanced HUD positioning to avoid overlap with the damage meter
- Improved positioning logic for better UI integration

### Added

- Sky blue color for speed display values

## 1.0.2

### Added

- Basic speedometer functionality with velocity detection
- Support for multiple velocity sources (Player velocity, Rigidbody, moveVelocity, currentMoveSpeed)
- F6 toggle for HUD visibility
- Real-time m/s display updates during gameplay

## 1.0.0

### Added

- MinVer
- thunderstore.toml for tcli
- LICENSE and CHANGELOG.md

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.5.5] - 2024-06-08

### Changed

- Remove microfreeze when opening / closing tables by greatly improving map exploration sharing handling.

## [0.5.4] - 2024-06-08

### Changed

- Add further compatibility for sharing private pins manually added by other mods.

## [0.5.3] - 2024-06-08

### Fixed

- Fix `NullReferenceException` errors due to keyhints UI update running when it should not.

## [0.5.2] - 2024-06-08

### Fixed

- Fix `NullReferenceException` errors due to table distance check running when it should not.

## [0.5.1] - 2024-06-07

### Changed

- Include missing `Ashlands update` tag for Thunderstore package.

## [0.5.0] - 2024-06-03

### Added

- Add config toggle for map exploration sharing.

## [0.4.3] - 2024-06-01

### Changed

- Document how to add new translations in the README.

## [0.4.2] - 2024-05-30

### Fixed

- Fix warnings `Failed to find rpc method` due to RPC methods not being registered immediately.
- Fix `NullReferenceException` errors due to Minimap toggles not being fully initialized.
- Fix shared map exploration toggle label not being replaced from its vanilla defaults.
- Fix toggles visibility on game load.

## [0.4.1] - 2024-05-29

### Changed

- Improve README.

## [0.4.0] - 2024-05-29

### Added

- Automatically close map when moving too far away from the cartography table.

### Changed

- Support for 0.218.15 (Ashlands).
- Improve compatibility with others mods that also interact with pins.
- Forbid changing table sharing mode if any pins are stored on the table (instead of allowing it with a warning).
- Switch to Jotunn for localization handling, instead of standalone LocalizationManager.

### Fixed

- Fix public player positions not always updating / displaying when more than 2 players are connected at once.
- Fix pins toggles positioning so they are always properly ordered without gaps.

## [0.3.0] - 2024-04-12

### Added

- Support for `NoMap` world modifier: interacting with a cartography table will display the map.
- Enforce mod to be installed on all clients if installed on the server (Jotunn version check).

### Changed

- Updated for Valheim 0.217.46.

### Fixed

- Fix `NullReferenceException` errors if shift-clicking a pin on the map without interacting with a cartography table.

## [0.2.0] - 2024-02-25

### Added

- Add limited one-way compatibility for public pins sharing from modded clients to vanilla non-modded clients.

### Fixed

- Fix shared pins disappearing if a cartography table is in use while a save occurs.
- Fix `NullReferenceException` errors if hovering a cartography table while the game is shutting down.

## [0.1.1] - 2024-02-18

### Fixed

- Restrict map interactions to sharable pin types.
- Remove debug logging.
- Fix pin type filter toggling flooding logs with `NullReferenceException` errors.

## [0.1.0] - 2024-02-12

### Added

- Initial release.

[unreleased]: https://github.com/nbusseneau/BetterCartographyTable/compare/0.5.5...HEAD
[0.5.5]: https://github.com/nbusseneau/BetterCartographyTable/compare/0.5.4...0.5.5
[0.5.4]: https://github.com/nbusseneau/BetterCartographyTable/compare/0.5.3...0.5.4
[0.5.3]: https://github.com/nbusseneau/BetterCartographyTable/compare/0.5.2...0.5.3
[0.5.2]: https://github.com/nbusseneau/BetterCartographyTable/compare/0.5.1...0.5.2
[0.5.1]: https://github.com/nbusseneau/BetterCartographyTable/compare/0.5.0...0.5.1
[0.5.0]: https://github.com/nbusseneau/BetterCartographyTable/compare/0.4.3...0.5.0
[0.4.3]: https://github.com/nbusseneau/BetterCartographyTable/compare/0.4.2...0.4.3
[0.4.2]: https://github.com/nbusseneau/BetterCartographyTable/compare/0.4.1...0.4.2
[0.4.1]: https://github.com/nbusseneau/BetterCartographyTable/compare/0.4.0...0.4.1
[0.4.0]: https://github.com/nbusseneau/BetterCartographyTable/compare/0.3.0...0.4.0
[0.3.0]: https://github.com/nbusseneau/BetterCartographyTable/compare/0.2.0...0.3.0
[0.2.0]: https://github.com/nbusseneau/BetterCartographyTable/compare/0.1.1...0.2.0
[0.1.1]: https://github.com/nbusseneau/BetterCartographyTable/compare/0.1.0...0.1.1
[0.1.0]: https://github.com/nbusseneau/BetterCartographyTable/compare/d81736f2634eb52248432e1a66f59ac0acb491b4...0.1.0

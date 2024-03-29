# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.0] - 2024-02-25

### Added

-   Add limited one-way compatibility for public pins sharing from modded clients to vanilla non-modded clients.

### Fixed

-   Fix shared pins disappearing if a cartography table is in use while a save occurs.
-   Fix `NullReferenceException` errors if hovering a cartography table while the game is shutting down.

## [0.1.1] - 2024-02-18

### Fixed

-   Restrict map interactions to sharable pin types.
-   Remove debug logging.
-   Fix pin type filter toggling flooding logs with `NullReferenceException` errors.

## [0.1.0] - 2024-02-12

### Added

-   Initial release.

[Unreleased]: https://github.com/nbusseneau/BetterCartographyTable/compare/0.2.0...HEAD

[0.2.0]: https://github.com/nbusseneau/BetterCartographyTable/compare/0.1.1...0.2.0

[0.1.1]: https://github.com/nbusseneau/BetterCartographyTable/compare/0.1.0...0.1.1

[0.1.0]: https://github.com/nbusseneau/BetterCartographyTable/compare/d81736f2634eb52248432e1a66f59ac0acb491b4...0.1.0

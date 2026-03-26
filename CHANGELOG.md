# Changelog

## [0.3.0] - 2026-03-25

### Changed

- Settings now track per-monitor auto-hide preferences instead of scenario-based rules (internal-only, external-only, both)
- Context menu shows interactive checkboxes for each monitor instead of scenario toggles
- Disconnected monitors remain in the menu (labeled "disconnected") and retain their saved preferences
- New monitors default to the current Windows taskbar auto-hide state when first detected

### Removed

- Scenario-based settings (`hide-when-internal-only`, `hide-when-external-only`, `hide-when-both`) replaced by per-monitor settings

### Fixed

- Checksum verification added to remote install script
- README updated to mention OLED burn-in prevention as primary use case

## [0.2.0] - 2026-03-21

### Added

- Display detected monitors with friendly names in the tray context menu
- Show monitor summary (internal/external) in the tray icon tooltip
- Remote Desktop detection and configurable RDP auto-hide scenario
- "Hide when:" prefix on scenario menu items for clarity

### Changed

- Settings keys renamed to `hide-when-*` format (e.g., `hide-when-internal-only`)

### Fixed

- Release workflow artifact download path so binaries attach to GitHub releases
- Combined per-platform checksums into single `checksums.txt`

## [0.1.0] - 2026-03-21

Initial public release.

### Added

- System tray app that automatically toggles taskbar auto-hide based on monitor configuration
- Detects monitor connect/disconnect events in real time via `WM_DISPLAYCHANGE`
- Distinguishes internal (laptop) vs external displays using `QueryDisplayConfig`
- Configurable per-scenario auto-hide preferences (internal only, external only, both monitors)
- Settings persisted across sessions in `~/.config/TuckBar/settings.yml`
- Taskbar auto-hide toggled via `SHAppBarMessage` with registry persistence
- System tray icon changes color to reflect current state (blue = ON, gray = OFF)
- Manual temporary override via context menu
- "Start with Windows" option via registry Run key
- PowerShell install scripts for both local builds and remote binary download

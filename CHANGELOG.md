# Changelog

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

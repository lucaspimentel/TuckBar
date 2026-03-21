# TuckBar

[![CI](https://github.com/lucaspimentel/TuckBar/actions/workflows/ci.yml/badge.svg)](https://github.com/lucaspimentel/TuckBar/actions/workflows/ci.yml) [![Release](https://github.com/lucaspimentel/TuckBar/actions/workflows/release.yml/badge.svg)](https://github.com/lucaspimentel/TuckBar/actions/workflows/release.yml)

A Windows system tray app that automatically toggles taskbar auto-hide based on your monitor configuration — designed to prevent OLED screen burn-in by hiding the static taskbar when an OLED display is active.

For example, if your laptop has an OLED screen but your external monitor does not, TuckBar can automatically enable auto-hide when only the internal OLED display is in use, and disable it when the external (non-OLED) monitor is connected.

| Scenario | Auto-hide (default) |
|---|---|
| Internal (laptop) monitor only | ON |
| External monitor only | OFF |
| Both monitors connected | ON |
| Remote Desktop session | OFF |

All scenarios are configurable via the tray icon context menu. Settings persist across sessions in `~/.config/TuckBar/settings.yml`.

## Features

- Detects monitor connect/disconnect events in real time
- Distinguishes internal (laptop) vs external displays using `QueryDisplayConfig`
- Toggles taskbar auto-hide via `SHAppBarMessage` (with registry persistence)
- System tray icon changes color to reflect current state (blue = auto-hide ON, gray = OFF)
- Configurable per-scenario auto-hide preferences with persistent settings
- Displays detected monitors with friendly names in the context menu
- Remote Desktop session detection with configurable auto-hide behavior
- Manual toggle and "Start with Windows" options in the context menu

## Installation

### Download pre-built binary

Requires PowerShell 7+.

```pwsh
irm https://raw.githubusercontent.com/lucaspimentel/TuckBar/main/install-remote.ps1 | iex
```

### Build from source

Requires PowerShell 7+ and [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```pwsh
git clone https://github.com/lucaspimentel/TuckBar
cd TuckBar
./install-local.ps1
```

Both scripts install to `~/.local/bin/TuckBar.exe`. Ensure that directory is in your `PATH`.

## Usage

Run `TuckBar` — it starts in the system tray. Right-click the tray icon for options:

- **Detected monitors** — read-only list of connected displays with names and types
- **Auto-hide (temporary)** — shows current state; click to manually override until the next display change
- **Hide when: ...** — choose which scenarios enable auto-hide (internal only, external only, both, Remote Desktop)
- **Start with Windows** — enable/disable auto-start on sign-in
- **Exit** — close the app

## License

This project is licensed under the [MIT License](LICENSE).

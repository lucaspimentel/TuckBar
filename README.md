# TuckBar

[![CI](https://github.com/lucaspimentel/TuckBar/actions/workflows/ci.yml/badge.svg)](https://github.com/lucaspimentel/TuckBar/actions/workflows/ci.yml) [![Release](https://github.com/lucaspimentel/TuckBar/actions/workflows/release.yml/badge.svg)](https://github.com/lucaspimentel/TuckBar/actions/workflows/release.yml)

A Windows system tray app that automatically toggles taskbar auto-hide based on your monitor configuration.

| Scenario | Auto-hide (default) |
|---|---|
| Internal (laptop) monitor only | ON |
| External monitor only | OFF |
| Both monitors connected | ON |

All scenarios are configurable via the tray icon context menu. Settings persist across sessions in `~/.config/TuckBar/settings.yml`.

## Features

- Detects monitor connect/disconnect events in real time
- Distinguishes internal (laptop) vs external displays using `QueryDisplayConfig`
- Toggles taskbar auto-hide via `SHAppBarMessage` (with registry persistence)
- System tray icon changes color to reflect current state (blue = auto-hide ON, gray = OFF)
- Configurable per-scenario auto-hide preferences with persistent settings
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

- **Auto-hide (temporary)** — shows current state; click to manually override until the next display change
- **Internal monitor only / External monitor only / Both monitors** — choose when auto-hide is enabled (persisted)
- **Start with Windows** — enable/disable auto-start on sign-in
- **Exit** — close the app

## License

This project is licensed under the [MIT License](LICENSE).

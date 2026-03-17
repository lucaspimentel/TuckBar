# PROJECT

[![CI](https://github.com/lucaspimentel/PROJECT_REPO/actions/workflows/ci.yml/badge.svg)](https://github.com/lucaspimentel/PROJECT_REPO/actions/workflows/ci.yml) [![Release](https://github.com/lucaspimentel/PROJECT_REPO/actions/workflows/release.yml/badge.svg)](https://github.com/lucaspimentel/PROJECT_REPO/actions/workflows/release.yml)

TODO: Add description.

Pre-built binaries support Windows and Linux. macOS is supported when built from source.

## Installation

### Scoop (Windows)

```powershell
scoop bucket add lucaspimentel https://github.com/lucaspimentel/scoop-bucket
scoop install PROJECT
```

### Download pre-built binary

Requires PowerShell 7+.

```pwsh
irm https://raw.githubusercontent.com/lucaspimentel/PROJECT_REPO/main/install-remote.ps1 | iex
```

### Build from source

Requires PowerShell 7+ and [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```pwsh
git clone https://github.com/lucaspimentel/PROJECT_REPO
cd PROJECT_REPO
./install-local.ps1
```

Both scripts install to `~/.local/bin/PROJECT`. Ensure that directory is in your `PATH`.

## Usage

```
PROJECT
```

## License

This project is licensed under the [MIT License](LICENSE).

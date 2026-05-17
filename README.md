# BeatGame

A Windows-native rhythm game built with C# .NET 8 and [Raylib-cs](https://github.com/raylib-cs/raylib-cs).

## Releases

You can download the finished game here: https://github.com/TamasKiss/BeatGame/releases/tag/v1.0.0 

## Requirements

- **Windows 10/11 x86-64**
- **.NET 8 SDK** (for building) — [download](https://dotnet.microsoft.com/download/dotnet/8.0)

## Build

```powershell
dotnet restore
dotnet build
```

## Run

```powershell
dotnet run --project src/BeatGame
```

## Test

```powershell
dotnet test
```

## Scoring

- **Hit**: pressing the correct key for an active beat within ±150ms of its scheduled timestamp.
- **Miss**: pressing the wrong key during a beat's window, or letting a beat's window expire without pressing.
- **Multiplier**: starts at 1.00, scales linearly to 5.00 after 100 consecutive hits, and resets to 1.00 on any miss.
- **Score per hit**: `1.0 × current multiplier`, displayed as an integer total.

Stray key presses outside any active beat window are ignored.
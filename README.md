# BeatGame

A Windows-native rhythm game built with C# .NET 8 and [Raylib-cs](https://github.com/raylib-cs/raylib-cs). The player listens to a song and presses 5 keyboard buttons in sync with on-screen scrolling beats. Consecutive hits build a multiplier that scales linearly from 1.00 to 5.00 over 100 perfect hits.

## Requirements

- **Windows 10/11 x86-64**
- **.NET 8 SDK** (for building) — [download](https://dotnet.microsoft.com/download/dotnet/8.0)
- An audio output device (recommended; the game runs without sound if missing)

## Build

```powershell
dotnet restore
dotnet build
```

## Run

```powershell
dotnet run --project src/BeatGame
```

## Publish (standalone Windows executable)

```powershell
dotnet publish src/BeatGame -c Release -r win-x64 -o ./publish
```

The output folder contains `BeatGame.exe` and `raylib.dll`. Copy the entire folder to distribute.

## Test

```powershell
dotnet test
```

Test coverage:
- Unit tests for scoring, multiplier, hit detection, key bindings, settings persistence, beat map validation, and game state transitions.
- An integration test that drives the full audio-timer → hit-detector → game-session pipeline using a fake audio timer (no audio device required).

## Controls (default)

| Lane | Default Key |
|------|-------------|
| 1 (leftmost)  | A |
| 2             | S |
| 3 (center)    | D |
| 4             | F |
| 5 (rightmost) | G |

All five keys are remappable in Settings to any letter A–Z (case-insensitive). Bindings are saved to `%APPDATA%/BeatGame/settings.json` and persist across sessions.

## Scoring

- **Hit**: pressing the correct key for an active beat within ±150ms of its scheduled timestamp.
- **Miss**: pressing the wrong key during a beat's window, or letting a beat's window expire without pressing.
- **Multiplier**: starts at 1.00, scales linearly to 5.00 after 100 consecutive hits, and resets to 1.00 on any miss.
- **Score per hit**: `1.0 × current multiplier`, displayed as an integer total.

Stray key presses outside any active beat window are silently ignored.

## Content

The game ships with a single song titled "First song" located at:

- `src/BeatGame/Content/first-song.mp3` — **the audio file is not bundled in this repo**. Drop a short pop-rock MP3 (60–120 seconds) into this path and ensure it matches the beat timings.
- `src/BeatGame/Content/first-song.json` — the beat map (timestamps in milliseconds, lanes 0–4).

The beat map shipped in this repo runs from 4s to 20s; the game will play silently and still demonstrate the full gameplay loop if the audio file is missing.

## Project structure

```
src/BeatGame/
├── Audio/          — AudioManager, AudioTimer (Raylib audio wrapper)
├── Content/        — Bundled song + beat map
├── Core/           — GameStateManager, Screen base class
├── Input/          — KeyBindings, HitDetector (pure logic, no Raylib)
├── Models/         — Beat, BeatMap, Song, GameSession
├── Rendering/      — UIRenderer, BeatRenderer, AnimationHelper
├── Screens/        — MenuScreen, SettingsScreen, SelectionScreen, PlayScreen
├── Storage/        — SettingsStore (JSON to AppData)
└── Program.cs      — Entry point + dependency wiring

tests/BeatGame.Tests/
├── Input/          — KeyBindings + HitDetector tests
├── Integration/    — End-to-end pipeline tests
├── Models/         — Beat map validation + state machine tests
├── Scoring/        — Score + multiplier tests
└── Storage/        — Settings persistence tests
```

## Specification

The full feature specification, implementation plan, and task breakdown live under `specs/001-core-game-screens/` (created via Spec Kit).

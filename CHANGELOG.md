# Changelog

All notable changes to this project will be documented in this file.

## [1.0.0] - 2026-05-17

### Added
- Menu screen with Play, Settings, and Quit options
- Song selection screen
- Settings screen with remappable key bindings (lanes 1–5, A–Z) and volume slider
- Play screen with scrolling beat visuals, 3-second countdown, and live HUD
- Hit detection: ±150ms window per beat (FR-024)
- Scoring system: 1 point × multiplier per hit
- Multiplier: scales linearly from 1.00 to 5.00 over 100 consecutive hits, resets on miss
- Game over after 15 consecutive misses, with danger bar indicator
- High score persistence to `%APPDATA%/BeatGame/scores.json`
- Key binding persistence to `%APPDATA%/BeatGame/settings.json`
- Press and hit flash animations on lane indicators
- ESC returns to menu during play; ESC from game over returns to menu
- Bundled song: "First song" (MP3 + beat map JSON)
- Native AOT release build — standalone Windows executable, no runtime required
- 65 unit and integration tests

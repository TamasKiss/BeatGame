"""
Beat map generator for BeatGame.
Uses librosa to detect beats and spectral features to assign lanes.
Lane 0 = bass end, Lane 4 = treble end (based on spectral centroid).
"""

import librosa
import numpy as np
import json
import sys
from pathlib import Path


def generate_beatmap(mp3_path: str, output_path: str) -> None:
    print(f"Loading: {mp3_path}")
    y, sr = librosa.load(mp3_path, sr=None, mono=True)
    duration = librosa.get_duration(y=y, sr=sr)
    print(f"Duration: {duration:.2f}s  |  Sample rate: {sr} Hz")

    # ── Beat tracking ──────────────────────────────────────────────────────────
    # Use percussive component for cleaner beat detection
    y_harm, y_perc = librosa.effects.hpss(y)
    tempo, beat_frames = librosa.beat.beat_track(y=y_perc, sr=sr, trim=False)
    tempo_bpm = float(np.squeeze(tempo))   # librosa 0.11 returns ndarray
    beat_times = librosa.frames_to_time(beat_frames, sr=sr)
    print(f"Tempo: {tempo_bpm:.1f} BPM  |  Raw beats detected: {len(beat_times)}")

    # ── Spectral centroid for lane assignment ──────────────────────────────────
    hop_length = 512
    S = np.abs(librosa.stft(y, hop_length=hop_length))
    freqs = librosa.fft_frequencies(sr=sr)
    centroid = librosa.feature.spectral_centroid(S=S, freq=freqs)[0]  # shape: (n_frames,)

    # Map each beat to its spectral centroid value
    beat_centroids = []
    for f in beat_frames:
        idx = min(int(f), len(centroid) - 1)
        beat_centroids.append(centroid[idx])

    bc = np.array(beat_centroids, dtype=float)

    # Use percentile clipping so outliers don't squash the range
    lo, hi = np.percentile(bc, 5), np.percentile(bc, 95)
    if hi > lo:
        normalized = np.clip((bc - lo) / (hi - lo), 0.0, 1.0) * 4.0
    else:
        normalized = np.full_like(bc, 2.0)

    raw_lanes = np.clip(np.round(normalized).astype(int), 0, 4)

    # ── Onset strength for filtering weak/inaudible beats ─────────────────────
    onset_env = librosa.onset.onset_strength(y=y, sr=sr, hop_length=hop_length)
    beat_onset_strength = []
    for f in beat_frames:
        idx = min(int(f), len(onset_env) - 1)
        beat_onset_strength.append(onset_env[idx])

    strength_threshold = np.percentile(beat_onset_strength, 20)  # drop weakest 20%

    # ── Assemble beat list ─────────────────────────────────────────────────────
    MIN_GAP_MS = 200   # never closer than 200 ms (well above the 150ms hit window)

    beats = []
    prev_ms = -9999

    for t, lane, strength in zip(beat_times, raw_lanes, beat_onset_strength):
        if strength < strength_threshold:
            continue                 # skip very weak beats

        ms = int(round(t * 1000))
        if ms - prev_ms < MIN_GAP_MS:
            continue                 # too close to previous beat

        beats.append({"timestampMs": ms, "lane": int(lane)})
        prev_ms = ms

    if not beats:
        print("ERROR: no beats generated — check the audio file.")
        sys.exit(1)

    # ── Lane variety pass ─────────────────────────────────────────────────────
    # If two consecutive beats share the same lane, nudge the second one by ±1
    # to keep gameplay varied (pure spectral analysis can cluster in one band).
    rng = np.random.default_rng(seed=42)
    for i in range(1, len(beats)):
        if beats[i]["lane"] == beats[i - 1]["lane"]:
            delta = int(rng.choice([-1, 1]))
            new_lane = beats[i]["lane"] + delta
            if 0 <= new_lane <= 4:
                beats[i]["lane"] = new_lane

    # ── Output ─────────────────────────────────────────────────────────────────
    result = {
        "title": "First song",
        "audioFile": "first-song.mp3",
        "bpm": int(round(tempo_bpm)),
        "beats": beats,
    }

    Path(output_path).parent.mkdir(parents=True, exist_ok=True)
    with open(output_path, "w", encoding="utf-8") as f:
        json.dump(result, f, indent=2)

    lane_counts = [sum(1 for b in beats if b["lane"] == i) for i in range(5)]
    print(f"Beats written : {len(beats)}")
    print(f"Coverage      : {beats[0]['timestampMs'] / 1000:.2f}s to {beats[-1]['timestampMs'] / 1000:.2f}s  (song length {duration:.2f}s)")
    print(f"Lane spread   : {lane_counts}  (lanes 0–4)")
    print(f"Output        : {output_path}")


if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python generate_beatmap.py <input.mp3> <output.json>")
        sys.exit(1)
    generate_beatmap(sys.argv[1], sys.argv[2])

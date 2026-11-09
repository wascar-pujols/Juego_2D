using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sintetizador chiptune: crea en tiempo de ejecución todos los efectos de sonido y la música.
/// No hace falta ningún archivo de audio externo (todo el audio es original).
/// </summary>
public static class Synth
{
    public const int SR = 22050;
    static System.Random rng = new System.Random(1234);

    public enum Wave { Square, Pulse, Triangle, Sine, Noise, Saw }

    static float Osc(Wave w, float phase)
    {
        phase -= Mathf.Floor(phase);
        switch (w)
        {
            case Wave.Square: return phase < 0.5f ? 0.8f : -0.8f;
            case Wave.Pulse: return phase < 0.25f ? 0.8f : -0.8f;
            case Wave.Triangle: return 4f * Mathf.Abs(phase - 0.5f) - 1f;
            case Wave.Sine: return Mathf.Sin(phase * 2f * Mathf.PI);
            case Wave.Saw: return (phase * 2f - 1f) * 0.7f;
            default: return (float)(rng.NextDouble() * 2.0 - 1.0);
        }
    }

    /// <summary>Añade un tono (con barrido de frecuencia) al buffer.</summary>
    public static void Tone(float[] buf, float start, float dur, float f0, float f1, Wave w, float vol,
                            float attack = 0.004f, float decayPow = 1f, float vibrato = 0f)
    {
        int s0 = (int)(start * SR);
        int n = (int)(dur * SR);
        float phase = 0f;
        float noiseHold = 0f; int noiseCount = 0;
        for (int i = 0; i < n; i++)
        {
            int idx = s0 + i;
            if (idx < 0 || idx >= buf.Length) break;
            float t = i / (float)n;
            float f = Mathf.Lerp(f0, f1, t);
            if (vibrato > 0) f *= 1f + 0.03f * Mathf.Sin(i / (float)SR * vibrato * 6.283f);
            phase += f / SR;
            float env = Mathf.Pow(1f - t, decayPow);
            float a = i / (attack * SR);
            if (a < 1f) env *= a;
            float v;
            if (w == Wave.Noise)
            {
                // ruido "muestreado" a la frecuencia indicada (estilo NES)
                int hold = Mathf.Max(1, (int)(SR / Mathf.Max(f, 50f)));
                if (noiseCount-- <= 0) { noiseHold = Osc(Wave.Noise, 0); noiseCount = hold; }
                v = noiseHold;
            }
            else v = Osc(w, phase);
            buf[idx] += v * vol * env;
        }
    }

    static AudioClip Clip(string name, float[] data)
    {
        Normalize(data, 0.9f);
        var c = AudioClip.Create(name, data.Length, 1, SR, false);
        c.SetData(data, 0);
        return c;
    }

    static void Normalize(float[] d, float peak)
    {
        float m = 0.0001f;
        for (int i = 0; i < d.Length; i++) m = Mathf.Max(m, Mathf.Abs(d[i]));
        if (m > peak) { float k = peak / m; for (int i = 0; i < d.Length; i++) d[i] *= k; }
    }

    static float[] Buf(float seconds) => new float[(int)(seconds * SR)];

    public static float Freq(string note)
    {
        // "A4", "C#5", "Eb3"
        int i = 0;
        int semis;
        switch (char.ToUpper(note[i]))
        {
            case 'C': semis = 0; break; case 'D': semis = 2; break; case 'E': semis = 4; break;
            case 'F': semis = 5; break; case 'G': semis = 7; break; case 'A': semis = 9; break;
            default: semis = 11; break;
        }
        i++;
        if (i < note.Length && note[i] == '#') { semis++; i++; }
        else if (i < note.Length && note[i] == 'b') { semis--; i++; }
        int oct = int.Parse(note.Substring(i));
        int midi = (oct + 1) * 12 + semis;
        return 440f * Mathf.Pow(2f, (midi - 69) / 12f);
    }

    // ------------------------------------------------------------------ EFECTOS
    public static AudioClip Sfx(string name)
    {
        float[] b;
        switch (name)
        {
            case "jump":
                b = Buf(0.18f); Tone(b, 0, 0.18f, 280, 720, Wave.Pulse, 0.5f, 0.002f, 1.5f); break;
            case "coin":
                b = Buf(0.35f); Tone(b, 0, 0.07f, 988, 988, Wave.Square, 0.45f, 0.001f, 0.2f);
                Tone(b, 0.07f, 0.28f, 1319, 1319, Wave.Square, 0.45f, 0.001f, 1.2f); break;
            case "stomp":
                b = Buf(0.2f); Tone(b, 0, 0.15f, 420, 90, Wave.Square, 0.6f, 0.001f, 1f);
                Tone(b, 0, 0.12f, 2000, 800, Wave.Noise, 0.3f, 0.001f, 2f); break;
            case "punch":
                b = Buf(0.12f); Tone(b, 0, 0.1f, 4000, 1500, Wave.Noise, 0.45f, 0.002f, 1.5f);
                Tone(b, 0, 0.08f, 220, 120, Wave.Triangle, 0.4f, 0.001f, 1f); break;
            case "hit":
                b = Buf(0.18f); Tone(b, 0, 0.16f, 3000, 600, Wave.Noise, 0.6f, 0.001f, 1.5f);
                Tone(b, 0, 0.12f, 300, 80, Wave.Square, 0.4f, 0.001f, 1f); break;
            case "hurt":
                b = Buf(0.4f); Tone(b, 0, 0.4f, 700, 150, Wave.Square, 0.5f, 0.002f, 1f, 9f); break;
            case "bump":
                b = Buf(0.12f); Tone(b, 0, 0.1f, 180, 90, Wave.Triangle, 0.9f, 0.001f, 1f); break;
            case "break":
                b = Buf(0.35f); Tone(b, 0, 0.33f, 1200, 200, Wave.Noise, 0.7f, 0.001f, 1.3f);
                Tone(b, 0, 0.1f, 160, 60, Wave.Square, 0.4f); break;
            case "powerup":
                b = Buf(0.6f);
                { string[] n = { "C5", "E5", "G5", "C6", "E6", "G6" }; for (int i = 0; i < n.Length; i++) Tone(b, i * 0.07f, 0.12f, Freq(n[i]), Freq(n[i]), Wave.Square, 0.4f, 0.002f, 0.5f); }
                break;
            case "checkpoint":
                b = Buf(0.5f);
                { string[] n = { "G5", "D6", "G6" }; for (int i = 0; i < n.Length; i++) Tone(b, i * 0.1f, 0.2f, Freq(n[i]), Freq(n[i]), Wave.Pulse, 0.4f, 0.002f, 1f); }
                break;
            case "shuriken":
                b = Buf(0.14f); Tone(b, 0, 0.13f, 2400, 1200, Wave.Sine, 0.35f, 0.002f, 1f);
                Tone(b, 0, 0.1f, 6000, 3000, Wave.Noise, 0.15f); break;
            case "fire":
                b = Buf(0.3f); Tone(b, 0, 0.3f, 900, 200, Wave.Noise, 0.5f, 0.01f, 1f); break;
            case "slam":
                b = Buf(0.6f); Tone(b, 0, 0.55f, 120, 35, Wave.Sine, 0.9f, 0.002f, 1f);
                Tone(b, 0, 0.45f, 600, 100, Wave.Noise, 0.5f, 0.002f, 1.5f); break;
            case "bosshit":
                b = Buf(0.35f); Tone(b, 0, 0.3f, 220, 60, Wave.Saw, 0.7f, 0.001f, 1f);
                Tone(b, 0, 0.2f, 3000, 500, Wave.Noise, 0.5f, 0.001f, 1.5f); break;
            case "dash":
                b = Buf(0.25f); Tone(b, 0, 0.25f, 300, 1800, Wave.Noise, 0.4f, 0.02f, 0.7f); break;
            case "crumble":
                b = Buf(0.3f); Tone(b, 0, 0.3f, 500, 150, Wave.Noise, 0.4f, 0.01f, 1f); break;
            case "pause":
                b = Buf(0.25f); Tone(b, 0, 0.08f, Freq("E6"), Freq("E6"), Wave.Square, 0.35f); Tone(b, 0.1f, 0.12f, Freq("B5"), Freq("B5"), Wave.Square, 0.35f); break;
            case "select":
                b = Buf(0.08f); Tone(b, 0, 0.07f, Freq("A5"), Freq("A5"), Wave.Pulse, 0.35f); break;
            case "death":
                b = Buf(2.2f);
                {
                    string[] n = { "B4", "F5", "-", "F5", "F5", "E5", "D5", "C5", "E4", "-", "E4", "C4" };
                    float[] d = { 0.15f, 0.15f, 0.15f, 0.15f, 0.2f, 0.2f, 0.2f, 0.2f, 0.15f, 0.15f, 0.15f, 0.4f };
                    float t = 0;
                    for (int i = 0; i < n.Length; i++) { if (n[i] != "-") Tone(b, t, d[i], Freq(n[i]), Freq(n[i]), Wave.Square, 0.4f, 0.003f, 0.6f); t += d[i]; }
                    Tone(b, 0, 0.6f, 120, 60, Wave.Triangle, 0.5f);
                }
                break;
            case "clear":
                b = Buf(2.6f);
                {
                    string[] n = { "G4", "C5", "E5", "G5", "C6", "E6", "G6", "E6", "Ab4", "C5", "Eb5", "Ab5", "C6", "Eb6", "Ab6", "Eb6", "Bb4", "D5", "F5", "Bb5", "D6", "F6", "Bb6", "C7" };
                    for (int i = 0; i < n.Length; i++)
                    {
                        float dur = i == n.Length - 1 ? 0.8f : 0.09f;
                        Tone(b, i * 0.075f, dur, Freq(n[i]), Freq(n[i]), Wave.Square, 0.35f, 0.002f, 0.7f);
                    }
                    Tone(b, 0, 0.6f, Freq("C3"), Freq("C3"), Wave.Triangle, 0.6f, 0.01f, 0.5f);
                    Tone(b, 0.6f, 0.6f, Freq("Ab2"), Freq("Ab2"), Wave.Triangle, 0.6f, 0.01f, 0.5f);
                    Tone(b, 1.2f, 1.2f, Freq("Bb2"), Freq("Bb2"), Wave.Triangle, 0.6f, 0.01f, 0.5f);
                }
                break;
            default:
                b = Buf(0.05f); break;
        }
        return Clip(name, b);
    }

    // ------------------------------------------------------------------ MÚSICA
    struct Track { public string notes; public Wave wave; public float vol; public bool pluck; }

    static void RenderTrack(float[] buf, Track tr, float eighth, float offset)
    {
        var tokens = tr.notes.Split(new[] { ' ', '\n', '\r', '|' }, System.StringSplitOptions.RemoveEmptyEntries);
        float t = offset;
        foreach (var tok in tokens)
        {
            var parts = tok.Split(':');
            float len = parts.Length > 1 ? float.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture) : 1f;
            float dur = len * eighth;
            if (parts[0] != "-")
            {
                float f = Freq(parts[0]);
                if (tr.pluck) Tone(buf, t, dur * 0.98f, f, f, tr.wave, tr.vol, 0.003f, 2.2f);
                else Tone(buf, t, dur * 0.92f, f, f, tr.wave, tr.vol, 0.006f, 0.35f);
            }
            t += dur;
        }
    }

    static void RenderDrums(float[] buf, string pattern, float eighth, float totalTime, float vol)
    {
        int steps = (int)(totalTime / eighth + 0.01f);
        for (int i = 0; i < steps; i++)
        {
            char c = pattern[i % pattern.Length];
            float t = i * eighth;
            if (c == 'k' || c == 'x') Tone(buf, t, 0.16f, 150, 45, Wave.Sine, vol * 1.3f, 0.001f, 1f);
            if (c == 's' || c == 'x') Tone(buf, t, 0.12f, 2500, 1200, Wave.Noise, vol * 0.6f, 0.001f, 1.6f);
            if (c != '.') Tone(buf, t, 0.03f, 9000, 9000, Wave.Noise, vol * 0.18f, 0.001f, 2f);
        }
    }

    static AudioClip Song(string name, float bpm, int eighths, Track[] tracks, string drums, float drumVol)
    {
        float eighth = 60f / bpm / 2f;
        float total = eighths * eighth;
        var buf = new float[(int)(total * SR)];
        foreach (var tr in tracks) RenderTrack(buf, tr, eighth, 0f);
        if (!string.IsNullOrEmpty(drums)) RenderDrums(buf, drums, eighth, total, drumVol);
        return Clip(name, buf);
    }

    static string BassLine(string[] roots, string pattern)
    {
        // pattern: "R:2 O:2 R:2 O:2" donde R=raíz, O=octava arriba
        var sb = new System.Text.StringBuilder();
        foreach (var r in roots)
        {
            string note = r.Substring(0, r.Length - 1);
            int oct = int.Parse(r.Substring(r.Length - 1));
            foreach (var p in pattern.Split(' '))
            {
                var parts = p.Split(':');
                string n = parts[0] == "R" ? r : parts[0] == "O" ? note + (oct + 1) : "-";
                sb.Append(n).Append(':').Append(parts[1]).Append(' ');
            }
        }
        return sb.ToString();
    }

    public static AudioClip Music(string name)
    {
        switch (name)
        {
            case "menu":
                return Song("menu", 88, 64, new[]
                {
                    new Track { wave = Wave.Triangle, vol = 0.55f, pluck = true, notes =
                        "D5:2 E5:1 G5:1 A5:4 | G5:2 E5:2 D5:4 | B4:2 D5:2 E5:2 G5:2 | E5:8 |" +
                        "A5:2 G5:1 E5:1 D5:4 | E5:2 D5:2 B4:4 | A4:2 B4:2 D5:2 E5:2 | D5:8" },
                    new Track { wave = Wave.Sine, vol = 0.35f, pluck = false, notes =
                        "D3:8 G2:8 E3:8 A2:8 D3:8 G2:8 A2:8 D3:8" },
                    new Track { wave = Wave.Triangle, vol = 0.2f, pluck = true, notes =
                        "A3:2 D4:2 A3:2 D4:2 B3:2 D4:2 B3:2 D4:2 B3:2 E4:2 B3:2 E4:2 A3:2 C#4:2 A3:2 E4:2 " +
                        "A3:2 D4:2 A3:2 D4:2 B3:2 D4:2 B3:2 D4:2 A3:2 C#4:2 A3:2 E4:2 A3:2 D4:2 F#4:2 D4:2" },
                }, null, 0);
            case "level1":
                return Song("level1", 140, 64, new[]
                {
                    new Track { wave = Wave.Pulse, vol = 0.4f, notes =
                        "A4:2 B4:2 D5:2 E5:2 | D5:1 B4:1 A4:2 G4:2 A4:2 | B4:2 D5:2 E5:2 G5:2 | E5:4 D5:4 |" +
                        "G5:2 E5:2 D5:2 B4:2 | D5:1 E5:1 D5:2 B4:2 A4:2 | G4:2 A4:2 B4:2 D5:2 | A4:6 -:2" },
                    new Track { wave = Wave.Triangle, vol = 0.55f, notes =
                        BassLine(new[] { "D3", "G2", "D3", "A2", "G2", "D3", "G2", "A2" }, "R:2 O:2 R:2 O:2") },
                }, "k.s.k.s.", 0.35f);
            case "level2":
                return Song("level2", 150, 64, new[]
                {
                    new Track { wave = Wave.Square, vol = 0.32f, notes =
                        "E5:2 F5:2 E5:2 C5:2 | B4:2 A4:2 B4:4 | A4:2 B4:2 C5:2 E5:2 | F5:4 E5:4 |" +
                        "A5:2 F5:2 E5:2 C5:2 | B4:2 C5:2 B4:2 A4:2 | F4:2 A4:2 B4:2 C5:2 | B4:6 -:2" },
                    new Track { wave = Wave.Triangle, vol = 0.55f, notes =
                        BassLine(new[] { "A2", "F2", "A2", "E2", "F2", "A2", "D2", "E2" }, "R:1 R:1 O:1 R:1 R:1 O:1 R:1 O:1") },
                }, "k.skk.s.", 0.35f);
            case "level3":
                return Song("level3", 118, 64, new[]
                {
                    new Track { wave = Wave.Pulse, vol = 0.36f, notes =
                        "D5:3 Eb5:1 D5:2 A4:2 | Bb4:2 A4:2 G4:4 | G4:2 A4:2 Bb4:2 D5:2 | Eb5:4 D5:4 |" +
                        "G5:3 Eb5:1 D5:2 Bb4:2 | A4:2 Bb4:2 A4:2 G4:2 | Eb4:2 G4:2 A4:2 Bb4:2 | A4:6 -:2" },
                    new Track { wave = Wave.Triangle, vol = 0.6f, notes =
                        BassLine(new[] { "D2", "G2", "D2", "Eb2", "G2", "D2", "Eb2", "A2" }, "R:2 R:2 O:2 R:2") },
                }, "k...s.k.", 0.3f);
            case "boss":
                return Song("boss", 168, 64, new[]
                {
                    new Track { wave = Wave.Saw, vol = 0.32f, notes =
                        "D5:1 D5:1 F5:1 D5:1 G5:2 F5:2 | Eb5:2 D5:2 C#5:2 A4:2 | D5:1 D5:1 F5:1 D5:1 A5:2 G5:2 | F5:2 Eb5:2 D5:4 |" +
                        "Bb5:2 A5:2 G5:2 F5:2 | G5:2 F5:2 Eb5:2 D5:2 | C#5:2 D5:2 E5:2 F5:2 | E5:4 A4:4" },
                    new Track { wave = Wave.Square, vol = 0.25f, notes =
                        BassLine(new[] { "D2", "Bb1", "D2", "D2", "G2", "Eb2", "A1", "A1" }, "R:1 R:1 O:1 R:1 R:1 O:1 R:1 O:1") },
                    new Track { wave = Wave.Triangle, vol = 0.4f, notes =
                        BassLine(new[] { "D2", "Bb1", "D2", "D2", "G2", "Eb2", "A1", "A1" }, "R:4 R:4") },
                }, "kskxkskx", 0.4f);
            case "victory":
                return Song("victory", 120, 32, new[]
                {
                    new Track { wave = Wave.Square, vol = 0.35f, notes =
                        "D5:1 E5:1 G5:1 A5:1 B5:4 | A5:2 G5:2 E5:2 G5:2 | A5:2 B5:2 D6:4 | B5:2 A5:2 D6:4" },
                    new Track { wave = Wave.Triangle, vol = 0.55f, notes = "D3:8 C3:8 G2:8 D3:8" },
                }, "k.s.k.s.", 0.3f);
        }
        return Sfx("none");
    }
}

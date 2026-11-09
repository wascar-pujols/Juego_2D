using System.Collections.Generic;
using UnityEngine;

/// <summary>Reproduce música de fondo y efectos de sonido (generados por Synth).</summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager I;
    AudioSource music, sfx, jingle;
    readonly Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();
    readonly Dictionary<string, AudioClip> musicClips = new Dictionary<string, AudioClip>();
    string currentMusic;
    public float musicVolume = 0.45f;

    void Awake()
    {
        I = this;
        music = gameObject.AddComponent<AudioSource>();
        music.loop = true; music.playOnAwake = false; music.volume = musicVolume;
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false; sfx.volume = 0.8f;
        jingle = gameObject.AddComponent<AudioSource>();
        jingle.playOnAwake = false; jingle.volume = 0.7f;
        // Pre-generar lo más usado para que no haya tirones durante el juego.
        foreach (var n in new[] { "jump", "coin", "stomp", "punch", "hit", "hurt", "bump", "break", "shuriken" }) Get(n);
    }

    AudioClip Get(string name)
    {
        if (!sfxClips.TryGetValue(name, out var c)) { c = Synth.Sfx(name); sfxClips[name] = c; }
        return c;
    }

    public static void Play(string name, float vol = 1f)
    {
        if (I == null) return;
        I.sfx.PlayOneShot(I.Get(name), vol);
    }

    public void PlayMusic(string name)
    {
        jingle.Stop();
        if (currentMusic == name && music.isPlaying) return;
        if (!musicClips.TryGetValue(name, out var c)) { c = Synth.Music(name); musicClips[name] = c; }
        currentMusic = name;
        music.clip = c;
        music.volume = musicVolume;
        music.Play();
    }

    public void StopMusic()
    {
        music.Stop();
        currentMusic = null;
    }

    /// <summary>Melodía corta (muerte, nivel completado): detiene la música.</summary>
    public void PlayJingle(string name)
    {
        StopMusic();
        jingle.clip = Get(name);
        jingle.Play();
    }

    public void SetPaused(bool p)
    {
        if (p) music.Pause(); else music.UnPause();
    }
}

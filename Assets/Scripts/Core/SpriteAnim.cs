using UnityEngine;

/// <summary>Animador sencillo de sprites por frames.</summary>
public class SpriteAnim : MonoBehaviour
{
    public SpriteRenderer sr;
    Sprite[] frames;
    float fps = 10f;
    bool loop = true;
    float t;
    public int Frame { get; private set; }
    public bool Done { get; private set; }
    public Sprite[] Current => frames;

    public void Play(Sprite[] f, float framesPerSecond, bool looping = true)
    {
        if (f == null || f.Length == 0) return;
        if (f == frames && Mathf.Approximately(framesPerSecond, fps) && looping == loop) return;
        frames = f; fps = framesPerSecond; loop = looping;
        t = 0; Frame = 0; Done = false;
        sr.sprite = f[0];
    }

    public void Restart(Sprite[] f, float framesPerSecond, bool looping = true)
    {
        frames = null;
        Play(f, framesPerSecond, looping);
    }

    /// <summary>Muestra un frame fijo (por ejemplo, para el salto según la velocidad).</summary>
    public void Hold(Sprite[] f, int index)
    {
        if (f == null || f.Length == 0) return;
        frames = f; fps = 0; loop = false; Done = true;
        Frame = Mathf.Clamp(index, 0, f.Length - 1);
        sr.sprite = f[Frame];
    }

    void Update()
    {
        if (frames == null || fps <= 0f || sr == null) return;
        t += Time.deltaTime;
        int n = (int)(t * fps);
        if (loop) n %= frames.Length;
        else if (n >= frames.Length) { n = frames.Length - 1; Done = true; }
        if (n != Frame) { Frame = n; sr.sprite = frames[n]; }
    }
}

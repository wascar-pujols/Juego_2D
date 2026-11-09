using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Carga las imágenes desde Assets/Resources/Sprites y las corta en sprites.
/// Los personajes vienen del "Free Shinobi Sprites Pixel Art" (CraftPix): hojas de 128x128 por frame.
/// Tiles, objetos y fondos son pixel art original del proyecto.
/// </summary>
public static class Art
{
    public const float CharPPU = 48f;   // 128 px por frame -> 2.67 unidades (personaje ~1.6 tiles)
    public const float TilePPU = 32f;   // 32 px -> 1 tile
    public const float BgPPU = 18f;     // fondos de 270 px de alto -> 15 unidades

    static readonly Dictionary<string, Sprite[]> cache = new Dictionary<string, Sprite[]>();
    static readonly Dictionary<string, Texture2D> texCache = new Dictionary<string, Texture2D>();

    // Número de frames de cada animación del pack (ancho original / 128).
    static readonly Dictionary<string, int> charFrames = new Dictionary<string, int>
    {
        {"Fighter/Attack_1",4},{"Fighter/Attack_2",3},{"Fighter/Attack_3",4},{"Fighter/Dead",3},{"Fighter/Hurt",3},
        {"Fighter/Idle",6},{"Fighter/Jump",10},{"Fighter/Run",8},{"Fighter/Shield",2},{"Fighter/Walk",8},
        {"Samurai/Attack_1",6},{"Samurai/Attack_2",4},{"Samurai/Attack_3",3},{"Samurai/Dead",3},{"Samurai/Hurt",2},
        {"Samurai/Idle",6},{"Samurai/Jump",12},{"Samurai/Run",8},{"Samurai/Shield",2},{"Samurai/Walk",8},
        {"Shinobi/Attack_1",5},{"Shinobi/Attack_2",3},{"Shinobi/Attack_3",4},{"Shinobi/Dead",4},{"Shinobi/Hurt",2},
        {"Shinobi/Idle",6},{"Shinobi/Jump",12},{"Shinobi/Run",8},{"Shinobi/Shield",4},{"Shinobi/Walk",8},
    };

    // Centro horizontal aproximado del cuerpo dentro del frame (para el pivote).
    static readonly Dictionary<string, float> charPivotX = new Dictionary<string, float>
    {
        {"Fighter", 0.48f}, {"Samurai", 0.5f}, {"Shinobi", 0.53f}
    };

    public static Texture2D Tex(string path)
    {
        if (texCache.TryGetValue(path, out var t) && t != null) return t;
        t = Resources.Load<Texture2D>("Sprites/" + path);
        if (t == null)
        {
            Debug.LogError("[Art] No se encontró la imagen Resources/Sprites/" + path);
            t = Texture2D.whiteTexture;
        }
        else
        {
            t.filterMode = FilterMode.Point;
            t.wrapMode = TextureWrapMode.Clamp;
        }
        texCache[path] = t;
        return t;
    }

    /// <summary>Corta una tira horizontal de frames del mismo tamaño.</summary>
    public static Sprite[] Strip(string path, int frames, float ppu, Vector2 pivot)
    {
        string key = path + "|" + frames + "|" + ppu + "|" + pivot.x + "," + pivot.y;
        if (cache.TryGetValue(key, out var s)) return s;
        var tex = Tex(path);
        // El ancho del frame se calcula con el tamaño real de la textura, por si Unity la reescaló.
        float fw = tex.width / (float)frames;
        float fh = tex.height;
        // Ajuste de PPU si la textura fue reescalada al importarla.
        s = new Sprite[frames];
        for (int i = 0; i < frames; i++)
        {
            var rect = new Rect(Mathf.Round(i * fw), 0, Mathf.Round(fw), fh);
            s[i] = Sprite.Create(tex, rect, pivot, ppu, 0, SpriteMeshType.FullRect);
            s[i].name = path + "_" + i;
        }
        cache[key] = s;
        return s;
    }

    public static Sprite One(string path, float ppu, Vector2 pivot) => Strip(path, 1, ppu, pivot)[0];
    public static Sprite One(string path, float ppu) => One(path, ppu, new Vector2(0.5f, 0.5f));

    public static Sprite[] Char(string who, string anim)
    {
        string key = who + "/" + anim;
        int n = charFrames.TryGetValue(key, out var f) ? f : 1;
        float px = charPivotX.TryGetValue(who, out var p) ? p : 0.5f;
        return Strip("Characters/" + key, n, CharPPU, new Vector2(px, 0f));
    }

    public static Sprite Tile(string name) => One("Tiles/" + name, TilePPU);

    static Sprite white;
    public static Sprite White
    {
        get
        {
            if (white == null) white = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f, 0, SpriteMeshType.FullRect);
            return white;
        }
    }

    /// <summary>Crea un objeto con SpriteRenderer.</summary>
    public static SpriteRenderer MakeSprite(string name, Transform parent, Sprite sprite, Vector3 pos, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        return sr;
    }
}

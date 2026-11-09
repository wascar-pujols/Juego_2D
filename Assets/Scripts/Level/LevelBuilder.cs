using UnityEngine;

/// <summary>
/// Construye un nivel completo a partir de su mapa de texto (LevelData):
/// terreno, bloques, objetos, enemigos, obstáculos, fondo con parallax y cámara.
/// </summary>
public class LevelBuilder : MonoBehaviour
{
    public static LevelBuilder Current;

    public int Index;
    public int W, H;
    bool[,] solid;
    string[] map;
    public float ArenaX = -1;
    public bool BossStarted;
    public Boss BossRef;

    static Transform fxRoot;
    /// <summary>Contenedor para objetos temporales (efectos, proyectiles).</summary>
    public static Transform Root
    {
        get
        {
            if (Current != null) return Current.transform;
            if (fxRoot == null) fxRoot = new GameObject("FX").transform;
            return fxRoot;
        }
    }

    public static readonly Color[] SkyColors =
    {
        new Color(0.45f, 0.72f, 0.94f), new Color(0.35f, 0.22f, 0.45f), new Color(0.04f, 0.05f, 0.14f)
    };

    // ------------------------------------------------------------ API
    public static LevelBuilder Build(int n)
    {
        ClearAll();
        var go = new GameObject("Nivel " + n + " - " + LevelData.Names[n - 1]);
        var lb = go.AddComponent<LevelBuilder>();
        Current = lb;
        lb.Generate(n);
        return lb;
    }

    public static void ClearAll()
    {
        if (Current != null) Destroy(Current.gameObject);
        Current = null;
        var menu = Compat.FindFirst<MenuController>();
        if (menu != null) Destroy(menu.gameObject);
        EnemyBase.All.Clear();
    }

    public char At(int x, int y)
    {
        if (y < 0 || y >= H) return ' ';
        string row = map[H - 1 - y];
        return x >= 0 && x < row.Length ? row[x] : ' ';
    }

    /// <summary>¿Hay un tile sólido fijo en esta posición del mundo?</summary>
    public bool IsSolid(float wx, float wy)
    {
        int x = Mathf.FloorToInt(wx), y = Mathf.FloorToInt(wy);
        if (x < 0 || x >= W) return true;
        if (y < 0 || y >= H) return false;
        return solid[x, y];
    }

    public void SetSolid(int x, int y, bool v)
    {
        if (x >= 0 && x < W && y >= 0 && y < H) solid[x, y] = v;
    }

    static bool IsSolidChar(char c) => c == '#' || c == 'S' || c == 'B' || c == '?' || c == 'h' || c == '<' || c == '>' || c == 'F';

    // ------------------------------------------------------------ Construcción
    void Generate(int n)
    {
        Index = n;
        map = LevelData.Maps[n - 1];
        H = map.Length;
        W = 0;
        foreach (var r in map) W = Mathf.Max(W, r.Length);
        solid = new bool[W, H];
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
                solid[x, y] = IsSolidChar(At(x, y));

        BuildBackground(n);
        BuildTerrain(n);

        // Objetos
        var objs = new GameObject("Objetos").transform; objs.SetParent(transform, false);
        var enemies = new GameObject("Enemigos").transform; enemies.SetParent(transform, false);
        var blocks = new GameObject("Bloques").transform; blocks.SetParent(transform, false);

        Vector2 playerStart = new Vector2(3.5f, 2f);
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
                if (At(x, y) == 'P') playerStart = new Vector2(x + 0.5f, y);

        var player = PlayerController.Create(playerStart, transform);
        GameManager.I.RespawnPoint = playerStart;

        for (int y = 0; y < H; y++)
        {
            for (int x = 0; x < W; x++)
            {
                char c = At(x, y);
                Vector2 cell = new Vector2(x + 0.5f, y + 0.5f);
                Vector2 feet = new Vector2(x + 0.5f, y);
                switch (c)
                {
                    case 'B': BumpBlock.Create(x, y, BumpBlock.Kind.Crate, blocks); break;
                    case '?': BumpBlock.Create(x, y, BumpBlock.Kind.Coin, blocks); break;
                    case 'h': BumpBlock.Create(x, y, BumpBlock.Kind.Heart, blocks); break;
                    case 'o': Coin.Create(cell, objs); break;
                    case 'H': HeartPickup.Create(cell, objs, false); break;
                    case '^': Hazard.CreateSpikes(cell, objs); break;
                    case 'R': Ronin.Create(feet, enemies); break;
                    case 'N': ShinobiEnemy.Create(feet, enemies); break;
                    case 'Y': Yokai.Create(cell, enemies); break;
                    case '<': Turret.Create(cell, -1, objs); break;
                    case '>': Turret.Create(cell, 1, objs); break;
                    case 'F': FireBar.Create(cell, objs, (x % 2) == 0); break;
                    case '=': CrumblePlatform.Create(cell, objs); break;
                    case 'M': MovingPlatform.Create(new Vector2(x + 1.5f, y + 0.75f), false, objs); break;
                    case 'V': MovingPlatform.Create(new Vector2(x + 1.5f, y + 0.75f), true, objs); break;
                    case 'K': Checkpoint.Create(feet, objs); break;
                    case 'G': Goal.Create(feet, objs); break;
                    case 'A': ArenaX = x; break;
                    case 'X': BossRef = Boss.Create(feet, enemies); break;
                    case 'W': OneWayPlatform.Create(cell, objs); break;
                    case 'L':
                        Art.MakeSprite("Farol", objs, Art.One("Props/lantern", Art.TilePPU, new Vector2(0.5f, 0f)), feet, -5);
                        break;
                }
            }
        }

        // Paredes invisibles en los bordes del nivel
        AddWall(-0.5f); AddWall(W + 0.5f);

        SetupCamera(n, player.transform);
    }

    void AddWall(float x)
    {
        var go = new GameObject("Borde");
        go.transform.SetParent(transform, false);
        go.transform.position = new Vector3(x, H, 0);
        var bc = go.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(1, H * 4);
    }

    void BuildTerrain(int n)
    {
        var terrain = new GameObject("Terreno");
        terrain.transform.SetParent(transform, false);
        var rb = terrain.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        var comp = terrain.AddComponent<CompositeCollider2D>();
        comp.geometryType = CompositeCollider2D.GeometryType.Polygons;

        Sprite top = Art.Tile("l" + n + "_top");
        Sprite fill = Art.Tile("l" + n + "_fill");
        Sprite hard = Art.Tile("hard");

        for (int y = 0; y < H; y++)
        {
            int x = 0;
            while (x < W)
            {
                int kind = TileKind(x, y);
                if (kind == 0) { x++; continue; }
                int x0 = x;
                while (x < W && TileKind(x, y) == kind) x++;
                int len = x - x0;
                Sprite s = kind == 1 ? top : kind == 2 ? fill : hard;
                var sr = Art.MakeSprite("Tiles", terrain.transform, s, new Vector3(x0 + len * 0.5f, y + 0.5f, 0), 0);
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(len, 1);
                var bc = sr.gameObject.AddComponent<BoxCollider2D>();
                bc.size = new Vector2(len, 1);
                Compat.MergeIntoComposite(bc);
            }
        }
        comp.GenerateGeometry();
    }

    // 0 = vacío, 1 = suelo con superficie, 2 = relleno, 3 = bloque duro
    int TileKind(int x, int y)
    {
        char c = At(x, y);
        if (c == '#') return At(x, y + 1) == '#' ? 2 : 1;
        if (c == 'S' || c == 'F') return 3;
        return 0;
    }

    void BuildBackground(int n)
    {
        var bg = new GameObject("Fondo").transform;
        bg.SetParent(transform, false);
        string p = "Backgrounds/l" + n + "_";
        AddLayer(bg, p + "sky", 1f, -100);
        AddLayer(bg, p + "far", 0.85f, -90);
        AddLayer(bg, p + "near", 0.6f, -80);
    }

    void AddLayer(Transform parent, string path, float factor, int order)
    {
        var sprite = Art.One(path, Art.BgPPU);
        var sr = Art.MakeSprite(path, parent, sprite, new Vector3(0, H * 0.5f, 0), order);
        sr.drawMode = SpriteDrawMode.Tiled;
        float tileW = sprite.bounds.size.x;
        float needed = 2f * (W * (1f - factor) + 60f);
        sr.size = new Vector2(Mathf.Ceil(needed / tileW) * tileW, sprite.bounds.size.y);
        var px = sr.gameObject.AddComponent<Parallax>();
        px.factor = factor; px.y = H * 0.5f;
    }

    void SetupCamera(int n, Transform player)
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = H * 0.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = SkyColors[n - 1];
        var cf = cam.GetComponent<CameraFollow>();
        if (cf == null) cf = cam.gameObject.AddComponent<CameraFollow>();
        cf.target = player;
        cf.minX = 0; cf.maxX = W; cf.fixedY = H * 0.5f;
        cf.lockMinX = float.NegativeInfinity;
        cam.transform.position = new Vector3(player.position.x, H * 0.5f, -10f);
        cf.Snap();
    }

    // ------------------------------------------------------------ Arena del jefe
    void Update()
    {
        if (ArenaX < 0 || BossStarted || PlayerController.I == null) return;
        if (PlayerController.I.transform.position.x > ArenaX + 0.5f) StartBossFight();
    }

    void StartBossFight()
    {
        BossStarted = true;
        int gx = Mathf.RoundToInt(ArenaX) - 4;
        // Puerta que encierra al jugador en la arena
        var sr = Art.MakeSprite("Puerta", transform, Art.Tile("hard"), new Vector3(gx + 0.5f, (2 + H) * 0.5f, 0), 1);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(1, H - 2);
        var bc = sr.gameObject.AddComponent<BoxCollider2D>();
        bc.size = sr.size;
        for (int y = 2; y < H; y++) SetSolid(gx, y, true);
        Fx.Debris(new Vector2(gx + 0.5f, 3f), Art.One("Props/debris", Art.TilePPU), 6);
        SoundManager.Play("slam");
        if (CameraFollow.I != null) { CameraFollow.I.lockMinX = gx; CameraFollow.I.Shake(0.4f); }
        if (BossRef != null) { BossRef.Activate(); GameManager.I.ActiveBoss = BossRef; }
        GameManager.I.StartBossMusic();
        GameManager.I.RespawnPoint = new Vector2(ArenaX + 0.5f, 2f);
    }
}

/// <summary>
/// Plataforma de madera de un solo sentido: el jugador la atraviesa saltando desde abajo
/// y puede pararse encima. El jefe la ignora y pasa por debajo.
/// </summary>
public class OneWayPlatform : MonoBehaviour
{
    public static readonly System.Collections.Generic.List<Collider2D> All = new System.Collections.Generic.List<Collider2D>();

    public static void Create(Vector2 cell, Transform parent)
    {
        var sr = Art.MakeSprite("Plataforma de madera", parent, Art.Tile("plank"), cell, 2);
        var bc = sr.gameObject.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(1f, 0.3f);
        bc.offset = new Vector2(0, 0.35f);
        bc.usedByEffector = true;
        var eff = sr.gameObject.AddComponent<PlatformEffector2D>();
        eff.useOneWay = true;
        eff.surfaceArc = 160f;
        sr.gameObject.AddComponent<OneWayPlatform>();
        All.Add(bc);
    }

    void OnDestroy() { All.Remove(GetComponent<Collider2D>()); }
}

using UnityEngine;

/// <summary>Menú de inicio: Jugar, Controles, Historia, Salir.</summary>
public class MenuController : MonoBehaviour
{
    enum Panel { Main, Controls, Story }
    Panel panel = Panel.Main;
    int sel;
    readonly string[] items = { "JUGAR", "CONTROLES", "HISTORIA", "SALIR" };
    Camera cam;
    float t;

    public static void Build()
    {
        LevelBuilder.ClearAll();
        GameManager.Ensure();
        GameManager.I.State = GState.Menu;
        GameManager.I.Banner = null;
        var go = new GameObject("Menu");
        go.AddComponent<MenuController>();
    }

    void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            var c = new GameObject("Main Camera");
            c.tag = "MainCamera";
            cam = c.AddComponent<Camera>();
            c.AddComponent<AudioListener>();
        }
        var cf = cam.GetComponent<CameraFollow>();
        if (cf != null) Destroy(cf);
        cam.orthographic = true;
        cam.orthographicSize = 7.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = LevelBuilder.SkyColors[0];
        cam.transform.position = new Vector3(0, 7.5f, -10);

        // Fondo con parallax
        string[] layers = { "sky", "far", "near" };
        float[] factors = { 1f, 0.85f, 0.6f };
        for (int i = 0; i < 3; i++)
        {
            var s = Art.One("Backgrounds/l1_" + layers[i], Art.BgPPU);
            var sr = Art.MakeSprite("Fondo " + layers[i], transform, s, new Vector3(0, 7.5f, 0), -100 + i * 10);
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(s.bounds.size.x * 300, s.bounds.size.y);
            var px = sr.gameObject.AddComponent<Parallax>();
            px.factor = factors[i]; px.y = 7.5f;
        }

        // Suelo
        var g = Art.MakeSprite("Suelo", transform, Art.Tile("l1_top"), new Vector3(0, 1.5f, 0), 0);
        g.drawMode = SpriteDrawMode.Tiled; g.size = new Vector2(8000, 1);
        var g2 = Art.MakeSprite("Suelo", transform, Art.Tile("l1_fill"), new Vector3(0, 0.5f, 0), 0);
        g2.drawMode = SpriteDrawMode.Tiled; g2.size = new Vector2(8000, 1);

        // Personajes
        // Kenji huye corriendo, perseguido por un ronin y un shinobi
        Pose("Kenji", "Fighter", "Run", new Vector3(4f, 2f, 0), false, Color.white, 1f);
        Pose("Ronin", "Samurai", "Run", new Vector3(-5f, 2f, 0), false, new Color(1f, 0.82f, 0.78f), 1f);
        Pose("Shinobi", "Shinobi", "Run", new Vector3(-8.5f, 2f, 0), false, Color.white, 1f);

        SoundManager.I.PlayMusic("menu");
    }

    void Pose(string name, string who, string animName, Vector3 pos, bool flip, Color tint, float scale)
    {
        var sr = Art.MakeSprite(name, transform, Art.Char(who, animName)[0], pos, 20);
        sr.flipX = flip; sr.color = tint; sr.transform.localScale = Vector3.one * scale;
        var an = sr.gameObject.AddComponent<SpriteAnim>(); an.sr = sr; an.Play(Art.Char(who, animName), 12);
        sr.gameObject.AddComponent<FollowCamX>().offset = pos.x;
    }

    void Update()
    {
        t += Time.deltaTime;
        if (cam != null) cam.transform.position = new Vector3(t * 5f, 7.5f, -10);

        if (panel != Panel.Main)
        {
            if (GameInput.SubmitDown || GameInput.PauseDown) { panel = Panel.Main; SoundManager.Play("select"); }
            return;
        }
        if (GameInput.UpDown) { sel = (sel + items.Length - 1) % items.Length; SoundManager.Play("select"); }
        if (GameInput.DownDown) { sel = (sel + 1) % items.Length; SoundManager.Play("select"); }
        if (GameInput.SubmitDown) Choose(sel);
    }

    void Choose(int i)
    {
        SoundManager.Play("coin");
        switch (i)
        {
            case 0: GameManager.I.NewGame(); break;
            case 1: panel = Panel.Controls; break;
            case 2: panel = Panel.Story; break;
            case 3:
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
    }

    void OnGUI()
    {
        UIStyle.Begin();
        float W = UIStyle.VW, H = UIStyle.VH;

        UIStyle.Text(new Rect(0, 40, W, 110), "KAGE NO MICHI", 96, new Color(1f, 0.92f, 0.75f), TextAnchor.MiddleCenter, 6);
        UIStyle.Text(new Rect(0, 140, W, 50), "El Camino de la Sombra", 34, new Color(1f, 0.45f, 0.35f), TextAnchor.MiddleCenter, 4);

        if (panel == Panel.Main)
        {
            float y0 = 240;
            for (int i = 0; i < items.Length; i++)
            {
                var r = new Rect(W * 0.5f - 170, y0 + i * 62, 340, 54);
                bool hover = r.Contains(Event.current.mousePosition);
                if (hover && Event.current.type == EventType.MouseMove) sel = i;
                bool s = i == sel;
                UIStyle.Rect(r, s ? new Color(0.75f, 0.12f, 0.1f, 0.9f) : new Color(0f, 0f, 0f, 0.55f));
                UIStyle.Rect(new Rect(r.x, r.y + r.height - 4, r.width, 4), s ? new Color(1f, 0.8f, 0.4f) : new Color(1, 1, 1, 0.2f));
                UIStyle.Text(r, (s ? "»  " : "") + items[i] + (s ? "  «" : ""), 30, Color.white, TextAnchor.MiddleCenter);
                if (Event.current.type == EventType.MouseDown && hover) { sel = i; Choose(i); Event.current.Use(); }
            }
            UIStyle.Text(new Rect(0, H - 120, W, 30), "Récord: " + GameManager.I.HighScore.ToString("000000"), 22, new Color(1f, 0.95f, 0.6f), TextAnchor.MiddleCenter);
            UIStyle.Text(new Rect(0, H - 50, W, 30), "Flechas + ENTER o clic para elegir   •   Wascar Pujols (1-22-0075) - Programación de Videojuegos", 16, new Color(1, 1, 1, 0.85f), TextAnchor.MiddleCenter, 2);
        }
        else
        {
            var r = new Rect(W * 0.5f - 420, 220, 840, 400);
            UIStyle.Rect(r, new Color(0, 0, 0, 0.75f));
            string title = panel == Panel.Controls ? "CONTROLES" : "HISTORIA";
            string body = panel == Panel.Controls
                ? "A / D  o  ← →        Moverse\n" +
                  "Shift                     Correr\n" +
                  "Espacio / W / ↑ / Z   Saltar (mantén para saltar más alto)\n" +
                  "J / X / K                 Golpear (rompe cajas y desvía shuriken)\n" +
                  "Esc / P                   Pausa  (M en pausa: volver al menú)\n\n" +
                  "Salta sobre los enemigos para derrotarlos. Golpea los bloques ? desde abajo.\n" +
                  "Tienes 3 vidas (corazones). Si mueres o se acaba el tiempo, vuelves al Nivel 1."
                : "El Shogun Oscuro ha robado el Pergamino Sagrado de la aldea del bambú y\n" +
                  "ha llenado los caminos de ronin, asesinos shinobi y espíritus yokai.\n\n" +
                  "Kenji, un joven luchador de cabello de fuego, parte para recuperarlo:\n" +
                  "cruzará la Aldea del Bambú, el Paso de la Montaña Kaze y finalmente\n" +
                  "el Castillo del Shogun, donde lo espera el combate final.";
            UIStyle.Text(new Rect(r.x, r.y + 16, r.width, 50), title, 36, new Color(1f, 0.8f, 0.4f), TextAnchor.UpperCenter);
            UIStyle.Text(new Rect(r.x + 40, r.y + 80, r.width - 80, r.height - 120), body, 21, Color.white, TextAnchor.UpperLeft, 2);
            UIStyle.Text(new Rect(r.x, r.yMax - 44, r.width, 30), "ENTER / Esc / clic: volver", 18, new Color(1, 1, 1, 0.7f), TextAnchor.MiddleCenter, 2);
            if (Event.current.type == EventType.MouseDown) { panel = Panel.Main; Event.current.Use(); }
        }
    }
}

/// <summary>Mantiene un objeto en la misma posición de pantalla (para el menú con cámara en movimiento).</summary>
public class FollowCamX : MonoBehaviour
{
    public float offset;
    void LateUpdate()
    {
        var c = Camera.main;
        if (c == null) return;
        var p = transform.position;
        p.x = c.transform.position.x + offset;
        transform.position = p;
    }
}

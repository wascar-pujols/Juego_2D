using UnityEngine;

/// <summary>Interfaz durante el juego: vidas, monedas, puntos, nivel, tiempo, barra del jefe y mensajes.</summary>
public class HUD : MonoBehaviour
{
    Sprite heart, coin;

    void OnGUI()
    {
        var gm = GameManager.I;
        if (gm == null || gm.State == GState.Menu) return;
        if (heart == null) heart = Art.One("Props/heart", Art.TilePPU);
        if (coin == null) coin = Art.Strip("Props/coin", 4, Art.TilePPU, new Vector2(0.5f, 0.5f))[0];

        UIStyle.Begin();
        float W = UIStyle.VW, H = UIStyle.VH;

        // Barra superior
        UIStyle.Rect(new Rect(0, 0, W, 64), new Color(0, 0, 0, 0.45f));
        UIStyle.Text(new Rect(20, 4, 200, 24), "VIDAS", 16, new Color(1f, 0.85f, 0.85f), TextAnchor.UpperLeft, 2);
        for (int i = 0; i < gm.Lives; i++)
            UIStyle.Sprite(new Rect(20 + i * 30, 28, 28, 28), heart);

        float cx = Mathf.Max(320, W * 0.28f);
        UIStyle.Text(new Rect(cx, 4, 200, 24), "MONEDAS", 16, new Color(1f, 0.95f, 0.6f), TextAnchor.UpperLeft, 2);
        UIStyle.Sprite(new Rect(cx, 28, 28, 28), coin);
        UIStyle.Text(new Rect(cx + 34, 24, 120, 36), "x " + gm.Coins.ToString("00"), 26, Color.white);

        float sx = W * 0.5f - 90;
        UIStyle.Text(new Rect(sx, 4, 180, 24), "PUNTOS", 16, new Color(0.8f, 0.9f, 1f), TextAnchor.UpperCenter, 2);
        UIStyle.Text(new Rect(sx, 24, 180, 36), gm.Score.ToString("000000"), 28, Color.white, TextAnchor.UpperCenter);

        float lx = W * 0.68f;
        UIStyle.Text(new Rect(lx, 4, 260, 24), "NIVEL " + gm.Level, 16, new Color(0.8f, 1f, 0.8f), TextAnchor.UpperLeft, 2);
        UIStyle.Text(new Rect(lx, 26, 300, 36), LevelData.Names[Mathf.Clamp(gm.Level - 1, 0, 2)], 20, Color.white);

        int t = Mathf.CeilToInt(gm.TimeLeft);
        Color tc = t <= 30 && ((int)(Time.unscaledTime * 3)) % 2 == 0 ? new Color(1f, 0.3f, 0.3f) : Color.white;
        UIStyle.Text(new Rect(W - 150, 4, 130, 24), "TIEMPO", 16, new Color(1f, 0.9f, 0.7f), TextAnchor.UpperRight, 2);
        UIStyle.Text(new Rect(W - 150, 24, 130, 36), t.ToString(), 28, tc, TextAnchor.UpperRight);

        // Barra de vida del jefe
        var boss = gm.ActiveBoss;
        if (boss != null && !boss.dead && gm.State == GState.Playing)
        {
            float bw = Mathf.Min(560, W - 80);
            float bx = (W - bw) * 0.5f, by = H - 64;
            UIStyle.Text(new Rect(bx, by - 30, bw, 28), "SHOGUN OSCURO", 20, new Color(0.9f, 0.7f, 1f), TextAnchor.UpperCenter);
            UIStyle.Rect(new Rect(bx - 4, by - 4, bw + 8, 30), new Color(0, 0, 0, 0.7f));
            UIStyle.Rect(new Rect(bx, by, bw, 22), new Color(0.25f, 0.05f, 0.08f));
            UIStyle.Rect(new Rect(bx, by, bw * boss.HealthFraction, 22), new Color(0.85f, 0.15f, 0.25f));
            UIStyle.Rect(new Rect(bx, by, bw * boss.HealthFraction, 6), new Color(1f, 0.5f, 0.55f));
        }

        // Ayuda de controles al empezar el nivel 1
        if (gm.State == GState.Playing && gm.LevelIntroTimer > 0 && gm.Level == 1)
        {
            float a = Mathf.Clamp01(gm.LevelIntroTimer);
            UIStyle.Text(new Rect(20, H - 70, W - 40, 60),
                "A/D o Flechas: mover    Shift: correr    Espacio/W: saltar    J/X: golpear    Esc: pausa",
                18, new Color(1, 1, 1, a), TextAnchor.LowerCenter, 2);
        }

        // Mensajes grandes
        if (gm.State == GState.Dying || gm.State == GState.Victory)
            UIStyle.Rect(new Rect(0, 0, W, H), new Color(0, 0, 0, gm.State == GState.Dying ? 0.55f : 0.45f));

        if (gm.State == GState.Paused)
        {
            UIStyle.Rect(new Rect(0, 0, W, H), new Color(0, 0, 0, 0.6f));
            UIStyle.Text(new Rect(0, H * 0.35f, W, 90), "PAUSA", 72, Color.white, TextAnchor.MiddleCenter, 5);
            UIStyle.Text(new Rect(0, H * 0.35f + 100, W, 60), "Esc / P: continuar      M: volver al menú", 24, new Color(0.9f, 0.9f, 0.9f), TextAnchor.MiddleCenter);
        }

        if (!string.IsNullOrEmpty(gm.Banner))
        {
            Color main = gm.State == GState.Dying ? new Color(1f, 0.25f, 0.25f)
                       : gm.State == GState.Victory ? new Color(1f, 0.85f, 0.3f)
                       : gm.State == GState.LevelClear ? new Color(0.5f, 1f, 0.6f)
                       : Color.white;
            float y = H * 0.3f;
            UIStyle.Text(new Rect(0, y, W, 100), gm.Banner, 64, main, TextAnchor.MiddleCenter, 5);
            if (!string.IsNullOrEmpty(gm.SubBanner))
                UIStyle.Text(new Rect(W * 0.1f, y + 100, W * 0.8f, 200), gm.SubBanner, 26, Color.white, TextAnchor.UpperCenter, 3);
        }
    }
}

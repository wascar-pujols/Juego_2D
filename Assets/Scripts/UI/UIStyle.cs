using UnityEngine;

/// <summary>Utilidades de dibujo para la interfaz (IMGUI), escalada a una resolución virtual de 1280x720.</summary>
public static class UIStyle
{
    public const float RefHeight = 720f;
    static GUIStyle label;
    static Texture2D white;

    public static float Scale => Screen.height / RefHeight;
    public static float VW => Screen.width / Scale;   // ancho virtual
    public static float VH => RefHeight;

    public static void Begin()
    {
        float s = Scale;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(s, s, 1f));
    }

    public static Texture2D White
    {
        get
        {
            if (white == null)
            {
                white = new Texture2D(1, 1);
                white.SetPixel(0, 0, Color.white);
                white.Apply();
            }
            return white;
        }
    }

    public static GUIStyle Label
    {
        get
        {
            if (label == null)
            {
                label = new GUIStyle(GUI.skin.label);
                var f = Compat.DefaultFont();
                if (f != null) label.font = f;
                label.fontStyle = FontStyle.Bold;
                label.wordWrap = true;
                label.richText = false;
            }
            return label;
        }
    }

    public static void Rect(Rect r, Color c)
    {
        var old = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, White);
        GUI.color = old;
    }

    public static void Text(Rect r, string text, int size, Color color, TextAnchor anchor = TextAnchor.UpperLeft, int shadow = 3)
    {
        var st = Label;
        st.fontSize = size;
        st.alignment = anchor;
        var old = GUI.color;
        if (shadow > 0)
        {
            GUI.color = new Color(0, 0, 0, color.a * 0.85f);
            GUI.Label(new Rect(r.x + shadow, r.y + shadow, r.width, r.height), text, st);
        }
        GUI.color = color;
        GUI.Label(r, text, st);
        GUI.color = old;
    }

    public static void Sprite(Rect r, Sprite s, Color? tint = null)
    {
        if (s == null) return;
        var tex = s.texture;
        var tr = s.textureRect;
        var uv = new Rect(tr.x / tex.width, tr.y / tex.height, tr.width / tex.width, tr.height / tex.height);
        var old = GUI.color;
        GUI.color = tint ?? Color.white;
        GUI.DrawTextureWithTexCoords(r, tex, uv);
        GUI.color = old;
    }
}

using UnityEngine;

/// <summary>Efectos visuales simples: escombros, destellos y textos flotantes de puntos.</summary>
public static class Fx
{
    public static void Debris(Vector2 pos, Sprite sprite, int count = 4, float size = 1f, Color? tint = null)
    {
        for (int i = 0; i < count; i++)
        {
            var sr = Art.MakeSprite("Escombro", LevelBuilder.Root, sprite, pos + Random.insideUnitCircle * 0.2f, 40);
            sr.transform.localScale = Vector3.one * size;
            if (tint.HasValue) sr.color = tint.Value;
            var p = sr.gameObject.AddComponent<FxParticle>();
            p.vel = new Vector2((i % 2 == 0 ? -1 : 1) * Random.Range(1.5f, 4f), Random.Range(6f, 11f));
            p.gravity = 30f; p.life = 1.2f; p.spin = Random.Range(-720f, 720f);
        }
    }

    public static void Sparks(Vector2 pos, Color color, int count = 8, float speed = 5f)
    {
        for (int i = 0; i < count; i++)
        {
            var sr = Art.MakeSprite("Chispa", LevelBuilder.Root, Art.White, pos, 45);
            sr.color = color;
            sr.transform.localScale = Vector3.one * Random.Range(0.08f, 0.16f) * 4f;
            var p = sr.gameObject.AddComponent<FxParticle>();
            float a = i / (float)count * Mathf.PI * 2f + Random.Range(-0.2f, 0.2f);
            p.vel = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * speed * Random.Range(0.6f, 1.2f);
            p.gravity = 4f; p.life = 0.4f; p.fade = true; p.shrink = true;
        }
    }

    public static void Puff(Vector2 pos, Color color) => Sparks(pos, color, 10, 3f);

    public static void Popup(Vector2 pos, string text, Color color)
    {
        var go = new GameObject("Puntos");
        go.transform.SetParent(LevelBuilder.Root, false);
        go.transform.position = pos;
        var tm = go.AddComponent<TextMesh>();
        var f = Compat.DefaultFont();
        if (f != null) { tm.font = f; go.GetComponent<MeshRenderer>().sharedMaterial = f.material; }
        tm.text = text;
        tm.fontSize = 48; tm.characterSize = 0.06f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = color;
        tm.fontStyle = FontStyle.Bold;
        go.GetComponent<MeshRenderer>().sortingOrder = 60;
        var p = go.AddComponent<FxParticle>();
        p.vel = new Vector2(0, 2.2f); p.gravity = 0f; p.life = 0.9f; p.fade = true; p.text = tm;
    }
}

public class FxParticle : MonoBehaviour
{
    public Vector2 vel;
    public float gravity, life = 1f, spin;
    public bool fade, shrink;
    public TextMesh text;
    float t;
    SpriteRenderer sr;
    Vector3 baseScale;

    void Start() { sr = GetComponent<SpriteRenderer>(); baseScale = transform.localScale; }

    void Update()
    {
        t += Time.deltaTime;
        vel.y -= gravity * Time.deltaTime;
        transform.position += (Vector3)(vel * Time.deltaTime);
        if (spin != 0) transform.Rotate(0, 0, spin * Time.deltaTime);
        float k = 1f - t / life;
        if (fade)
        {
            if (sr != null) { var c = sr.color; c.a = k; sr.color = c; }
            if (text != null) { var c = text.color; c.a = k; text.color = c; }
        }
        if (shrink) transform.localScale = baseScale * Mathf.Max(0.05f, k);
        if (t >= life) Destroy(gameObject);
    }
}

using UnityEngine;

/// <summary>Moneda "mon" de oro. 100 monedas = 1 vida extra.</summary>
public class Coin : MonoBehaviour
{
    bool taken;

    public static Coin Create(Vector2 pos, Transform parent)
    {
        var sr = Art.MakeSprite("Moneda", parent, null, pos, 5);
        var an = sr.gameObject.AddComponent<SpriteAnim>(); an.sr = sr;
        an.Play(Art.Strip("Props/coin", 4, Art.TilePPU, new Vector2(0.5f, 0.5f)), 8);
        var c = sr.gameObject.AddComponent<CircleCollider2D>();
        c.isTrigger = true; c.radius = 0.3f;
        return sr.gameObject.AddComponent<Coin>();
    }

    public void Collect()
    {
        if (taken) return;
        taken = true;
        GameManager.I.AddCoin();
        Fx.Sparks(transform.position, new Color(1f, 0.9f, 0.4f), 6, 3f);
        Destroy(gameObject);
    }

    /// <summary>Moneda que salta de un bloque "?" y se recoge sola.</summary>
    public static void Pop(Vector2 pos)
    {
        var sr = Art.MakeSprite("Moneda", LevelBuilder.Root, null, pos, 6);
        var an = sr.gameObject.AddComponent<SpriteAnim>(); an.sr = sr;
        an.Play(Art.Strip("Props/coin", 4, Art.TilePPU, new Vector2(0.5f, 0.5f)), 16);
        var p = sr.gameObject.AddComponent<FxParticle>();
        p.vel = new Vector2(0, 11f); p.gravity = 35f; p.life = 0.55f;
        GameManager.I.AddCoin();
        Fx.Popup(pos + Vector2.up * 2f, "100", Color.yellow);
    }
}

/// <summary>Corazón: da una vida extra.</summary>
public class HeartPickup : MonoBehaviour
{
    bool taken;
    float rise;
    Vector3 target;
    float t;

    public static HeartPickup Create(Vector2 pos, Transform parent, bool riseFromBlock)
    {
        var sr = Art.MakeSprite("Corazon", parent, Art.One("Props/heart", Art.TilePPU), pos, 5);
        sr.transform.localScale = Vector3.one * 1.4f;
        var c = sr.gameObject.AddComponent<CircleCollider2D>();
        c.isTrigger = true; c.radius = 0.25f;
        var h = sr.gameObject.AddComponent<HeartPickup>();
        h.target = (Vector3)pos + (riseFromBlock ? Vector3.up : Vector3.zero);
        h.rise = riseFromBlock ? 1f : 0f;
        return h;
    }

    void Update()
    {
        t += Time.deltaTime;
        if (rise > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * 2f);
            if (transform.position == target) rise = 0;
        }
        else transform.position = target + Vector3.up * Mathf.Sin(t * 3f) * 0.1f;
    }

    public void Collect()
    {
        if (taken || rise > 0) return;
        taken = true;
        GameManager.I.AddLife();
        Fx.Popup(transform.position + Vector3.up, "+1 VIDA", new Color(1f, 0.4f, 0.5f));
        Fx.Sparks(transform.position, new Color(1f, 0.4f, 0.5f), 10, 4f);
        Destroy(gameObject);
    }
}

/// <summary>
/// Bloques que se golpean desde abajo: bloque "?" (moneda o corazón) y caja de madera rompible.
/// </summary>
public class BumpBlock : MonoBehaviour
{
    public enum Kind { Coin, Heart, Crate }
    public Kind kind;
    bool used;
    float bumpT = -1;
    SpriteRenderer sr;
    SpriteAnim anim;
    int gx, gy;

    public static BumpBlock Create(int x, int y, Kind kind, Transform parent)
    {
        var go = new GameObject(kind == Kind.Crate ? "Caja rompible" : "Bloque ?");
        go.transform.SetParent(parent, false);
        go.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0);
        go.AddComponent<BoxCollider2D>().size = Vector2.one;
        var sr = Art.MakeSprite("Sprite", go.transform, kind == Kind.Crate ? Art.Tile("brick") : null, go.transform.position, 1);
        var b = go.AddComponent<BumpBlock>();
        b.kind = kind; b.sr = sr; b.gx = x; b.gy = y;
        if (kind != Kind.Crate)
        {
            b.anim = sr.gameObject.AddComponent<SpriteAnim>(); b.anim.sr = sr;
            b.anim.Play(Art.Strip("Tiles/question", 2, Art.TilePPU, new Vector2(0.5f, 0.5f)), 2.5f);
        }
        return b;
    }

    public void Bump()
    {
        if (used || bumpT >= 0) return;
        // derrota enemigos que estén encima
        foreach (var e in EnemyBase.All.ToArray())
        {
            if (e == null || e.dead) continue;
            Vector2 d = e.transform.position - transform.position;
            if (Mathf.Abs(d.x) < 0.9f && d.y > 0.3f && d.y < 1.2f) e.Damage(99, 0);
        }

        switch (kind)
        {
            case Kind.Crate:
                SoundManager.Play("break");
                Fx.Debris(transform.position, Art.One("Props/debris", Art.TilePPU), 4, 1.5f);
                GameManager.I.AddScore(50);
                LevelBuilder.Current?.SetSolid(gx, gy, false);
                Destroy(gameObject);
                return;
            case Kind.Coin:
                SoundManager.Play("bump");
                Coin.Pop((Vector2)transform.position + Vector2.up);
                break;
            case Kind.Heart:
                SoundManager.Play("bump");
                HeartPickup.Create(transform.position, LevelBuilder.Current != null ? LevelBuilder.Current.transform : null, true);
                SoundManager.Play("powerup", 0.5f);
                break;
        }
        used = true;
        if (anim != null) Destroy(anim);
        sr.sprite = Art.Tile("used");
        bumpT = 0;
    }

    void Update()
    {
        if (bumpT < 0) return;
        bumpT += Time.deltaTime;
        float k = Mathf.Clamp01(bumpT / 0.2f);
        sr.transform.localPosition = new Vector3(0, Mathf.Sin(k * Mathf.PI) * 0.3f, 0);
        if (k >= 1) { sr.transform.localPosition = Vector3.zero; bumpT = -2; }
    }
}

/// <summary>Bandera de control: al tocarla, si caes al vacío reapareces aquí.</summary>
public class Checkpoint : MonoBehaviour
{
    bool active;
    SpriteRenderer sr;
    Sprite[] frames;

    public static void Create(Vector2 feet, Transform parent)
    {
        var frames = Art.Strip("Props/checkpoint", 2, Art.TilePPU, new Vector2(0.5f, 0f));
        var sr = Art.MakeSprite("Checkpoint", parent, frames[0], feet, 3);
        var bc = sr.gameObject.AddComponent<BoxCollider2D>();
        bc.isTrigger = true; bc.size = new Vector2(1f, 2f); bc.offset = new Vector2(0, 1f);
        var cp = sr.gameObject.AddComponent<Checkpoint>();
        cp.sr = sr; cp.frames = frames;
    }

    public void Activate()
    {
        if (active) return;
        active = true;
        sr.sprite = frames[1];
        GameManager.I.RespawnPoint = new Vector2(transform.position.x, transform.position.y + 0.1f);
        SoundManager.Play("checkpoint");
        Fx.Sparks(transform.position + Vector3.up * 1.5f, new Color(1f, 0.3f, 0.3f), 10, 4f);
        Fx.Popup(transform.position + Vector3.up * 2.6f, "CHECKPOINT", Color.white);
    }
}

/// <summary>Meta del nivel: el torii (puerta sagrada).</summary>
public class Goal : MonoBehaviour
{
    public static void Create(Vector2 feet, Transform parent)
    {
        var sr = Art.MakeSprite("Meta (Torii)", parent, Art.One("Props/torii", Art.TilePPU, new Vector2(0.5f, 0f)), feet, -2);
        var bc = sr.gameObject.AddComponent<BoxCollider2D>();
        bc.isTrigger = true; bc.size = new Vector2(1f, 3f); bc.offset = new Vector2(0, 1.5f);
        sr.gameObject.AddComponent<Goal>();
    }
}

using UnityEngine;

/// <summary>
/// OBSTÁCULO - Pinchos de bambú (y cualquier cosa que dañe al tocarla).
/// </summary>
public class Hazard : MonoBehaviour
{
    public static Hazard CreateSpikes(Vector2 cell, Transform parent)
    {
        var sr = Art.MakeSprite("Pinchos de bambu", parent, Art.Tile("spikes"), cell, 3);
        var bc = sr.gameObject.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bc.size = new Vector2(0.8f, 0.55f);
        bc.offset = new Vector2(0, -0.2f);
        return sr.gameObject.AddComponent<Hazard>();
    }
}

/// <summary>Proyectil (shuriken). Lo disparan las torretas y el jefe. Se puede desviar con un puñetazo.</summary>
public class Projectile : MonoBehaviour
{
    public Vector2 velocity;
    public float life = 6f;
    float spin = 900f;
    Transform visual;

    public static Projectile CreateShuriken(Vector2 pos, Vector2 vel, Transform parent)
    {
        var go = new GameObject("Shuriken");
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var c = go.AddComponent<CircleCollider2D>();
        c.isTrigger = true; c.radius = 0.2f;
        var sr = Art.MakeSprite("Sprite", go.transform, Art.Strip("Props/shuriken", 2, Art.TilePPU, new Vector2(0.5f, 0.5f))[0], pos, 15);
        var p = go.AddComponent<Projectile>();
        p.velocity = vel;
        p.visual = sr.transform;
        return p;
    }

    void Update()
    {
        if (GameManager.I != null && GameManager.I.State == GState.Paused) return;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        if (visual != null) visual.Rotate(0, 0, -spin * Time.deltaTime * Mathf.Sign(velocity.x == 0 ? 1 : velocity.x));
        life -= Time.deltaTime;
        var lv = LevelBuilder.Current;
        if (life <= 0 || (lv != null && lv.IsSolid(transform.position.x, transform.position.y)))
        {
            Fx.Sparks(transform.position, new Color(0.8f, 0.85f, 0.9f), 4, 2f);
            Destroy(gameObject);
        }
    }

    public void HitPlayer(PlayerController p)
    {
        p.TakeDamage(transform.position.x - velocity.x);
        Destroy(gameObject);
    }

    public void Deflect()
    {
        SoundManager.Play("bump", 0.6f);
        Fx.Sparks(transform.position, Color.white, 6, 4f);
        GameManager.I?.AddScore(50);
        Destroy(gameObject);
    }
}

/// <summary>Onda de fuego que recorre el suelo (ataque del jefe).</summary>
public class Shockwave : MonoBehaviour
{
    int dir; float life = 1.8f;

    public static void Create(Vector2 feet, int dir, Transform parent)
    {
        var go = new GameObject("Onda de fuego");
        go.transform.SetParent(parent, false);
        go.transform.position = feet + new Vector2(0, 0.45f);
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var bc = go.AddComponent<BoxCollider2D>();
        bc.isTrigger = true; bc.size = new Vector2(0.6f, 0.8f);
        var sr = Art.MakeSprite("Sprite", go.transform, null, go.transform.position, 16);
        sr.transform.localScale = new Vector3(1.6f, 2f, 1f);
        var an = sr.gameObject.AddComponent<SpriteAnim>(); an.sr = sr;
        an.Play(Art.Strip("Props/fire", 2, Art.TilePPU, new Vector2(0.5f, 0.5f)), 10);
        go.AddComponent<Hazard>();
        go.AddComponent<Shockwave>().dir = dir;
        SoundManager.Play("fire", 0.6f);
    }

    void Update()
    {
        transform.position += new Vector3(dir * 7f * Time.deltaTime, 0, 0);
        life -= Time.deltaTime;
        var lv = LevelBuilder.Current;
        if (life <= 0 || (lv != null && lv.IsSolid(transform.position.x + dir * 0.3f, transform.position.y))) Destroy(gameObject);
    }
}

/// <summary>
/// OBSTÁCULO - Torreta: linterna de piedra embrujada que dispara shuriken cada cierto tiempo.
/// Es sólida (se puede pisar) y no se puede destruir.
/// </summary>
public class Turret : MonoBehaviour
{
    int dir;
    float timer;

    public static void Create(Vector2 cell, int dir, Transform parent)
    {
        var sr = Art.MakeSprite("Torreta de shuriken", parent, Art.One("Props/turret", Art.TilePPU), cell, 4);
        sr.flipX = dir > 0;
        sr.gameObject.AddComponent<BoxCollider2D>().size = new Vector2(1f, 1f);
        var t = sr.gameObject.AddComponent<Turret>();
        t.dir = dir;
        t.timer = 1.2f;
    }

    void Update()
    {
        if (GameManager.I == null || GameManager.I.State != GState.Playing) return;
        var cam = Camera.main;
        if (cam == null) return;
        float half = cam.orthographicSize * cam.aspect + 1f;
        if (Mathf.Abs(transform.position.x - cam.transform.position.x) > half) return;
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            timer = 2.4f;
            Projectile.CreateShuriken((Vector2)transform.position + new Vector2(dir * 0.6f, 0.05f), new Vector2(dir * 6f, 0), LevelBuilder.Root);
            SoundManager.Play("shuriken", 0.5f);
        }
    }
}

/// <summary>
/// OBSTÁCULO - Barra de fuego onibi: cadena de llamas azules que gira alrededor de un bloque.
/// </summary>
public class FireBar : MonoBehaviour
{
    float speed;
    Rigidbody2D rb;
    float angle;

    public static void Create(Vector2 center, Transform parent, bool clockwise)
    {
        var go = new GameObject("Barra de fuego");
        go.transform.SetParent(parent, false);
        go.transform.position = center;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var frames = Art.Strip("Props/onibi", 2, Art.TilePPU, new Vector2(0.5f, 0.5f));
        for (int i = 1; i <= 5; i++)
        {
            var sr = Art.MakeSprite("Llama", go.transform, frames[0], center + new Vector2(i * 0.5f, 0), 14);
            var an = sr.gameObject.AddComponent<SpriteAnim>(); an.sr = sr; an.Play(frames, 8);
            var c = sr.gameObject.AddComponent<CircleCollider2D>();
            c.isTrigger = true; c.radius = 0.18f;
            sr.gameObject.AddComponent<Hazard>();
            sr.gameObject.AddComponent<KeepUpright>();
        }
        var fb = go.AddComponent<FireBar>();
        fb.speed = clockwise ? -95f : 95f;
        fb.angle = clockwise ? 90f : 270f;
    }

    void Awake() { rb = GetComponent<Rigidbody2D>(); }

    void FixedUpdate()
    {
        if (GameManager.I != null && GameManager.I.State == GState.Paused) return;
        angle += speed * Time.fixedDeltaTime;
        rb.MoveRotation(angle);
    }
}

/// <summary>Mantiene un sprite derecho aunque su padre gire.</summary>
public class KeepUpright : MonoBehaviour
{
    void LateUpdate() { transform.rotation = Quaternion.identity; }
}

/// <summary>
/// OBSTÁCULO - Puente de tablas que se cae poco después de pisarlo y reaparece luego.
/// </summary>
public class CrumblePlatform : MonoBehaviour
{
    Vector3 home;
    float timer = -1;
    bool falling;
    float vy;
    BoxCollider2D bc;
    SpriteRenderer sr;

    public static void Create(Vector2 cell, Transform parent)
    {
        var sr = Art.MakeSprite("Puente fragil", parent, Art.Tile("plank"), cell, 2);
        var bc = sr.gameObject.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(1f, 0.3f);
        bc.offset = new Vector2(0, 0.35f);
        sr.gameObject.AddComponent<CrumblePlatform>();
    }

    void Awake() { home = transform.position; bc = GetComponent<BoxCollider2D>(); sr = GetComponent<SpriteRenderer>(); }

    public void Step()
    {
        if (timer < 0 && !falling) { timer = 0.45f; SoundManager.Play("crumble", 0.5f); }
    }

    void Update()
    {
        if (timer >= 0 && !falling)
        {
            timer -= Time.deltaTime;
            transform.position = home + new Vector3(Random.Range(-0.04f, 0.04f), Random.Range(-0.03f, 0.03f), 0);
            if (timer < 0) { falling = true; bc.enabled = false; vy = 0; }
        }
        else if (falling)
        {
            vy -= 25f * Time.deltaTime;
            transform.position += new Vector3(0, vy * Time.deltaTime, 0);
            if (transform.position.y < -4f) { falling = false; timer = -1; Invoke(nameof(Reappear), 3f); sr.enabled = false; transform.position = home; }
        }
    }

    void Reappear()
    {
        transform.position = home;
        sr.enabled = true;
        bc.enabled = true;
        Fx.Puff(home + Vector3.up * 0.3f, new Color(0.9f, 0.8f, 0.6f));
    }
}

/// <summary>Plataforma de madera que se mueve en horizontal o vertical.</summary>
public class MovingPlatform : MonoBehaviour
{
    public Vector2 Velocity { get; private set; }
    Vector2 origin;
    bool vertical;
    float t;
    Rigidbody2D rb;
    const float Range = 3f, Speed = 1.1f;

    public static void Create(Vector2 center, bool vertical, Transform parent)
    {
        var sr = Art.MakeSprite("Plataforma movil", parent, Art.One("Tiles/raft", Art.TilePPU), center, 2);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = new Vector2(3f, 0.5f);
        var rb = sr.gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var bc = sr.gameObject.AddComponent<BoxCollider2D>();
        bc.size = new Vector2(3f, 0.45f);
        var mp = sr.gameObject.AddComponent<MovingPlatform>();
        mp.vertical = vertical;
    }

    void Awake() { rb = GetComponent<Rigidbody2D>(); origin = transform.position; }

    void FixedUpdate()
    {
        if (GameManager.I != null && GameManager.I.State == GState.Paused) { Velocity = Vector2.zero; return; }
        t += Time.fixedDeltaTime;
        float o = Mathf.Sin(t * Speed) * Range;
        Vector2 target = origin + (vertical ? new Vector2(0, o) : new Vector2(o, 0));
        Velocity = (target - rb.position) / Time.fixedDeltaTime;
        rb.MovePosition(target);
    }
}

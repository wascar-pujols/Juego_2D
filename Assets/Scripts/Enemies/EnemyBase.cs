using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base de todos los enemigos: vida, daño por contacto, ser pisado (stomp), recibir puñetazos,
/// parpadeo al recibir daño y muerte. Cada enemigo concreto implementa Think().
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    public static readonly List<EnemyBase> All = new List<EnemyBase>();

    protected Rigidbody2D rb;
    protected Collider2D body;
    protected Collider2D hitbox;
    protected SpriteRenderer sr;
    protected SpriteAnim anim;

    public int hp = 1;
    public int maxHp = 1;
    public bool stompable = true;
    public bool punchable = true;
    public int scoreValue = 200;
    public float stompMargin = 0.35f;
    public bool dead;
    protected bool awake;
    protected int facing = -1;
    protected float invulnTimer;
    protected float flashTimer;
    protected Color baseColor = Color.white;
    protected bool grounded;

    protected LevelBuilder Level => LevelBuilder.Current;
    protected PlayerController Player => PlayerController.I;

    // ------------------------------------------------------------ Construcción común
    protected static T MakeCharacter<T>(string name, Vector2 feet, Transform parent, Sprite first, Vector2 bodySize, Vector2 hitSize, float spriteScale = 1f)
        where T : EnemyBase
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = feet;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = 3f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        var bc = go.AddComponent<BoxCollider2D>();
        bc.size = bodySize;
        bc.offset = new Vector2(0, bodySize.y * 0.5f);
        bc.sharedMaterial = Compat.NoFriction;

        var hb = new GameObject("Hitbox");
        hb.transform.SetParent(go.transform, false);
        var hbc = hb.AddComponent<BoxCollider2D>();
        hbc.isTrigger = true;
        hbc.size = hitSize;
        hbc.offset = new Vector2(0, hitSize.y * 0.5f);

        var sr = Art.MakeSprite("Sprite", go.transform, first, feet, 10);
        sr.transform.localScale = Vector3.one * spriteScale;
        var an = sr.gameObject.AddComponent<SpriteAnim>();
        an.sr = sr;
        return go.AddComponent<T>();
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        body = GetComponent<Collider2D>();
        var hb = transform.Find("Hitbox");
        hitbox = hb != null ? hb.GetComponent<Collider2D>() : body;
        anim = GetComponentInChildren<SpriteAnim>();
        sr = anim != null ? anim.sr : GetComponentInChildren<SpriteRenderer>();
        All.Add(this);
        if (rb != null) rb.simulated = false;
    }

    protected virtual void Start()
    {
        if (PlayerController.I != null) IgnorePlayer(PlayerController.I.GetComponent<Collider2D>());
        maxHp = Mathf.Max(maxHp, hp);
    }

    protected virtual void OnDestroy() { All.Remove(this); }

    public void IgnorePlayer(Collider2D playerCol)
    {
        if (playerCol == null) return;
        if (body != null && !body.isTrigger) Physics2D.IgnoreCollision(playerCol, body, true);
    }

    // ------------------------------------------------------------ Bucle
    protected virtual bool ShouldWake()
    {
        var cam = Camera.main;
        if (cam == null) return true;
        float half = cam.orthographicSize * cam.aspect + 2f;
        return Mathf.Abs(transform.position.x - cam.transform.position.x) < half;
    }

    protected virtual void Update()
    {
        if (!awake)
        {
            if (ShouldWake()) { awake = true; if (rb != null) rb.simulated = true; OnWake(); }
            else return;
        }
        if (invulnTimer > 0) invulnTimer -= Time.deltaTime;
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            sr.color = ((int)(flashTimer * 20f)) % 2 == 0 ? new Color(1f, 0.3f, 0.3f) : Color.white;
            if (flashTimer <= 0) sr.color = baseColor;
        }
        if (dead) return;
        if (GameManager.I != null && GameManager.I.State != GState.Playing && GameManager.I.State != GState.Dying)
        {
            if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic) rb.SetVelX(0);
            return;
        }
        CheckGround();
        Think();
        if (sr != null) sr.flipX = facing < 0;
        if (transform.position.y < -3f) Destroy(gameObject);
    }

    protected virtual void OnWake() { }
    protected abstract void Think();

    protected void CheckGround()
    {
        if (Level == null) { grounded = true; return; }
        Vector2 p = transform.position;
        grounded = rb != null && rb.Vel().y <= 0.1f &&
                   (Level.IsSolid(p.x - 0.2f, p.y - 0.1f) || Level.IsSolid(p.x + 0.2f, p.y - 0.1f));
    }

    protected bool WallAhead(float height = 0.5f, float dist = 0.45f) =>
        Level != null && Level.IsSolid(transform.position.x + facing * dist, transform.position.y + height);

    protected bool GapAhead(float dist = 0.45f) =>
        Level != null && !Level.IsSolid(transform.position.x + facing * dist, transform.position.y - 0.3f);

    protected bool HasGroundWithin(int tiles)
    {
        if (Level == null) return false;
        for (int i = 1; i <= tiles; i++)
            if (Level.IsSolid(transform.position.x + facing * i, transform.position.y - 0.3f)) return true;
        return false;
    }

    protected Vector2 ToPlayer => Player != null ? (Vector2)(Player.transform.position - transform.position) : new Vector2(999, 999);

    // ------------------------------------------------------------ Interacción con el jugador
    public virtual void OnPlayerContact(PlayerController p)
    {
        if (dead || !awake) return;
        float feet = p.transform.position.y;
        float top = hitbox.bounds.max.y;
        if (stompable && p.VelY <= 1f && feet > top - stompMargin)
        {
            OnStomped(p);
            return;
        }
        if (p.VelY < -2f && feet > top - stompMargin && !stompable)
        {
            // enemigo que no se puede pisar (pinchudo): rebota y daña
            p.Bounce(8f);
        }
        p.TakeDamage(transform.position.x);
    }

    protected virtual void OnStomped(PlayerController p)
    {
        p.Bounce();
        SoundManager.Play("stomp");
        Damage(99, 0);
    }

    public virtual void OnPunched(int dir)
    {
        if (!punchable || dead || !awake) return;
        SoundManager.Play("hit");
        Fx.Sparks(hitbox.bounds.center, Color.white, 6, 4f);
        Damage(1, dir);
    }

    public virtual void Damage(int amount, int dir)
    {
        if (dead || invulnTimer > 0) return;
        hp -= amount;
        flashTimer = 0.3f;
        invulnTimer = 0.25f;
        if (hp <= 0) { Die(); return; }
        if (dir != 0 && rb != null && rb.bodyType == RigidbodyType2D.Dynamic) rb.SetVel(new Vector2(dir * 5f, 5f));
        OnHurt();
    }

    protected virtual void OnHurt() { }

    protected virtual void Die()
    {
        dead = true;
        GameManager.I?.AddScore(scoreValue);
        Fx.Popup((Vector2)transform.position + Vector2.up * 1.5f, scoreValue.ToString(), Color.white);
        if (hitbox != null) hitbox.enabled = false;
        if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic) rb.SetVelX(0);
        PlayDeath();
        Destroy(gameObject, 1.4f);
    }

    protected virtual void PlayDeath()
    {
        Fx.Puff(hitbox.bounds.center, Color.white);
    }
}

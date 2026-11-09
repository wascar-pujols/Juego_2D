using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Kenji, el protagonista. Puede caminar, correr, saltar (salto variable), caer sobre los enemigos
/// para derrotarlos, golpear con el puño y romper bloques golpeándolos desde abajo.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static PlayerController I;

    // Ajustes de movimiento
    const float WalkSpeed = 6.5f, RunSpeed = 9.5f;
    const float GroundAccel = 70f, AirAccel = 45f;
    const float JumpVelocity = 17f;
    const float Gravity = 3.2f, FallGravityMult = 1.5f, MaxFall = 22f;
    const float CoyoteTime = 0.1f, JumpBufferTime = 0.12f;
    const float InvulnTime = 1.6f;

    Rigidbody2D rb;
    CapsuleCollider2D col;
    SpriteRenderer sr;
    SpriteAnim anim;
    Sprite[] aIdle, aWalk, aRun, aJump, aAtk1, aAtk2, aHurt, aDead;

    public bool Dead { get; private set; }
    public int Facing { get; private set; } = 1;
    public bool Grounded { get; private set; }
    public float VelY => rb.Vel().y;
    public bool Invulnerable => invuln > 0;

    float coyote, jumpBuffer, invuln, hurtTimer, attackTimer, attackCooldown;
    bool jumpCut, hitDone, celebrating;
    int attackCombo;
    Collider2D groundCol;
    readonly List<Collider2D> results = new List<Collider2D>();
    ContactFilter2D solidFilter, allFilter;

    public static PlayerController Create(Vector2 pos, Transform parent)
    {
        var go = new GameObject("Kenji (Jugador)");
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.tag = "Player";
        var rb = go.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale = Gravity;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var c = go.AddComponent<CapsuleCollider2D>();
        c.size = new Vector2(0.6f, 1.5f);
        c.offset = new Vector2(0, 0.75f);
        c.sharedMaterial = Compat.NoFriction;
        var sr = Art.MakeSprite("Sprite", go.transform, Art.Char("Fighter", "Idle")[0], pos, 20);
        var an = sr.gameObject.AddComponent<SpriteAnim>();
        an.sr = sr;
        return go.AddComponent<PlayerController>();
    }

    void Awake()
    {
        I = this;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        anim = GetComponentInChildren<SpriteAnim>();
        sr = anim.sr;
        aIdle = Art.Char("Fighter", "Idle");
        aWalk = Art.Char("Fighter", "Walk");
        aRun = Art.Char("Fighter", "Run");
        aJump = Art.Char("Fighter", "Jump");
        aAtk1 = Art.Char("Fighter", "Attack_1");
        aAtk2 = Art.Char("Fighter", "Attack_2");
        aHurt = Art.Char("Fighter", "Hurt");
        aDead = Art.Char("Fighter", "Dead");
        solidFilter = new ContactFilter2D { useTriggers = false };
        solidFilter.NoFilter(); solidFilter.useTriggers = false;
        allFilter = new ContactFilter2D(); allFilter.NoFilter(); allFilter.useTriggers = true;
    }

    void Start()
    {
        foreach (var e in EnemyBase.All) e.IgnorePlayer(col);
        anim.Play(aIdle, 8);
    }

    void OnDestroy() { if (I == this) I = null; }

    bool CanControl => !Dead && !celebrating && GameManager.I != null && GameManager.I.State == GState.Playing;

    // ------------------------------------------------------------ Update
    void Update()
    {
        float dt = Time.deltaTime;
        if (invuln > 0) invuln -= dt;
        if (hurtTimer > 0) hurtTimer -= dt;
        if (attackCooldown > 0) attackCooldown -= dt;
        if (jumpBuffer > 0) jumpBuffer -= dt;
        coyote = Grounded ? CoyoteTime : coyote - dt;

        if (CanControl)
        {
            if (GameInput.JumpDown) jumpBuffer = JumpBufferTime;
            if (jumpBuffer > 0 && coyote > 0 && hurtTimer <= 0) DoJump();
            if (!GameInput.JumpHeld && !jumpCut && rb.Vel().y > 0)
            {
                rb.SetVelY(rb.Vel().y * 0.5f);
                jumpCut = true;
            }
            if (GameInput.AttackDown && attackCooldown <= 0 && hurtTimer <= 0) StartAttack();

            if (attackTimer > 0)
            {
                attackTimer -= dt;
                if (!hitDone && attackTimer < 0.2f) { DoAttackHit(); hitDone = true; }
            }

            if (transform.position.y < -2f) GameManager.I.PlayerFell();
        }
        else if (Dead || celebrating)
        {
            if (transform.position.y < -4f) { rb.simulated = false; }
        }

        // parpadeo de invulnerabilidad
        sr.enabled = invuln <= 0 || Dead || ((int)(invuln * 14f)) % 2 == 0;
        sr.flipX = Facing < 0;
        UpdateAnimation();
    }

    void DoJump()
    {
        rb.SetVelY(JumpVelocity);
        jumpBuffer = 0; coyote = 0; Grounded = false; jumpCut = false;
        SoundManager.Play("jump", 0.6f);
    }

    void StartAttack()
    {
        attackTimer = 0.32f;
        attackCooldown = 0.36f;
        hitDone = false;
        attackCombo = 1 - attackCombo;
        anim.Restart(attackCombo == 0 ? aAtk1 : aAtk2, 14, false);
        SoundManager.Play("punch", 0.7f);
    }

    void DoAttackHit()
    {
        Vector2 center = (Vector2)transform.position + new Vector2(Facing * 0.85f, 0.8f);
        results.Clear();
        Physics2D.OverlapBox(center, new Vector2(1.1f, 1.0f), 0, allFilter, results);
        var done = new HashSet<Object>();
        foreach (var c in results)
        {
            if (c == null || c == col) continue;
            var e = c.GetComponentInParent<EnemyBase>();
            if (e != null && done.Add(e)) { e.OnPunched(Facing); continue; }
            var p = c.GetComponentInParent<Projectile>();
            if (p != null && done.Add(p)) { p.Deflect(); continue; }
            var b = c.GetComponent<BumpBlock>();
            if (b != null && b.kind == BumpBlock.Kind.Crate && done.Add(b)) b.Bump();
        }
    }

    void UpdateAnimation()
    {
        if (Dead)
        {
            anim.Play(aDead, 6, false);
            return;
        }
        if (celebrating)
        {
            if (Grounded) anim.Play(aIdle, 8);
            return;
        }
        if (hurtTimer > 0) { anim.Play(aHurt, 10, false); return; }
        if (attackTimer > 0) return; // la animación del ataque ya está sonando
        if (!Grounded)
        {
            float vy = rb.Vel().y;
            int n = aJump.Length;
            int idx = vy > 4f ? (int)(n * 0.3f) : vy > -3f ? (int)(n * 0.5f) : (int)(n * 0.7f);
            anim.Hold(aJump, idx);
            return;
        }
        float vx = Mathf.Abs(rb.Vel().x);
        if (vx > WalkSpeed + 0.5f) anim.Play(aRun, 14);
        else if (vx > 0.3f) anim.Play(aWalk, 11);
        else anim.Play(aIdle, 8);
    }

    // ------------------------------------------------------------ Física
    void FixedUpdate()
    {
        if (!rb.simulated) return;
        CheckGround();

        Vector2 v = rb.Vel();
        float platformVx = 0;
        if (Grounded && groundCol != null)
        {
            var mp = groundCol.GetComponent<MovingPlatform>();
            if (mp != null) platformVx = mp.Velocity.x;
            var cp = groundCol.GetComponent<CrumblePlatform>();
            if (cp != null) cp.Step();
        }

        if (Dead)
        {
            v.x = Mathf.MoveTowards(v.x, 0, 20f * Time.fixedDeltaTime);
        }
        else if (celebrating)
        {
            v.x = Mathf.MoveTowards(v.x, platformVx, 30f * Time.fixedDeltaTime);
        }
        else if (hurtTimer <= 0 && CanControl)
        {
            float input = GameInput.Horizontal;
            if (input != 0 && attackTimer <= 0) Facing = input > 0 ? 1 : -1;
            float speed = GameInput.RunHeld ? RunSpeed : WalkSpeed;
            float target = input * speed + platformVx;
            float accel = Grounded ? GroundAccel : AirAccel;
            v.x = Mathf.MoveTowards(v.x, target, accel * Time.fixedDeltaTime);
        }

        rb.gravityScale = v.y < 0 ? Gravity * FallGravityMult : Gravity;
        if (v.y < -MaxFall) v.y = -MaxFall;
        rb.SetVel(v);
    }

    void CheckGround()
    {
        Grounded = false; groundCol = null;
        if (rb.Vel().y > 0.5f) return;
        Vector2 p = transform.position;
        results.Clear();
        Physics2D.OverlapBox(p + new Vector2(0, -0.04f), new Vector2(0.5f, 0.12f), 0, solidFilter, results);
        foreach (var c in results)
        {
            if (c == col || c.isTrigger) continue;
            if (c.GetComponentInParent<EnemyBase>() != null) continue;
            Grounded = true; groundCol = c;
            if (c.GetComponent<MovingPlatform>() != null || c.GetComponent<CrumblePlatform>() != null) break;
        }
    }

    // ------------------------------------------------------------ Colisiones
    void OnCollisionEnter2D(Collision2D c)
    {
        if (Dead) return;
        // ¿Golpeó un bloque con la cabeza?
        BumpBlock best = null; float bestDist = 99f;
        int n = c.contactCount;
        for (int i = 0; i < n; i++)
        {
            var cp = c.GetContact(i);
            if (cp.normal.y < -0.5f)
            {
                var b = cp.collider.GetComponent<BumpBlock>();
                if (b == null) continue;
                float d = Mathf.Abs(b.transform.position.x - transform.position.x);
                if (d < bestDist) { bestDist = d; best = b; }
            }
        }
        if (best != null) best.Bump();
    }

    void OnTriggerEnter2D(Collider2D other) => HandleTrigger(other);
    void OnTriggerStay2D(Collider2D other) => HandleTrigger(other);

    void HandleTrigger(Collider2D other)
    {
        if (Dead || GameManager.I == null || GameManager.I.State != GState.Playing) return;
        var e = other.GetComponentInParent<EnemyBase>();
        if (e != null) { e.OnPlayerContact(this); return; }
        var h = other.GetComponent<Hazard>();
        if (h != null) { TakeDamage(other.bounds.center.x); return; }
        var pr = other.GetComponent<Projectile>();
        if (pr != null) { pr.HitPlayer(this); return; }
        var coin = other.GetComponent<Coin>();
        if (coin != null) { coin.Collect(); return; }
        var heart = other.GetComponent<HeartPickup>();
        if (heart != null) { heart.Collect(); return; }
        var cp = other.GetComponent<Checkpoint>();
        if (cp != null) { cp.Activate(); return; }
        var g = other.GetComponent<Goal>();
        if (g != null) { GameManager.I.LevelComplete(); return; }
    }

    // ------------------------------------------------------------ Acciones públicas
    public void Bounce(float strength = 0f)
    {
        float v = strength > 0 ? strength : (GameInput.JumpHeld ? 15f : 10f);
        rb.SetVelY(v);
        Grounded = false; coyote = 0; jumpCut = !GameInput.JumpHeld;
    }

    public void TakeDamage(float fromX)
    {
        if (Dead || invuln > 0 || celebrating) return;
        invuln = InvulnTime;
        hurtTimer = 0.35f;
        attackTimer = 0;
        float dir = Mathf.Sign(transform.position.x - fromX);
        if (dir == 0) dir = -Facing;
        rb.SetVel(new Vector2(dir * 6f, 8f));
        SoundManager.Play("hurt");
        if (CameraFollow.I != null) CameraFollow.I.Shake(0.25f);
        GameManager.I.PlayerHurt();
    }

    public void Die()
    {
        if (Dead) return;
        Dead = true;
        invuln = 0;
        sr.enabled = true;
        rb.SetVel(new Vector2(0, Mathf.Max(rb.Vel().y, 6f)));
    }

    public void Respawn(Vector2 pos)
    {
        transform.position = pos;
        rb.simulated = true;
        rb.SetVel(Vector2.zero);
        invuln = 2f;
        hurtTimer = 0;
        Dead = false;
    }

    public void Celebrate()
    {
        celebrating = true;
        invuln = 0;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + new Vector3(Facing * 0.85f, 0.8f), new Vector3(1.1f, 1f));
    }
}

using UnityEngine;

/// <summary>
/// JEFE FINAL - El Shogun Oscuro: samurái gigante del Nivel 3.
/// Ataques: embestida con la katana, salto aplastante que lanza ondas de fuego,
/// y lluvia de shuriken. Con media vida entra en fase 2 (más rápido y agresivo).
/// Recibe daño al pisarlo o con puñetazos (8 golpes). Avisa antes de embestir
/// y queda cansado tras cada ataque, que es el momento para golpearlo.
/// </summary>
public class Boss : EnemyBase
{
    enum S { Waiting, Idle, Walk, Windup, Dash, Jump, Throw, Hurt, Dying }
    S state = S.Waiting;
    float timer;
    int lastAttack = -1;
    bool phase2;
    bool landed;
    int throwsLeft;
    Sprite[] aIdle, aWalk, aRun, aJump, aAtk1, aAtk2, aAtk3, aHurt, aDead;
    const float Scale = 2.0f;

    public static Boss Create(Vector2 feet, Transform parent)
    {
        var b = MakeCharacter<Boss>("Shogun Oscuro (Jefe)", feet, parent, Art.Char("Samurai", "Idle")[0],
            new Vector2(1.2f, 2.8f), new Vector2(1.4f, 2.8f), Scale);
        return b;
    }

    protected override void Awake()
    {
        base.Awake();
        hp = 8; maxHp = 8; scoreValue = 5000;
        stompMargin = 0.6f;
        aIdle = Art.Char("Samurai", "Idle");
        aWalk = Art.Char("Samurai", "Walk");
        aRun = Art.Char("Samurai", "Run");
        aJump = Art.Char("Samurai", "Jump");
        aAtk1 = Art.Char("Samurai", "Attack_1");
        aAtk2 = Art.Char("Samurai", "Attack_2");
        aAtk3 = Art.Char("Samurai", "Attack_3");
        aHurt = Art.Char("Samurai", "Hurt");
        aDead = Art.Char("Samurai", "Dead");
        baseColor = new Color(0.72f, 0.55f, 0.95f);
        sr.color = baseColor;
        sr.sortingOrder = 11;
        rb.mass = 20f;
        anim.Play(aIdle, 6);
    }

    protected override void Start()
    {
        base.Start();
        // El jefe pasa por debajo de las plataformas de madera de la arena
        foreach (var c in OneWayPlatform.All)
            if (c != null) Physics2D.IgnoreCollision(body, c, true);
    }

    public float HealthFraction => Mathf.Clamp01(hp / (float)maxHp);

    // El jefe solo se activa cuando el jugador entra a la arena.
    protected override bool ShouldWake() => true;

    public void Activate()
    {
        if (state != S.Waiting) return;
        Go(S.Idle, 1.5f);
    }

    void Go(S s, float t)
    {
        state = s; timer = t;
        switch (s)
        {
            case S.Idle: anim.Play(aIdle, 7); break;
            case S.Walk: anim.Play(aWalk, 10); break;
            case S.Windup: anim.Hold(aAtk1, 0); SoundManager.Play("pause", 0.6f); break;
            case S.Dash: anim.Restart(aRun, 16); SoundManager.Play("dash"); break;
            case S.Jump: anim.Hold(aJump, 3); break;
            case S.Throw: anim.Restart(aAtk2, 10, false); break;
            case S.Hurt: anim.Restart(aHurt, 8, false); break;
        }
    }

    void FacePlayer()
    {
        float dx = ToPlayer.x;
        if (Mathf.Abs(dx) > 0.2f) facing = dx > 0 ? 1 : -1;
    }

    protected override void Think()
    {
        float dt = Time.deltaTime;
        timer -= dt;
        float speedMul = phase2 ? 1.15f : 1f;

        switch (state)
        {
            case S.Waiting:
                rb.SetVelX(0);
                FacePlayer();
                break;

            case S.Idle:
                rb.SetVelX(0);
                FacePlayer();
                if (timer <= 0) ChooseAttack();
                break;

            case S.Walk:
                FacePlayer();
                rb.SetVelX(facing * 2.5f * speedMul);
                if (timer <= 0 || Mathf.Abs(ToPlayer.x) < 2.5f) ChooseAttack();
                break;

            case S.Windup:
                // aviso antes de la embestida: se detiene y parpadea en blanco
                rb.SetVelX(0);
                sr.color = ((int)(timer * 16f)) % 2 == 0 ? Color.white : baseColor;
                if (timer <= 0) { sr.color = baseColor; Go(S.Dash, 1.2f); }
                break;

            case S.Dash:
                rb.SetVelX(facing * 8.5f * speedMul);
                if (Mathf.FloorToInt(timer * 20) % 3 == 0) Fx.Sparks((Vector2)transform.position + Vector2.up * 0.2f, new Color(0.8f, 0.7f, 1f), 1, 2f);
                if (WallAhead(1f, 1.2f) || timer <= 0)
                {
                    rb.SetVelX(0);
                    anim.Restart(aAtk1, 14, false);
                    SoundManager.Play("hit", 0.6f);
                    Go(S.Idle, phase2 ? 1.4f : 1.9f);  // queda cansado: momento para golpearlo
                    anim.Restart(aAtk1, 14, false);
                }
                break;

            case S.Jump:
                if (!landed && rb.Vel().y < 0 && grounded)
                {
                    landed = true;
                    rb.SetVelX(0);
                    SoundManager.Play("slam");
                    if (CameraFollow.I != null) CameraFollow.I.Shake(0.5f);
                    Vector2 p = transform.position;
                    Shockwave.Create(p + new Vector2(-1f, 0), -1, LevelBuilder.Root);
                    Shockwave.Create(p + new Vector2(1f, 0), 1, LevelBuilder.Root);
                    Go(S.Idle, phase2 ? 1.3f : 1.7f);
                }
                else
                {
                    float vy = rb.Vel().y;
                    anim.Hold(aJump, vy > 3f ? 4 : vy > -3f ? 6 : 8);
                }
                break;

            case S.Throw:
                rb.SetVelX(0);
                if (timer <= 0)
                {
                    if (throwsLeft > 0)
                    {
                        throwsLeft--;
                        ThrowVolley();
                        timer = 1.1f;
                        anim.Restart(aAtk2, 10, false);
                    }
                    else Go(S.Idle, 1.6f);
                }
                break;

            case S.Hurt:
                if (timer <= 0) { FacePlayer(); Go(S.Idle, 1.0f); }
                break;
        }
    }

    void ChooseAttack()
    {
        FacePlayer();
        int pick;
        do { pick = Random.Range(0, 4); } while (pick == lastAttack && pick != 3);
        lastAttack = pick;
        float dist = Mathf.Abs(ToPlayer.x);
        if (pick == 3 && dist < 6f) pick = 0;
        switch (pick)
        {
            case 0: Go(S.Windup, 0.7f); break;
            case 1:
                Go(S.Jump, 3f);
                landed = false;
                float vx = Mathf.Clamp(ToPlayer.x * 0.9f, -8f, 8f);
                rb.SetVel(new Vector2(vx, 15f));
                SoundManager.Play("jump", 0.8f);
                break;
            case 2:
                throwsLeft = phase2 ? 2 : 1;
                Go(S.Throw, 0.3f);
                break;
            default: Go(S.Walk, 1.5f); break;
        }
    }

    void ThrowVolley()
    {
        if (Player == null) return;
        Vector2 from = (Vector2)transform.position + new Vector2(facing * 1.0f, 2.0f);
        Vector2 dir = ((Vector2)Player.transform.position + Vector2.up * 0.6f - from).normalized;
        int n = 3;
        float spread = 22f;
        for (int i = 0; i < n; i++)
        {
            float a = (i - (n - 1) * 0.5f) * spread;
            Vector2 d = Quaternion.Euler(0, 0, a) * dir;
            Projectile.CreateShuriken(from, d * 5.5f, LevelBuilder.Root);
        }
        SoundManager.Play("shuriken");
    }

    public override void OnPlayerContact(PlayerController p)
    {
        if (dead || state == S.Waiting) return;
        float feet = p.transform.position.y;
        float top = hitbox.bounds.max.y;
        if (p.VelY <= 1f && feet > top - stompMargin)
        {
            p.Bounce(14f);
            if (invulnTimer <= 0)
            {
                SoundManager.Play("bosshit");
                Damage(1, 0);
            }
            return;
        }
        // Solo hace daño al tocarlo mientras embiste o salta; quieto se le puede golpear.
        if (state == S.Dash || state == S.Jump) p.TakeDamage(transform.position.x);
    }

    public override void OnPunched(int dir)
    {
        if (dead || state == S.Waiting || invulnTimer > 0) return;
        SoundManager.Play("bosshit");
        Fx.Sparks(hitbox.bounds.center, Color.white, 8, 5f);
        Damage(1, 0);
    }

    public override void Damage(int amount, int dir)
    {
        if (dead || invulnTimer > 0) return;
        hp -= amount;
        flashTimer = 0.4f;
        invulnTimer = 0.4f;
        if (hp <= 0) { Die(); return; }
        if (!phase2 && hp <= maxHp / 2)
        {
            phase2 = true;
            baseColor = new Color(1f, 0.45f, 0.55f);
            Fx.Sparks(hitbox.bounds.center, new Color(1f, 0.3f, 0.3f), 20, 7f);
            if (CameraFollow.I != null) CameraFollow.I.Shake(0.4f);
        }
        if (state == S.Idle || state == S.Walk || state == S.Throw || state == S.Windup) { sr.color = baseColor; Go(S.Hurt, 0.35f); }
    }

    protected override void Die()
    {
        dead = true;
        state = S.Dying;
        GameManager.I?.AddScore(scoreValue);
        Fx.Popup((Vector2)transform.position + Vector2.up * 3.5f, scoreValue.ToString(), Color.yellow);
        hitbox.enabled = false;
        rb.SetVelX(0);
        anim.Restart(aDead, 4, false);
        sr.color = baseColor;
        SoundManager.Play("slam");
        if (CameraFollow.I != null) CameraFollow.I.Shake(0.8f);
        for (int i = 0; i < 4; i++) Fx.Sparks((Vector2)transform.position + Random.insideUnitCircle * 1.2f + Vector2.up, new Color(1f, 0.8f, 0.3f), 14, 7f);
        Invoke(nameof(Finish), 2f);
    }

    void Finish()
    {
        GameManager.I?.BossDefeated();
    }
}

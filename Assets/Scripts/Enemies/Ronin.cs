using UnityEngine;

/// <summary>
/// ENEMIGO 1 - Ronin: samurái errante que patrulla. Da media vuelta en paredes y bordes.
/// Si Kenji se acerca de frente, ataca con la katana (el tajo también hace daño).
/// Se derrota pisándolo o con un puñetazo.
/// </summary>
public class Ronin : EnemyBase
{
    Sprite[] aWalk, aAttack, aDead, aHurt;
    float attackTimer, attackCooldown, hurtTimer;
    bool slashDone;
    const float Speed = 2.2f;

    public static Ronin Create(Vector2 feet, Transform parent)
    {
        var e = MakeCharacter<Ronin>("Ronin", feet, parent, Art.Char("Samurai", "Walk")[0],
            new Vector2(0.6f, 1.45f), new Vector2(0.7f, 1.4f));
        return e;
    }

    protected override void Awake()
    {
        base.Awake();
        hp = 1; scoreValue = 200;
        aWalk = Art.Char("Samurai", "Walk");
        aAttack = Art.Char("Samurai", "Attack_1");
        aDead = Art.Char("Samurai", "Dead");
        aHurt = Art.Char("Samurai", "Hurt");
        baseColor = new Color(1f, 0.82f, 0.78f);
        sr.color = baseColor;
        anim.Play(aWalk, 9);
    }

    protected override void Think()
    {
        float dt = Time.deltaTime;
        if (attackCooldown > 0) attackCooldown -= dt;
        if (hurtTimer > 0) { hurtTimer -= dt; return; }

        if (attackTimer > 0)
        {
            attackTimer -= dt;
            rb.SetVelX(0);
            if (!slashDone && anim.Frame >= 3)
            {
                slashDone = true;
                // tajo de katana delante del ronin
                if (Player != null && !Player.Dead)
                {
                    Vector2 d = ToPlayer;
                    if (Mathf.Sign(d.x) == facing && Mathf.Abs(d.x) < 1.9f && Mathf.Abs(d.y) < 1.3f)
                        Player.TakeDamage(transform.position.x);
                }
            }
            if (attackTimer <= 0) anim.Play(aWalk, 9);
            return;
        }

        if (grounded && (WallAhead() || GapAhead())) facing = -facing;
        rb.SetVelX(facing * Speed);
        anim.Play(aWalk, 9);

        Vector2 tp = ToPlayer;
        if (attackCooldown <= 0 && Mathf.Sign(tp.x) == facing && Mathf.Abs(tp.x) < 2.2f && Mathf.Abs(tp.y) < 1.2f)
        {
            attackTimer = 0.6f; attackCooldown = 1.6f; slashDone = false;
            anim.Restart(aAttack, 10, false);
            SoundManager.Play("shuriken", 0.5f);
        }
    }

    protected override void OnHurt()
    {
        hurtTimer = 0.3f;
        anim.Restart(aHurt, 8, false);
    }

    protected override void PlayDeath()
    {
        anim.Restart(aDead, 6, false);
    }
}

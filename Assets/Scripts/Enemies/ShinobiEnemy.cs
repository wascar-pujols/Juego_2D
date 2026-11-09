using UnityEngine;

/// <summary>
/// ENEMIGO 2 - Shinobi asesino: espera escondido y, cuando ve a Kenji, corre hacia él,
/// salta obstáculos y precipicios. Aguanta 2 puñetazos (o un pisotón).
/// </summary>
public class ShinobiEnemy : EnemyBase
{
    Sprite[] aIdle, aRun, aJump, aAttack, aDead, aHurt;
    float hurtTimer, attackTimer, jumpCooldown;
    const float Speed = 4.4f, JumpV = 13f, SightX = 10f;

    public static ShinobiEnemy Create(Vector2 feet, Transform parent)
    {
        return MakeCharacter<ShinobiEnemy>("Shinobi", feet, parent, Art.Char("Shinobi", "Idle")[0],
            new Vector2(0.6f, 1.45f), new Vector2(0.7f, 1.4f));
    }

    protected override void Awake()
    {
        base.Awake();
        hp = 2; scoreValue = 400;
        aIdle = Art.Char("Shinobi", "Idle");
        aRun = Art.Char("Shinobi", "Run");
        aJump = Art.Char("Shinobi", "Jump");
        aAttack = Art.Char("Shinobi", "Attack_1");
        aDead = Art.Char("Shinobi", "Dead");
        aHurt = Art.Char("Shinobi", "Hurt");
        anim.Play(aIdle, 8);
    }

    protected override void Think()
    {
        float dt = Time.deltaTime;
        if (jumpCooldown > 0) jumpCooldown -= dt;
        if (hurtTimer > 0) { hurtTimer -= dt; return; }

        Vector2 tp = ToPlayer;
        bool sees = Player != null && !Player.Dead && Mathf.Abs(tp.x) < SightX && Mathf.Abs(tp.y) < 5f;

        if (attackTimer > 0)
        {
            attackTimer -= dt;
            rb.SetVelX(facing * 1.5f);
            if (attackTimer <= 0) anim.Play(aRun, 14);
            return;
        }

        if (!sees)
        {
            if (grounded) rb.SetVelX(0);
            anim.Play(aIdle, 8);
            return;
        }

        if (Mathf.Abs(tp.x) > 0.3f) facing = tp.x > 0 ? 1 : -1;

        if (grounded)
        {
            bool wall = WallAhead(0.5f, 0.5f);
            bool gap = GapAhead(0.5f);
            bool playerAbove = tp.y > 1.5f && Mathf.Abs(tp.x) < 3.5f;
            if (jumpCooldown <= 0 && (wall || playerAbove || (gap && HasGroundWithin(4))))
            {
                rb.SetVelY(JumpV);
                jumpCooldown = 0.8f;
            }
            else if (gap)
            {
                rb.SetVelX(0);
                anim.Play(aIdle, 8);
                return;
            }
        }

        rb.SetVelX(facing * Speed);

        if (Mathf.Abs(tp.x) < 1.6f && Mathf.Abs(tp.y) < 1.2f && grounded)
        {
            attackTimer = 0.4f;
            anim.Restart(aAttack, 12, false);
            SoundManager.Play("punch", 0.4f);
            return;
        }

        if (!grounded)
        {
            float vy = rb.Vel().y;
            anim.Hold(aJump, vy > 2f ? 3 : vy > -2f ? 5 : 8);
        }
        else anim.Play(aRun, 14);
    }

    protected override void OnHurt()
    {
        hurtTimer = 0.35f;
        anim.Restart(aHurt, 8, false);
    }

    protected override void PlayDeath()
    {
        anim.Restart(aDead, 7, false);
    }
}

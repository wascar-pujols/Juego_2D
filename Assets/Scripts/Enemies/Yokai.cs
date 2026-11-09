using UnityEngine;

/// <summary>
/// ENEMIGO 3 - Chōchin-obake (yokai farol): espíritu volador que flota en ondas
/// de un lado a otro. Se derrota pisándolo o golpeándolo.
/// </summary>
public class Yokai : EnemyBase
{
    Vector2 origin;
    float t;
    const float RangeX = 3f, AmpY = 0.8f;

    public static Yokai Create(Vector2 center, Transform parent)
    {
        var go = new GameObject("Yokai Farol");
        go.transform.SetParent(parent, false);
        go.transform.position = center;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var c = go.AddComponent<CircleCollider2D>();
        c.isTrigger = true; c.radius = 0.38f;
        var sr = Art.MakeSprite("Sprite", go.transform, null, center, 12);
        var an = sr.gameObject.AddComponent<SpriteAnim>(); an.sr = sr;
        return go.AddComponent<Yokai>();
    }

    protected override void Awake()
    {
        base.Awake();
        hp = 1; scoreValue = 300;
        stompMargin = 0.25f;
        origin = transform.position;
        anim.Play(Art.Strip("Props/yokai", 4, Art.TilePPU, new Vector2(0.5f, 0.5f)), 6);
        t = Random.Range(0f, 6f);
    }

    protected override void Think()
    {
        t += Time.deltaTime;
        float x = origin.x + Mathf.Sin(t * 0.8f) * RangeX;
        float y = origin.y + Mathf.Sin(t * 2.6f) * AmpY;
        facing = Mathf.Cos(t * 0.8f) >= 0 ? 1 : -1;
        rb.MovePosition(new Vector2(x, y));
    }

    public override void OnPlayerContact(PlayerController p)
    {
        if (dead || !awake) return;
        if (p.VelY <= 1f && p.transform.position.y > transform.position.y)
        {
            OnStomped(p);
            return;
        }
        p.TakeDamage(transform.position.x);
    }

    protected override void PlayDeath()
    {
        Fx.Sparks(transform.position, new Color(1f, 0.6f, 0.2f), 12, 5f);
        sr.enabled = false;
    }
}

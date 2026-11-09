using UnityEngine;

/// <summary>
/// Pequeñas utilidades para que el código funcione igual en Unity 2022 y en Unity 6.
/// </summary>
public static class Compat
{
    public static Vector2 Vel(this Rigidbody2D rb)
    {
#if UNITY_6000_0_OR_NEWER
        return rb.linearVelocity;
#else
        return rb.velocity;
#endif
    }

    public static void SetVel(this Rigidbody2D rb, Vector2 v)
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = v;
#else
        rb.velocity = v;
#endif
    }

    public static void SetVelX(this Rigidbody2D rb, float x) { var v = rb.Vel(); v.x = x; rb.SetVel(v); }
    public static void SetVelY(this Rigidbody2D rb, float y) { var v = rb.Vel(); v.y = y; rb.SetVel(v); }

    public static void MergeIntoComposite(Collider2D c)
    {
#if UNITY_2023_1_OR_NEWER
        c.compositeOperation = Collider2D.CompositeOperation.Merge;
#else
        c.usedByComposite = true;
#endif
    }

    public static T FindFirst<T>() where T : Object
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindFirstObjectByType<T>();
#else
        return Object.FindObjectOfType<T>();
#endif
    }

    static Font font;
    public static Font DefaultFont()
    {
        if (font != null) return font;
        try { font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
        if (font == null) { try { font = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { } }
        return font;
    }

    static PhysicsMaterial2D noFriction;
    public static PhysicsMaterial2D NoFriction
    {
        get
        {
            if (noFriction == null) noFriction = new PhysicsMaterial2D("SinFriccion") { friction = 0f, bounciness = 0f };
            return noFriction;
        }
    }
}

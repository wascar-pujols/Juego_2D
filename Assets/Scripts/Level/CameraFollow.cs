using UnityEngine;

/// <summary>Cámara que sigue al jugador en horizontal, limitada a los bordes del nivel.</summary>
public class CameraFollow : MonoBehaviour
{
    public static CameraFollow I;
    public Transform target;
    public float minX, maxX, fixedY;
    public float lockMinX = float.NegativeInfinity;
    Camera cam;
    float vx;
    float shake;
    float look;

    void Awake() { I = this; cam = GetComponent<Camera>(); }

    public float HalfWidth => cam.orthographicSize * cam.aspect;

    public void Shake(float amount) { shake = Mathf.Max(shake, amount); }

    public void Snap()
    {
        if (target == null) return;
        var p = transform.position;
        p.x = ClampX(target.position.x);
        p.y = fixedY;
        transform.position = p;
    }

    float ClampX(float x)
    {
        float lo = Mathf.Max(minX, lockMinX) + HalfWidth;
        float hi = maxX - HalfWidth;
        if (hi < lo) return (Mathf.Max(minX, lockMinX) + maxX) * 0.5f;
        return Mathf.Clamp(x, lo, hi);
    }

    void LateUpdate()
    {
        if (target == null) return;
        var pc = target.GetComponent<PlayerController>();
        float dir = pc != null ? pc.Facing : 1;
        look = Mathf.MoveTowards(look, dir * 1.5f, Time.deltaTime * 3f);
        float goal = ClampX(target.position.x + look);
        var p = transform.position;
        p.x = Mathf.SmoothDamp(p.x, goal, ref vx, 0.12f);
        p.x = ClampX(p.x);
        p.y = fixedY;
        if (shake > 0)
        {
            shake = Mathf.MoveTowards(shake, 0, Time.deltaTime * 1.5f);
            p += (Vector3)(Random.insideUnitCircle * shake);
        }
        transform.position = p;
    }
}

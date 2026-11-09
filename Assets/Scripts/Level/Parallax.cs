using UnityEngine;

/// <summary>Capa de fondo con efecto parallax (se mueve más despacio que la cámara).</summary>
public class Parallax : MonoBehaviour
{
    public float factor;   // 1 = pegado a la cámara, 0 = fijo en el mundo
    public float y;
    Transform cam;

    void Start() { cam = Camera.main != null ? Camera.main.transform : null; }

    void LateUpdate()
    {
        if (cam == null) { if (Camera.main != null) cam = Camera.main.transform; else return; }
        transform.position = new Vector3(cam.position.x * factor, y, transform.position.z);
    }
}

using UnityEngine;

/// <summary>
/// Punto de entrada de cada escena (Menu, Nivel1, Nivel2, Nivel3).
/// sceneId: 0 = menú, 1..3 = niveles. El contenido del nivel se construye desde LevelData.
/// </summary>
public class SceneEntry : MonoBehaviour
{
    public int sceneId;

    void Start()
    {
        GameManager.Ensure();
        if (sceneId <= 0) MenuController.Build();
        else GameManager.I.BeginLevel(Mathf.Clamp(sceneId, 1, 3));
    }

    /// <summary>Si se pulsa Play en una escena vacía, arranca el menú igualmente.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoStart()
    {
        if (Compat.FindFirst<SceneEntry>() != null) return;
        var go = new GameObject("Escena");
        go.AddComponent<SceneEntry>().sceneId = 0;
    }
}

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Configuración automática del proyecto la primera vez que se abre en Unity:
/// crea las escenas Menu, Nivel1, Nivel2 y Nivel3, las agrega a Build Settings
/// y ajusta los datos del Player (nombre del juego, WebGL para Itch.io).
/// También se puede ejecutar desde el menú "Kage no Michi > Regenerar escenas".
/// </summary>
[InitializeOnLoad]
public static class KageSetup
{
    static readonly string[] SceneNames = { "Menu", "Nivel1", "Nivel2", "Nivel3" };
    const string Folder = "Assets/Scenes";

    static KageSetup()
    {
        EditorApplication.delayCall += () => Run(false);
    }

    [MenuItem("Kage no Michi/Regenerar escenas")]
    static void ForceRun() => Run(true);

    [MenuItem("Kage no Michi/Abrir menú principal")]
    static void OpenMenu()
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(Folder + "/Menu.unity");
    }

    static void Run(bool force)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;

        bool created = false;
        if (!Directory.Exists(Folder)) Directory.CreateDirectory(Folder);
        for (int i = 0; i < SceneNames.Length; i++)
        {
            string path = Folder + "/" + SceneNames[i] + ".unity";
            if (File.Exists(path) && !force) continue;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.45f, 0.72f, 0.94f);
            camGo.transform.position = new Vector3(13f, 7.5f, -10f);
            camGo.AddComponent<AudioListener>();

            var entry = new GameObject("Escena (" + SceneNames[i] + ")");
            entry.AddComponent<SceneEntry>().sceneId = i;

            EditorSceneManager.SaveScene(scene, path);
            created = true;
        }

        var wanted = SceneNames.Select(n => Folder + "/" + n + ".unity").ToArray();
        var current = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
        if (!wanted.SequenceEqual(current))
            EditorBuildSettings.scenes = wanted.Select(p => new EditorBuildSettingsScene(p, true)).ToArray();

        if (created)
        {
            PlayerSettings.productName = "Kage no Michi";
            PlayerSettings.companyName = "Wascar Pujols";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.runInBackground = true;
            try
            {
                PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            }
            catch { }
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(Folder + "/Menu.unity");
            Debug.Log("[Kage no Michi] Escenas creadas y agregadas a Build Settings. Pulsa Play en la escena Menu.");
        }
    }
}

/// <summary>Importa todas las imágenes del juego como sprites pixel art (sin filtro ni compresión).</summary>
public class KageTextureImport : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.Replace('\\', '/').Contains("/Resources/Sprites/")) return;
        var ti = (TextureImporter)assetImporter;
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.filterMode = FilterMode.Point;
        ti.mipmapEnabled = false;
        ti.npotScale = TextureImporterNPOTScale.None;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.alphaIsTransparency = true;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.maxTextureSize = 2048;
    }
}

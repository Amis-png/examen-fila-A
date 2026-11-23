using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Solo funciona dentro del editor (no afecta al juego):
// 1) Crea y abre la escena Assets/Scenes/Juego.unity si no existe.
// 2) Construye el nivel DENTRO de la escena y la guarda, para que se vea sin darle Play.
// Menu: Juego > Reconstruir nivel  (borra el nivel de la escena y lo vuelve a crear)
[InitializeOnLoad]
public static class CrearEscenaInicial
{
    const string RutaEscena = "Assets/Scenes/Juego.unity";
    const string RutaSprite = "Assets/Sprites/Cuadrado.png";
    const string RutaFisica = "Assets/Sprites/SinFriccion.physicsMaterial2D";

    static CrearEscenaInicial()
    {
        EditorApplication.delayCall += Revisar;
    }

    static void Revisar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        if (!File.Exists(RutaEscena))
        {
            Directory.CreateDirectory("Assets/Scenes");
            Scene nueva = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(nueva, RutaEscena);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(RutaEscena, true) };
        }
        else
        {
            Scene actual = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(actual.path) && !actual.isDirty)
                EditorSceneManager.OpenScene(RutaEscena);
        }

        Scene escena = SceneManager.GetActiveScene();
        if (escena.path == RutaEscena && GameObject.Find("Nivel") == null)
            ConstruirEnEscena();
    }

    [MenuItem("Juego/Reconstruir nivel")]
    static void Reconstruir()
    {
        if (EditorApplication.isPlaying) return;
        GameObject viejo = GameObject.Find("Nivel");
        if (viejo != null) Object.DestroyImmediate(viejo);
        Camera c = Camera.main;
        if (c != null)
        {
            Transform nombre = c.transform.Find("Nombre");
            if (nombre != null) Object.DestroyImmediate(nombre.gameObject);
        }
        ConstruirEnEscena();
    }

    static void ConstruirEnEscena()
    {
        Sprite sprite = ObtenerSprite();
        PhysicsMaterial2D fisica = ObtenerMaterialFisico();

        Camera cam = Camera.main;
        if (cam != null)
        {
            Juego.ConfigurarCamara(cam);
            EditorUtility.SetDirty(cam);
        }

        Juego.ConstruirNivel(sprite, fisica, null);

        Scene escena = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Debug.Log("Nivel construido y guardado en la escena.");
    }

    // Guarda un cuadrado blanco como imagen del proyecto (para que la escena lo recuerde)
    static Sprite ObtenerSprite()
    {
        if (!File.Exists(RutaSprite))
        {
            Directory.CreateDirectory("Assets/Sprites");
            Texture2D tex = new Texture2D(4, 4);
            Color[] blanco = new Color[16];
            for (int i = 0; i < 16; i++) blanco[i] = Color.white;
            tex.SetPixels(blanco);
            tex.Apply();
            File.WriteAllBytes(RutaSprite, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(RutaSprite);

            TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(RutaSprite);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 4;
            ti.filterMode = FilterMode.Point;
            ti.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(RutaSprite);
    }

    static PhysicsMaterial2D ObtenerMaterialFisico()
    {
        PhysicsMaterial2D pm = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(RutaFisica);
        if (pm == null)
        {
            Directory.CreateDirectory("Assets/Sprites");
            pm = new PhysicsMaterial2D("SinFriccion");
            pm.friction = 0f;
            AssetDatabase.CreateAsset(pm, RutaFisica);
            AssetDatabase.SaveAssets();
        }
        return pm;
    }
}

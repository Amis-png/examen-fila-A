using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Al abrir el proyecto por primera vez crea y abre la escena Assets/Scenes/Juego.unity.
// Solo funciona dentro del editor; no afecta al juego.
[InitializeOnLoad]
public static class CrearEscenaInicial
{
    const string Ruta = "Assets/Scenes/Juego.unity";

    static CrearEscenaInicial()
    {
        EditorApplication.delayCall += Revisar;
    }

    static void Revisar()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        if (!File.Exists(Ruta))
        {
            Directory.CreateDirectory("Assets/Scenes");
            Scene escena = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(escena, Ruta);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Ruta, true) };
            AssetDatabase.Refresh();
        }
        else
        {
            Scene actual = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(actual.path) && !actual.isDirty)
                EditorSceneManager.OpenScene(Ruta);
        }
    }
}

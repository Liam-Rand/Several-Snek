using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class EditorBootstrap : EditorWindow
{
    void OnGUI()
    {
        // Use the Object Picker to select the start SceneAsset
        EditorSceneManager.playModeStartScene = (SceneAsset)EditorGUILayout.ObjectField(new GUIContent("Editor Playmode Start Scene"), EditorSceneManager.playModeStartScene, typeof(SceneAsset), false);

        // Or set the start Scene from code
        /*var scenePath = "Assets/Scene3.unity";
        if (GUILayout.Button("Set Editor Playmode Start Scene: " + scenePath))
            SetPlayModeStartScene(scenePath);*/
    }

    void SetPlayModeStartScene(string scenePath)
    {
        SceneAsset myWantedStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        if (myWantedStartScene != null)
            EditorSceneManager.playModeStartScene = myWantedStartScene;
        else
            Debug.Log("Could not find Scene " + scenePath);
    }

    [MenuItem("Coba Platinum/Editor Bootstrap")]
    static void Open()
    {
        GetWindow<EditorBootstrap>();
    }
}

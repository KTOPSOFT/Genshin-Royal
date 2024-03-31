using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;
using UnityEditor.SceneManagement;
using MFPS.Addon.Compass;

public class UCompassSceneInstancer : EditorWindow
{
    public static Dictionary<string,string> PrefabPaths = new Dictionary<string, string>()
    {
        { "Assets/Addons/UCompassBar/Content/Prefabs/Compass.prefab","Simple" },
         { "Assets/Addons/UCompassBar/Content/Prefabs/Compass [BlackOut].prefab","BlackOut" },
          { "Assets/Addons/UCompassBar/Content/Prefabs/Compass [Fallout].prefab","Fallout" },
           { "Assets/Addons/UCompassBar/Content/Prefabs/Compass [Fortnite].prefab","Fortnite" },
            { "Assets/Addons/UCompassBar/Content/Prefabs/Compass [Skyrim].prefab","Skyrim" },
             { "Assets/Addons/UCompassBar/Content/Prefabs/Compass [Squad].prefab","Squad" },
    };

    private void OnEnable()
    {
        minSize = new Vector2(300, 300);
        titleContent = new GUIContent("Compass");
    }

    private void OnGUI()
    {
        MFPSEditorStyles.DrawBackground(new Rect(0, 0, position.width, position.height), new Color(0, 0, 0, 0.5f));
        GUILayout.Space(20);
        bool canInstance = ValidCompass();

        if (canInstance)
        {
            GUILayout.Label("Select the Compass bar to add in this scene.");
        }
        else
        {
            GUILayout.Label("Can't instance a compass bar in this scene, it's not a room scene or a compass is already instance");
        }
        GUILayout.Space(10);
        GUI.enabled = canInstance;
        foreach (var c in PrefabPaths.Keys)
        {
            if (GUILayout.Button(PrefabPaths[c]))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath(c, typeof(GameObject)) as GameObject;
                if (prefab != null)
                {
                    GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                    instance.transform.SetParent(bl_UIReferences.Instance.PlayerUI.PlayerUICanvas.transform, false);
                    instance.transform.SetSiblingIndex(10);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    EditorUtility.SetDirty(instance);
                    Selection.activeObject = instance;
                    EditorGUIUtility.PingObject(instance);
                    Debug.Log($"<color=green>Compass {PrefabPaths[c]}</color> has been integrated in this scene.");
                    Close();
                }
            }
        }
        GUI.enabled = true;
    }

    private static bool ValidCompass()
    {
        bl_Compass compass = GameObject.FindObjectOfType<bl_Compass>();
        bl_GameManager gm = GameObject.FindObjectOfType<bl_GameManager>();
        return compass == null && gm != null;
    }

    [MenuItem("MFPS/Addons/CompassBar/Add Compass")]
    static void Open()
    {
        GetWindow<UCompassSceneInstancer>();
    }
}
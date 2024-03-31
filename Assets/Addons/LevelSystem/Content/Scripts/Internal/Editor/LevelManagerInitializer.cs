using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MFPSEditor;
using MFPS.Addon.LevelManager;

public class LevelManagerInitializer 
{
    private const string DEFINE_KEY = "LM";

#if !LM
    [MenuItem("MFPS/Addons/LevelManager/Enable")]
    private static void Enable()
    {
       EditorUtils.SetEnabled(DEFINE_KEY, true);
    }
#endif

#if LM
    [MenuItem("MFPS/Addons/LevelManager/Disable")]
    private static void Disable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }
#endif

    [MenuItem("MFPS/Addons/LevelManager/Integrate")]
    private static void Instegrate()
    {
        if (AssetDatabase.IsValidFolder("Assets/MFPS/Scenes"))
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            GameObject inp = AssetDatabase.LoadAssetAtPath("Assets/Addons/LevelSystem/Content/Prefabs/LevelProgress [Lobby].prefab", typeof(GameObject)) as GameObject;
            if (inp != null)
            {
                if (EditorSceneManager.sceneCountInBuildSettings > 0)
                {
                    EditorSceneManager.OpenScene("Assets/MFPS/Scenes/MainMenu.unity", OpenSceneMode.Single);
                    bl_Lobby lb = Object.FindObjectOfType<bl_Lobby>();
                    if (lb != null)
                    {
                        if (inp != null)
                        {
                            GameObject pr = PrefabUtility.InstantiatePrefab(inp, EditorSceneManager.GetActiveScene()) as GameObject;
                            GameObject ccb = lb.AddonsButtons[15];
                            if (ccb != null) { pr.transform.SetParent(ccb.transform, false); }
                            EditorUtility.SetDirty(pr);

                            pr = AddonIntegrationWizard.InstancePrefab("Assets/Addons/LevelSystem/Content/Prefabs/Integration/Level Render [Identity].prefab", true);
                            pr.transform.SetParent(bl_LobbyUI.Instance.AddonsButtons[18].transform, false);
                            EditorUtility.SetDirty(pr);

                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                            Debug.Log("Level System integrate!");
                        }
                        else
                        {
                            Debug.Log("Can't found the Level Manager prefab.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Scenes has not been added in Build Settings, Can't integrate Level System Add-on.");
                }
            }
            else
            {
                Debug.Log("Can't found prefab!");
            }
        }
        else
        {
            Debug.LogWarning("Can't integrate the addons because MFPS folder structure has been change, please do the manual integration.");
        }
    }

    [MenuItem("MFPS/Addons/LevelManager/Integrate", true)]
    private static bool IntegrateValidate()
    {
        bl_Lobby km = GameObject.FindObjectOfType<bl_Lobby>();
        bl_LevelProgression gm = null;
        if(km != null)
        {
            gm = km.transform.GetComponentInChildren<bl_LevelProgression>(true);
        }
        return (km != null && gm == null);
    }

    [MenuItem("MFPS/Addons/LevelManager/Integrate RLN")]
    private static void InstegrateRLN()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/Addons/LevelSystem/Content/Prefabs/Runtime Level Notifier.prefab", typeof(GameObject)) as GameObject;
        if (prefab != null)
        {
            GameObject g = PrefabUtility.InstantiatePrefab(prefab, EditorSceneManager.GetActiveScene()) as GameObject;
            Transform gmo = bl_UIReferences.Instance.PlayerUI.transform;
            g.transform.SetParent(gmo, false);
            g.transform.SetSiblingIndex(10);

            EditorUtility.SetDirty(g);
            EditorUtility.SetDirty(gmo);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorGUIUtility.PingObject(g);
            var view = (SceneView)SceneView.sceneViews[0];
            view.ShowNotification(new GUIContent("Level Notifier integrate in this map!"));
            Debug.Log("<color=green><b>Level Notifier</b> integrate in this map!</color>");
        }
        else
        {
            Debug.Log("Can't found prefab!");
        }
    }

    [MenuItem("MFPS/Addons/LevelManager/Integrate RLN", true)]
    private static bool IntegrateValidateRNL()
    {
        bl_GameManager km = GameObject.FindObjectOfType<bl_GameManager>();
        bl_LevelChecker gm = GameObject.FindObjectOfType<bl_LevelChecker>();
        return (km != null && gm == null);
    }
}
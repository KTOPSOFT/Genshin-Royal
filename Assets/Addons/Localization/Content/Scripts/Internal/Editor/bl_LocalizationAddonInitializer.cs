using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditor;
using MFPSEditor;

public class bl_LocalizationAddonInitializer 
{
    private const string DEFINE_KEY = "LOCALIZATION";

#if !LOCALIZATION
    [MenuItem("MFPS/Addons/Localization/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, true);
    }
#endif
#if LOCALIZATION
    [MenuItem("MFPS/Addons/Localization/Disable")]
    private static void Disable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }
#endif

   // [MenuItem("MFPS/Addons/Localization/Integrate")]
    private static void Instegrate()
    {
        if (AssetDatabase.IsValidFolder("Assets/MFPS/Scenes"))
        {
            GameObject selector = AssetDatabase.LoadAssetAtPath("Assets/Addons/Localization/Content/Prefabs/UI/LanguageSelector[1.8].prefab", typeof(GameObject)) as GameObject;
            if (selector != null)
            {
                if (EditorSceneManager.GetActiveScene() == null || EditorSceneManager.GetActiveScene().name != "MainMenu")
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    string path = "Assets/MFPS/Scenes/MainMenu.unity";
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                }
                bl_Lobby lb = GameObject.FindObjectOfType<bl_Lobby>();
                if (lb != null)
                {
                    GameObject ccb = lb.AddonsButtons[6];
                    if (ccb != null)
                    {
                        GameObject inst = PrefabUtility.InstantiatePrefab(selector, EditorSceneManager.GetActiveScene()) as GameObject;
                        inst.transform.SetParent(ccb.transform, false);
                        inst.transform.SetAsLastSibling();
                        EditorUtility.SetDirty(inst);
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        Debug.Log("Localization integrate!");
                    }
                }
                else
                {
                    Debug.Log("Can't found Menu scene.");
                }
            }
            else { Debug.Log("Can't found selector prefab."); }
        }
    }

   // [MenuItem("MFPS/Addons/Localization/Integrate", true)]
    private static bool InstegrateValidate()
    {
        bl_Lobby lb = GameObject.FindObjectOfType<bl_Lobby>();
        bl_LanguageSelector ls = GameObject.FindObjectOfType<bl_LanguageSelector>();
        return lb != null && ls == null;
    }
}
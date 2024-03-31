using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;
using UnityEditor.SceneManagement;
using MFPS.Addon.PlayerSelector;

public class PlayerSelectorInitializer : MonoBehaviour
{
    private const string DEFINE_KEY = "PSELECTOR";

#if !PSELECTOR
    [MenuItem("MFPS/Addons/PlayerSelector/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, true);
    }
#endif

#if PSELECTOR
    [MenuItem("MFPS/Addons/PlayerSelector/Disable")]
    private static void Disable()
    {
        EditorUtils.SetEnabled(DEFINE_KEY, false);
    }
#endif


    [MenuItem("MFPS/Addons/PlayerSelector/InMatch Integrate")]
    private static void Instegrate()
    {
        bl_PlayerSelectorData.Instance.PlayerSelectorMode = bl_PlayerSelectorData.PSType.InMatch;
        EditorUtility.SetDirty(bl_PlayerSelectorData.Instance);
        Debug.Log("<color=green>Player Selector</color> integrated InMatch mode!");
    }

    [MenuItem("MFPS/Addons/PlayerSelector/InLobby Integrate")]
    private static void InstegrateInLobby()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/Addons/PlayerSelector/Content/Prefabs/UI/OperatorsUI.prefab", typeof(GameObject)) as GameObject;
        if (prefab != null)
        {
            GameObject g = PrefabUtility.InstantiatePrefab(prefab, UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()) as GameObject;
            bl_Lobby l = FindObjectOfType<bl_Lobby>();
            g.transform.SetParent(l.AddonsButtons[11].transform, false);
            EditorUtility.SetDirty(g);
            bl_PlayerSelectorData.Instance.PlayerSelectorMode = bl_PlayerSelectorData.PSType.InLobby;
            EditorUtility.SetDirty(bl_PlayerSelectorData.Instance);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("<color=green>Player Selector</color> integrate!");
        }
        else
        {
            Debug.Log("Can't found prefab!");
        }
    }

    [MenuItem("MFPS/Addons/PlayerSelector/InLobby Integrate", true)]
    private static bool IntegrateValidateInLobby()
    {
        bl_Lobby l = FindObjectOfType<bl_Lobby>();
        return (l != null);
    }
}
using MFPSEditor.Addons;
using MFPSEditor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class ULoginAddonIntegration : AddonIntegrationWizard
{
    private const string ADDON_NAME = "ULogin Pro";
    private const string ADDON_KEY = "ULSP";

    /// <summary>
    /// 
    /// </summary>
    public override void OnEnable()
    {
        base.OnEnable();
        addonName = ADDON_NAME;
        addonKey = ADDON_KEY;
        allSteps = 2;

        MFPSAddonsInfo addonInfo = MFPSAddonsData.Instance.Addons.Find(x => x.KeyName == addonKey);
        Dictionary<string, string> info = new Dictionary<string, string>();
        if (addonInfo != null) { info = addonInfo.GetInfoInDictionary(); }
        Initializate(info);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stepID"></param>
    public override void DrawWindow(int stepID)
    {
        if (stepID == 1)
        {
            GUILayout.Space(20);
            DrawText("<b><i><color=#B2B2B2FF>This integration only set up ULogin in the MFPS scenes, for the server setup, refer to the ULogin Documentation.</color></i></b>\n\n<color=#B2B2B2FF>First, let's enabled ULogin Pro, click on the button below if the ULogin has not been enabled yet:</color>");
            GUILayout.Space(15);
            using (new TutorialWizard.CenteredScope())
            {
#if !ULSP
                if (DrawButton("Enable ULogin"))
                {
                    EditorUtils.SetEnabled(ADDON_KEY, true);
                }
#else
                DrawText("<color=#89FF4EFF><b>ULogin Pro is enabled</b>, continue with the next step!</color>");
                GUILayout.Space(10);
                if (DrawButton("Next"))
                {
                    NextStep();
                }
#endif
            }
        }
        else if (stepID == 2)
        {
            DrawText("<size=11>The integration has to be made in the <b>MainMenu</b> scene.\n \nOpen the <b>MainMenu</b> scene run the integration.</size>");

            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.BeginVertical();
            {
                DrawText("<b><size=14>MainMenu Integration</size></b>.");
                GUILayout.Space(10);
                DrawText("<color=#939393FF><size=10><i>Open the <b>MainMenu</b> scene and then click on the <b>Integrate</b> button.</i></size></color>");
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            {
                GUILayout.Space(10);
                if (DrawButton("Open MainMenu"))
                {
                    OpenMainMenuScene();
                }

                GUI.enabled = bl_LobbyUI.Instance != null;
                if (DrawButton("Integrate"))
                {
                    if (IntegrateInMainMenu())
                    {
                        Finish();
                    }
                    else
                    {
                      //  Debug.LogWarning($"The integration did not succeed.");
                    }
                }
                GUI.enabled = true;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            DrawText("<color=#939393FF><size=9><i>If you see a <b>green log in the console</b> means that the integration succeed.\nOnce you run both integrations you are all set!</i></size></color>");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private bool IntegrateInMainMenu()
    {
        var lobbyUI = bl_LobbyUI.Instance;
        if (lobbyUI == null)
        {
            Debug.LogWarning($"The MainMenu scene is not open yet, you have to open it to run the auto integration.");
            return false;
        }

        int doneCount = 0;

        if (lobbyUI.transform.GetComponentInChildren<bl_UserProfile>(true) == null)
        {
            var prefab1 = "Assets/Addons/ULoginSystemPro/Content/Prefabs/UI/Profile [MFPS].prefab";
            var parent = lobbyUI.AddonsButtons[4].transform;

            var instance = InstancePrefab(prefab1);
            instance.transform.SetParent(parent, false);
            doneCount++;
        }
        else doneCount++;


        if (lobbyUI.transform.GetComponentInChildren<bl_RankingPro>(true) == null)
        {
            var prefab1 = "Assets/Addons/ULoginSystemPro/Content/Prefabs/UI/Ranking [MFPS1.9].prefab";
            var parent = lobbyUI.AddonsButtons[3].transform;

            var instance = InstancePrefab(prefab1);
            instance.transform.SetParent(parent, false);
            doneCount++;
        }
        else doneCount++;

        MarkSceneDirty();

        if (doneCount >= 2)
        {
            ShowSuccessIntegrationLog(lobbyUI.transform.GetComponentInChildren<bl_RankingPro>(true));
            return true;
        }
        else
        {
            return false;
        }
    }

    [MenuItem("MFPS/Addons/ULogin/Integrate")]
    private static void Integrate()
    {
        GetWindow<ULoginAddonIntegration>();
    }

#if !ULSP
    [MenuItem("MFPS/Addons/ULogin/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled(ADDON_KEY, true);
    }
#endif
#if ULSP
    [MenuItem("MFPS/Addons/ULogin/Disable")]
    private static void Disable()
    {
        EditorUtils.SetEnabled(ADDON_KEY, false);
    }
#endif
}
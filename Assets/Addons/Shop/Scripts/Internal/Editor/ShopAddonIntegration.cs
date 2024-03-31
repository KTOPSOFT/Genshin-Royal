using UnityEngine;
using UnityEditor;
using MFPSEditor;
using MFPSEditor.Addons;
using System.Collections.Generic;
using MFPS.Shop;

public class ShopAddonIntegration : AddonIntegrationWizard
{
    private const string ADDON_NAME = "Shop";
    private const string ADDON_KEY = "SHOP";

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
            DrawText("<color=#B2B2B2FF><b>This addon requires to be enabled and integrated into the MainMenu scene, you only have to click a few buttons to run the auto-integration.</b>\n \n<b>1.</b> Click the button below to enable the addon</color>");
            GUILayout.Space(15);
            using (new TutorialWizard.CenteredScope())
            {
#if !SHOP
                if (DrawButton("Enable Shop"))
                {
                    EditorUtils.SetEnabled(ADDON_KEY, true);
                }
#else
                DrawText("<color=#89FF4EFF><b>Shop is enabled</b>, continue with the next step!</color>");
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

        if (lobbyUI.transform.GetComponentInChildren<bl_ShopManager>(true) == null)
        {
            var parent = lobbyUI.AddonsButtons[12].transform;
            var instance = InstancePrefab("Assets/Addons/Shop/Prefabs/ShopUI.prefab");
            instance.transform.SetParent(parent, false);
            EditorUtility.SetDirty(instance);
            doneCount++;
        }
        else doneCount++;


        if (lobbyUI.transform.GetComponentInChildren<bl_CheckoutWindow>(true) == null)
        {
            var parent = lobbyUI.AddonsButtons[16].transform;
            var instance = InstancePrefab("Assets/Addons/Shop/Prefabs/Shop Screens.prefab");
            instance.transform.SetParent(parent, false);
            EditorUtility.SetDirty(instance);
            doneCount++;
        }
        else doneCount++;

        MarkSceneDirty();

        if (doneCount >= 2)
        {
            ShowSuccessIntegrationLog(lobbyUI.transform.GetComponentInChildren<bl_ShopManager>(true));
            return true;
        }
        else
        {
            return false;
        }
    }

#if !SHOP
    [MenuItem("MFPS/Addons/Shop/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled(ADDON_KEY, true);
    }
#endif

#if SHOP
    [MenuItem("MFPS/Addons/Shop/Disable")]
    private static void Disable()
    {
        EditorUtils.SetEnabled(ADDON_KEY, false);
    }
#endif

    [MenuItem("MFPS/Addons/Shop/Integrate")]
    public static void Open()
    {
        GetWindowWithRect<ShopAddonIntegration>(new Rect(Screen.width * 0.5f - 150, Screen.height * 0.5f - 150, 300, 500));
    }
}
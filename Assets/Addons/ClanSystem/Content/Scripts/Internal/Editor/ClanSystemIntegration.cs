using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MFPSEditor;
using MFPSEditor.Addons;
using MFPS.Addon.Clan;

public class ClanSystemIntegration : AddonIntegrationWizard
{
    private const string ADDON_NAME = "Clan System";
    private const string ADDON_KEY = "CLANS";

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

#if CLANS
        currentStep = 2;//skip the activation step.
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stepID"></param>
    public override void DrawWindow(int stepID)
    {
        if (stepID == 1)
        {
            DrawText("First, you have to <b>Enable</b> this addon, for it simply click on the button below:");
#if CLANS
            DrawText("The addons is already enabled, continue with the next step.\n");
#else
            DrawAddonActivationButton();
#endif
        }
        else if (stepID == 2)
        {
            DrawText("The clan menu has to be integrated in the <b>MainMenu</b> scene, for it open the <b>MainMenu</b> scene and click the button bellow.");
            GUILayout.Space(10);
            var lobbyUI = bl_LobbyUI.Instance;
            if(lobbyUI == null)
            {
                if (DrawOpenMainMenu())
                {
                    OpenMainMenuScene();
                }
            }
            else
            {
                var clanUI = lobbyUI.GetComponentInChildren<bl_ClanManager>(true);
                if (clanUI == null)
                {
                    if (DrawButton("Integrate"))
                    {
                        var prefab = InstancePrefab("Assets/Addons/ClanSystem/Content/Prefabs/Clan UI.prefab", false);
                        if(prefab == null)
                        {
                            return;
                        }

                        prefab.transform.SetParent(lobbyUI.AddonsButtons[14].transform, false);

                        prefab = InstancePrefab("Assets/Addons/ClanSystem/Content/Prefabs/Misc/Clan Name Text.prefab", false);
                        prefab.transform.SetParent(lobbyUI.AddonsButtons[17].transform, false);

                        ShowSuccessIntegrationLog(prefab);
                        MarkSceneDirty();
                    }
                }
                else
                {
                    //DrawText("The <b>Clan Menu</b> has been already integrated!");
                    DrawFinish();
                }
            }
        }
    }

    [MenuItem("MFPS/Addons/ClanSystem/Integrate")]
    private static void Integrate()
    {
        GetWindow<ClanSystemIntegration>();
    }

#if !CLANS
    [MenuItem("MFPS/Addons/ClanSystem/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled(ADDON_KEY, true);
    }
#endif

#if CLANS
    [MenuItem("MFPS/Addons/ClanSystem/Disable")]
    private static void Disable()
    {
        EditorUtils.SetEnabled(ADDON_KEY, false);
    }
#endif
}
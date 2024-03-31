using UnityEngine;
using UnityEditor;
using MFPSEditor;
using MFPS.Addon.Avatars;

public class EmblemsDocumentation : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    public string FolderPath = "mfps2/editor/emblems/";
    public NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-0.png", Image = null},
        new NetworkImages{Name = "img-1.png", Image = null},
        new NetworkImages{Name = "img-2.png", Image = null},
        new NetworkImages{Name = "img-3.png", Image = null},
        new NetworkImages{Name = "img-4.png", Image = null},
    };
    public Steps[] AllSteps = new Steps[] {
    new Steps { Name = "Get Started", StepsLenght = 0 , DrawFunctionName = nameof(GetStartedDoc)},
    new Steps { Name = "Add Emblems", StepsLenght = 0, DrawFunctionName = nameof(SecondSection) },
    new Steps { Name = "Add Calling Cards", StepsLenght = 0, DrawFunctionName = nameof(AddCallingCardsDoc) },
    };
    private readonly GifData[] AnimatedImages = new GifData[]
   {
        new GifData{ Path = "name.gif" },
   };

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, FolderPath, AnimatedImages);
        allowTextSuggestions = true;
    }

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }
    //final required////////////////////////////////////////////////

    void GetStartedDoc()
    {
        DrawSuperText("<?background=#CCCCCCFF>Active the addon</background>\n\nFirst, lets enabled the addon, click in the button below\n<i><size=10><color=#76767694>(After click the button wait until the compilation finish)</color></size></i>");

#if !EACC
        if (GUILayout.Button("Enable", MFPSEditorStyles.EditorSkin.customStyles[11]))
        {
            Enable();
        }

#else
        GUILayout.Label("<color=#75C52FFF><i><size=11>The addon is enabled, continue below.</size></i></color>", Style.TextStyle);
#endif
        Space(20);
        DrawHorizontalSeparator();

        DrawSuperText("<?background=#CCCCCCFF>MainMenu Integration</background>\n \n- Open the MainMenu scene and click on the button below:");
        Space(5);
        if (bl_Lobby.Instance != null)
        {
            if (MenuIntegrationVerification())
            {
                if (GUILayout.Button("MainMenu Integration", MFPSEditorStyles.EditorSkin.customStyles[11]))
                {
                    MainMenuIntegration();
                }
            }
            else
            {
                DrawText("The addon is already integrated in the MainMenu.");
            }
        }
        else
        {
            DrawText("<i>Open the MainMenu first.</i>");
        }

        DrawHorizontalSeparator();

        DrawSuperText("<?background=#CCCCCCFF>Map Integration</background>\n \n- Open your map scene <i>(you have to do this in each map scene)</i>\n- Click on the button below to run the auto integration:");
        Space(5);
        if (bl_UIReferences.Instance != null)
        {
            if (MapIntegrationVerification())
            {
                if (GUILayout.Button("Map Integration", MFPSEditorStyles.EditorSkin.customStyles[11]))
                {
                    MapIntegration();
                }
            }
            else
            {
                DrawText("The addon is already integrated in this map scene.");
            }
        }
        else
        {
            DrawText("Open a map scene to integrate.");
        }
    }

    void SecondSection()
    {
        DrawSuperText("<?background=#CCCCCCFF>ADD EMBLEM</background>\n \n▣ Duplicate one of the existing emblem prefabs located at <i>Assets ➔ Addons ➔ EmblemsAndCards ➔ Content ➔ Prefabs ➔ Emblems➔*</i>, to duplicate simply select one of them in the Project View window ➔ press <b>Ctrl + D</b> on Windows or <b>Command + D</b> on Mac.\n \n▣ Select the duplicated prefab ➔ in the inspector window assign the emblem image in the <b>Emblem</b> field and customize the properties as needed.\n \n▣ Finally, click the button <b>List this Emblem</b> that shows at the bottom of the inspector and you are done.");
        DrawServerImage("img-0.png");
    }

    void AddCallingCardsDoc()
    {
        DrawSuperText("<?background=#CCCCCCFF>ADD CALLING CARD</background>\n \nDuplicate one of the existing calling card prefabs located at <i>Assets ➔ Addons ➔ EmblemsAndCards ➔ Content ➔ Prefabs ➔ CallingCards➔*</i>, to duplicate simply select one of them in the Project View window ➔ press <b>Ctrl + D</b> on Windows or <b>Command + D</b> on Mac.\n \nSelect the duplicated prefab ➔ in the inspector window assign the card image in the <b>Card</b> field and customize the properties as needed.\n \nFinally, click the button <b>List this Card</b> that shows at the bottom of the inspector and you are done.");
        DrawServerImage("img-1.png");
    }

    [MenuItem("MFPS/Addons/Emblems/Documentation")]
    [MenuItem("MFPS/Tutorials/Emblems")]
    static void Open()
    {
        GetWindow<EmblemsDocumentation>();
    }

    [MenuItem("MFPS/Addons/Emblems/MainMenu Integration")]
    static void MainMenuIntegration()
    {
        if (bl_Lobby.Instance == null) return;

        var prefab = AddonIntegrationWizard.InstancePrefab("Assets/Addons/EmblemsAndCards/Content/Prefabs/UI/Integration/Emblems Selector.prefab");
        var parent = bl_LobbyUI.Instance.AddonsButtons[21].transform;

        prefab.transform.SetParent(parent, false);

        prefab = AddonIntegrationWizard.InstancePrefab("Assets/Addons/EmblemsAndCards/Content/Prefabs/UI/Integration/Cards Selector.prefab");
        parent = bl_LobbyUI.Instance.AddonsButtons[22].transform;

        prefab.transform.SetParent(parent, false);

        prefab = AddonIntegrationWizard.InstancePrefab("Assets/Addons/EmblemsAndCards/Content/Prefabs/UI/Integration/EmblemRender [Identity].prefab");
        parent = bl_LobbyUI.Instance.AddonsButtons[20].transform;

        prefab.transform.SetParent(parent, false);

        prefab = AddonIntegrationWizard.InstancePrefab("Assets/Addons/EmblemsAndCards/Content/Prefabs/UI/Integration/CallingCard [Identity].prefab");
        parent = bl_LobbyUI.Instance.AddonsButtons[19].transform;

        prefab.transform.SetParent(parent, false);
        MarkSceneDirty();

        AddonIntegrationWizard.ShowSuccessIntegrationLog(parent, "Emblems");
    }

    [MenuItem("MFPS/Addons/Emblems/MainMenu Integration", true)]
    static bool MenuIntegrationVerification()
    {
        if (bl_Lobby.Instance == null) return false;
        if (bl_Lobby.Instance.GetComponentInChildren<bl_EmblemSelector>(true) != null) return false;

        return true;
    }

    [MenuItem("MFPS/Addons/Emblems/Map Integration")]
    static void MapIntegration()
    {
        if (bl_UIReferences.Instance == null) return;

        var defaultKillCam = bl_UIReferences.Instance.GetComponentInChildren<bl_KillCamUI>();
        if (defaultKillCam == null)
        {
            Debug.LogWarning("Couldn't found the default kill cam.");
            return;
        }

        var prefab = AddonIntegrationWizard.InstancePrefab("Assets/Addons/EmblemsAndCards/Content/Prefabs/UI/Integration/Kill Cam [Pro].prefab");
        prefab.transform.SetParent(defaultKillCam.transform.parent, false);
        prefab.transform.SetSiblingIndex(defaultKillCam.transform.GetSiblingIndex());

        defaultKillCam.gameObject.SetActive(false);
        EditorUtility.SetDirty(defaultKillCam);

        MarkSceneDirty();

        AddonIntegrationWizard.ShowSuccessIntegrationLog(prefab, "Emblems");
    }

    [MenuItem("MFPS/Addons/Emblems/Map Integration", true)]
    static bool MapIntegrationVerification()
    {
        if (bl_UIReferences.Instance == null) return false;
        if (bl_UIReferences.Instance.GetComponentInChildren<bl_KillCamUIPro>(true) != null) return false;

        return true;
    }

#if !EACC
    [MenuItem("MFPS/Addons/Emblems/Enable")]
    private static void Enable()
    {
        EditorUtils.SetEnabled("EACC", true);
    }
#endif

#if EACC
    [MenuItem("MFPS/Addons/Emblems/Disable")]
    private static void Disable()
    {
        EditorUtils.SetEnabled("EACC", false);
    }
#endif
}
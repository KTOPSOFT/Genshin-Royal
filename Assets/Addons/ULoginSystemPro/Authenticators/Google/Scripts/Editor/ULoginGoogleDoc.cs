using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFPSEditor;
using UnityEditor;
using MFPS.ULogin;
using MFPS.ULogin.Google;

public class ULoginGoogleDoc : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "login-pro/editor/google/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-0.png", Image = null},
        new NetworkImages{Name = "img-1.png", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.png", Image = null},
        new NetworkImages{Name = "img-5.png", Image = null},
    };
    private readonly GifData[] AnimatedImages = new GifData[]
    {
        new GifData{ Path = "none.gif" },

    };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "Get Started", StepsLenght = 0, DrawFunctionName = nameof(GetStartedDoc) },
     new Steps { Name = "Upload File", StepsLenght = 0, DrawFunctionName = nameof(UploadFileDoc) },
     new Steps { Name = "Set Up", StepsLenght = 0, DrawFunctionName = nameof(SetupDoc) },
    };

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder, AnimatedImages);
        GUISkin gs = Resources.Load<GUISkin>("content/MFPSEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
            base.m_GUISkin = gs;
        }
        FetchWebTutorials("login-pro/tutorials/google/");
    }
    //final required////////////////////////////////////////////////

    void GetStartedDoc()
    {
        DrawHyperlinkText("This addon doesn't require to be enabled, you simply have to integrate it into the <link=event:os>Login</link> scene.\n \nOpen the Login scene, by default located at: <i>Assets -> Addons -> ULoginSystemPro -> Content -> Scenes -> Login</i>, with the scene opened click on the button below to run the auto-integration.", (e) =>
        {
            if (e == "os")
            {
                OpenScene("Assets/Addons/ULoginSystemPro/Content/Scenes/Login.unity");
            }
        });
        Space(10);
        using (new MFPSEditorStyles.CenteredScope())
        {
            if (Buttons.OutlineButton("Integrate", Style.blueLightColor))
            {
                Integrate();
            }
        }
        DownArrow();
        DrawText("Save the scene and you are ready to set up your Facebook app info.");
    }

    void UploadFileDoc()
    {
        DrawText("Since this addon requires ULogin Pro, you should have a web hosting at this point, and you know how to upload files to it as you did with the ULogin Pro files.\n \nYou need to upload a file in the <b>same directory where you uploaded all the ULogin Pro files</b> <i>(bl_Common.php, etc...)</i>, the file that you have to upload is located at: <i>Assets -> Addons -> ULoginSystemPro -> Authenticators -> Google -> Scripts -> Server -> <b>g-oauth.php</b></i>\n \nOnce you upload it, continue with the next step.");
    }

    public string url;
    void SetupDoc()
    {
        DrawText("This plugin uses the <b>Google OAuth 2.0</b> to access to the Web APIs, any application that uses OAuth 2.0 to access Google APIs must have authorization credentials that identify the application to Google's OAuth 2.0 server. The following steps explain how to create the credentials required to get your <b>Client ID</b>.");
        DrawHyperlinkText("If you are using any other Google API you probably already have a project created, otherwise you must create a new Project: in the dashboard click on <b>Create Project</b> button -> set the project name and click on <b>Create</b> -> once the project is created:\n\n•  Go to the <link=https://console.developers.google.com/apis/credentials>Credentials</link> page.\n\n•  Click <b>Create credentials > OAuth client ID</b>.\n\n•  If this is your first time using a Google API, you probably will be required to set up a <b>OAuth consent screen</b>, if this happens simply fill the required info, once finish, start again from the previous step.\n\n•  Select the <b>Web application</b> application type.\n\n•  Name your <b>OAuth 2.0</b> client and in the <b>Authorized redirect URIs</b> click on the <b>Add URI</b> button and paste the following the following URL:");
        if (string.IsNullOrEmpty(url))
        {
            url = bl_LoginProDataBase.Instance.PhpHostPath;
            if (!url.EndsWith("/")) url += "/";
            url += "g-oauth.php";
        }
        Space(5);
        EditorGUILayout.TextField(url);
        Space(5);
        DrawText("•  Click on <b>Create</b>\n \n\nA popup window will appear with your <b>Client ID</b> and <b>Client Secret</b>, you need copy these and paste in <i><size=8><color=#76767694>(Login scene)</color></size></i> select the <b>Google</b> game object in the hierarchy -> bl_GoogleAccountOauth -> Client ID and Client Secret respectively.");
        DrawServerImage(0);
        DownArrow();
        DrawText("with this you are all set!\n\nJust remember that when your app is about to go in production you have to publish your OAuth consent screen, you can do this by selection your project in console.developers.google.com -> OAuth consent screen -> <b>Publish App</b>.");
    }

    [MenuItem("MFPS/Addons/ULogin/Google/Integrate")]
    private static void Integrate()
    {
        var fbs = FindObjectOfType<bl_GoogleAccountOauth>();
        if (fbs != null)
        {
            Debug.Log("This addon is already integrated here.");
            return;
        }

        var fbg = new GameObject("Google");
        fbg.AddComponent<bl_GoogleAccountOauth>();

        var uui = bl_ULoginUI.Instance;
        if (uui != null)
        {
            if (uui.addonObjects[1] != null)
                uui.addonObjects[1].SetActive(true);

            var prefab = AssetDatabase.LoadAssetAtPath("Assets/Addons/ULoginSystemPro/Authenticators/Google/Prefabs/Google Button.prefab", typeof(GameObject)) as GameObject;
            var instance = PrefabUtility.InstantiatePrefab(prefab, uui.addonObjects[0].transform);
            EditorUtility.SetDirty(instance);
        }

        EditorUtility.SetDirty(fbg);
        MarkSceneDirty();
    }

    [MenuItem("MFPS/Addons/ULogin/Google/Integrate", true)]
    private static bool IntegrateVerify()
    {
        if (bl_ULoginUI.Instance == null) return false;
        var fbs = FindObjectOfType<bl_GoogleAccountOauth>();
        if (fbs != null) return false;

        return true;
    }

    [MenuItem("MFPS/Tutorials/ULogin Google")]
    private static void Open()
    {
        EditorWindow.GetWindow(typeof(ULoginGoogleDoc));
    }

    [MenuItem("MFPS/Addons/ULogin/Google/Documentation")]
    private static void Open2()
    {
        EditorWindow.GetWindow(typeof(ULoginGoogleDoc));
    }
}
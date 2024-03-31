using System.Collections.Generic;
using UnityEngine;
using MFPSEditor;
using UnityEditor;
using MFPS.ULogin.Facebook;
using MFPS.ULogin;

public class ULoginFacebookDoc : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "login-pro/editor/facebook/";
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
     new Steps { Name = "OAuth Method", StepsLenght = 0, DrawFunctionName = nameof(OAuthMethodDoc) },
     new Steps { Name = "Setup Facebook", StepsLenght = 0, DrawFunctionName = nameof(FacebookDoc) },
     new Steps { Name = "OAuth Redirect", StepsLenght = 0, DrawFunctionName = nameof(OAuthRedirectDoc) },
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
        FetchWebTutorials("login-pro/tutorials/facebook/");
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
        DrawText("Save the scene and you are ready to set up your Google API info.");
    }

    void OAuthMethodDoc()
    {
        DrawText("With this Facebook authenticator, you can use two Facebook login methods, <b>Web OAuth</b> and <b>Facebook SDK for Unity</b>, the main difference between these two is that the Web OAuth can be used in any platform, unlike the Facebook SDK that works only on iOS, Android, or Web platforms.\n \nIf you are using this for a mobile project or Web then you should use the SDK method since it uses natives implementations on these platforms.\n \nYou can define which method use by turn on/off the toggle <b>Use SDK</b> in <b>bl_FacebookAuth</b>.cs, this script is attached in the game object Facebook in the login scene:");
        DrawServerImage(0);
        DownArrow();
        DrawHyperlinkText("If you use the SDK method, you simply have to do a few extra steps:\n \n1. Download and Import the Facebook SDK for Unity from the official page: <link=https://developers.facebook.com/docs/unity/>https://developers.facebook.com/docs/unity/</link>\n \n2. Uncomment line 0 of the script bl_FacebookAuth.cs -> Open that script and remove the double slashes (//) from line 0:");
        DrawText("So instead of:");
        DrawCodeText("//#define FBSDK");
        DrawText("It should be:");
        DrawCodeText("#define FBSDK");
    }

    void FacebookDoc()
    {
        DrawHyperlinkText("This plugin uses the official Facebook API to authenticate,\nin order to use the Facebook API you need to set up a few things.\n\n<b><size=17>Before You Start</size></b>\n\nYou will need the following:\n \n■ <link=https://developers.facebook.com/docs/apps/register>A Facebook Developer Account.</link>\n\n■ A <link=https://developers.facebook.com/docs/development/create-an-app>Facebook App</link> with Basic Settings configured. This app should be in Development Mode.\n\nAfter you have these two requisites ready, you can obtain your Facebook App ID and App Secret.");
        DownArrow();
        DrawHyperlinkText("You can get your Facebook App ID and App Secret in your Facebook App dashboard, go to the <link=https://developers.facebook.com/apps/>Facebook Developer page</link> -> Open your App  -> Settings -> Basic -> There you will see the <b>App ID</b> and <b>App Secret</b> tokens.");
        DrawServerImage(1);
        DownArrow();
        DrawText("Now you need to paste these two tokens in:\n\n<b><size=14>If you are using the Facebook SDK method:</size></b>\n\n- In the Unity editor, select <b>Facebook > Edit Settings</b>.");
        DrawServerImage(2);
        DrawText("In the Inspector <b>FacebookSettings</b>, paste in your Facebook <b>App ID</b>.");
        DrawServerImage(3);
        DownArrow();
        DrawText("<b><size=14>If you are using the Web OAuth method:</size></b>\n\n- Open the Login scene.\n- Select the <b>Facebook</b> game object in the hierarchy.\n- Paste the App ID and App Secret in <b>bl_FacebookAuth</b> inspector.");
        DrawServerImage(4);
        DrawText("Finally, for the Web OAuth method, there's an extra step, continue with the following page.");
    }

    public string fbUrl;
    void OAuthRedirectDoc()
    {
        DrawText("The <b>Web OAuth</b> method requires that you have a secured server <i><size=8><color=#76767694>(with a SSL certificated)</color></size></i> to listen to the authentication responses.\n \nSince this addon <b>required ULogin Pro</b>, most likely you already have a server/hosting and you already know how to upload files to your hosting.\n \nWell, you will need to upload a new file to the same directory where you uploaded the ULogin Pro files, the new file is located in: <i>Assets -> Addons -> ULoginSystemPro -> Authenticators -> Facebook -> Scripts -> Server -> <b>fb-oauth.php</b></i>\n \nOnce you uploaded this file, copy its URL and go to your Facebook App dashboard.");
        if (string.IsNullOrEmpty(fbUrl))
        {
            fbUrl = bl_LoginProDataBase.Instance.PhpHostPath;
            if (!fbUrl.EndsWith("/")) fbUrl += "/";
            fbUrl += "fb-oauth.php";
        }
        DrawText("Based in the base URL that you set in LoginDataBasePro, the URL to the file that you just uploaded should be:");
        EditorGUILayout.TextField(fbUrl);
        DownArrow();
        DrawText("In the dashboard, go to Facebook Login -> Settings -> Valid OAuth Redirect URIs -> there you will need to paste the URL to your just uploaded file, to avoid any issue paste all the URL variants, using http, https, using and not using www, etc...\n \nAfter this, click on the <b>Save Changes</b> buttons and you're all set.");
        DrawServerImage(5);

    }

    [MenuItem("MFPS/Addons/ULogin/Facebook/Integrate")]
    private static void Integrate()
    {
        var fbs = FindObjectOfType<bl_FacebookAuth>();
        if (fbs != null)
        {
            Debug.Log("This addon is already integrated here.");
            return;
        }

        var fbg = new GameObject("Facebook");
        fbg.AddComponent<bl_FacebookAuth>();

        var uui = bl_ULoginUI.Instance;
        if (uui != null)
        {
            if (uui.addonObjects[1] != null)
                uui.addonObjects[1].SetActive(true);

            var prefab = AssetDatabase.LoadAssetAtPath("Assets/Addons/ULoginSystemPro/Authenticators/Facebook/Prefab/Facebook Button.prefab", typeof(GameObject)) as GameObject;
            var instance = PrefabUtility.InstantiatePrefab(prefab, uui.addonObjects[0].transform);
            EditorUtility.SetDirty(instance);
        }

        EditorUtility.SetDirty(fbg);
        MarkSceneDirty();
    }

    [MenuItem("MFPS/Addons/ULogin/Facebook/Integrate", true)]
    private static bool IntegrateVerify()
    {
        if (bl_ULoginUI.Instance == null) return false;
        var fbs = FindObjectOfType<bl_FacebookAuth>();
        if (fbs != null) return false;

        return true;
    }

    [MenuItem("MFPS/Tutorials/ULogin Facebook")]
    private static void Open()
    {
        EditorWindow.GetWindow(typeof(ULoginFacebookDoc));
    }

    [MenuItem("MFPS/Addons/ULogin/Facebook/Documentation")]
    private static void Open2()
    {
        EditorWindow.GetWindow(typeof(ULoginFacebookDoc));
    }
}
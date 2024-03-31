using UnityEngine;
using UnityEditor;
using MFPSEditor;

public class ClanSystemTutorial : TutorialWizard
{

    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "clan-system/editor/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-1.jpg", Image = null},
    };
    private Steps[] AllSteps = new Steps[] {
    new Steps { Name = "Get Started", StepsLenght = 0 },
    new Steps { Name = "Set Up", StepsLenght = 0 },
    new Steps { Name = "Clan Settings", StepsLenght = 0 },
    };
    //final required////////////////////////////////////////////////

    EditorWWW www = new EditorWWW();

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder);
        GUISkin gs = Resources.Load<GUISkin>("content/MFPSEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
        }
    }

    public override void WindowArea(int window)
    {
       if(window == 0)
        {
            DrawRequired();
        }else if(window == 1) { DrawSetup(); }
        else if (window == 2) { ClanSettingsDoc(); }
    }

    void DrawRequired()
    {
        DrawText("<b>NOTE:</b> Clan System require ULogin Pro add-on to work, you need have set up ULogin Pro before continue with Clan System.");
#if !ULSP
        GUILayout.Label("<color=red>ULogin Pro is not enabled</color>");
#endif
        DownArrow();
        DrawSuperText("<?background=#FFFFFF>GET STARTED</background>\n\n■ After import the package in your Unity MFPS project go to (toolbar) MFPS ➔ Addons ➔ ClanSystem ➔ <b>Enable</b>\n\n■ Open the MFPS MainMenu in the editor, then go to (toolbar) MFPS ➔ Addons ➔ ClanSystem ➔ <b>Integrate</b>\n\n■ Set up a few things in the server-side following the <b>Set Up</b> section.\n\n\nOnce all the this is done, you can open the clan window in-game in the 'Barraks' window ➔ Clan.");
    }

    void ClanSettingsDoc()
    {
        DrawText("You can find the Clan System frontend settings in the MFPS Manager window (Ctrl + M) or in the Scriptableobject located at: <i>Assets ➔ Addons ➔ ClanSystem ➔ Resources ➔ ClanSettings</i>");
    }

    void DrawSetup()
    {
        DrawText("If you are in this part that means that you have already set up ULogin System, so you already know how to upload the php scripts to your web hosting, you need to upload a few new PHP scripts in the same directory where you uploaded the 'ULogin Pro' php scripts.\n \nOpen your FTP Client (or your web hosting panel) and upload the script <b>bl_Clan.php</b> located in <i>Assets ➔ Addons ➔ ClanSystem ➔ Content ➔ Scripts ➔ Php ➔ <b>bl_Clan.php</b></i> and <b>clan-sql.sql</b>");
        DownArrow();
        DrawText("Once you uploaded them, you need create a new table in your DataBase, you can do that manually with the sql code from clan-sql.sql or automatically (clicking the button below)");
        GUI.enabled = createState == 0;
        GUILayout.Space(10);
        if(DrawButton("Create Clan Table"))
        {
            WWWForm wf = new WWWForm();
            wf.AddField("type", 5);
            www.SendRequest(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Creator), wf, CreateResult);
            createState = 1;
        }
        GUI.enabled = true;
        if(createState == 1)
        {
            GUILayout.Label("LOADING...");
        }else if(createState == 2)
        {
            DrawText("Done!, Clan table has been created successfully, you are ready to use Clan System");
        }
        else if (createState == 3)
        {
            DrawText(string.Format("A error occur: <color=red>{0}</color>",createLog));
        }
    }

    int createState = 0;
    string createLog = "";

    void CreateResult(string data, bool isError)
    {
        if(data.Contains("done"))
        {
            createState = 2;
          
        }
        else
        {
            createLog = data;
            createState = 3;
        }
        Repaint();
    }

    [MenuItem("MFPS/Addons/ClanSystem/Tutorial")]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ClanSystemTutorial));
    }

    [MenuItem("MFPS/Tutorials/Clan System")]
    private static void ShowWindowMFPS()
    {
        EditorWindow.GetWindow(typeof(ClanSystemTutorial));
    }
}
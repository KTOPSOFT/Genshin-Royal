using System.Collections;
using UnityEngine;
using UnityEditor;
using MFPSEditor;
using UnityEngine.Networking;

public class ULoginDocumentation : TutorialWizard
{

    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "login-pro/editor/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "https://www.awardspace.com/images/web_hosting_v2_04.jpg",Type = NetworkImages.ImageType.Custom},
        new NetworkImages{Name = "img-1.jpg", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.png", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.jpg", Image = null},
        new NetworkImages{Name = "img-9.jpg", Image = null},
        new NetworkImages{Name = "img-10.jpg", Image = null},
        new NetworkImages{Name = "img-11.jpg", Image = null},
        new NetworkImages{Name = "img-12.jpg", Image = null},
        new NetworkImages{Name = "img-13.png", Image = null},
        new NetworkImages{Name = "img-14.png", Image = null},
        new NetworkImages{Name = "img-15.png", Image = null},
        new NetworkImages{Name = "img-16.png", Image = null},
        new NetworkImages{Name = "img-17.png", Image = null},//16
        new NetworkImages{Name = "img-18.png", Image = null},
        new NetworkImages{Name = "img-19.png", Image = null},
        new NetworkImages{Name = "img-20.png", Image = null},
        new NetworkImages{Name = "img-21.png", Image = null},
        new NetworkImages{Name = "img-22.png", Image = null},
    };
    private readonly GifData[] AnimatedImages = new GifData[]
   {
        new GifData{ Path = "lsp-cd-1.gif" },
        new GifData{ Path = "lsp-uffz-2.gif" },
   };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "Required", StepsLenght = 0 , DrawFunctionName = nameof(DrawRequire)},
    new Steps { Name = "Hosting SetUp", StepsLenght = 3  , DrawFunctionName = nameof(DrawHosting)},
    new Steps { Name = "ULogin SetUp", StepsLenght = 4  , DrawFunctionName = nameof(DrawULogin)},
    new Steps { Name = "Admin Panel", StepsLenght = 3  , DrawFunctionName = nameof(DrawAdminPanel),
    SubStepsNames = new string[] { "Admin Panel", "Player Operations", "DataBase Health" } },
    new Steps { Name = "Email Confirmation", StepsLenght = 2  , DrawFunctionName = nameof(DrawEmailConfirmation),
    SubStepsNames = new string[] { "Email Set Up" , "SMTP Set Up" } },
    new Steps { Name = "Security", StepsLenght = 0  , DrawFunctionName = nameof(SecurityDoc)},
    new Steps { Name = "Photon Authentication", StepsLenght = 0  , DrawFunctionName = nameof(PhotonAuthDoc)},
    new Steps { Name = "Version Checking", StepsLenght = 0 , DrawFunctionName = nameof(DrawVersionChecking) },
    new Steps { Name = "Common Problems", StepsLenght = 0  , DrawFunctionName = nameof(DrawCommonProblems)},
    new Steps { Name = "Installation Service", StepsLenght = 0  , DrawFunctionName = nameof(InstallationServiceDoc)},
    };
    //final required////////////////////////////////////////////////

    EditorWWW www = new EditorWWW();
    EditorSpinnerGUI spinner;
    public bool isLoadingWWW = false;
    public HostingType hostingType = HostingType.None;

    // public ULoginFileUploader fileUploader;
    public enum HostingType
    {
        None,
        Awardspace,
        DirectAdmin,
        CPanel
    }

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder, AnimatedImages);
        GUISkin gs = Resources.Load<GUISkin>("content/MFPSEditorSkin") as GUISkin;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
        }
        FetchWebTutorials("login-pro/tuts/");
        spinner = new EditorSpinnerGUI();
        spinner.Initializated(this);
    }

    public override void WindowArea(int window)
    {
        AutoDrawWindows();
    }

    void DrawRequire()
    {
        allowTextSuggestions = true;
        DrawText("ULogin Pro System allows the players to authenticate so they can save and load their game progress and other data through sessions, to allow this, all these data are stored in an external database hosted in your own server, accessing it through the server-side REST API written in PHP and Mysql.");
        DrawText("In order to setup ULogin Pro you need:");
        DrawHorizontalColumn("Domain", "domain is the address where Internet users can access your website or hosting files, for example: <i>lovattostudio.com</i>, you can use your server IP too but is not recommended.");
        DrawHorizontalColumn("Hosting Space", "an online server to host your web files, this can be a dedicated server, shared server, VPS, etc...");
        DrawHorizontalColumn("FTP Client", "stands for File Transfer Protocol. Using an FTP client is a method to upload, download, and manage files on our server, there are some free third party program" +
            " or optionals some hosting services provide a admin panel for this.");
        DrawNote("For some platforms like iOS and Android a valid SSL Certificated is needed in order to use HTTPS.");
        DrawText("if you have or had a website, you may be familiar with these requirements, if not, you'll learn how to acquire them even for free.\n  \n<b>Although we are going to give you a free alternative for the hosting provider in this tutorial</b>, is recommended that if you can pay for a hosting/server plan, you do it, since free plans offer very limited server resources which may not be suitable for a production-ready game, plus some platform and feature required an SSL certificate for your domain which the free hostings plans doesn't count with.");

        DrawWebPanelOptions();
    }

    void DrawWebPanelOptions()
    {
        DrawText("<b><size=14>HOSTING PANEL</size></b>\n \nBefore continuing, this tutorial comes with the steps to setup correctly ULogin Pro in some specific hosting panels, if you are using a different hosting panel not listed here, don't worry, most dashboards simply have a different UI but the name of the tools/options should be the same or at least similar.\n \n<b>Select the guide for the hosting panel you will like to follow:</b>");

        var obr = EditorGUILayout.BeginHorizontal("box", GUILayout.Height(110));
        {
            GUILayout.Space(10);
            var ir = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Width(128), GUILayout.Height(64));
            GUI.DrawTexture(ir, GetServerImage("awardspace.png"));
            GUILayout.Space(10);
            GUILayout.Label("<b><size=14>Awardspace</size></b> <i>(Free)</i>\n \nFree hosting plan with a free subdomain, limited resources, good for the development phase or low-profile games.", Style.TextStyle);
            GUILayout.Space(10);
        }
        EditorGUILayout.EndHorizontal();

        if (GUI.Button(obr, GUIContent.none, GUIStyle.none))
        {
            hostingType = HostingType.Awardspace;
            NextStep();
        }

        obr = EditorGUILayout.BeginHorizontal("box", GUILayout.Height(110));
        {
            GUILayout.Space(10);
            var ir = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Width(128), GUILayout.Height(64));
            GUI.DrawTexture(ir, GetServerImage("direct-admin.png"));
            GUILayout.Space(10);
            GUILayout.Label("<b><size=14>Direct Admin</size></b>\n \nEasy to use with plans that start from $2.50 per month, SSD, fast TTFB, free unlimited SSL certificate, and independent mail server.\n \nBest starting plan.", Style.TextStyle);
            GUILayout.Space(10);
        }
        EditorGUILayout.EndHorizontal();

        if (GUI.Button(obr, GUIContent.none, GUIStyle.none))
        {
            hostingType = HostingType.DirectAdmin;
            NextStep();
        }
    }

    void DrawHosting()
    {
        if(hostingType == HostingType.None)
        {
            DrawWebPanelOptions();
            return;
        }
        if (hostingType == HostingType.Awardspace)
        {
            if (subStep == 0)
            {
                DrawText("Like I said before, you need a web hosting in order to use ULogin Pro, if you already have one and you know how to upload files to your server you can skip this step.");
                DownArrow();
                DrawText("there are a lot of web hosting services around the Internet where you can register a domain and use hosting space, the majority of these offers only paid plans, but there are someones that offer free accounts with limitations, of course, you can use these free features in your game development phase and upgrade to a better plan when you release your game.");
                DrawText("In this tutorial I will use a Hosting Provider that offers a relative good free hosting plan, it's called <b>Awardspace</b>, you can check their website here:");
                if (DrawImageAsButton(GetServerImage(0)))
                {
                    Application.OpenURL("https://www.awardspace.com/?id=AW-16015-0");
                }
                DownArrow();
                DrawText("Open the website from the above link, and then <b>create a new account</b>, once they ask you to select a plan select the free plan option or the plan that you prefer.");
                DrawText("Once you have registered your account, you should see this dashboard:");
                DrawImage(GetServerImage(1));
                DrawText("If you are on the same page, continue with the next step.");
            }
            else if (subStep == 1)
            {
                DrawText("<b><size=16>REGISTER A DOMAIN</size></b>");
                DrawText("Now you need to register a domain, for it go to <i>(in the website dashboard menu)</i> <b>Hosting Tools ➔ Domain Manager</b>:");
                DrawImage(GetServerImage(2));
                DownArrow();
                DrawText("Now if you have the a free plan you need to select <b>Register a Free Subdomain</b> or if you have a free plan but wanna have a custom domain like .com,.net,.uk, etc... <i>(Recommended)</i> you can use <b>Register a Domain</b> option and purchase it, but in this tutorial I will use only free alternatives, it is all up to you. So write your domain name in the input field like for example: <i>mygametest</i> and click on <b>Create</b>.");
                DrawImage(GetServerImage(3));
                DrawText("If the domain is available to register, you will see other steps to complete the registration, so just follow them until the registration is complete, just be sure to not select any paid feature that they may offer <i>(unless you want it)</i>.");
                DownArrow();
                DrawText("Now that you have the hosting and domain name ready you can upload files to your server space, but more on that later.");
            }
            else if (subStep == 2)
            {
                DrawText("<b><size=16>CREATE DATABASE</size></b>");
                DrawText("Now you have to create a database instance and the database tables to store the users and game data.");
                DrawText("Go to <b>Hosting Tools ➔ MySQL DataBases</b>, in the loaded page you will see a form to create the database, simply set the database name and a password, for the name you can simply set something like \"game\" and a secure password, and then click on the <b>Create DataBase</b> button");
                DrawImage(GetServerImage(5));
                DownArrow();
                DrawText("Good, now you have your web hosting and database ready!, continue with the next step to set up ULogin");
            }
        }
        else if (hostingType == HostingType.DirectAdmin)
        {
            if (subStep == 0)
            {
                DrawText("Like I said before, you need a web hosting in order to use ULogin Pro, if you already have one and you know how to upload files to your server you can skip this step.");
                DownArrow();
                DrawText("For this guide, we are going to use a paid hosting plan that offers all we need to set up ULogin Pro, this hosting plan comes with <b>DirectAdmin</b> hosting panel which we are going to use in this tutorial for reference, keep in mind that UI could be look different from the images here if the panel gets update but you should be able to find the options with the same name anyway.");

                DrawText("To acquire this hosting plan for a cheap price, we can use the starter plan from <b>InterServer</b> here:");
                Space(10);
                using (new CenteredScope())
                {
                    if (GUILayout.Button("InterServer Hosting Plan", MFPSEditorStyles.EditorSkin.customStyles[11], GUILayout.Height(30), GUILayout.Width(200)))
                    {
                        LovattoStats.SetStat("interserver-aff", 1);
                        Application.OpenURL("https://www.interserver.net/r/570746");
                    }
                }

                DownArrow();
                DrawText("Open the hosting website from the above link, and then purchase the started plan or the plan you prefer.\n \nIn the purchase process, you may be asked to register a domain name, <b>this is optional and only needed if you want to pay for a custom domain name or use SSL in your domain, otherwise the free subdomain</b> included in the plan is good to go, so if you don't want to pay for a custom domain simply type anything in the domain name input field and select <b>No</b> in the register toggle.\n \nOnce you purchase the plan and receive the confirmation by email you can login into your hosting panel by <i>(in the InterServer site)</i> going to web hosting tab > find your purchase hosting in the list > next to it <i>(at the far right)</i> you should see a small \"setting\" button, click on it and you should see something like this:");
                DrawServerImage("img-27.png");
                DrawText("Click on the <b>Login to DirectAdmin</b> button and you will be redirected to the hosting panel that should look similar to this");
                DrawServerImage("img-28.png");
                DrawText("If you are on the same page, continue with the next step.");
            }
            else if (subStep == 1)
            {
                DrawText("<b><size=16>Active SSL</size></b>\n \n<b><i>This is can be only be done if you purchased a custom domain name along with the plan.</i></b>\n \nAs mentioned before some platforms and features require an SSL certificate for secure connections over HTTPS, you can active this certificate for your custom domain by selecting the <b>SSL Certificates</b> option in the DirectAdmin panel > select the <b>Get automatic certificate from ACME Provider</b> option in the tab menu > leave the default values <i>(unless you want to change something and you know what are you doing)</i> > click on the <b>Save</b> button.");
            }
            else if (subStep == 2)
            {
                DrawText("<b><size=16>CREATE DATABASE</size></b>");
                DrawText("Now you have to create a database instance and the database tables to store the users and game data.");
                DrawText("For it, in the DirectAdmin panel go to the <b>MySQL Management</b> option");
                DrawServerImage("img-29.png", TextAlignment.Center);
                DrawText("This will redirect you to a page where you will see the button <b>CREATE NEW DATABASE</b> in the top far right of page, click on it.");
                DrawServerImage("img-30.png", TextAlignment.Center);
                DrawText("On the new page, you will have a few fields where you have to set a name for the database, it can be anything you want, e.g <b><i>gamedb</i></b>, you can also set the same name for the <b>Database User</b> field or you can change it for a different one.\n \nAfter this, you should see another field to set a password for the database, set your password, then click on the <b>CREATE DATABASE</b> button");
                DrawServerImage("img-31.png");
                DrawText("If the database is created, you will see a message like this:");
                DrawServerImage("img-32.png");
                DrawText("<b>Copy that information since you will need it later</b>, <i>you can select it just like in the screenshot above and then copy the text and paste it into any Notepad for later.</i>");            
                DownArrow();
                DrawText("Good, now you have your web hosting and database ready!, continue with the next step to set up ULogin");
            }
        }
    }

    int checkID = 0;
    public string unformatedURL = "";
    public string formatedURL = "";
    public string domainStr = "";
    void DrawULogin()
    {
        if (hostingType == HostingType.Awardspace)
        {
            if (subStep == 0)
            {
                DrawText("Now that you have all the necessary, it's time to set up the ULogin Pro files.\n \nThe first thing that you need to do is upload the PHP scripts in a directory of your web hosting space, These PHP scripts allow the communication from the game client to your server, the server-side scripts handle all the Database communication with the information received from the client.\n \nBut before uploading the scripts you have to set up some information regarding your database instance and your game in one of the PHP scripts.");
                DrawText("<b>Open</b> the script called <b>bl_Common.php</b> from the ULogin Folder located at <i>Assets ➔ Addons ➔ ULoginSystemPro ➔ Content ➔ Scripts ➔ Php➔*</i>, you can open it with any Text Editor program.");
                DrawServerImage("img-8.png");
                DownArrow();
                DrawText("In that script, you need to set your database credentials that is used to establish a connection with the database.\n \nTo obtain that information depends on where the database is hosted, if you are using Awardspace you can find that information in <b>Hosting Tools ➔ MySQL DataBase</b> ➔ in the new page click on the button under <b>Options</b> ➔ Information ➔ database information will display like this:");
                DrawServerImage("img-9.png");
                DownArrow();
                DrawText("Now in <b>bl_Common.php</b> you need set the database information like this");
                DrawHorizontalColumn("HOST_NAME", "The name of your host, get it from the database info in the hosting page");
                DrawHorizontalColumn("DATA_BASE_NAME", "The name the database, get it from the database info in the hosting page");
                DrawHorizontalColumn("DATA_BASE_USER", "The user name of the database, in Awardspace this is the same as the database name");
                DrawHorizontalColumn("DATA_BASE_PASSWORLD", "The password that you set when create the database");
                DrawHorizontalColumn("SECRET_KEY", "Set a custom 'password' key, that just work as a extra layer of security to prevent others can execute the php code, is <b>Highly recommended that you set your own secret key</b>, just make sure to remember it since you will need it in the next step.");
                DrawHorizontalColumn("ADMIN_EMAIL", "Your server email from where 'Register confirmation' will be send (Only require if you want use confirmation for register), <b>NOTE:</b> this email need to be configure in your hosting, in Awardspace you can create a " +
                    "email account in <b>Hosting Tools -> E-mail Account</b>.");
                DrawText("So after you set all the info, your script should look like this <i>(with your own database credentials of course)</i>");
                DrawImage(GetServerImage(21));
                DrawText("Don't forget to save the changes in the script.");
            }
            else if (subStep == 1)
            {
                DrawText("Now the next step is upload the server side scripts (.php, .sql and .xml) to your hosting space directory.\n\nLets explain first in what consist a directory first, very short, let say your registered domain is <color=#79D3FFFF>myexampledomain.com</color>, that is the root of your domain address/URL, now you can create subfolders in that domain to extend the address, so if you create a folder called <i>phpfiles</i> then the address to point to that folder will be <color=#79D3FFFF>myexampldomain.com/phpfiles/</color> if you create another folder inside this you simple add the folder name plus a right-slash at the end.");

                DrawHyperlinkText("Now to manage your folders/domain directory and upload files to it you need an FTP Client, since some web hosting file uploaders don't support nested folders uploads we have to use <b>FileZilla FTP Client</b> to upload our files to our server, FileZilla is free software, you can download it here: <link=https://filezilla-project.org/>https://filezilla-project.org/</link> or use your preferred FTP Client program.\n\n<b>Download the FTP Client</b> not the FTP Server.\n\nOnce you download it -> install it following the installation wizard of the program -> then <b>Open</b> it.\n\nNow to be able to upload the files using this tools you need to set your FTP Credentials, all web hostings provide these credentials in their panel/dashboard, in Awardspace you can find these credentials in: <b>Hosting Tools -> FTP Manager</b>");
                DrawServerImage(16);
                DownArrow();
                DrawText("There you will see a form to create a FTP account, fill the required info and click on <b>Create FTP Account</b> button");
                DrawServerImage(17);
                DownArrow();
                DrawText("After this below that box you'll see the FTP Account section, there you should see the FTP account that you just create, click on the <i><b>settings</b></i> button on the right side of your FTP account -> then select the <b>Information</b> tab, there you will see your FTP account credentials with which you can access to your server:");
                DrawServerImage(18);
                DrawText("Now open FileZilla and at the top of the menu you will see the fields to insert your FTP credentials, insert the credentials showing in your hosting, in Awardspace, they are the ones that you just open above, once you insert the credentials click on the <b>Quickconnect</b> button, if the credentials are correct you should see a message like <i><b>Directory listing of \"/\" successful</b></i>, if that's so then you are ready to upload the files.");
                DrawServerImage(19);
                DownArrow();
                DrawText("So now on the FileZilla in the <b>Remote Site side panel</b> you should see a directory folder with at least a folder in it, open the folder <b>that has your domain name</b> -> now with the root directory open you have two options: upload the scripts files right there in the root or created a sub directory to be more organized, I recommend create a subfolder , for it simple right click on the window -> Create directory -> in the popup window that will appear set the folder name as you want, for this tutorial I will create two nested folders: <b>mygame -> php</b> <i>where I'll upload my files)</i>");
                DrawAnimatedImage(0);
                DrawText("With the created folder open you have to upload the ULogin Pro server files, for it in the Unity Editor go to the folder located at: <i><b>Assets -> Addons -> ULoginSystemPro -> Content -> Scripts-> Php</b></i> select one of the script on that folder -> Right Mouse Click over it -> Show on Explorer.");
                DrawServerImage(15, TextAlignment.Center);
                DrawText("Now in the OS window explorer <b>select all the files including the phpseclib folder</b> <i><size=10><color=#76767694>(without the .meta files)</color></size></i> and drag them in the <b>Remote Site</b> panel on <b>FileZilla</b>");
                DrawAnimatedImage(1);
                DrawNote("Since version 2.0.0 make sure to also upload the <b>phpmailer and templates</b> folder.");
                DownArrow();
                DrawText("Once all scripts are uploaded, copy the URL from the <b>Remote Site</b> directory field <i>(on FileZilla)</i>");
                DrawServerImage(20);
                DrawText("This is just the directory to your files folder container, you have to format it to a working URL  by adding the <i>HTTP</i> or <i>HTTPS</i> prefix, for it simply paste in the field below the URL that you copy from the FTP Client and then click on the <b>Format</b> button, after this check that URL is correct and click on the <b>Assign URL</b> button.");

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(100);
                    if (string.IsNullOrEmpty(formatedURL))
                    {
                        unformatedURL = EditorGUILayout.TextField(unformatedURL);
                        GUI.enabled = !string.IsNullOrEmpty(unformatedURL);
                        if (GUILayout.Button("Format", EditorStyles.toolbarButton, GUILayout.Width(100)))
                        {
                            string url = unformatedURL;
                            if (url.StartsWith("/"))
                            {
                                url = url.Remove(0, 1);
                                url = $"http://www.{url}";
                            }
                            else if (!url.StartsWith("http"))
                            {
                                url = $"http://www.{url}";
                            }

                            if (!url.EndsWith("/"))
                            {
                                url += "/";
                            }
                            formatedURL = url;
                            GUI.FocusControl(null);
                            Repaint();
                        }
                    }
                    else
                    {
                        GUILayout.Label(EditorGUIUtility.IconContent("Collab").image, GUILayout.Width(20));
                        formatedURL = EditorGUILayout.TextField(formatedURL);
                        GUI.enabled = !string.IsNullOrEmpty(formatedURL);
                        if (GUILayout.Button("Assign URL", EditorStyles.toolbarButton, GUILayout.Width(100)))
                        {
                            bl_LoginProDataBase.Instance.PhpHostPath = formatedURL;
                            EditorUtility.SetDirty(bl_LoginProDataBase.Instance);
                            Selection.activeObject = bl_LoginProDataBase.Instance;
                            EditorGUIUtility.PingObject(bl_LoginProDataBase.Instance);
                        }
                    }
                    GUI.enabled = true;
                    GUILayout.Space(100);
                }
                GUILayout.EndHorizontal();
                DrawNote("If you are using a free domain, you may need to remove the <b>www.</b> from the URL for it to work.");
                DownArrow();
                DrawHyperlinkText("Now in the Inspector Window of the Unity Editor you should see the fields of <link=asset:Assets/Addons/ULoginSystemPro/Resources/LoginDataBasePro.asset>LoginDataBasePro</link> <i>(otherwise click the button below)</i>, if everything work correctly you should see your URL assigned in the <b>PhpHostPath</b> field.\n\n-Remember from the last step the <b>SecretKey</b> that was in <b>bl_Common.php -> SECRET_KEY</b>, if you have change it in <b>bl_Common.php</b> you have to set the same Key in the field <b>Secret Key</b> of <link=asset:Assets/Addons/ULoginSystemPro/Resources/LoginDataBasePro.asset>LoginDataBasePro</link>.");
                DrawImage(GetServerImage(11));
                if (DrawButton("LoginDataBasePro"))
                {
                    Selection.activeObject = bl_LoginProDataBase.Instance;
                    EditorGUIUtility.PingObject(bl_LoginProDataBase.Instance);
                }
            }
            else if (subStep == 2)
            {
                DrawText("Now you need set up the tables in your database, for this we'll use SQL, you can do that manually or automatically," +
                    "for do it manually you can use some database tool like PhpMyAdmin and run the SQL query in their sql panel or you can do it here.");
                DownArrow();
                DrawText("First let's check that the tables have not been created yet, click on the button below to check the tables.");
                GUILayout.Space(5);
                bool isLURL = bl_LoginProDataBase.Instance.PhpHostPath.ToString().Contains("lovatto");
                if (isLURL)
                {
                    DrawNote("<color=#CA2525FF>You're still using the lovatto studio URL which is for demonstration only, please use your own URL to continue.</color>");
                }
                GUI.enabled = (!isLoadingWWW && !isLURL);
                Space(10);
                using (new CenteredScope())
                {
                    if (GUILayout.Button("Verify Database Status", MFPSEditorStyles.EditorSkin.customStyles[11], GUILayout.Height(30), GUILayout.Width(200)))
                    {
                        if (spinner != null) spinner.SetActive(true);
                        isLoadingWWW = true;
                        EditorCoroutines.StartBackgroundTask(Check());
                        IEnumerator Check()
                        {
                            WWWForm wf = new WWWForm();
                            wf.AddField("type", 3);

                            using (var w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Creator), wf))
                            {
                                checkID = 1;
                                w.SendWebRequest();

                                while (!w.isDone) yield return null;

                                isLoadingWWW = false;
                                if (spinner != null) spinner.SetActive(false);

                                int code = (int)w.responseCode;

                                if (bl_DataBaseUtils.IsNetworkError(w))
                                {
                                    checkID = 6;
                                    Debug.LogError(w.error);

                                    if (code == 404)
                                    {
                                        checkLog = "<color=#CA2525FF>Error: 404 Not Found, make sure you have uploaded the PHP scripts and that the URL you defined in LoginDataBasePro > PhpHostPath is correct.</color>";
                                    }
                                    else
                                    {
                                        checkLog = "<color=#CA2525FF>Unexpected error with the request, check the console for more information.</color>";
                                    }

                                    yield break;
                                }

                                string result = w.downloadHandler.text;
                                Debug.Log($"({code}): {result}");

                                if (code == 201)
                                {
                                    checkID = 2;
                                    checkLog = "<color=#4DAD23FF>Database tables are already created!</color>";
                                }
                                else if (code == 204)
                                {
                                    checkID = 3;
                                    checkLog = "<color=#B99429FF>Database tables are not created yet.</color>";
                                }
                                else if (code == 404)
                                {
                                    checkLog = "<color=#CA2525FF>Error: 404 Not Found, make sure you have uploaded the PHP scripts and that the URL you defined in LoginDataBasePro > PhpHostPath is correct.</color>";
                                    checkID = 6;
                                }
                                else
                                {
                                    checkID = 3;
                                    checkLog = $"<color=#B99429FF>Unexpected response: {result}</color>";
                                }
                            }
                        }
                    }
                }

                using (new CenteredScope())
                {
                    GUI.enabled = true;
                    if (checkID == 1)
                    {
                        GUILayout.Label("LOADING...", Style.TextStyle);
                    }
                    else if (checkID == 2)
                    {
                        GUILayout.Label(checkLog, Style.TextStyle);
                    }
                    else if (checkID >= 3)
                    {
                        GUI.enabled = (!isLoadingWWW);
                        DrawText("The tables have not been created yet, click on the button below to create the tables automatically.");
                        GUILayout.Space(5);
                        if (GUILayout.Button("Setup Database", MFPSEditorStyles.EditorSkin.customStyles[11]))
                        {
                            EditorCoroutines.StartBackgroundTask(DoCreateTables());
                            checkID = 4;
                        }
                        GUI.enabled = true;
                        if (checkID == 4) { GUILayout.Label("LOADING..."); }
                        if (checkID == 5)
                        {
                            DrawText("<color=#4DAD23FF>Done!, Tables has been created successfully, continue with the next step.</color>");
                        }
                        else if (checkID == 6)
                        {
                            DrawText("<color=#BE1818FF>Couldn't create the tables in the database, check the console for more information regarding the error.</color>");
                        }
                    }

                    if (spinner != null && isLoadingWWW)
                    {

                        spinner.DrawSpinner();
                    }
                }
                Repaint();
            }
            else if (subStep == 3)
            {
                DrawText("You are all set! you have set up your database and are ready to use ULogin System!");
                DownArrow();
                DrawText("So let me explain some the options in <b>LoginDataBasePro</b>");
                DrawHorizontalColumn("Update IP", "ULogin collect the IP of the register user in order to block their IP if they get banned in game, if you enable this ULogin will check the IP each time" +
                    " that the user log-in so always keep the store IP update");
                DrawHorizontalColumn("Detect Ban", "Basically is = to say: you wanna use Ban Features.");
                DrawHorizontalColumn("Require Email Verification", "After register a account user is required to verify his email in order to log-in?");
                DrawHorizontalColumn("Check Ban In Mid", "Check if the player has not been banned each certain amount of time, if this is false that check will only be fire when player log-in");
                DrawHorizontalColumn("Can Register Same Email", "Can an email be use to register different accounts?");
            }
        }
        else if (hostingType == HostingType.DirectAdmin)
        {
            if (subStep == 0)
            {
                DrawText("Now that you have all the necessary, it's time to set up the ULogin Pro files.\n \nThe first thing that you need to do is upload the PHP scripts in a directory of your web hosting space, These PHP scripts allow the communication from the game client to your server, the server-side scripts handle all the Database communication with the information received from the client.\n \nBut before uploading the scripts you have to set up some information regarding your database instance and your game in one of the PHP scripts.");
                DrawText("<b>Open</b> the script called <b>bl_Common.php</b> from the ULogin Folder located at <i>Assets ➔ Addons ➔ ULoginSystemPro ➔ Content ➔ Scripts ➔ Php➔*</i>, you can open it with any Text Editor program.");
                DrawServerImage("img-8.png");
                DownArrow();
                DrawText("In that script, you need to set your database credential that is used to establish a connection with the database.\n  \nThat information is the data you just copy after creating the database from the previous step, which const of:");
                DrawServerImage("img-32.png");
                DownArrow();
                DrawText("Now in <b>bl_Common.php</b> you need set the database information like this");
                DrawHorizontalColumn("HOST_NAME", "The name of your host, get it from the database info in the hosting page");
                DrawHorizontalColumn("DATA_BASE_NAME", "The name the database, get it from the database info in the hosting page");
                DrawHorizontalColumn("DATA_BASE_USER", "The user name for the connection authentication database");
                DrawHorizontalColumn("DATA_BASE_PASSWORLD", "The password that you set when create the database");
                DrawHorizontalColumn("SECRET_KEY", "Set a custom 'password' key, that just work as a extra layer of security to prevent others can execute the php code, is <b>Highly recommended that you set your own secret key</b>, just make sure to remember it since you will need it in the next step.");
                DrawHorizontalColumn("ADMIN_EMAIL", "Your server email from where 'Register confirmation' will be send (Only require if you want use confirmation for register), <b>NOTE:</b> this email need to be configure in your hosting first, check the EMAIL CONFIRMATION section for this.");
                DrawText("So after you set all the info, your script should look like this <i>(with your own database credentials of course)</i>");
                DrawImage(GetServerImage(21));
                DrawText("Don't forget to save the changes in the script.");
            }
            else if (subStep == 1)
            {
                DrawText("Now the next step is upload the server side scripts (.php, .sql and .xml) to your hosting space directory.\n\nLets explain first in what consist a directory first, very short, let say your registered domain is <color=#79D3FFFF>myexampledomain.com</color>, that is the root of your domain address/URL, now you can create subfolders in that domain to extend the address, so if you create a folder called <i>phpfiles</i> then the address to point to that folder will be <color=#79D3FFFF>myexampldomain.com/phpfiles/</color> if you create another folder inside this you simple add the folder name plus a right-slash at the end.");

                DrawHyperlinkText("Now to manage your folders/domain directory and upload files to it you need an FTP Client, since some web hosting file uploaders don't support nested folders uploads we have to use <b>FileZilla FTP Client</b> to upload our files to our server, FileZilla is free software, you can download it here: <link=https://filezilla-project.org/>https://filezilla-project.org/</link> or use your preferred FTP Client program.\n\n<b>Download the FTP Client</b> not the FTP Server.\n\nOnce you download it -> install it following the installation wizard of the program -> then <b>Open</b> it.\n\nNow to be able to upload the files using this tools you need to set your FTP Credentials, all web hostings provide these credentials in their panel/dashboard, in DirectAdmin you can find or create these credentials in the<b>FTP Management</b> option");
                DrawServerImage("img-33.png");
                DownArrow();
                DrawText("After clicking it you will be redirected to a page where you should see the button <b>CREATE FTP ACCOUNT</b> > click it > in the new page set a username and a password and respective fields then click on the <b>CREATE</b> button.");
                DrawServerImage("img-34.png");
                DownArrow();
                DrawText("If the FTP account gets created a popup should be prompt with your account username and password.\n \nNow open FileZilla and at the top of the menu you will see the fields to insert your FTP credentials, in the <b>Username</b> and <b>Password</b> fields set the values from your just created FTP account, and in the <b>Host</b> field paste your account username again <b>but remove the account name and the '@'</b> so just keep the domain name, <b>e.g</b> <i>if the username is ftpdev@wh1010101.ispot.cc just leave wh1010101.ispot.cc</i>, then in the <b>Port</b> field set <b>21</b>, then Click on the <b>Quickconnect</b> button.\n\nIf the credentials are correct you should see a message like <i><b>Directory listing of \"/\" successful</b></i>, if that's so then you are ready to upload the files.");
                DrawServerImage("img-35.png");
                DownArrow();
                DrawText("So now on the FileZilla in the <b>Remote Site side panel</b> you should see a directory folder with some folders, open the <b>public_html</b> folder.\n\nNow you have two options: upload the scripts files right there in the root of your domain or created a sub directory to be more organized, I recommend creating at least subfolder, for it simply right click on the window ➔ <b>Create directory</b> ➔ in the popup window that will appear set the folder name as you want, for this tutorial I will create two nested folders: mygame ➔ php <i>(where I'll upload my files)</i>");
                DrawAnimatedImage(0);
                DrawText("With the created folder open you have to upload the ULogin Pro server files, for it in the Unity Editor go to the folder located at: <i><b>Assets -> Addons -> ULoginSystemPro -> Content -> Scripts-> Php</b></i> select one of the script on that folder -> Right Mouse Click over it -> Show on Explorer.");
                DrawServerImage(15, TextAlignment.Center);
                DrawText("Now in the OS window explorer <b>select all the files including the phpseclib folder</b> <i><size=10><color=#76767694>(without the .meta files)</color></size></i> and drag them in the <b>Remote Site</b> panel on <b>FileZilla</b>");
                DrawAnimatedImage(1);
                DrawNote("Since version 2.0.0 make sure to also upload the <b>phpmailer and templates</b> folder.");
                DownArrow();
                DrawText("Once all scripts are uploaded, copy the URL from the <b>Remote Site</b> directory field <i>(on FileZilla)</i>");
                DrawServerImage(20);
                DrawText("This is just the directory to your files folder container, paste it in the <b>Directory</b> field below, then add your domain name in the <b>Domain</b> field.\n \nIf you don't know which is your domain name, in the DirectAdmin panel select the <b>Domain Setup</b> option:");
                DrawServerImage("img-36.png");
                DrawText("On the new page, you should see two or more domains in the list, one is the domain you specified when purchased the hosting plan, this is the domain you should <b>ONLY use it if you purchased the domain name</b>, keep in mind that the domain is not included in the same payment of the hosting plan, so if you didn't pay for the domain separately, you have to use the free domain included with your plan, which should be the second domain in the list.");
                DrawServerImage("img-37.png");
                DrawText("Copy your domain and then paste it below in the <b>Domain</b> field, then click on the <b>Format</b> button to generate the correct URL.");
                Space(10);
                EditorGUILayout.BeginVertical("box");
                if (string.IsNullOrEmpty(formatedURL))
                {
                    GUILayout.Label("Directory <i>e.g: /public_html/mygame/php</i>", Style.TextStyle);
                    unformatedURL = EditorGUILayout.TextField(unformatedURL);
                    Space(18);
                    GUILayout.Label("Domain <i>e.g: mydomain.com or wh1082556.ispot.cc</i>", Style.TextStyle);
                    domainStr = EditorGUILayout.TextField(domainStr);

                    GUI.enabled = !string.IsNullOrEmpty(unformatedURL) && !string.IsNullOrEmpty(domainStr);
                    if (GUILayout.Button("Format", EditorStyles.toolbarButton, GUILayout.Width(100)))
                    {
                        string url = domainStr;
                        if (url.StartsWith("/"))
                        {
                            url = url.Remove(0, 1);
                            url = $"http://www.{url}";
                        }
                        else if (!url.StartsWith("http"))
                        {
                            url = $"http://www.{url}";
                        }

                        string dir = unformatedURL;
                        if (dir.StartsWith("/"))
                        {
                            dir = dir.Remove(0, 1);
                        }
                        if (dir.Contains("public_html/"))
                        {
                            dir = dir.Replace("public_html/", "");
                        }

                        if (!url.EndsWith("/"))
                        {
                            url += "/";
                        }

                        url = $"{url}{dir}";

                        if (!url.EndsWith("/"))
                        {
                            url += "/";
                        }

                        formatedURL = url;
                        GUI.FocusControl(null);
                        Repaint();
                    }
                }
                else
                {
                    GUILayout.Label(EditorGUIUtility.IconContent("Collab").image, GUILayout.Width(20));
                    formatedURL = EditorGUILayout.TextField(formatedURL);
                    GUI.enabled = !string.IsNullOrEmpty(formatedURL);
                    if (GUILayout.Button("Assign URL", EditorStyles.toolbarButton, GUILayout.Width(100)))
                    {
                        bl_LoginProDataBase.Instance.PhpHostPath = formatedURL;
                        EditorUtility.SetDirty(bl_LoginProDataBase.Instance);
                        Selection.activeObject = bl_LoginProDataBase.Instance;
                        EditorGUIUtility.PingObject(bl_LoginProDataBase.Instance);
                    }
                }
                GUI.enabled = true;
                EditorGUILayout.EndVertical();
                DrawNote("If you are using a free domain, you may need to remove the <b>www.</b> from the URL for it to work.");
                DownArrow();
                DrawHyperlinkText("Now in the Inspector Window of the Unity Editor you should see the fields of <link=asset:Assets/Addons/ULoginSystemPro/Resources/LoginDataBasePro.asset>LoginDataBasePro</link> <i>(otherwise click the button below)</i>, if everything work correctly you should see your URL assigned in the <b>PhpHostPath</b> field.\n\n-Remember from the last step the <b>SecretKey</b> that was in <b>bl_Common.php -> SECRET_KEY</b>, if you have change it in <b>bl_Common.php</b> you have to set the same Key in the field <b>Secret Key</b> of <link=asset:Assets/Addons/ULoginSystemPro/Resources/LoginDataBasePro.asset>LoginDataBasePro</link>.");
                DrawImage(GetServerImage(11));
                if (DrawButton("LoginDataBasePro"))
                {
                    Selection.activeObject = bl_LoginProDataBase.Instance;
                    EditorGUIUtility.PingObject(bl_LoginProDataBase.Instance);
                }
            }
            else if (subStep == 2)
            {
                DrawText("Now you need set up the tables in your database, for this we'll use SQL, you can do that manually or automatically," +
                    "for do it manually you can use some database tool like PhpMyAdmin and run the SQL query in their sql panel or you can do it here.");
                DownArrow();
                DrawText("First let's check that the tables have not been created yet, click on the button below to check the tables.");
                GUILayout.Space(5);
                bool isLURL = bl_LoginProDataBase.Instance.PhpHostPath.ToString().Contains("lovatto");
                if (isLURL)
                {
                    DrawNote("<color=#CA2525FF>You're still using the lovatto studio URL which is for demonstration only, please use your own URL to continue.</color>");
                }
                GUI.enabled = (!isLoadingWWW && !isLURL);
                Space(10);
                using (new CenteredScope())
                {
                    if (GUILayout.Button("Verify Database Status", MFPSEditorStyles.EditorSkin.customStyles[11], GUILayout.Height(30), GUILayout.Width(200)))
                    {
                        if (spinner != null) spinner.SetActive(true);
                        isLoadingWWW = true;
                        EditorCoroutines.StartBackgroundTask(Check());
                        IEnumerator Check()
                        {
                            WWWForm wf = new WWWForm();
                            wf.AddField("type", 3);

                            using (var w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Creator), wf))
                            {
                                checkID = 1;
                                w.SendWebRequest();

                                while (!w.isDone) yield return null;

                                isLoadingWWW = false;
                                if (spinner != null) spinner.SetActive(false);

                                int code = (int)w.responseCode;

                                if (bl_DataBaseUtils.IsNetworkError(w))
                                {
                                    checkID = 6;
                                    Debug.LogError(w.error);

                                    if (code == 404)
                                    {
                                        checkLog = "<color=#CA2525FF>Error: 404 Not Found, make sure you have uploaded the PHP scripts and that the URL you defined in LoginDataBasePro > PhpHostPath is correct.</color>";
                                    }
                                    else
                                    {
                                        checkLog = "<color=#CA2525FF>Unexpected error with the request, check the console for more information.</color>";
                                    }

                                    yield break;
                                }

                                string result = w.downloadHandler.text;
                                Debug.Log($"({code}): {result}");

                                if (code == 201)
                                {
                                    checkID = 2;
                                    checkLog = "<color=#4DAD23FF>Database tables are already created!</color>";
                                }
                                else if (code == 204)
                                {
                                    checkID = 3;
                                    checkLog = "<color=#B99429FF>Database tables are not created yet.</color>";
                                }
                                else if (code == 404)
                                {
                                    checkLog = "<color=#CA2525FF>Error: 404 Not Found, make sure you have uploaded the PHP scripts and that the URL you defined in LoginDataBasePro > PhpHostPath is correct.</color>";
                                    checkID = 6;
                                }
                                else
                                {
                                    checkID = 3;
                                    checkLog = $"<color=#B99429FF>Unexpected response: {result}</color>";
                                }
                            }
                        }
                    }
                }

                using (new CenteredScope())
                {
                    GUI.enabled = true;
                    if (checkID == 1)
                    {
                        GUILayout.Label("LOADING...", Style.TextStyle);
                    }
                    else if (checkID == 2)
                    {
                        GUILayout.Label(checkLog, Style.TextStyle);
                    }
                    else if (checkID >= 3)
                    {
                        GUI.enabled = (!isLoadingWWW);
                        DrawText("The tables have not been created yet, click on the button below to create the tables automatically.");
                        GUILayout.Space(5);
                        if (GUILayout.Button("Setup Database", MFPSEditorStyles.EditorSkin.customStyles[11]))
                        {
                            EditorCoroutines.StartBackgroundTask(DoCreateTables());
                            checkID = 4;
                        }
                        GUI.enabled = true;
                        if (checkID == 4) { GUILayout.Label("LOADING..."); }
                        if (checkID == 5)
                        {
                            DrawText("<color=#4DAD23FF>Done!, Tables has been created successfully, continue with the next step.</color>");
                        }
                        else if (checkID == 6)
                        {
                            DrawText("<color=#BE1818FF>Couldn't create the tables in the database, check the console for more information regarding the error.</color>");
                        }
                    }

                    if (spinner != null && isLoadingWWW)
                    {

                        spinner.DrawSpinner();
                    }
                }
                Repaint();
            }
            else if (subStep == 3)
            {
                DrawText("You are all set! you have set up your database and are ready to use ULogin System!");
                DownArrow();
                DrawText("So let me explain some the options in <b>LoginDataBasePro</b>");
                DrawHorizontalColumn("Update IP", "ULogin collect the IP of the register user in order to block their IP if they get banned in game, if you enable this ULogin will check the IP each time" +
                    " that the user log-in so always keep the store IP update");
                DrawHorizontalColumn("Detect Ban", "Basically is = to say: you wanna use Ban Features.");
                DrawHorizontalColumn("Require Email Verification", "After register a account user is required to verify his email in order to log-in?");
                DrawHorizontalColumn("Check Ban In Mid", "Check if the player has not been banned each certain amount of time, if this is false that check will only be fire when player log-in");
                DrawHorizontalColumn("Can Register Same Email", "Can an email be use to register different accounts?");
            }
        }
    }

    IEnumerator DoCreateTables()
    {
        WWWForm wf = new WWWForm();
        wf.AddField("type", 7);
        wf.AddField("dbname", "sql-tables.sql");
        
        var url = bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Creator);
        isLoadingWWW = true;
        if (spinner != null) spinner.SetActive(true);

        using (UnityWebRequest w = UnityWebRequest.Post(url, wf))
        {
            w.SendWebRequest();
            while (!w.isDone) { yield return null; }

            isLoadingWWW = false;
            if (spinner != null) spinner.SetActive(false);

            if (!bl_DataBaseUtils.IsNetworkError(w))
            {
                var text = w.downloadHandler.text;
                if (w.responseCode == 202)
                {
                    checkLog = "Tables created successfully";
                    checkID = 5;
                }
                else if(w.responseCode == 204)
                {
                    checkLog = $"The require file was not found in the server, make sure you already uploaded the ULogin Pro files in the correct server directory.";
                    checkID = 6;
                }
                else
                {
                    checkLog = text;
                    checkID = 6;
                    Debug.LogWarning($"Unexpected response, Code: {w.responseCode}, Response: {text}, Error: {w.error}");
                }
                Repaint();
            }
            else
            {
                if (w.error.Contains("destination host"))
                {
                    Debug.LogError($"{w.error}\nTry removing the www. from the URL in LoginDataBasePro->PhpHostPath");
                }
                else
                {
                    Debug.LogError($"{w.error}\n{url} - {w.responseCode}");
                }
                checkID = 6;
            }
        }
    }

    void DrawAdminPanel()
    {
        if (subStep == 0)
        {
            DrawText("ULogin Pro comes with an Admin Panel with which you can perform some user management operations like<b>ban users, resets stats, change user roles</b>, etc... and see some useful information regarding the game data stored in the database.\n \n<b>The Admin Panel comes as a Unity scene</b>, which you can include in the game build or open directly in the Unity Editor,\nthe Admin Panel scene is located at: <i>Assets ➔ Addons ➔ ULoginSystemPro ➔ Content ➔ Scenes ➔ <b>AdminPanel</b></i>\n \nWhen the scene is included in the game build, the scene is only accessible for registered users with the rank/role of Admin and Moderators, when one of these users successfully login, they'll be able to see the button to redirect to the Admin Panel scene.\n \nIn order to set the role to a player account, you have to use this Admin Panel, so<b>for the first account</b> (yours or the game admin), you have to open the scene in the Editor and set the Admin role.\n \nFor the basic operations check the next section 'Player Operations'");
        }
        else if (subStep == 1)
        {
            DrawSuperText("With an Admin Panel with which you can perform some user management operations like ban users, resets stats, change user roles, etc... and see some useful information regarding the game data stored in the database.\n \nSimply open the Admin Panel scene and then you can:\n \n<?background=#CCCCCCFF>SEARCH A PLAYER</background>\n \nIn the left menu select <b>Users</b> ➔ in the top input field set the nickname of the player that you want to ban and click on the <b>Search User</b> button ➔ If the player is found in the database you will see the information of that player account, otherwise a message in the console will inform you that it couldn't be found.\n \nWhen you found the wanted player, you can perform the following operations:");
            Space(20);
            DrawSuperText("<?background=#CCCCCCFF>BAN PLAYER</background>\n \nNext to the found player information you will see some <b>Actions</b> buttons, in order to ban that player simply click on the <b>BAN</b> button ➔ a window will appear where you have to write a <b>Reason</b> of why you are banning that player, that message will be displayed to the player next time that he tries to log in, Finally click on the <b>Ban</b> button.\n \nOnce a player is banned, you can unban him by again searching the player account ➔ in the Actions section click on the <b>UNBAN</b> button.");
            Space(20);
            DrawSuperText("<?background=#CCCCCCFF>CHANGE PLAYER ROLE</background>\n \nThe player's roles identify accounts that allow to grant or deny certain features in-game, <b>Admin and Moderator</b> roles allow the account to access the Admin Panel and the player name will be highlighted with a distinctive color in-game, the ban role, of course, means that the player is banned therefore can't access to the game anymore.\n \nIn order to change a player account role, simply search the user account, and in the player information next to the current account role you will see a few buttons representing each role ➔ click the button with the name of the role that you want to assign to that account, confirm the operation, and that's.");
            Space(20);
            DrawSuperText("<?background=#CCCCCCFF>RESET PLAYER STATS</background>\n \nIn order to reset a player account stats <i>(Kills, Deaths, and Score)</i>, simply search the player account ➔ in the Actions section click on the <b>RESET STATS</b> button.\n \n<?background=#CCCCCCFF>ALTER PLAYER COINS</background>\n \nIf you want to give or deduct coins to a player account simply search the player account ➔ in the account information next to the current account coins field you will see a button Edit, click on it ➔ on the window that will prompt set the number of coins in the input field and next to it, in the dropdown select the virtual coin to applied, and finally click on the button that represents the type of operation that you want to perform <b>(Add or Deduct)</b>.");
        }
        else if(subStep == 2)
        {
            DrawSuperText("<?background=#CCCCCCFF>DIAGNOSTIC</background>\n \nThis window in the Admin Panel allows you to perform a quick diagnostic of the game database and server reachability.\n \nJust open the window and the data will be automatically fetched, if there's something wrong you will see the value highlighted in a reddish color.");
        }
    }

    void DrawVersionChecking()
    {
        DrawText("ULogin 1.6 comes with a new featured which is 'Version Checking' what this do is compare the local game version with the server game version, if the local version is different then " +
        "players will be not able to login or play the game until they update the game.");
        DownArrow();
        DrawText("To enable this feature, simple go to <i>Assets -> Addons -> ULoginSystemPro -> Content -> Resources -> LoginDataBasePro -> check 'Check Game Version'");
        DownArrow();
        DrawText("Now to change the Game Version, first the Local Game Version is in the GameData in the Resources folder of MFPS -> GameData -> GameVersion.");
        DrawText("For change the server Game Version you have to edit the bl_Common.php script in your server and edit the variable '$GameVersion'");
    }
    void DrawEmailConfirmation()
    {
        if (hostingType == HostingType.None)
        {
            DrawWebPanelOptions();
            return;
        }
        
        if (hostingType == HostingType.Awardspace)
        {
            if (subStep == 0)
            {
                DrawText("ULogin Pro comes with an <b>Email confirmation</b> feature, this feature is optional and is disable by default you can enabled it in <b>LoginDataBasePro</b> -> Required Email Verification.\n\nOnce the feature is enable, when an user register/create a new account a email will send to the user email to verified that the email is real, player wont be able to log in until the email is verified.\n\nNow, this feature use SMTP <i>(Simple Mail Transport Protocol)</i> which is the protocol to send emails to servers, <b>this feature has to be enabled in your server in order to be able to send emails</b>, if you are using Awardspace SMTP is enabled by default, if you are using other hosting provider and you don't know how to check if it's enabled or how to enable it, contact the support of your hosting provider.\n\nOk, once SMTP is activated, you also need an Email account on your server that catch all your server mails, normally hosting doesn't have an default email so you have to manually created it, in Awardspace to create an Email account go to: Hosting Tools -> Emails Accounts.\n");
                DrawServerImage(12, TextAlignment.Center);
                DrawText("Now in the open page you will have a form to create an Email account, just fill the email name and set the password, but before created click in the \"<b>Advance Settings</b>\" and in the new settings that will appear <b>Turn On</b> the \"<b>Catch-all</b>\" and \"<b>Default e-mail for scripts</b>\" toggles, then click on <b>Create</b> button.");
                DrawServerImage(13, TextAlignment.Center);
                DrawText("Now that the email is created you have to assign it in <b>bl_Common.php</b> script <i>(the one that you upload in your server)</i> ➔ <b>ADMIN_EMAIL</b>\n\nNow your email confirmation system will be ready.");
            }
            else if (subStep == 1)
            {
                DrawSuperText("You may encounter the issue that even after setting up your server mail account, emails are not being delivered, even tho could be due to various reasons one of the most common reasons is due to the server only allows sending emails from the domain authenticated mail accounts, and in order to authenticate an email, you need to set up your SMTP account information.\n \n<?background=#FFF>1. PHPMailer library</background>\n \nIn order to set up SMTP in PHP you need an external library called PHPMailer, the required scripts are included in the ULogin Pro package from version 2.0.0++, they are located under the phpmailer folder under the Php folder, so if you didn't upload that folder when uploaded all the other ULogin Php files, you have to upload them now in the same directory in your server.\n \n<?background=#FFF>2. Set Up the SMTP Account</background>\n \nIn your server directory where all the PHP scripts are located ➔ open the script <b>bl_Mailer.php</b>\n \nSimilar to bl_Common.php you will see some const variables in which you have to replace the parameter with your SMTP account information.\n \nWhere to find the SMTP information?\n \nYou can generally find your SMTP email server address in the account or settings section of your mail client, if you are not sure you can contact your hosting provider assistance to point you in the right direction.\n \nIf you are using Awardspace you can find that information in:\nHosting Tools ➔ Mail Account ➔ Select your email account in the <b>E-Mail Accounts</b> Panel ➔ Click on the right-side icon (tool icon) ➔ that will fold out some buttons ➔ click on the <b>Information</b> button ➔ this will prompt a list of tabs, under the <b>E-Mail Settings</b> you will find all the required information, copy that information <b>(Server/Host, Port, Username, and Password)</b> and paste in the bl_Mailer.php ➔ const variables respectively.\n \nAfter that simply set the variable <b>IS_SMTP</b> to <b>true</b> and save the script changes.\n \nNow the emails will be sent authenticated!");
            }
        }
        else if (hostingType == HostingType.DirectAdmin)
        {
            if (subStep == 0)
            {
                DrawText("ULogin Pro comes with an <b>Email confirmation</b> feature, this feature is optional and is disable by default you can enabled it in <b>LoginDataBasePro</b> ➔ Required Email Verification.\n \nOnce the feature is enabled, when a user register/creates a new account an email will send to the user email to verify that the email is real, the player won't be able to log in until the email is verified.\n \nNow, this feature uses SMTP <i>(Simple Mail Transport Protocol)</i> which is the protocol to send emails to servers, <b>this feature has to be enabled in your server in order to be able to send emails</b>, if you are using DirectAdmin SMTP is enabled by default, if you are using other hosting provider and you don't know how to check if it's enabled or how to enable it, contact the support of your hosting provider.\n \nOnce SMTP is activated, you also need an Email account on your server that catch all your server mails, normally hosting doesn't have a default email so you have to manually create it, in DirectAdmin to create an Email account select the E-Mail Accounts option");
                DrawServerImage("img-38.png", TextAlignment.Center);
                DrawText("On the new page, in the top right corner, you will see the button <b>CREATE ACCOUNT</b> > click it and on the new page fill the new account form then click on the <b>CREATE ACCOUNT</b> button");             
                DrawServerImage("img-39.png", TextAlignment.Center);
                DrawText("If the account is created a popup window should be prompt with your account information, copy that information since you will need it if you want to active the SMTP authentication in the next step.");
                DrawServerImage("img-40.png", TextAlignment.Center);
                DrawText("Now that the email is created you have to assign it in <b>bl_Common.php</b> script <i>(the one that you upload in your server)</i> ➔ <b>ADMIN_EMAIL</b>\n\nNow your email confirmation system will be ready.");
            }
            else if (subStep == 1)
            {
                DrawSuperText("You may encounter the issue that even after setting up your server mail account, emails are not being delivered, even tho could be due to various reasons one of the most common reasons is due to the server only allows sending emails from the domain authenticated mail accounts, and in order to authenticate an email, you need to set up your SMTP account information.\n  \n<?background=#FFF>1. PHPMailer library</background>\n  \nIn order to set up SMTP in PHP you need an external library called PHPMailer, the required scripts are included in the ULogin Pro package from version 2.0.0++, they are located under the phpmailer folder under the Php folder, so if you didn't upload that folder when uploaded all the other ULogin Php files, you have to upload them now in the same directory in your server.\n  \n<?background=#FFF>2. Set Up the SMTP Account</background>\n  \nIn your server directory where all the PHP scripts are located ➔ open the script <b>bl_Mailer.php</b>\n  \nSimilar to bl_Common.php you will see some const variables in which you have to replace the parameter with your SMTP account information.\n  \nWhere to find the SMTP information?\n  \nYou can generally find your SMTP email server address in the account or settings section of your mail client, if you are not sure you can contact your hosting provider assistance to point you in the right direction.\n  \nIf you are using DirectAdmin and come from the previous step, then you may have copied that information, if not, then you can find that information by selecting the E-Mail Accounts options in the DirectAdmin panel > Click on the + button on the far right side next to your email account > click on the Download... option > a file should be downloaded > open that file with a text editor.\n \nOnce you have all the required information <b>(Server/Host, Port, Username, and Password)</b> paste in the bl_Mailer.php (the one in your server) ➔ in the const variables respectively.\n  \nAfter that simply set the variable <b>IS_SMTP</b> to <b>true</b> and save the script changes.\n  \nNow the emails will be sent authenticated!");
            }
        }
    }
    
    void SecurityDoc()
    {
        DrawText("ULogin Pro uses many security measures in both clients and server-side to prevent common attacks/exploits to make sure that the user data is safe and also to protect the game database.\n\nJust to enums some of them:\n\n■ SQL sanitize to prevent SQL injection from the user input.\n■ Custom hash required to execute functions on server-side.\n■ P2P encryption using RSA and AES algorithms on all sensitivity requests using the latest phpseclib functions.\n■  Max login attempts.\n■ Password hashed and encrypted.\n■ Email verification.\n■ None sensitivity data is serialized in the game build.\n\nBut by default some of those features are disabled, here is a list of things that you should do to use ULogin with the maximum security measures:\n\n<size=16>1. Set a custom and secure <b>Secret Key</b>:</size>");
        DrawHyperlinkText("- You have to set this key in two places in <link=asset:Assets/Addons/ULoginSystemPro/Resources/LoginDataBasePro.asset>LoginProDataBase</link> ➔ Secret Key and in the PHP script bl_Common.php ➔ SECRET_KEY (the one that you upload to your server), it has to be the same key, make sure it is at least 16 chars long and includes letters, digits, and symbols.");
        Space(25);
        DrawHyperlinkText("<size=16>2 - Enable PeerToPeer encryption:</size>\n\n- This feature is disabled by default because it uses RSA encryption on the server-side which is a little bit slower and at a large scale requires more server resources.\n\nTo enable it simply turn on the toggle '<b>PeerToPeer Encryption</b>' in <link=asset:Assets/Addons/ULoginSystemPro/Resources/LoginDataBasePro.asset>LoginDataBasePro</link> and in <b>bl_Common.php</b> <i>(the script in your server)</i> set the const '<b>PER_TO_PER_ENCRYPTION</b>' = true.");
        DrawText("<size=16>3 - Use IL2CPP</size>\n\nWhen building your game, <b>make sure to use IL2CPP scripting backend</b> and <b>not Mono</b>, although Mono builds faster and you can use it to quickly test your builds it is old, easy to decompile and inject code in-runtime making your game vulnerable to hackers, so make sure use IL2CPP in your release builds.");
        DrawHyperlinkText("<size=16>4 - Use HTTPS</size>\n\n- This feature is external to ULogin but is a highly recommended and some times required feature that your server should have, <b>many platforms like iOS and Android required the use of secure https</b> request so if you are planning release your game on one of those platforms you must have a valid SSL certificated for your domain.\n\nTo get an SSL certificate <i>(and be able to use HTTPS instead of HTTP)</i>\nyou need to buy it, if you don't know how to install an SSL certificate on your server your best option is contact to your server hosting provider since more likely they self sell and install certificates.\n\nOnce you have a valid SSL certificated installed in your domain, you simply have to use HTTPS instead of HTTP in the URL that you set in <link=asset:Assets/Addons/ULoginSystemPro/Resources/LoginDataBasePro.asset>LoginDataBasePro</link> ➔ <b>PhpHostPath</b>, e.g: <i>http://www.mydomain.com/files/</i> ➔ should be: <i>https://www.mydomain.com/files/</i>");
        
        DrawSuperText("<size=16>5 - Use an obfuscator</size>\n\n- Using a tool to obfuscate the game client code can made it harder for hackers to decompile and deserialized parameters from the game, by default you can use the <?link=https://assetstore.unity.com/packages/tools/utilities/anti-cheat-toolkit-2021-202695?aid=1101lJFi>AntiCheat Toolkit</link> asset which is already integrated with ULogin Pro to obfuscate the sensitive serialized data included in the game, you simply have to import it and enable it <i>(check the AntiCheat section in the MFPS documentation)</i>.");

        DrawText("<size=16>6 - Protect your bl_Common.php</size>\n\n- The <i>bl_Common.php</i> file is one of the most important, hence vulnerable files of ULogin Pro. It hosts crucial information and data about your whole ULogin Pro setup.\n\nOne simple thing you can do is to change the bl_Common.php file permission in your server to be executed and read only from the same server, for this, depending on your type of server and how you access to this file you can change the permission on different ways, normally, you can change the file permission using <b>FileZilla</b> by doing right click over the <i>bl_Common.php</i> file in the FilleZilla window > select the <b>File Permissions...</b> option > in the <b>Numeric Value</b> input field set <b>600</b> and click in the <b>Ok</b> button to save the changes.");

        DrawSuperText("<size=16>7 - Rename the PHP scripts</size>\n\n- Changing the names of some of the PHP scripts can also improve the security, you can easily rename some of the PHP scripts by simpling rename then in your server and then change then also in <?link=asset:Assets/Addons/ULoginSystemPro/Resources/LoginDataBasePro.asset>LoginDataBasePro</link> > <b>Script Names</b> fields to match them.");
    }

    public string photonAuthURL;
    void PhotonAuthDoc()
    {
        DrawText("Since version 2.1.0 ULogin Pro supports <b>Photon Authentication</b>, this feature adds an extra security layer that makes it much more difficult for players to bypass bans\n \nThe functionality is simple: when a player login and redirects to the MainMenu scene the Photon connection will first do authentication of the user to check if the IP or device ID is banned, if a ban is detected then Photon will not connect to the server making the player not able to play the game, all this happens in the server-side so even if the players are able to inject code in the client-side they won't be able to bypass this easily.\n \nThis feature is disabled by default since it requires setting up a few things in your Photon Account dashboard first.");
        DrawSuperText("<b><size=16>Setup Photon Authentication.</size></b>\n \n- To active Photon Authentication you need 3 things:\n \n1. Enable Photon Authentication in <b>LoginDataBasePro</b> > <b>Use Photon Authentication</b>.\n \n2. Have uploaded the <b>bl_Photon.php</b> to your server, which if you already set up ULogin you have already done.\n \n3. Setting up the Authentication Provider in the Photon Dashboard, for this, you have to sign in to your Photon account at: <?link=https://dashboard.photonengine.com/>https://dashboard.photonengine.com/</link>\n \nOnce you have entered your account, go to your Photon RealTime application > in the bottom of the box click on the <b>Manage</b> button");
        DrawServerImage("img-23.png");
        DrawText("In the new window scroll down to the Authentication section > click on the <b>CUSTOM SERVER</b> button.");
        DrawServerImage("img-24.png");
        DrawText("in the new window, you will paste the following URL in the <b>Authentication URL</b> field:");

        if (string.IsNullOrEmpty(photonAuthURL))
        {
            photonAuthURL = $"{bl_LoginProDataBase.Instance.PhpHostPath}bl_Photon.php";
        }
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Space(20);
            EditorGUILayout.TextField(photonAuthURL);
            if(GUILayout.Button("Copy", GUILayout.Width(75)))
            {
                GUIUtility.systemCopyBuffer = photonAuthURL;
            }
            GUILayout.Space(20);
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(20);
        DrawText("Then click the button <b>ADD NEW PAIR</b> > and in the new pair fields set the Key as <b>photonKey</b> and as the value set your custom password/phrase > then click on the <b>CREATE</b> button.");
        DrawServerImage("img-25.png");
        DrawText("The last thing you have to do is set up the same password/phrase you use in bl_Photon.php > <b>PHOTON_KEY</b>, for it, open the <b>bl_Photon.php</b> script in your server and replace the default value with your custom phrase.");
        DrawServerImage("img-26.png");
        DrawText("You are all set.");
    }

    void DrawCommonProblems()
    {

    }

    void InstallationServiceDoc()
    {
        DrawText("If you are having trouble or just want to save time setting up the server-side database, we offer the installation service upon requests for a small fee.\n");
        if (Buttons.FlowButton("Contact Us"))
        {
            Application.OpenURL("https://www.lovattostudio.com/en/select-support/index.html");
        }
    }

    string checkLog = "";

    void CheckCreation(string data, bool isError)
    {
        if (data.Contains("done"))
        {
            checkLog = "Tables created successfully";
            checkID = 5;
        }
        else
        {
            checkLog = data;
            checkID = 6;
        }
        Repaint();
    }

    [MenuItem("MFPS/Addons/ULogin/Documentation")]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ULoginDocumentation));
    }

    [MenuItem("MFPS/Tutorials/ULogin Pro")]
    private static void ShowWindowMFPS()
    {
        EditorWindow.GetWindow(typeof(ULoginDocumentation));
    }
}
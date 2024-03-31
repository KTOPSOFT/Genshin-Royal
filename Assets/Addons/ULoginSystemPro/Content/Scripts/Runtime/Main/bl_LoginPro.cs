using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Globalization;
using MFPS.ULogin;
using TMPro;
#if CLANS
using MFPS.Addon.Clan;
#endif

public class bl_LoginPro : bl_LoginProBase
{

    #region Public members
    [Header("References")]
    [SerializeField] private GameObject LoadingUI = null;
    [SerializeField] private GameObject DataBasePrefab = null;
    [SerializeField] private GameObject OutdatedUI = null;
    [SerializeField] private GameObject noConnectionUI = null;
    [SerializeField] private TextMeshProUGUI LogText = null;
    [SerializeField] private CanvasGroup FadeAlpha = null;
    [SerializeField] private GameObject GuestButton;

    public UnityEvent OnLogin;
    public UnityEvent OnLogOut;
    public UnityEvent onReady;

    public CustomAuthCredentials customAuthCredentials { get; set; }
    public static Action<AuthenticationType> onRequestAuth;
    public Dictionary<string, string> loginUserData;
    #endregion

    #region Private members
    private bool isRequesting = false;
    private bool isLogin = false;
    private int remaingAttempts = 5;
    private LoginUserInfo LocalUserInfo;
    private string lastKey = string.Empty;
    private bl_BanSystem Ban;
    private string currentIP;
    private bl_SignIn SignIn;
    #endregion

    #region Unity Methods
    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        if (FindObjectOfType<bl_DataBase>() == null)
        {
            Instantiate(DataBasePrefab);
        }
        Ban = FindObjectOfType<bl_BanSystem>();
        SignIn = transform.GetComponentInChildren<bl_SignIn>(true);
        SetPlayAsGuestButtonActive(false);
        bl_ULoginUI.Instance.SetActiveAuthUI(false);
    }

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        remaingAttempts = ULoginSettings.maxLoginAttempts;
        LoadingUI.SetActive(false);
        InitProccess();

        bl_Input.Initialize();
        bl_Input.CheckGamePadRequired();
    } 
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public void InitProccess()
    {
        StartCoroutine(DoStartSequence());
    }

    #region Login
    /// <summary>
    /// 
    /// </summary>
    public void Login(string user, string pass)
    {
        if (isRequesting || bl_LoginProDataBase.Instance == null)
            return;

        StartCoroutine(LoginProcess(user, pass));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator LoginProcess(string user, string pass)
    {
        isRequesting = true;
        LoadingUI.SetActive(true);
        SetLogText("");

        var formData = CreateForm(false, true);
        formData.AddSecureField("name", user);
        formData.AddSecureField("password", pass);
        formData.AddSecureField("appAuth", "ulogin");

        var credential = new CustomAuthCredentials()
        {
            UserName = user,
            UniqueID = pass,
            authenticationType = AuthenticationType.ULogin,
        };

        using (UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Login), formData))
        {
            //Wait for server response...
            yield return www.SendWebRequest();

            var response = new ULoginResult(www);
            //check if we have some error
            if (!response.isError)
            {
                if (ParseLoginData(response.RawTextReadable))
                {
                    //'Decompile' information from response
                    LoginUserInfo info = new LoginUserInfo();
                    info.ParseFullData(loginUserData);
                    info.IP = currentIP;
#if CLANS
                    info.Clan = new bl_ClanInfo();
                    info.Clan.GetSplitInfo(loginUserData);
                    yield return StartCoroutine(info.Clan.GetClanBasicInfo());
#endif
                    //send information to local database
                    DataBase.OnLogin(info);
                    LocalUserInfo = info;
                    SignIn.OnLogin(info);
                    DataBase.CacheAccessToken = pass;

                    OnAuthenticated(credential, info);
                    Debug.Log($"Sign-in as <b>{info.NickName}</b>");
                }
                else
                {
                    //Some error with the server setup.
                    if (bl_LoginProDataBase.Instance.FullLogs)
                        Debug.Log(response.RawTextReadable);
                    
                    if (www.responseCode == 401)//wrong credentials
                    {
                        OnWrongCredentials(credential);
                    }
                    else
                        ErrorType(response.RawTextReadable);
                }
            }
            else
            {
                if (www.responseCode == 401)//wrong credentials
                {
                    OnWrongCredentials(credential);
                }
                else
                    response.PrintError();
            }
        }
        bl_ULoginLoadingWindow.Instance?.SetActive(false);
        LoadingUI.SetActive(false);
        isRequesting = false;
    }

    /// <summary>
    /// Login with custom credentials
    /// You can use this to login/register a player with they ID from authenticator like Facebook, Steam, GooglePlay, etc...
    /// </summary>
    /// <param name="credentials"></param>
    public void Authenticate(CustomAuthCredentials credentials)
    {
        if (string.IsNullOrEmpty(credentials.UniqueID) || string.IsNullOrEmpty(credentials.UserName)) return;

        bl_ULoginLoadingWindow.Instance?.SetText($"Authenticating with {credentials.authenticationType.ToString()}...", true);
        customAuthCredentials = credentials;
        WWWForm wf = CreateForm(false, true);
        wf.AddSecureField("name", credentials.UserName);
        wf.AddSecureField("password", credentials.GetUniquePassword());
        wf.AddSecureField("appAuth", credentials.authenticationType.ToString().ToLower());
        LoadingUI.SetActive(true);

        WebRequest.POST(GetURL(bl_LoginProDataBase.URLType.Login), wf, (result) =>
         {
             if (!result.isError)
             {
                 //if the login is successful
                 if (ParseLoginData(result.RawTextReadable))
                 {
                     //parse the player data retrieved by the server
                     var info = BuildUserData();

#if CLANS
                     info.Clan = new bl_ClanInfo();
                     info.Clan.GetSplitInfo(loginUserData);
                     StartCoroutine(info.Clan.GetClanBasicInfo(() =>
                     {
                         OnAuthenticated(credentials, info);
                     }));
#else
                     OnAuthenticated(credentials, info);

#endif
                 }
                 else //if we can't login
                 {
                     result.Print(true);
                 }
             }
             else
             {
                 if (result.HTTPCode == 401) //because the account is not registered yet
                 {
                     Debug.Log("Account doesn't exists");
                     if (string.IsNullOrEmpty(credentials.NickName))
                     {
                         ChangePanel(8);
                         bl_ULoginUI.Instance.GetWindowObject("nick-input").GetComponent<bl_NickNamePanel>().SetCallback((nickName) =>
                         {
                             customAuthCredentials.NickName = nickName;
                             CreateAccountWithCredentials(customAuthCredentials);
                         });
                     }
                     else
                     {
                         CreateAccountWithCredentials(customAuthCredentials);
                         return;
                     }
                 }
                 else
                 {
                     result.PrintError();
                 }
             }
             bl_ULoginLoadingWindow.Instance.SetActive(false);
             LoadingUI.SetActive(false);
         });
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnAuthenticated(CustomAuthCredentials credentials, LoginUserInfo info)
    {
        SetPlayAsGuestButtonActive(false);
        SetLogText("Sign In success!");

        //detect if this account is banned before load next level
        Ban.CheckAccount(info, (isClean) =>
        {
            if (!isClean)
            {
                // if account is banned
                return;
            }

            //if it's OK, load next level of show continue menu
            isLogin = true;
            OnLogin?.Invoke();
            //check if session has to be remember
            if (bl_LoginProDataBase.Instance.rememberMeBehave == RememberMeBehave.RememberSession)
            {
                if (SignIn != null && SignIn.RememberMe())
                {
                    string p = credentials.authenticationType == AuthenticationType.ULogin ? credentials.UniqueID : credentials.GetUniquePassword();
                    //if it's so, encrypt and save the authentication credentials that the user used.
                    bl_LoginProDataBase.Instance.RememberCredentials = $"{(int)credentials.authenticationType}:{credentials.UserName}:{p}";
                }
            }
            if (bl_LoginProDataBase.Instance.afterLoginBehave == bl_LoginProDataBase.AfterLoginBehave.LoadNextScene)//load the scene after login success (without continue menu)
            {
                Continue();
            }
            else
            {
                ChangePanel(3);
            }
        });
    }

    /// <summary>
    /// 
    /// </summary>
    void CreateAccountWithCredentials(CustomAuthCredentials credentials)
    {
        if (string.IsNullOrEmpty(credentials.UniqueID) || string.IsNullOrEmpty(credentials.UserName)) return;

        bl_ULoginLoadingWindow.Instance?.SetText($"Creating account with {credentials.authenticationType.ToString()}...", true);

        string loginName = string.IsNullOrEmpty(credentials.UserName) ? credentials.NickName : credentials.UserName;
        string password = credentials.GetUniquePassword();
        //Used for security check for authorization to modify database
        string hash = bl_DataBaseUtils.Md5Sum(loginName + password + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        WWWForm wf = CreateForm(false, true);
        wf.AddSecureField("name", loginName); // adds the login name to the form
        wf.AddSecureField("nick", credentials.NickName); // adds the nick name to the form
        wf.AddSecureField("password", password); // adds the player password to the form
        wf.AddSecureField("coins", bl_ULoginMFPS.GetLocalUserInitialCoins());
        wf.AddSecureField("multiemail", 0);
        wf.AddSecureField("emailVerification", 1);
        wf.AddSecureField("uIP", currentIP);
        wf.AddSecureField("hash", hash); // adds the security hash for Authorization

        LoadingUI.SetActive(true);
        WebRequest.POST(GetURL(bl_LoginProDataBase.URLType.Register), wf, (result) =>
        {
            if (!result.isError)
            {
                string text = result.Text;
                if (bl_LoginProDataBase.Instance.FullLogs)
                {
                    Debug.Log("Register Result: " + result.RawText);
                }
                if (result.resultState == ULoginResult.Status.Success)
                {
                    //show success
                    SetLogText("Register success!");
                    Authenticate(credentials);
                }
                else if (text == "008")
                {
                    SetLogText("This Nickname is already taken.");
                }
                else
                {
                    ErrorType(text);
                    result.Print();
                }
            }
            else
            {
                result.PrintError();
            }
            bl_ULoginLoadingWindow.Instance.SetActive(false);
            LoadingUI.SetActive(false);
        });
    }

    /// <summary>
    /// 
    /// </summary>
    private LoginUserInfo BuildUserData()
    {
        LoginUserInfo info = new LoginUserInfo();
        info.ParseFullData(loginUserData);
        info.IP = currentIP;
        info.authenticationType = customAuthCredentials != null ? customAuthCredentials.authenticationType : AuthenticationType.ULogin;
        //send information to local database
        DataBase.OnLogin(info);
        LocalUserInfo = info;
        SignIn.OnLogin(info);
        return info;
    }

    /// <summary>
    /// 
    /// </summary>
    private bool ParseLoginData(string result)
    {
        string[] rows = result.Split('\n');
        if (!rows[0].Contains("success")) return false;

        loginUserData = new Dictionary<string, string>();
        for (int i = 0; i < rows.Length; i++)
        {
            if (string.IsNullOrEmpty(rows[i]) || i == 0) continue;

            string[] keyValuePair = rows[i].Split('|');
            loginUserData.Add(keyValuePair[0], keyValuePair[1]);
        }
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayAsGuest()
    {
        DataBase.isGuest = true;
        Continue();
    }

    /// <summary>
    /// Called when the user try to login with wrong credentials (user name or password)
    /// </summary>
    private void OnWrongCredentials(CustomAuthCredentials credentials)
    {
        remaingAttempts--;
        if (remaingAttempts > 0)
        {
            if (remaingAttempts > 3)
                SetLogText("The username or password you've entered are incorrect!");
            else SetLogText($"The username or password you've entered are incorrect!, attempts left: {remaingAttempts}");
        }
        else
        {
            var currentTime = DateTime.Now.ToUniversalTime();
            currentTime = currentTime.AddSeconds(ULoginSettings.waitTimeAfterFailAttempts);
            var gt = new CultureInfo("en-US");
            string dateStr = currentTime.ToString(gt);
            string key = bl_DataBaseUtils.Encrypt(bl_DataBaseUtils.LOCK_TIME_KEY);
            PlayerPrefs.SetString(key, bl_DataBaseUtils.Encrypt(dateStr));
            SetPlayAsGuestButtonActive(false);
            ChangePanel(9);
            remaingAttempts = ULoginSettings.maxLoginAttempts;
        }
    }
    #endregion

    #region SignUp

    /// <summary>
    /// 
    /// </summary>
    public void SinUp(string user, string nick, string pass, string email)
    {
        if (isRequesting)
            return;
        if (bl_LoginProDataBase.Instance == null)
            return;

        StartCoroutine(RegisterProcess(user, nick, pass, email));
    }

    /// <summary>
    /// Connect with database
    /// </summary>
    /// <returns></returns>
    IEnumerator RegisterProcess(string user, string nick, string pass, string email)
    {
        isRequesting = true;
        SetLogText("");
        //Used for security check for authorization to modify database
        string hash = bl_DataBaseUtils.Md5Sum(user + pass + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        LoadingUI.SetActive(true);
        //Assigns the data we want to save
        //Where -> Form.AddField("name" = matching name of value in SQL database
        WWWForm mForm = CreateForm(false, true);
        mForm.AddSecureField("name", user); // adds the login name to the form
        mForm.AddSecureField("nick", nick); // adds the nick name to the form
        mForm.AddSecureField("password", pass); // adds the player password to the form
        mForm.AddSecureField("coins", bl_ULoginMFPS.GetLocalUserInitialCoins());
        if (bl_LoginProDataBase.Instance.RequiredEmailVerification && !string.IsNullOrEmpty(email))
        {
            mForm.AddSecureField("email", email);
        }
        else
        {
            mForm.AddSecureField("email", "none");
        }
        mForm.AddSecureField("multiemail", bl_LoginProDataBase.Instance.CanRegisterSameEmailInt());
        mForm.AddSecureField("emailVerification", bl_LoginProDataBase.Instance.RequiereVerification());
        mForm.AddSecureField("uIP", currentIP);
        mForm.AddSecureField("hash", hash); // adds the security hash for Authorization

        //Creates instance of WWW to runs the PHP script to save data to mySQL database
        using (UnityWebRequest www = UnityWebRequest.Post(GetURL(bl_LoginProDataBase.URLType.Register), mForm))
        {
            yield return www.SendWebRequest();

            if (www.error == null)
            {
                string result = www.downloadHandler.text;
                if (bl_LoginProDataBase.Instance.FullLogs)
                {
                    Debug.Log("Register Result: " + result);
                }
                if (result.Contains("success") == true)
                {
                    //show success
                    ChangePanel(4);
                    SetLogText("Register success!");
                }
                else
                {
                    //Debug.Log(www.downloadHandler.text);
                    ErrorType(www.downloadHandler.text);
                }
            }
            else
            {
                Debug.Log("Error:" + www.error);
            }
            LoadingUI.SetActive(false);
        }
        isRequesting = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Continue()
    {
        StartCoroutine(LoadSceneSecuence(bl_LoginProDataBase.Instance.OnLoginLoadLevel));
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator LoadSceneSecuence(string scene)
    {
        while (FadeAlpha.alpha < 1)
        {
            FadeAlpha.alpha += Time.deltaTime * 2;
            yield return new WaitForEndOfFrame();
        }
        bl_DataBaseUtils.LoadLevel(scene);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator DoStartSequence()
    {
        FadeAlpha.alpha = 1;
        while (FadeAlpha.alpha > 0)
        {
            FadeAlpha.alpha -= Time.deltaTime;
            yield return null;
        }

#if !UNITY_WEBGL
        if (ULoginSettings.checkInternetConnection)
            yield return StartCoroutine(CheckConnection());
#endif

        StartCoroutine(InitRequest());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckConnection()
    {
        bl_ULoginLoadingWindow.Instance?.SetActive(true);
        bl_ULoginLoadingWindow.Instance?.SetText("Checking connection...");
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            noConnectionUI.SetActive(true);
            yield break;
        }

        using (UnityWebRequest w = UnityWebRequest.Get("https://www.google.com"))
        {
            w.SetRequestHeader("Access-Control-Allow-Credentials", "true");
            w.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
            w.SetRequestHeader("Access-Control-Allow-Origin", "*");

            yield return w.SendWebRequest();
            if (!bl_DataBaseUtils.IsNetworkError(w))
            {
                if (w.responseCode != 200)
                {
                    noConnectionUI.SetActive(true);
                    Debug.LogWarning("No Internet: " + w.responseCode);
                    StopAllCoroutines();
                }
            }
            else
            {
                noConnectionUI.SetActive(true);
                StopAllCoroutines();
            }
        }
    }


    #endregion

    #region Change Password
    public void ChangePassword(string pass, string newpass)
    {
        if (isRequesting)
            return;
        if (bl_LoginProDataBase.Instance == null)
            return;
        if (!isLogin) { SetLogText("Need login to request a password change."); return; }

        StartCoroutine(ChangePasswordRequest(pass, newpass));

    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator ChangePasswordRequest(string pass, string newpass)
    {
        isRequesting = true;
        LoadingUI.SetActive(true);
        SetLogText("");
        //Used for security check for authorization to modify database
        // Create instance of WWWForm
        WWWForm wf = CreateForm(FormHashParm.ID, true);
        //sets the mySQL query to the amount of rows to load
        wf.AddSecureField("id", LocalUserInfo.ID);
        wf.AddSecureField("type", DBCommands.ACCOUNT_CHANGE_PASSWORD);
        wf.AddSecureField("password", pass);
        wf.AddSecureField("data", newpass);
        wf.AddSecureField("hash", bl_DataBaseUtils.CreateSecretHash(bl_DataBase.Instance.LocalUser.ID.ToString()));

        //Creates instance to run the php script to access the mySQL database
        using (UnityWebRequest www = UnityWebRequest.Post(GetURL(bl_LoginProDataBase.URLType.Account), wf))
        {
            //Wait for server response...
            yield return www.SendWebRequest();
            string result = www.downloadHandler.text;

            //check if we have some error
            if (!bl_DataBaseUtils.IsNetworkError(www))
            {
                if (result.Contains("success"))
                {
                    SetLogText("your password has been changed successfully.");
                    ChangePanel(3);
                }
                else//Wait, have a error?, please contact me for help with the result of next debug log.
                {
                    ErrorType(result);
                }
            }
            else
            {
                Debug.LogError("Error: " + www.error);
            }
            LoadingUI.SetActive(false);
        }
        isRequesting = false;
    }
    #endregion

    #region Reset Password
    public void RequestNewPassword(string user, string email)
    {
        if (isRequesting)
            return;
        if (bl_LoginProDataBase.Instance == null)
            return;

        StartCoroutine(PasswordRequest(user, email));
    }

    public void ResetPassword(string user, string pass)
    {
        if (isRequesting)
            return;
        if (bl_LoginProDataBase.Instance == null)
            return;

        StartCoroutine(ResetPasswordCall(user, pass));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator PasswordRequest(string user, string email)
    {
        isRequesting = true;
        LoadingUI.SetActive(true);
        SetLogText("");
        //Used for security check for authorization to modify database
        lastKey = string.Format("LS-{0}", bl_DataBaseUtils.GenerateKey(8));
        // Create instance of WWWForm
        WWWForm wf = CreateForm(false, true);
        //sets the mySQL query to the amount of rows to load
        wf.AddSecureField("id", user);
        wf.AddSecureField("email", email);
        wf.AddSecureField("data", lastKey);
        wf.AddSecureField("type", DBCommands.ACCOUNT_CHANGE_PASSWORD_VERIFICATION);
        wf.AddSecureField("hash", bl_DataBaseUtils.CreateSecretHash(user));

        //Creates instance to run the php script to access the mySQL database
        using (UnityWebRequest www = UnityWebRequest.Post(GetURL(bl_LoginProDataBase.URLType.Account), wf))
        {
            //Wait for server response...
            yield return www.SendWebRequest();
            string result = www.downloadHandler.text;

            //check if we have some error
            if (!bl_DataBaseUtils.IsNetworkError(www))
            {
                if (result.Contains("success"))
                {
                    SetLogText("An email with your reset key has been sent to your email-address");
                    ChangePanel(7);
                }
                else//Wait, have a error?, please contact me for help with the result of next debug logwarning.
                {
                    ErrorType(result);
                }
            }
            else
            {
                Debug.LogError("Error: " + www.error);
            }
            LoadingUI.SetActive(false);
        }
        isRequesting = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator ResetPasswordCall(string user, string pass)
    {
        isRequesting = true;
        LoadingUI.SetActive(true);
        SetLogText("");
        // Create instance of WWWForm
        WWWForm wf = CreateForm(false, true);
        //sets the mySQL query to the amount of rows to load
        wf.AddSecureField("id", user);
        wf.AddSecureField("password", pass);
        wf.AddSecureField("type", DBCommands.ACCOUNT_CHANGE_PASSWORD_WITHOUT_V);
        wf.AddSecureField("hash", bl_DataBaseUtils.CreateSecretHash(user));

        //Creates instance to run the php script to access the mySQL database
        using (UnityWebRequest www = UnityWebRequest.Post(GetURL(bl_LoginProDataBase.URLType.Account), wf))
        {
            //Wait for server response...
            yield return www.SendWebRequest();
            string result = www.downloadHandler.text;

            //check if we have some error
            if (www.error == null)
            {
                if (result.Contains("success"))
                {
                    SetLogText("your password has been reset successfully, you can sign in now.");
                    ChangePanel(0);
                }
                else//Wait, have a error?, please contact me for help with the result of next debug logwarning.
                {
                    ErrorType(result);
                }
            }
            else
            {
                Debug.LogError("Error: " + www.error);
            }
            LoadingUI.SetActive(false);
        }
        isRequesting = false;
    }
    #endregion

    #region IP
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator InitRequest()
    {
        bl_ULoginLoadingWindow.Instance?.SetActive(true);
        bl_ULoginLoadingWindow.Instance?.SetText("Collecting data base information...\n<size=10>Please wait.</size>");
        yield return new WaitForSeconds(0.5f);
        if (!bl_GameData.isDataCached)
        {
            yield return StartCoroutine(bl_GameData.AsyncLoadData());
        }
        //Request public IP to the server
        using (UnityWebRequest w = UnityWebRequest.Get(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Init)))
        {
            //Wait for response
            yield return w.SendWebRequest();
            if (!bl_DataBaseUtils.IsNetworkError(w))
            {
                string result = w.downloadHandler.text;
                if (bl_LoginProDataBase.Instance.FullLogs)
                {
                    Debug.Log($"Initial Server Response: {result}");
                }
                string[] info = result.Split("|"[0]);
                currentIP = info[0];
                bl_DataBase.Instance.SessionIP = currentIP;

                if (bl_LoginProDataBase.Instance.CheckGameVersion)
                {
                    if (bl_GameData.Instance.GameVersion != info[1])
                    {
                        OutdatedUI.SetActive(true);
                        bl_ULoginLoadingWindow.Instance?.SetActive(false);
                        Debug.LogWarning(string.Format("Outdated version: {0} - {1}", bl_GameData.Instance.GameVersion, info[1]));
                        yield break;
                    }
                }

                if (bl_LoginProDataBase.Instance.PeerToPeerEncryption)
                {
                    bool rsaDone = false;
                    bl_ULoginLoadingWindow.Instance?.SetText("Securing connection...");
                    bl_LoginProSecurity.Instance.RequestRSAPublicKey((r) =>
                    {
                        rsaDone = true;
                    });
                    while (!rsaDone) yield return null;
                }

                if (bl_LoginProDataBase.Instance.DetectBan)
                {
                    Ban.VerifyIP(OnFinishInitialConnection);
                }
                else
                {
                    OnFinishInitialConnection();
                }
            }
            else
            {
                Debug.LogError($"HTTP Error: {w.error} Echo: {w.downloadHandler.text}");
            }
        }
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    private void OnFinishInitialConnection()
    {
        bl_ULoginUI.Instance.SetActiveAuthUI(true);
        if (!CheckSessionCredentials())
        {
            bl_ULoginLoadingWindow.Instance?.SetActive(false);
            if (!bl_DataBaseUtils.IsLoginBlocked())
            {
                SetPlayAsGuestButtonActive(true);
                onReady?.Invoke();
            }
            else
            {
                // if the player is forbidden to authenticate
                // due max fail authentications attempts
                ChangePanel(9);
            }
        }
        else
        {
            bl_ULoginLoadingWindow.Instance.SetText("Authenticating previous session user...");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool CheckSessionCredentials()
    {
        if (bl_LoginProDataBase.Instance.rememberMeBehave == RememberMeBehave.RememberSession)
        {
            string credentials = bl_LoginProDataBase.Instance.RememberCredentials;
            if (!string.IsNullOrEmpty(credentials))
            {
                if (credentials.Contains(":"))
                {
                    string[] userCredentials = credentials.Split(':');
                    if (userCredentials.Length <= 1)
                    {
                        Debug.Log($"Resetting saved credentials due they were for a different remember method");
                        bl_LoginProDataBase.Instance.DeleteRememberCredentials();
                        return false;
                    }
                    try
                    {
                        var authMethod = (AuthenticationType)int.Parse(userCredentials[0]);
                        if (authMethod == AuthenticationType.ULogin)
                        {
                            SignIn.SetCredentials(userCredentials[1], userCredentials[2]);
                            if (!SignIn.SignIn())
                            {
                                return false;
                            }
                        }
                        else
                        {
                            var customCredentials = new CustomAuthCredentials()
                            {
                                UserName = userCredentials[1],
                                UniqueID = bl_DataBaseUtils.ReverseChars(userCredentials[2]),
                                authenticationType = authMethod
                            };

                            Authenticate(customCredentials);
                        }

                        return true;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.StackTrace);
                        bl_LoginProDataBase.Instance.DeleteRememberCredentials();
                    }
                }
                else
                {
                    bl_LoginProDataBase.Instance.DeleteRememberCredentials();
                }
            }
            return false;
        }
        else return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SignOut()
    {
        bl_DataBase.Instance.SignOut();
        bl_LoginProDataBase.Instance.DeleteRememberCredentials();
        SignIn.ResetFields();
        ChangePanel(0);
        OnLogOut?.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetLogText(string text)
    {
        if(LogText != null) LogText.text = text;
        if (ULoginSettings.FullLogs && !string.IsNullOrEmpty(text)) Debug.Log(text);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangePanel(int id)
    {
        bl_ULoginUI.Instance.OpenWindow(id);
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadAdminPanel()
    {
        StartCoroutine(LoadSceneSecuence("AdminPanel"));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public void SetPlayAsGuestButtonActive(bool active)
    {
        if (GuestButton == null) return;
        if (!ULoginSettings.allowPlayAsGuest)
        {
            GuestButton.gameObject.SetActive(false);
            return;
        }
        GuestButton.gameObject.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    private void ErrorType(string error)
    {
        int code = 0;
        if (int.TryParse(error, out code))
        {
            switch (code)
            {
                case (int)LoginErrorCode.PasswordIncorrect:
                    SetLogText("Password is incorrect!");
                    break;
                case (int)LoginErrorCode.UserAlreadyRegister:
                    SetLogText("User with this name already exist");
                    break;
                case (int)LoginErrorCode.UserNotExist:
                    SetLogText("User with this name not exist");
                    break;
                case (int)LoginErrorCode.InvalidEmail:
                    SetLogText("Invalid email address please type a valid email.");
                    break;
                case (int)LoginErrorCode.EmailExist:
                    SetLogText("The email is already taken, please try new.");
                    break;
                case (int)LoginErrorCode.NotActive:
                    SetLogText("Your account is not active yet, verify your email to active.");
                    ChangePanel(2);
                    break;
                case (int)LoginErrorCode.UserNotFound:
                    SetLogText("User not found, have you sign out?");
                    break;
                case (int)LoginErrorCode.EmailNotSend:
                    SetLogText("Email can't be send.");
                    break;
                case (int)LoginErrorCode.UserAndEmailNotFound:
                    SetLogText("User or email is not found, or they are not of the same account.");
                    break;
                case (int)LoginErrorCode.NickAlreadyTaken:
                    SetLogText("This nick name is already taken.");
                    break;
                default:
                    SetLogText("Error code not define: " + error);
                    break;
            }
        }
        else
        {
            SetLogText("Unknown error: " + error);
        }
    }
    public string GetKey { get { return lastKey; } }
    public string CurrentIp { get { return currentIP; } }

    private static bl_LoginPro _instance;
    public static bl_LoginPro Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_LoginPro>(); }
            return _instance;
        }
    }
}
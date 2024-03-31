using UnityEngine;
using System;
using MFPS.ULogin;
#if ACTK_IS_HERE
using CodeStage.AntiCheat.ObscuredTypes;
#endif

public class bl_LoginProDataBase : ScriptableObject
{
    public const string ObjectName = "LoginDataBasePro";

    [Header("Host Path")]
    [Tooltip("The Url of folder where your php scripts are located in your host.")]

#if !ACTK_IS_HERE
    public string PhpHostPath;
    public string SecretKey = "123456";
#else
    public ObscuredString PhpHostPath;
    public ObscuredString SecretKey = "123456";
#endif
    public string OnLoginLoadLevel = "NextLevelName";

    [Header("Settings")]
    [LovattoToogle] public bool CheckGameVersion = true;
    [LovattoToogle] public bool PeerToPeerEncryption = false;
    [LovattoToogle] public bool ForceLoginScene = true;
    [LovattoToogle] public bool allowPlayAsGuest = true;
    [LovattoToogle] public bool DetectBan = true;
    [LovattoToogle] public bool RequiredEmailVerification = true;
    [LovattoToogle] public bool usePhotonAuthentication = false;
    [LovattoToogle] public bool CanRegisterSameEmail = false;
    [LovattoToogle] public bool checkInternetConnection = true;
    [LovattoToogle] public bool BanComprobationInMid = true; //keep checking ban status each certain time
    [LovattoToogle] public bool PlayerCanChangeNick = true; // can players change their nick name?
    [LovattoToogle] public bool UpdateIP = true;
    [Tooltip("Check that the user name doesn't contain a bad word from the black word list.")]
    [LovattoToogle] public bool FilterUserNames = true;
    [LovattoToogle] public bool showStatusPrefix = true;
    public int maxNickNameLenght = 16;
    [Range(3, 12)] public int MinPasswordLenght = 5;
    [Tooltip("Set 0 for unlimited attempts")]
    [Range(0, 12)] public int maxLoginAttempts = 5;
    [Tooltip("In seconds")]
    [Range(30, 3000)] public int waitTimeAfterFailAttempts = 300;
    [Range(10, 300)] public int CheckBanEach = 10;
    public ULoginBanMethod banMethod = ULoginBanMethod.IP;
    public AfterLoginBehave afterLoginBehave = AfterLoginBehave.ShowAccountResume;
    public RememberMeBehave rememberMeBehave = RememberMeBehave.RememberSession;

    [Header("Script Names")]
#if !ACTK_IS_HERE
    public string LoginPhp = "bl_Login";
    public string RegisterPhp = "bl_Register";
    public string DataBasePhp = "bl_DataBase";
    public string AdminPhp = "bl_Admin";
    public string AccountPhp = "bl_Account";
    public string RankingPhp = "bl_Ranking";
    public string InitPhp = "bl_Init";
    public string BanListPhp = "bl_BanList";
    public string SupportPhp = "bl_Support";
    public string DataBaseCreator = "bl_DatabaseCreator";
    public string ClanPhp = "bl_Clan";
    public string ShopPhp = "bl_Shop";
    public string SecurityPhp = "bl_Security";
    public string OAuthPhp = "bl_OAuth";
#else
    public string LoginPhp = "bl_Login";
    public string RegisterPhp = "bl_Register";
    public string DataBasePhp = "bl_DataBase";
    public string AdminPhp = "bl_Admin";
    public string AccountPhp = "bl_Account";
    public string RankingPhp = "bl_Ranking";
    public string InitPhp = "bl_Init";
    public string BanListPhp = "bl_BanList";
    public string SupportPhp = "bl_Support";
    public string DataBaseCreator = "bl_DatabaseCreator";
    public string ClanPhp = "bl_Clan";
    public string ShopPhp = "bl_Shop";
    public string SecurityPhp = "bl_Security";
    public string OAuthPhp = "bl_OAuth";
#endif

    public bool FullLogs = false;

    public readonly string[] UserNameFilters = new string[] { "fuck", "fucker", "motherfucker", "nigga", "nigger", "porn", "pussy", "cock", "anus", "racist", "vih", "puto", "fagot", "shit", "bullshit", "gay","sex", "nazi", "bitch"};

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_type"></param>
    /// <returns></returns>
    public string GetUrl(URLType _type)
    {
        string scriptName = "None";
        switch (_type)
        {
            case URLType.Login:
                scriptName = LoginPhp;
                break;
            case URLType.Register:
                scriptName = RegisterPhp;
                break;
            case URLType.DataBase:
                scriptName = DataBasePhp;
                break;
            case URLType.Init:
                scriptName = InitPhp;
                break;
            case URLType.BanList:
                scriptName = BanListPhp;
                break;
            case URLType.Admin:
                scriptName = AdminPhp;
                break;
            case URLType.Ranking:
                scriptName = RankingPhp;
                break;
            case URLType.Account:
                scriptName = AccountPhp;
                break;
            case URLType.Support:
                scriptName = SupportPhp;
                break;
            case URLType.Creator:
                scriptName = DataBaseCreator;
                break;
            case URLType.Clans:
                scriptName = ClanPhp;
                break;
            case URLType.Shop:
                scriptName = ShopPhp;
                break;
            case URLType.Security:
                scriptName = SecurityPhp;
                break;
            case URLType.OAuth:
                scriptName = OAuthPhp;
                break;
        }
        if (!PhpHostPath.EndsWith("/")) { PhpHostPath += "/"; }
        string url = string.Format("{0}{1}.php", PhpHostPath, scriptName);
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute)) { Debug.Log("URL is not well formed, please check if your php script have the same name and have assign the host path."); }
        return url;
    }

    /// <summary>
    /// Return the url for the give endpoint
    /// </summary>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    public static string GetAPIEndpoint(string endpoint)
    {
        return Instance.PhpHostPath + endpoint;
    }

    /// <summary>
    /// Return the basic token without extra parameters
    /// </summary>
    /// <returns></returns>
    public static string GetAPIToken()
    {
        return bl_DataBaseUtils.Md5Sum(Instance.SecretKey).ToLower();
    }

    /// <summary>
    /// 
    /// </summary>
    public string GetPhpFolder
    {
        get
        {
            string folder = PhpHostPath;
            if (!folder.EndsWith("/")) { folder += "/"; }
            return folder;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int CanRegisterSameEmailInt()
    {
        return (CanRegisterSameEmail == true) ? 1 : 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int RequiereVerification()
    {
        return (RequiredEmailVerification == true) ? 0 : 1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public bool FilterName(string userName)
    {
        userName = userName.ToLower();
        for (int i = 0; i < UserNameFilters.Length; i++)
        {
            if (userName.Contains(UserNameFilters[i].ToLower()))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string RememberCredentials
    {
        get
        {
            string data = PlayerPrefs.GetString(GetRememberMeKey(), string.Empty);
            data = bl_DataBaseUtils.Decrypt(data);
            return data;
        }
        set
        {
            string data = bl_DataBaseUtils.Encrypt(value);
            PlayerPrefs.SetString(GetRememberMeKey(), data);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private string GetRememberMeKey()
    {
        return $"{Application.productName}.login.remember";
    }

    /// <summary>
    /// 
    /// </summary>
    public void DeleteRememberCredentials()
    {
        PlayerPrefs.DeleteKey(GetRememberMeKey());
    }

    /// <summary>
    /// 
    /// </summary>
    public void SignOut()
    {
        DeleteRememberCredentials();
        if (bl_DataBase.Instance != null) bl_DataBase.Instance.LocalUser = new LoginUserInfo();
        bl_DataBaseUtils.LoadLevel("Login");
    }

    /// <summary>
    /// Does the ban uses the device unique ID?
    /// </summary>
    /// <returns></returns>
    public static bool UseDeviceId()
    {
        return Instance.banMethod == ULoginBanMethod.DeviceID || Instance.banMethod == ULoginBanMethod.Both;
    }

    private static bl_LoginProDataBase _dataBase;
    public static bl_LoginProDataBase Instance
    {
        get
        {
            if (_dataBase == null) { _dataBase = Resources.Load("LoginDataBasePro", typeof(bl_LoginProDataBase)) as bl_LoginProDataBase; }
            return _dataBase;
        }
    }

    [Serializable]
    public enum AfterLoginBehave
    {
        ShowAccountResume = 0,
        LoadNextScene
    }

    [Serializable]
    public enum URLType
    {
        Login,
        Register,
        DataBase,
        Ranking,
        Account,
        Init,
        BanList,
        Support,
        Creator,
        Clans,
        Shop,
        Admin,
        Security,
        OAuth,
    }
}
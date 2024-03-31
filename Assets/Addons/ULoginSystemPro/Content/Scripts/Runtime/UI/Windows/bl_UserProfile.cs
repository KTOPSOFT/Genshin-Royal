using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using MFPS.ULogin;
using TMPro;
using UnityEngine.UI;

public class bl_UserProfile : bl_LoginProBase
{
    [Serializable]
    public class MenuWindow
    {
        public string Name;
        public GameObject Window;
        public Button OpenButton;
        public bl_EventHandler.UEvent onOpen;
        public bl_EventHandler.UEvent onBack;

        public void SetActive(bool active)
        {
            if (Window != null) Window.SetActive(active);
            if (OpenButton != null) OpenButton.interactable = !active;
            onOpen?.Invoke();
        }

        public void Back()
        {
            onBack?.Invoke();
        }
    }

    [Header("Windows")]
    public string firstWindow = "list";
    public List<MenuWindow> windows;

    public int CurrentWindow { get; set; } = -1;

    [Header("References")]
    [SerializeField] private GameObject accountUI = null;
    [SerializeField] private TextMeshProUGUI NameText = null;
    [SerializeField] private TextMeshProUGUI ProfileNameText = null;
    [SerializeField] private TextMeshProUGUI ScoreText = null;
    [SerializeField] private TextMeshProUGUI KillsText = null;
    [SerializeField] private TextMeshProUGUI DeathsText = null;
    [SerializeField] private TextMeshProUGUI PlayTimeText = null;
    public TextMeshProUGUI ClanText;
    [SerializeField] private TextMeshProUGUI LogText = null;
    [SerializeField] private GameObject ProfileWindow = null;
    [SerializeField] private GameObject SettingsWindow = null;
    [SerializeField] private GameObject ChangeNameWindow = null;
    [SerializeField] private GameObject SuccessWindow = null;
    [SerializeField] private GameObject ChangeNameButton = null;
    [SerializeField] private GameObject ChangePassButton = null;
    [SerializeField] private TMP_InputField CurrentPassNick = null;
    [SerializeField] private TMP_InputField NewNickInput = null;
    [SerializeField] private TMP_InputField CurrentPassInput = null;
    [SerializeField] private TMP_InputField NewPassInput = null;
    [SerializeField] private TMP_InputField RePassInput = null;

    private bool isSettingOpen = false;
    private bool isOpen = false;

    #region Unity Methods
    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        if (bl_DataBase.Instance != null && bl_DataBase.Instance.isLogged)
        {
            OnLogin();
        }
        else
        {
            if (accountUI != null) accountUI.SetActive(false);
        }
        ChangeNameButton.SetActive(bl_LoginProDataBase.Instance.PlayerCanChangeNick);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_DataBase.OnUpdateData += OnUpdateData;
        SetupButtons();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_DataBase.OnUpdateData -= OnUpdateData;
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userInfo"></param>
    void OnUpdateData(LoginUserInfo userInfo)
    {
        OnLogin();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnLogin()
    {
        if (bl_DataBase.Instance == null) return;

        if (ProfileNameText != null)
            ProfileNameText.text = bl_DataBase.Instance.LocalUser.NickName;
        if (NameText != null)
            NameText.text = bl_DataBase.Instance.LocalUser.NickName;
        if (accountUI != null) accountUI.SetActive(true);
        ScoreText.text = bl_DataBase.Instance.LocalUser.Score.ToString();
        KillsText.text = bl_DataBase.Instance.LocalUser.Kills.ToString();
        DeathsText.text = bl_DataBase.Instance.LocalUser.Deaths.ToString();
#if CLANS
        if (bl_DataBase.Instance.LocalUser.HaveAClan())
        {
            ClanText.text = bl_DataBase.Instance.LocalUser.Clan.Name;
        }
        else
        {
            ClanText.transform.parent.gameObject.SetActive(false);
        }
#else
        ClanText.transform.parent.gameObject.SetActive(false);
#endif
        PlayTimeText.text = bl_DataBaseUtils.TimeFormat(bl_DataBase.Instance.LocalUser.PlayTime);
        gameObject.SetActive(true);
        SetupButtons();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnSettings()
    {
        isSettingOpen = !isSettingOpen;
        SettingsWindow.SetActive(isSettingOpen);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnProfile()
    {
        isOpen = !isOpen;
        ProfileWindow.SetActive(isOpen);
        if (!isOpen)
        {
            SettingsWindow.SetActive(false);
            ChangeNameWindow.SetActive(false);
        }
        else
        {
            OnLogin();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeName()
    {
        if (bl_DataBase.Instance == null) return;

        string pass = CurrentPassNick.text;
        string nick = NewNickInput.text;
        if (pass != bl_DataBase.Instance.CacheAccessToken)
        {
            Debug.Log("Password doesn't match!");
            SetLog("Password doesn't match!");
            return;
        }
        if (string.IsNullOrEmpty(nick))
        {
            SetLog("Empty nick name");
            return;
        }
        if (nick.Length < 3)
        {
            SetLog("Nick name should have 3 or more characters");
            return;
        }
        StartCoroutine(SetChangeName(nick));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator SetChangeName(string nick)
    {
        WWWForm wf = CreateForm(false, true);
        wf.AddSecureField("id", bl_DataBase.Instance.LocalUser.LoginName);
        wf.AddSecureField("data", nick);
        wf.AddSecureField("type", DBCommands.ACCOUNT_CHANGE_NICKNAME);
        wf.AddSecureField("hash", bl_DataBaseUtils.CreateSecretHash(bl_DataBase.Instance.LocalUser.LoginName));

        using (UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Account), wf))
        {
            yield return www.SendWebRequest();

            if (!bl_DataBaseUtils.IsNetworkError(www))
            {
                if (www.downloadHandler.text.Contains("success"))
                {
                    bl_DataBase.Instance.LocalUser.NickName = nick;
                    ProfileNameText.text = bl_DataBase.Instance.LocalUser.NickName;
                    NameText.text = bl_DataBase.Instance.LocalUser.NickName;
                    Debug.Log("Changed nick name!");
                    SuccessWindow.SetActive(true);
                }
                else
                {
                    Debug.Log(www.downloadHandler.text);
                }
            }
            else
            {
                Debug.LogError(www.error);
            }
        }
    }

    public void ChangePass()
    {
        if (bl_DataBase.Instance == null) return;

        string cp = CurrentPassInput.text;
        string np = NewPassInput.text;
        string rp = RePassInput.text;

        if (cp != bl_DataBase.Instance.CacheAccessToken)
        {
            Debug.Log("Password doesn't match!");
            SetLog("Password doesn't match!");
            return;
        }
        if (np != rp)
        {
            Debug.Log("New password doesn't match!");
            SetLog("New password doesn't match!");
            return;
        }
        if (np.Length < bl_LoginProDataBase.Instance.MinPasswordLenght)
        {
            string t = string.Format("Password should have {0} or more character", bl_LoginProDataBase.Instance.MinPasswordLenght);
            Debug.Log(t);
            SetLog(t);
            return;
        }
        StartCoroutine(SetChangePass(cp, np));
    }

    IEnumerator SetChangePass(string pass, string newpass)
    {
        // Create instance of WWWForm
        WWWForm wf = CreateForm(FormHashParm.ID, true);
        //sets the mySQL query to the amount of rows to load
        wf.AddSecureField("id", bl_DataBase.Instance.LocalUser.ID);
        wf.AddSecureField("type", DBCommands.ACCOUNT_CHANGE_PASSWORD);
        wf.AddSecureField("password", pass);
        wf.AddSecureField("data", newpass);

        //Creates instance to run the php script to access the mySQL database
        using (UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Account), wf))
        {
            //Wait for server response...
            yield return www.SendWebRequest();
            string result = www.downloadHandler.text;

            //check if we have some error
            if (!bl_DataBaseUtils.IsNetworkError(www))
            {
                if (result.Contains("success"))
                {
                    Debug.Log("Change password!");
                    bl_DataBase.Instance.CacheAccessToken = newpass;
                    SuccessWindow.SetActive(true);
                }
                else//Wait, have a error?, please contact me for help with the result of next debug log.
                {
                    // ErrorType(result);
                }
            }
            else
            {
                Debug.LogError("Error: " + www.error);
                SetLog(www.error);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SignOut()
    {
        if (bl_DataBase.Instance != null)
            bl_DataBase.Instance.LocalUser = new LoginUserInfo();
        bl_DataBase.Instance.isLogged = false;
        if (bl_Lobby.Instance != null) { bl_Lobby.Instance.SignOut(); }
        if (bl_PhotonNetwork.Instance != null) bl_PhotonNetwork.LocalPlayer.NickName = string.Empty;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    /// <summary>
    /// 
    /// </summary>
    private void SetupButtons()
    {
        if (bl_DataBase.IsUserLogged)
        {
            ChangePassButton.SetActive(bl_DataBase.LocalUserInstance.authenticationType == AuthenticationType.ULogin);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OpenRanking()
    {
        bl_RankingPro rp = FindObjectOfType<bl_RankingPro>();
        rp?.Open();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnBackSettingButton()
    {
        if (CurrentWindow < 0) return;

        windows[CurrentWindow].Back();
    }

    void SetLog(string t)
    {
        if (LogText == null) return;
        LogText.text = t;
        Invoke("CleanLog", 5);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="windowName"></param>
    public void OpenWindow(string windowName)
    {
        var id = windows.FindIndex(x => x.Name == windowName);
        if (id == -1)
        {
            Debug.LogWarning($"Window {windowName} doesn't exist.");
            return;
        }
        OpenWindow(id);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="windowName"></param>
    public void OpenWindow(int windowIndex)
    {
        if (windowIndex == CurrentWindow) return;

        CurrentWindow = windowIndex;

        windows.ForEach(x => x.SetActive(false));

        var window = windows[CurrentWindow];
        window.SetActive(true);
    }

    void CleanLog() { LogText.text = string.Empty; }

    private static bl_UserProfile _instance;
    public static bl_UserProfile Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<bl_UserProfile>();
            }
            return _instance;
        }
    }
}
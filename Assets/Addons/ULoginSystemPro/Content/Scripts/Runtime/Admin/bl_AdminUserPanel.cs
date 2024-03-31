using UnityEngine;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_AdminUserPanel : bl_LoginProBase
    {
        [SerializeField] private GameObject infoPanel = null;
        [SerializeField] private InputField SearchInput = null;
        [SerializeField] private Text UserNameText = null;
        [SerializeField] private Text KillsText = null;
        [SerializeField] private Text ScoreText = null;
        [SerializeField] private Text DeathsText = null;
        [SerializeField] private Text PlayTimeText = null;
        [SerializeField] private Text IPText = null;
        [SerializeField] private Text StatusText, coinsText, dateText = null;
        [SerializeField] private GameObject[] BanUI = null;
        [SerializeField] private Button[] StatusButtons = null;
        [SerializeField] private GameObject[] opButtons;
        
        public bl_ConfirmationWindow confirmationWindow;

        private LoginUserInfo m_currentUser;
        public LoginUserInfo CurrentUser
        {
            get => m_currentUser;
            set => m_currentUser = value;
        }

        private bool isRequesting = false;

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            foreach (GameObject g in BanUI) { g.SetActive(false); }
            foreach (Button g in StatusButtons) { g.gameObject.SetActive(false); }
            foreach (var item in opButtons)
            {
                item.SetActive(false);
            }
            bl_AdminWindowManager.SetLoading(false);
        }

        private void OnEnable()
        {
            infoPanel.SetActive(CurrentUser != null);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Search()
        {
            string user = SearchInput.text;
            if (string.IsNullOrEmpty(user))
                return;
            if (isRequesting)
                return;

            SetBusy(true);
            WWWForm wf = CreateForm(false, true);
            wf.AddSecureField("name", user);
            wf.AddSecureField("type", DBCommands.ADMIN_GET_USER_STATS);
            wf.AddSecureField("hash", bl_DataBaseUtils.CreateSecretHash(user));

            var url = GetURL(bl_LoginProDataBase.URLType.Admin);
            WebRequest.POST(url, wf, (result) =>
             {
                 if (result.isError)
                 {
                     result.PrintError();
                     return;
                 }

                 string raw = result.RawText;
                 string[] split = raw.Split("|"[0]);
                 if (split[0].Contains("success"))
                 {
                     LoginUserInfo info = new LoginUserInfo();
                     info.LoginName = split[1];
                     info.Kills = split[2].ToInt();
                     info.Deaths = split[3].ToInt();
                     info.Score = split[4].ToInt();
                     info.IP = split[5];
                     info.UserStatus = (LoginUserInfo.Status)split[6].ToInt();
                     info.PlayTime = split[7].ToInt();
                     info.NickName = split[8];
                     info.ID = split[9].ToInt();
                     if (split.Length >= 11) info.Coins = LoginUserInfo.ParseCoins(split[10]);
                     if (split.Length >= 12) info.SetUserDate(split[11]);

                     ShowUserInfo(info);
                     CurrentUser = info;

                     bl_AdminWindowManager.Log("information obtained.");
                 }
                 else
                 {
                     bl_AdminWindowManager.Log(raw);
                 }
                 SetBusy(false);
             });
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetStatsOfCurrentUser()
        {
            if(CurrentUser == null || string.IsNullOrEmpty(CurrentUser.NickName))
            {
                Debug.LogWarning("There's not user selected.");
                return;
            }

            confirmationWindow.AskConfirmation("Reset user statistics?", () =>
            {
                SetBusy(true);
                string data = "kills='0',deaths='0',score='0',playtime='0'";
                WWWForm wf = CreateForm(false, true);
                wf.AddSecureField("name", CurrentUser.ID);
                wf.AddSecureField("type", DBCommands.ADMIN_UPDATE_VALUES);
                wf.AddSecureField("unsafe", data);
                wf.AddSecureField("hash", bl_DataBaseUtils.CreateSecretHash(CurrentUser.ID.ToString()));

                var url = GetURL(bl_LoginProDataBase.URLType.Admin);
                WebRequest.POST(url, wf, (result) =>
                 {
                     if (result.isError)
                     {
                         result.PrintError();
                         return;
                     }

                     if (result.Text.Contains("done"))
                     {
                         CurrentUser.Kills = 0;
                         CurrentUser.Deaths = 0;
                         CurrentUser.Score = 0;
                         CurrentUser.PlayTime = 0;
                         ShowUserInfo(CurrentUser);
                         bl_AdminWindowManager.Log("Player stats updated.");
                     }
                     else
                     {
                         result.Print(true);
                     }
                     SetBusy(false);
                 });
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void IntentBan()
        {
            bl_AdminWindowManager.ShowConfirmationInputWindow("BAN REASON", (input) =>
            {
                Ban(input);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void Ban(string reason)
        {
            if (CurrentUser == null)
            {
                bl_AdminWindowManager.Log("Don't have any user to ban");
                return;
            }
            if(CurrentUser.UserStatus == LoginUserInfo.Status.Admin)
            {
                bl_AdminWindowManager.Log("Admins can't be banned.");
                return;
            }
            if(CurrentUser.UserStatus == LoginUserInfo.Status.Moderator)
            {
                if (bl_DataBase.Instance == null || bl_DataBase.Instance.LocalUser == null || bl_DataBase.Instance.LocalUser.UserStatus != LoginUserInfo.Status.Admin)
                {
                    bl_AdminWindowManager.Log("You have to be logged as Admin to ban a Moderator."); 
                    return;
                }
            }
            if (string.IsNullOrEmpty(reason))
            {
                bl_AdminWindowManager.Log("Write an reason to ban this user.");
                return;
            }
            if (isRequesting)
                return;

            confirmationWindow.AskConfirmation($"Ban {CurrentUser.NickName}?", () =>
            {
                SetBusy(true);
                WWWForm wf = CreateForm(false, true);
                wf.AddSecureField("name", CurrentUser.LoginName);
                wf.AddSecureField("userId", CurrentUser.ID);
                wf.AddSecureField("data", reason);
                wf.AddSecureField("ip", CurrentUser.IP);
                var author = (!bl_DataBase.IsUserLogged || string.IsNullOrEmpty((string)bl_DataBase.Instance.LocalUser.NickName))
                ? "Admin" : (string)bl_DataBase.Instance.LocalUser.NickName;
                wf.AddSecureField("author", author);
                wf.AddSecureField("type", DBCommands.ADMIN_BAN);
                wf.AddSecureField("hash", bl_DataBaseUtils.CreateSecretHash(CurrentUser.LoginName));                

                var url = GetURL(bl_LoginProDataBase.URLType.Admin);
                WebRequest.POST(url, wf, (result) =>
                {
                    if (result.isError)
                    {
                        result.PrintError();
                        return;
                    }

                    if (result.resultState == ULoginResult.Status.Success)
                    {
                        CurrentUser.UserStatus = LoginUserInfo.Status.Banned;
                        ShowUserInfo(CurrentUser);
                        bl_AdminWindowManager.Log("Player banned");
                        BanUI[0].SetActive(false);
                        BanUI[1].SetActive(true);
                    }
                    else
                    {
                        result.Print(true);
                    }
                    SetBusy(false);
                });
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void UnBan()
        {
            if (CurrentUser == null || CurrentUser.UserStatus != LoginUserInfo.Status.Banned) return;
            if (isRequesting)
                return;

            confirmationWindow.AskConfirmation($"Restore {CurrentUser.NickName} account?", () =>
            {
                SetBusy(true);
                WWWForm wf = CreateForm(false, true);
                wf.AddSecureField("name", CurrentUser.LoginName);
                wf.AddSecureField("type", DBCommands.ADMIN_UNBAN);
                wf.AddSecureField("hash", bl_DataBaseUtils.CreateSecretHash(CurrentUser.LoginName));

                var url = GetURL(bl_LoginProDataBase.URLType.Admin);
                WebRequest.POST(url, wf, (result) =>
                {
                    if (result.isError)
                    {
                        result.PrintError();
                        return;
                    }

                    if (result.resultState == ULoginResult.Status.Success)
                    {
                        CurrentUser.UserStatus = LoginUserInfo.Status.NormalUser;
                        ShowUserInfo(CurrentUser);
                        bl_AdminWindowManager.Log("Player account restored");
                        BanUI[0].SetActive(true);
                        BanUI[1].SetActive(false);
                    }
                    else
                    {
                        result.Print(true);
                    }
                    SetBusy(false);
                });
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void ChangeStatus(int newstatus)
        {
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.LoginName)) return;
            if((LoginUserInfo.Status)newstatus == LoginUserInfo.Status.Admin || (LoginUserInfo.Status)newstatus == LoginUserInfo.Status.Moderator)
            {
#if !UNITY_EDITOR
                if(bl_DataBase.Instance == null || bl_DataBase.Instance.LocalUser.UserStatus != LoginUserInfo.Status.Admin)
                {
                     bl_AdminWindowManager.Log("Only Admins can rise other Admins or Moderators.");
                    return;
                }
#endif
            }
            if (isRequesting)
                return;

            confirmationWindow.AskConfirmation($"Change account role?", () =>
            {
                foreach (Button g in StatusButtons) { g.interactable = false; }
                SetBusy(true);
                WWWForm wf = CreateForm(false, true);
                wf.AddSecureField("name", CurrentUser.LoginName);
                wf.AddSecureField("type", DBCommands.ADMIN_CHANGE_USER_STATUS);
                wf.AddSecureField("data", newstatus);
                wf.AddSecureField("hash", bl_DataBaseUtils.CreateSecretHash(CurrentUser.LoginName));

                var url = GetURL(bl_LoginProDataBase.URLType.Admin);
                WebRequest.POST(url, wf, (result) =>
                {
                    if (result.isError)
                    {
                        result.PrintError();
                        return;
                    }

                    if (result.resultState == ULoginResult.Status.Success)
                    {
                        CurrentUser.UserStatus = (LoginUserInfo.Status)newstatus;
                        ShowUserInfo(CurrentUser);
                        bl_AdminWindowManager.Log("Player role changed.");
                        foreach (Button g in StatusButtons) { g.interactable = true; }
                        StatusButtons[newstatus].interactable = false;
                    }
                    else
                    {
                        result.Print(true);
                    }
                    SetBusy(false);
                });
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isBusy"></param>
        public void SetBusy(bool isBusy)
        {
            isRequesting = isBusy;
            bl_AdminWindowManager.SetLoading(isBusy);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public void ShowUserInfo(LoginUserInfo info)
        {
            if (info == null) info = CurrentUser;

            UserNameText.text = string.Format("<b>Name:</b> {0}", info.NickName);
            KillsText.text = string.Format("<b>Kills:</b> {0}", info.Kills);
            DeathsText.text = string.Format("<b>Deaths:</b> {0}", info.Deaths);
            ScoreText.text = string.Format("<b>Score:</b> {0}", info.Score);
            IPText.text = string.Format("<b>IP:</b> {0}", info.IP);
            StatusText.text = string.Format("<b>Status:</b> {0}", info.UserStatus.ToString());
            PlayTimeText.text = string.Format("<b>Play Time:</b> {0}", bl_DataBaseUtils.TimeFormat(info.PlayTime));
            coinsText.text = $"<b>Coins:</b> ";
            for (int i = 0; i < info.Coins.Length; i++)
            {
                coinsText.text += $"- [{bl_MFPS.Coins.GetCoinData(i).CoinName}: <b>{info.Coins[i]}</b>] ";
            }
            dateText.text = $"<b>Register Date:</b> {info.UserDate}";

            foreach (GameObject g in BanUI) { g.SetActive(true); }
            if (info.UserStatus == LoginUserInfo.Status.Banned)
            {
                BanUI[0].SetActive(false);
            }
            else
            {
                BanUI[1].SetActive(false);
            }

            bool canChangeStatus = false;
            if (bl_DataBase.IsUserLogged)
            {
                canChangeStatus = bl_DataBase.LocalUserInstance.UserStatus == LoginUserInfo.Status.Admin;
            }
            else
            {
#if UNITY_EDITOR
                // if player is not logged but is in the editor
                canChangeStatus = true;
#endif
            }

            if (info.UserStatus != LoginUserInfo.Status.Banned && canChangeStatus)
            {
                foreach (Button g in StatusButtons) { g.gameObject.SetActive(true); g.interactable = true; }
                StatusButtons[(int)info.UserStatus].interactable = false;
                foreach (var item in opButtons) item.SetActive(true);
            }
            else { foreach (Button g in StatusButtons) { g.gameObject.SetActive(false); } }

            infoPanel.SetActive(true);
        }

        public void LoadLevel(string l)
        {
            bl_UtilityHelper.LoadLevel(l);
        }

        public bool HasUserFetched => CurrentUser != null && !string.IsNullOrEmpty(CurrentUser.LoginName);

        private static bl_AdminUserPanel _Instance;
        public static bl_AdminUserPanel Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<bl_AdminUserPanel>();
                }
                return _Instance;
            }
        }
    }
}
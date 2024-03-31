using UnityEngine;
using System;
using MFPS.ULogin;

public class bl_DataBase : bl_LoginProBase
{
    [Header("Local User Info")]  
    public LoginUserInfo LocalUser;

    #region Public properties
    /// <summary>
    /// Is the local player logged into an account?
    /// </summary>
    public bool isLogged 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// Cached session access 'token' locally
    /// </summary>
    public string CacheAccessToken 
    { 
        get; 
        set;
    }

    /// <summary>
    /// Is the local player a 'Guest' = not logged with an account
    /// </summary>
    public bool isGuest 
    { 
        get; 
        set; 
    }

    /// <summary>
    /// The public encryption key received from the server
    /// We used this to encrypt data before send to the server.
    /// </summary>
    public string RSAPublicKey 
    { 
        get; 
        set;
    }

    /// <summary>
    /// The IP with which the player start this session.
    /// </summary>
    public string SessionIP
    {
        get;
        set;
    }

    /// <summary>
    /// Local player database data
    /// </summary>
    public static LoginUserInfo LocalLoggedUser => Instance.LocalUser;
    #endregion

    #region Private members
    private int TasksRunning = 0;
    private bl_LoginProDataBase Data;
    private float StartPlayTime = 0;
    #endregion

    public delegate void OnUpdateDataEvent(LoginUserInfo userInfo);
    public static OnUpdateDataEvent OnUpdateData;

    public static LoginUserInfo LocalUserInstance => Instance.LocalUser;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        gameObject.name = "Database";
        DontDestroyOnLoad(gameObject);
        Data = bl_LoginProDataBase.Instance;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnLogin(LoginUserInfo info)
    {
        LocalUser = info;
        isLogged = true;
        isGuest = false;
        if (Data.BanComprobationInMid)
        {
            float t = Data.CheckBanEach;
            InvokeRepeating(nameof(BanComprobation), t, t);
        }
        if(info.UserStatus == LoginUserInfo.Status.Admin || info.UserStatus == LoginUserInfo.Status.Moderator)
        {
            if (!bl_GameData.Instance.GameTeam.Exists(x => x.UserName == info.NickName))
            {
                bl_GameData.GameTeamInfo ti = new bl_GameData.GameTeamInfo();
                ti.UserName = info.NickName;
                if (info.UserStatus == LoginUserInfo.Status.Admin)
                {
                    ti.m_Role = bl_GameData.GameTeamInfo.Role.Admin;
                    ti.m_Color = Color.red;
                }
                else
                {
                    ti.m_Role = bl_GameData.GameTeamInfo.Role.Moderator;
                    ti.m_Color = Color.blue;
                }
                bl_GameData.Instance.GameTeam.Add(ti);
                bl_GameData.Instance.CurrentTeamUser = bl_GameData.Instance.GameTeam[bl_GameData.Instance.GameTeam.Count - 1];
            }
        }
    }

    /// <summary>
    /// Save a key pair value
    /// </summary>
    /// <param name="key">database colum name</param>
    /// <param name="value">value to save (string or int)</param>
    public void SaveValue(string key, object value, Action callBack = null, bool forceClearData = false)
    {
        var wf = bl_DataBaseUtils.CreateWWWForm(FormHashParm.Name, true);
        wf.AddSecureField("typ", DBCommands.DATABASE_UPDATE_VALUE);
        wf.AddSecureField("id", LocalUser.ID);
        wf.AddSecureField("key", key);
        if (!forceClearData) wf.AddSecureField("data", (string)value);
        else wf.AddField("data", (string)value);

        WebRequest.POST(DataBaseURL, wf, (result) =>
         {
             if (!result.isError)
             {
                 if (result.resultState == ULoginResult.Status.Success)
                 {
                     Debug.Log($"{key} value saved!");
                 }
                 else
                     result.Print();
             }
             else
             {
                 result.PrintError();
             }
             callBack?.Invoke();
         });
    }

    /// <summary>
    /// Sync the local player metadata information in the database
    /// </summary>
    public void SaveUserMetaData(Action callBack = null)
    {
        SaveValue("meta", LocalUser.metaData.ToString(), () => { callBack?.Invoke(); }, true);
    }

    /// <summary>
    /// Update the given fields of the logged user in the external database
    /// </summary>
    public void UpdateUserData(ULoginUpdateFields fieldsToUpdate, Action<bool> callback = null)
    {
        if (LocalUser == null || !isLogged || isGuest)
        {
            Debug.LogWarning("To save data user have to be log-in.");
            return;
        }
        if (fieldsToUpdate == null) return;

        var wf = CreateForm(false, true);
        wf.AddSecureField("hash", bl_DataBaseUtils.CreateSecretHash(LocalUser.LoginName));
        wf.AddSecureField("name", LocalUser.LoginName);
        wf.AddSecureField("id", LocalUser.ID);
        wf.AddSecureField("typ", DBCommands.DATABASE_UPDATE_ASSOC_VALUES);
        wf = fieldsToUpdate.AddToWebForm(wf);

        WebRequest.POST(GetURL(bl_LoginProDataBase.URLType.DataBase), wf, (result) =>
        {
            if (result.isError)
            {
                result.PrintError();
                callback?.Invoke(false);
                return;
            }
            if (ULoginSettings.FullLogs) result.Print();

            if (result.resultState == ULoginResult.Status.Success)
            {
                Debug.LogFormat("Data update successfully for user: {0}", LocalUser.NickName);
                FireUpdateData();
            }
            else if (result.resultState == ULoginResult.Status.Fail)
            {
                Debug.Log("Update user data fail, that could be caused to the data sent were the same that the one already saved.");
            }
            callback?.Invoke(true);
        });
    }

    /// <summary>
    /// Add NEW coins to the user wallet
    /// </summary>
    /// <param name="newCoins"></param>
    public void SaveNewCoins(int newCoins, int coinID)
    {
        UpdateUserCoins(newCoins, coinID, ULoginCoinsOp.Add);
    }

    /// <summary>
    /// Subtract coins from the user wallet
    /// </summary>
    /// <param name="coins"></param>
    public bool SubtractCoins(int coins, int coinID)
    {
        UpdateUserCoins(coins, coinID, ULoginCoinsOp.Deduct);
        return true;
    }

    /// <summary>
    /// Update user coins.
    /// </summary>
    /// <param name="coins"></param>
    /// <param name="coinsOp">add or deduct coins?</param>
    public void UpdateUserCoins(int coins, int coinID, ULoginCoinsOp coinsOp, Action callback = null)
    {
        if (!IsUserLogged) return;

        if(coinsOp == ULoginCoinsOp.Deduct && (LocalUser.Coins[coinID] - coins) <= 0)
        {
            Debug.LogWarning("User wallet shouldn't be negatived.");
        }

        var wf = CreateForm(FormHashParm.Name, true);
        wf.AddSecureField("id", LocalUser.ID);
        wf.AddSecureField("typ", DBCommands.DATABASE_UPDATE_USER_COINS);
        wf.AddSecureField("values", coins);
        wf.AddSecureField("param", coinID);
        wf.AddSecureField("key", (int)coinsOp);

        WebRequest.POST(GetURL(bl_LoginProDataBase.URLType.DataBase), wf, (result) =>
        {
            if (result.isError)
            {
                Debug.Log(result.RawText);
                result.PrintError();
                return;
            }
            if (ULoginSettings.FullLogs) result.Print();

            if (result.resultState == ULoginResult.Status.Success)
            {
                int newCoins = 0;
                if (int.TryParse(result.Text, out newCoins))
                {
                    LocalUser.SetCoinLocally(coinID, newCoins);
                    Debug.LogFormat("User coins updated, the new total is: {0}", newCoins);
                    bl_EventHandler.DispatchCoinUpdate(null);
                    FireUpdateData();
                }
                else
                {
                    Debug.LogWarning("Unknown response: " + result.RawText);
                }
            }
            else result.Print(true);

            callback?.Invoke();
        });
    }

#if CLANS
    public void SetClanScore(int newScore)
    {
        if (LocalUser.Clan == null || !LocalUser.HaveAClan()) return;

        WWWForm wf = new WWWForm();
        wf.AddField("type", 99);
        wf.AddField("hash", GetUserToken());
        wf.AddField("clanID", LocalUser.Clan.ID);
        wf.AddField("settings", newScore);

        WebRequest.POST(GetURL(bl_LoginProDataBase.URLType.Clans), wf, (r) =>
          {
              if (r.isError)
              {
                  r.PrintError();
                  return;
              }

              if (r.Text.Contains("done"))
              {
                  Debug.Log("Clan Score update");
              }
              else
              {
                  Debug.LogWarning($"Unexpected response: {r.Text}");
              }
          });
    }
#endif

    /// <summary>
    /// Store virtual coins that has been acquired by purchase
    /// if the coins was NOT acquired by a real purchase use SaveNewCoins() function instead
    /// </summary>
    public void SetCoinPurchase(CoinPurchaseData data, Action<bool> callback)
    {
        var wf = CreateForm(FormHashParm.Name, true);
        wf.AddSecureField("typ", DBCommands.DATABASE_SAVE_COINS_PURCHASE);
        wf.AddSecureField("id", LocalUser.ID);
        var jsonData = JsonUtility.ToJson(data);
        wf.AddField("data", jsonData);

        WebRequest.POST(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.DataBase), wf, (result) =>
        {
            result.Print();
            if (result.resultState == ULoginResult.Status.Success)
            {
                LocalUser.SetCoinLocally(data.coinID, int.Parse(result.Text.Trim()));
                bl_EventHandler.DispatchCoinUpdate(null);
            }

            if (callback != null) { callback.Invoke(result.resultState == ULoginResult.Status.Success); }
        });
    }

    /// <summary>
    /// Check every certain time if the player has not been banned
    /// </summary>
    void BanComprobation()
    {
        WWWForm wf = new WWWForm();
        wf.AddField("typ", DBCommands.BANLIST_EXIST_IP_OR_NAME);
        wf.AddField("name", LocalUser.LoginName);
        wf.AddField("ip", LocalUser.IP);

        WebRequest.POST(GetURL(bl_LoginProDataBase.URLType.BanList), wf, (result) =>
          {
              if(result.isError)
              {
                  result.PrintError();
                  CancelInvoke();
                  return;
              }

              if(result.HTTPCode == 202)
              {
                  Debug.Log("You're banned");
                  bl_DataBaseUtils.LoadLevel(0);
              }
          });
    }

    /// <summary>
    /// Verify if user exist
    /// </summary>
    public static void CheckIfUserExist(MonoBehaviour reference, string where, string index, Action<bool> callback)
    {
        bl_ULoginWebRequest webRequest = new bl_ULoginWebRequest(reference);
        var wf = bl_DataBaseUtils.CreateWWWForm(FormHashParm.Name, true);
        wf.AddSecureField("typ", DBCommands.DATABASE_CHECK_IF_USER_EXIST);
        wf.AddSecureField("key", where);
        wf.AddSecureField("values", index);

        webRequest.POST(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.DataBase), wf, (result) =>
        {
            if (result.isError)
            {
                if (result.HTTPCode == 409)//response means user doesn't exists
                {
                    callback?.Invoke(false);
                    return;
                }
                else
                {
                    result.PrintError();
                    return;
                }
            }

            callback?.Invoke(result.resultState == ULoginResult.Status.Success);
        });
    }

    /// <summary>
    /// Delete user account from database
    /// </summary>
    /// <param name="userId">The userId of the player account to delete</param>
    /// <param name="callback">Callback after the request is processed, true = account deleted, false = request fail.</param>
    public static void DeleteAccount(int userId, string loginName, Action<bool> callback)
    {
        if (Instance == null) return;
        if (!IsUserLogged)
        {
            Debug.LogWarning("User doesn't have an account");
            callback?.Invoke(false);
            return;
        }
        if(userId != LocalUserInstance.ID)
        {
            Debug.LogWarning("Deleting other users accounts is not allowed from here.");
            callback?.Invoke(false);
            return;
        }
        
        var wf = bl_DataBaseUtils.CreateWWWForm(FormHashParm.ID, true);
        wf.AddSecureField("id", userId);
        wf.AddSecureField("data", loginName);
        wf.AddSecureField("type", DBCommands.ACCOUNT_DELETE_ACCOUNT);
        
        Instance.WebRequest.POST(Instance.GetURL(bl_LoginProDataBase.URLType.Account), wf, (r) =>
        {
            if (r.isError)
            {            
                callback?.Invoke(false);
                r.PrintError();
                if (r.HTTPCode == 405)
                {            
                    Debug.LogWarning("Account parameters doesn't match, players can only delete the account they are currently login.");
                }
                return;
            }

            if(r.HTTPCode == 202)
            {
                callback?.Invoke(true);
            }
            else
            {
                Debug.LogWarning($"Unexpected response: {r.Text}");
                callback?.Invoke(false);
            }
        });
    }

#if SHOP
    /// <summary>
    /// Check if an item has been purchased by the local player
    /// </summary>
    /// <param name="itemType">Check MFPSItemUnlockability.ItemTypeEnum for the list of ID's.</param>
    /// <param name="itemID">The unique ID of the item.</param>
    /// <returns></returns>
    public static bool IsItemPurchased(int itemType, int itemID)
    {
        if (!IsUserLogged || LocalLoggedUser.ShopData == null) return false;
        return LocalLoggedUser.ShopData.isItemPurchase((ShopItemType)itemType, itemID);
    }
#endif

    /// <summary>
    /// Start recording the play time from this moment
    /// </summary>
    public void RecordTime() => StartPlayTime = Time.time;

    /// <summary>
    /// stop record time and save and sum this play time to the store 
    /// in database
    /// </summary>
    public void StopAndSaveTime()
    {
        if (!IsUserLogged || StartPlayTime <= 1)
            return;

        float total = Time.time - StartPlayTime;
        int minutes = Mathf.CeilToInt(total / 60);
        if (minutes <= 1)
            return;

        var wf = CreateForm(FormHashParm.Name, true);
        wf.AddSecureField("id", LocalUser.ID);
        wf.AddSecureField("values", minutes);
        wf.AddSecureField("typ", 3);

        WebRequest.POST(GetURL(bl_LoginProDataBase.URLType.DataBase), wf, (result) =>
        {
            if (result.isError)
            {
                result.PrintError();
                return;
            }
            if (ULoginSettings.FullLogs) result.Print();

            if (result.resultState == ULoginResult.Status.Success)
            {
                LocalUser.PlayTime += minutes;
                TimeSpan t = TimeSpan.FromMinutes((double)LocalUser.PlayTime);
                string answer = string.Format("{0:D2}h:{1:D2}m", t.Hours, t.Minutes);
                Debug.Log("Save Time: " + minutes + " Total Play Time: " + answer);
                FireUpdateData();
            }
        });
    }

    /// <summary>
    /// 
    /// </summary>
    void FireUpdateData() => OnUpdateData?.Invoke(LocalUser);

    /// <summary>
    /// 
    /// </summary>
    public void SignOut()
    {
        LocalUser = new LoginUserInfo();
        isLogged = false;
        isGuest = false;
        CacheAccessToken = string.Empty;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetUserToken()
    {
        return bl_DataBaseUtils.Md5Sum(bl_LoginProDataBase.Instance.SecretKey).ToLower();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetUserTokenComplete()
    {
        return bl_DataBaseUtils.Md5Sum(LocalUser.LoginName + bl_LoginProDataBase.Instance.SecretKey).ToLower();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bl_UserMetaData GetLocalUserMetaData()
    {
        if (!IsUserLogged || Instance.LocalUser == null) return null;
        return Instance.LocalUser.metaData;
    }

    public bool IsRunningTask { get { return TasksRunning > 0; } }
    public string DataBaseURL => bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.DataBase);

    /// <summary>
    /// Is the local player logged with an account?
    /// </summary>
    public static bool IsUserLogged => Instance != null && Instance.LocalUser != null && Instance.isLogged && !Instance.isGuest;

    /// <summary>
    /// Is the local player a 'Guest' = not logged with an account
    /// </summary>
    public static bool IsGuest => Instance == null || Instance.isGuest;

    private static bl_DataBase _Instance;
    public static bl_DataBase Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<bl_DataBase>();
            }
            return _Instance;
        }
    }
}
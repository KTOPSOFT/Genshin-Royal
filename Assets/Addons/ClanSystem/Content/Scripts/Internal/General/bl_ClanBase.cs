using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFPS.ULogin;

namespace MFPS.Addon.Clan
{
    public class bl_ClanBase : bl_LoginProBase
    {

        /// <summary>
        /// 
        /// </summary>
        public bl_ClanInfo PlayerClan => bl_ClanManager.Instance.ClanInfo;

        /// <summary>
        /// 
        /// </summary>
        public LoginUserInfo LocalUserInfo => bl_DataBase.LocalUserInstance;

        /// <summary>
        /// 
        /// </summary>
        public new bl_ULoginWebRequest WebRequest => base.WebRequest;

        /// <summary>
        /// 
        /// </summary>
        public string ClanApiUrl => GetURL(bl_LoginProDataBase.URLType.Clans);

        /// <summary>
        /// 
        /// </summary>
        public bl_ClanSettings ClanSettings => bl_ClanSettings.Instance;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void ShowLoading(bool active) => bl_ClanManager.ShowOverAllLoading(active);

        /// <summary>
        /// 
        /// </summary>
        public bool IsPlayerClanNull => PlayerClan == null || PlayerClan.ID == -1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public void ShowNotification(string text) => bl_LobbyUI.ShowOverAllMessage(text);
    }
}
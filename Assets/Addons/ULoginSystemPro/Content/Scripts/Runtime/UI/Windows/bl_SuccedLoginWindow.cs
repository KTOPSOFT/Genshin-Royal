using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MFPS.ULogin
{
    public class bl_SuccedLoginWindow : MonoBehaviour
    {
        [Serializable]
        public class TextParm
        {
            public string Name;
            public TextMeshProUGUI Text;
        }

        public List<TextParm> Texts;
        [SerializeField] private GameObject[] adminObjects = null;
        [SerializeField] private GameObject changePass = null;

        public void ShowUserInfo(LoginUserInfo account)
        {
            SetText("username", $"{bl_GameData.Instance.RolePrefix} {account.NickName}");
            SetText("kills", account.Kills.ToString());
            SetText("deaths", account.Deaths.ToString());
            SetText("score", account.Score.ToString());
            SetText("playtime", bl_DataBaseUtils.TimeFormat(account.PlayTime));

#if CLANS
            if (account.HaveAClan())
                SetText("clan", account.Clan.Name);
            else SetActiveText("clan", false);
#else
            SetActiveText("clan", false);
#endif

            foreach (var item in adminObjects)
            {
                if (item == null) continue;
                item.SetActive(account.HaveModerationRights());
            }

            if (changePass != null) changePass.SetActive(account.authenticationType == AuthenticationType.ULogin);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textName"></param>
        /// <param name="content"></param>
        public void SetText(string textName, string content)
        {
            foreach (TextParm t in Texts)
            {
                if (t.Name == textName)
                {
                    t.Text.text = content;
                    return;
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="textName"></param>
        /// <param name="active"></param>
        public void SetActiveText(string textName, bool active)
        {
            foreach (TextParm t in Texts)
            {
                if (t.Name == textName)
                {
                    t.Text.gameObject.SetActive(active);
                    return;
                }
            }
        }
    }
}
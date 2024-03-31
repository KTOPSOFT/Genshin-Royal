using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MFPS.Addon.Clan
{
    public class bl_PlayerInviteWindow : bl_ClanBase
    {
        [SerializeField] private TMP_InputField nickInput = null;
        [SerializeField] private TextMeshProUGUI logText = null;
        [SerializeField] private GameObject loadingUI = null;
        [SerializeField] private GameObject blockerUI = null;
        private bool isBussy = false;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            loadingUI.SetActive(false);
            CheckPermission();
        }

        /// <summary>
        /// 
        /// </summary>
        public void SendInvitation()
        {
            if (isBussy) return;

            isBussy = true;
            var nick = nickInput.text;
            if (!DoLocalVerifications(nick)) return;

            int clanID = 0;
#if CLANS
            clanID = LocalUserInfo.Clan.ID;
#endif
            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.SEND_INVITATION);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", clanID);
            wf.AddField("userID", nick);

            loadingUI.SetActive(true);
            WebRequest.POST(ClanApiUrl, wf, (r) =>
            {
                loadingUI.SetActive(false);
                if (r.isError) { r.PrintError(); return; }

                string t = r.Text;
                string[] split = t.Split("|"[0]);
                if (split[0].Contains("done"))
                {
                    logText.text = "<color=green>Player Invited.</color>";
                    nickInput.text = "";
                }
                else
                {
                    logText.text = r.Text;
                    Debug.LogWarning($"Unexpected response: {r.Text}");
                }
            });

            isBussy = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool DoLocalVerifications(string nick)
        {
            if (string.IsNullOrEmpty(nick))
            {
                logText.text = "Player Nick Name can't be empty";
                return false;
            }
            if (nick == LocalUserInfo.NickName)
            {
                logText.text = "You can't invite yourself.";
                return false;
            }
            if(PlayerClan.MembersCount >= bl_ClanSettings.Instance.maxClanMembers)
            {
                logText.text = "Clan is full, you can't invite more players at the moment.";
                return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        void CheckPermission()
        {
            if (!bl_DataBase.IsUserLogged) { blockerUI.SetActive(true); return; }
            if (IsPlayerClanNull) { blockerUI.SetActive(true); return; }
            if (!PlayerClan.AllCanInvite && PlayerClan.PlayerRole() < ClanMemberRole.Commander) { blockerUI.SetActive(true); return; }

            blockerUI.SetActive(false);
        }
    }
}
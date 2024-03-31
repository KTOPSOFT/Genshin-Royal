using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MFPS.Addon.Clan
{
    public class bl_ClanDisplayer : bl_ClanBase
    {
        [SerializeField] private GameObject content = null;
        [SerializeField] private TextMeshProUGUI ClanNameText = null;
        [SerializeField] private TextMeshProUGUI ScoreText = null;
        [SerializeField] private TextMeshProUGUI MembersText = null;
        [SerializeField] private TextMeshProUGUI DateText = null;
        [SerializeField] private TextMeshProUGUI DescriptionText = null;
        [SerializeField] private TextMeshProUGUI StatusText = null;
        [SerializeField] private TextMeshProUGUI ClanInfoLogText = null;
        public GameObject JoinButton;
        public GameObject RequestJoinButton;
        public GameObject InvitationsButtons;

        public bl_EventHandler.UEvent onJoinToClan;

        public bl_ClanInfo DisplayingClan { get; private set; }
        private bool isBussy = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clan"></param>
        public void Display(bl_ClanInfo clan)
        {
            DisplayingClan = clan;
            string tagColor = ColorUtility.ToHtmlStringRGB(clan.GetTagColor());
            ClanNameText.text = $"{clan.Name} [<color=#{tagColor}>{clan.Tag}</color>]";
            ScoreText.text = clan.Score.ToString();
            MembersText.text = string.Format($"{clan.MembersCount} / {bl_ClanSettings.Instance.maxClanMembers}");
            DateText.text = clan.Date;
            DescriptionText.text = clan.Description;
            StatusText.text = clan.isPublic ? "PUBLIC" : "PRIVATE";
            ClanInfoLogText.text = "";
            bool isFull = (clan.MembersCount >= bl_ClanSettings.Instance.maxClanMembers);
            if (isFull)
            {
                StatusText.text += " [FULL]";
            }

#if CLANS
            if (!bl_DataBase.Instance.LocalUser.HaveAClan() && !isFull)
            {
                if (bl_DataBase.Instance.LocalUser.Clan != null && bl_DataBase.Instance.LocalUser.Clan.MyInvitations.Contains(clan.ID))
                {
                    InvitationsButtons.SetActive(true);
                    JoinButton.SetActive(false);
                    RequestJoinButton.SetActive(false);
                }
                else
                {
                    InvitationsButtons.SetActive(false);
                    JoinButton.SetActive(clan.isPublic);
                    RequestJoinButton.SetActive(!clan.isPublic);
                }
            }
            else
            {
                InvitationsButtons.SetActive(false);
                JoinButton.SetActive(false);
                RequestJoinButton.SetActive(false);
            }
#endif

            content.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void AcceptClanInvitation()
        {
            if (DisplayingClan == null || isBussy) return;

            isBussy = true;
            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.ACCEPT_INVITATION_TO_CLAN);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", DisplayingClan.ID);
            wf.AddField("userID", LocalUserInfo.ID);
            wf.AddField("settings", LocalUserInfo.Score);
            wf.AddField("param", bl_ClanSettings.Instance.maxClanMembers);

            ShowLoading(true);
            WebRequest.POST(ClanApiUrl, wf, (r) =>
              {
                  isBussy = false;
                  ShowLoading(false);
                  if (r.isError)
                  {
                      r.PrintError();
                      return;
                  }

                  if (r.Text.Contains("done"))
                  {
#if CLANS
                      LocalUserInfo.Clan.ID = DisplayingClan.ID;
                      bl_ClanManager.Instance.ClanInfo = DisplayingClan;
#endif
                      JoinButton.SetActive(false);
                      InvitationsButtons.SetActive(false);
                      onJoinToClan?.Invoke();
                      Debug.Log($"Joined to clan {DisplayingClan.Name} ({DisplayingClan.ID})!");
                  }
                  else Debug.LogWarning($"Unexpected response: {r.Text}");
              });
        }

        /// <summary>
        /// 
        /// </summary>
        public void DenyClanInvitation()
        {
            DenyClanInvitation(DisplayingClan.ID, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clanID"></param>
        /// <param name="callback"></param>
        public void DenyClanInvitation(int clanID, Action callback)
        {
            List<int> ids = new List<int>();
#if CLANS
            ids.AddRange(bl_DataBase.Instance.LocalUser.Clan.MyInvitations.ToArray());
#endif
            ids.Remove(clanID);
            string[] all = ids.Select(x => x.ToString()).ToArray();
            string line = string.Join(",", all);
            if (line.Length > 0) { line += ","; }

            WWWForm wf = new WWWForm();
            wf.AddField("type", ClanCommands.DENY_INVITATION_TO_CLAN);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("settings", line);
            wf.AddField("userID", bl_DataBase.Instance.LocalUser.ID);

            ShowLoading(true);
            WebRequest.POST(ClanApiUrl, wf, (r) =>
            {
                ShowLoading(false);
                if (r.isError)
                {
                    r.PrintError();
                    return;
                }

                if (r.Text.Contains("done"))
                {
#if CLANS
                    bl_DataBase.LocalUserInstance.Clan.MyInvitations = ids;
#endif
                    InvitationsButtons.SetActive(false);
                    callback?.Invoke();
                }
                else Debug.LogWarning($"Unexpected response: {r.Text}");
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void RequestJoinToClan()
        {
            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.REQUEST_JOIN_TO_CLAN);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", DisplayingClan.ID);
            wf.AddField("userID", LocalUserInfo.ID);
            wf.AddField("param", bl_ClanSettings.Instance.maxClanJoinRequests);

            ShowLoading(true);
            WebRequest.POST(ClanApiUrl, wf, (r) =>
            {
                ShowLoading(false);
                if (r.isError)
                {
                    r.PrintError();
                    return;
                }

                if (r.Text.Contains("done"))
                {
                    InvitationsButtons.SetActive(false);
                    RequestJoinButton.SetActive(false);
                    ClanInfoLogText.text = "Join request send";
                }
                else Debug.LogWarning($"Unexpected response: {r.Text}");
            });
        }

        private static bl_ClanDisplayer _instance;
        public static bl_ClanDisplayer Instance
        {
            get
            {
                if (_instance == null) _instance = FindObjectOfType<bl_ClanDisplayer>();
                return _instance;
            }
        }
    }
}
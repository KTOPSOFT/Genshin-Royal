using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MFPS.ULogin;

namespace MFPS.Addon.Clan
{
    public class bl_ClanInformationWindow : bl_ClanBase
    {
        public bl_MyClan myClan;
        public TextMeshProUGUI ClanNameText;
        public TextMeshProUGUI ScoreText;
        public TextMeshProUGUI LeaderText;
        public TextMeshProUGUI MembersText;
        public TextMeshProUGUI DateText;
        public TextMeshProUGUI StatusText;
        public TextMeshProUGUI LastMemberText;
        public TextMeshProUGUI DescriptionText;
        public TextMeshProUGUI InvitationText;

        public TMP_InputField DescriptionInput;
        public GameObject EditInfoUI;
        public GameObject EditButton;
        public Button InvitationAllButton;
        public Button InvitationOLButton;
        public Button StatePublicButton;
        public Button StatePrivateButton;

        private bl_ClanInfo currentClan;
        private bool isClanPublic = false;
        private bool AllCanInvite = false;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            ClanEvents.onClanUpdate += ShowClan;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if (!bl_DataBase.IsUserLogged || PlayerClan == null) return;

            ShowClan(PlayerClan);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDestroy()
        {
            ClanEvents.onClanUpdate -= ShowClan;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public void ShowClan(bl_ClanInfo info)
        {
            if (IsPlayerClanNull) return;

            bl_ClanInfo i = PlayerClan;
            currentClan = i;
            string tagColor = ColorUtility.ToHtmlStringRGB(i.GetTagColor());
            ClanNameText.text = string.Format("<b>NAME:</b> {0} [<color=#{1}>{2}</color>]", i.Name, tagColor, i.Tag);
            var leaderName = i.Leader() == null ? "None" : i.Leader().Name;
            LeaderText.text = string.Format("<b>LEADER:</b> {0}", leaderName);
            ScoreText.text = string.Format("<b>SCORE:</b> {0}", i.Score);
            DateText.text = string.Format("<b>DATE:</b> {0}", i.Date);
            MembersText.text = string.Format("<b>MEMBERS:</b> {0} / 20", i.Members.Count);
            LastMemberText.text = string.Format("<b>LAST MEMBERS:</b> {0}", i.Members[i.Members.Count - 1].Name);
            DescriptionText.text = string.Format("<b>DESCRIPTIONS:\n</b><size=9>{0}</size>", i.Description);
            DescriptionInput.text = i.Description;
            StatusText.text = string.Format("<b>STATE:</b> {0}", i.isPublic ? "PUBLIC" : "PRIVATE");
            InvitationText.text = string.Format("<b>INVITATIONS:</b> {0}", i.AllCanInvite ? "ALL" : "ONLY LEADER");
            StatePrivateButton.interactable = i.isPublic;
            StatePublicButton.interactable = !i.isPublic;
            InvitationAllButton.interactable = !i.AllCanInvite;
            InvitationOLButton.interactable = i.AllCanInvite;
            AllCanInvite = currentClan.AllCanInvite;
            isClanPublic = currentClan.isPublic;
            EditButton.SetActive((int)i.PlayerRole() > 1);
        }

        /// <summary>
        /// 
        /// </summary>
        public void LeaveClan()
        {
#if CLANS
            bl_ClanInfo.ClanMember me = bl_ClanManager.Instance.ClanInfo.Members.Find(x => x.ID == bl_DataBase.Instance.LocalUser.ID);
            if (me == null) return;

            if (bl_DataBase.Instance.LocalUser.Clan.PlayerRole() != ClanMemberRole.Leader)
            {
               myClan.KickMember(me, true);
            }
            else
            {
                if (bl_ClanManager.Instance.ClanInfo.Members.Count > 1)
                {
                    //can't leave yet
                    bl_LobbyUI.ShowOverAllMessage("LEADER CAN'T LEAVE THE CLAN WHILE THERE IS STILL MEMBERS IN IT.");
                }
                else
                {
                    myClan.KickMember(me, true);
                }
            }
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public void EditInfo()
        {
            bool active = EditInfoUI.activeSelf;
            active = !active;
            EditInfoUI.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Save()
        {
            string t = DescriptionInput.text;
            if (string.IsNullOrEmpty(t))
            {
                Debug.Log("Description can't be empty");
                return;
            }
            if (!Regex.IsMatch(t, ClanCommands.CLAN_DESCRIPTION_REGEX))
            {
                Debug.Log("Description contain no allowed characters.");
                return;
            }
            UpdateSettings(t);
        }

        /// <summary>
        /// 
        /// </summary>
        public void IntentLeaveClan()
        {
            bl_LobbyUI.ShowConfirmationWindow("ARE YOU SURE THAT WANT TO <b>LEAVE</b> THE CLAN?", () =>
             {
                 LeaveClan();
             });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        public void UpdateSettings(string description)
        {
            bl_LobbyUI.ShowEmptyLoading(true);
            WWWForm wf = new WWWForm();
            wf.AddField("type", ClanCommands.EDIT_CLAN_SETTINGS);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", bl_ClanManager.Instance.ClanInfo.ID);
            string settings = string.Format("{0},{1},", AllCanInvite ? 1 : 0, isClanPublic ? 1 : 0);
            wf.AddField("settings", settings);
            wf.AddField("desc", description);

            WebRequest.POST(GetURL(bl_LoginProDataBase.URLType.Clans), wf, (r) =>
            {
                bl_LobbyUI.ShowEmptyLoading(false);
                if (r.isError)
                {
                    r.PrintError();
                    return;
                }

                if (r.Text.Contains("done"))
                {
                    var i = bl_ClanManager.Instance.ClanInfo;
                    i.AllCanInvite = AllCanInvite;
                    i.isPublic = isClanPublic;
                    i.Description = description;
                    ClanEvents.DispatchClanUpdate(i);
                    SetEditModeActive(false);
                }
                else
                {
                    Debug.LogWarning($"Unexpected response: {r.Text}");
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void SetEditModeActive(bool active) => EditInfoUI.SetActive(active);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isPublic"></param>
        public void SetClanState(bool isPublic) => isClanPublic = isPublic;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AllCan"></param>
        public void SetClanInvitationSettings(bool AllCan) => AllCanInvite = AllCan;

    }
}
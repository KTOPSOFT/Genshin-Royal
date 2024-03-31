using UnityEngine;
using TMPro;

namespace MFPS.Addon.Clan
{
    public class bl_MemberListUI : bl_ClanBase
    {
        public TextMeshProUGUI NameText;
        [SerializeField] private TextMeshProUGUI scoreText = null;
        public GameObject KickButton;
        public GameObject AscendButton;
        public GameObject DesendButton;

        private bl_ClanInfo.ClanMember MemberInfo;
        private bl_MembersWindow membersWindow;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="mc"></param>
        public void Set(bl_ClanInfo.ClanMember info, bl_MembersWindow mc)
        {
            membersWindow = mc;
            MemberInfo = info;
            NameText.text = MemberInfo.GetNameWithRole();
            scoreText.text = info.Score.ToString();
            KickButton.SetActive(false);
            AscendButton.SetActive(false);
            DesendButton.SetActive(false);

#if CLANS
            var localUser = bl_DataBase.Instance.LocalUser;
            ClanMemberRole pr = localUser.Clan.PlayerRole();
            if (pr != ClanMemberRole.Member)
            {
                int localRoleID = (int)pr;
                int memberRoleID = (int)MemberInfo.Role;
                if (localRoleID > memberRoleID)
                {
                    if (!ClanSettings.onlyLeaderCanKickMembers)
                    {
                        //don't allow kick ourselves
                        KickButton.SetActive(MemberInfo.ID != localUser.ID);
                    }
                    if (!ClanSettings.onlyLeaderCanPromoteMembers)
                    {
                        DesendButton.SetActive(memberRoleID > 0);
                        if ((localRoleID - memberRoleID) >= 2)//parent ranks can't accent others just one above him.
                        {
                            AscendButton.SetActive(true);
                        }
                    }
                }

                if (ClanSettings.allowTransferLeadership && pr == ClanMemberRole.Leader)
                {
                    if (MemberInfo.Role == ClanMemberRole.Commander)
                    {
                        AscendButton.SetActive(true);
                    }
                }
            }

            if (ClanSettings.onlyLeaderCanKickMembers)
            {
                KickButton.SetActive(pr == ClanMemberRole.Leader && MemberInfo.ID != localUser.ID);
            }

            if (ClanSettings.onlyLeaderCanPromoteMembers && pr == ClanMemberRole.Leader && MemberInfo.ID != localUser.ID)
            {
                AscendButton.SetActive(MemberInfo.Role != ClanMemberRole.Leader);
                DesendButton.SetActive(MemberInfo.Role != ClanMemberRole.Member);
            }
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public void Kick()
        {
            bl_ClanManager.AskComfirmationFor("Kick this member?", () =>
            {
                membersWindow.Kick(MemberInfo, false);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void Ascend()
        {
#if CLANS
            if (MemberInfo.Role == ClanMemberRole.Commander)
            {
                bl_ClanManager.AskComfirmationFor("Transfer Leadership to this member?", () =>
                {
                    membersWindow.myClan.TransferLeadership(MemberInfo);
                });
            }
            else
            {
                bl_ClanManager.AskComfirmationFor("Ascend this member?", () =>
                 {
                     membersWindow.ChangeMemberRole(MemberInfo, true);
                 });
            }
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public void Desend()
        {
            bl_ClanManager.AskComfirmationFor("Descend this member?", () =>
            {
                membersWindow.ChangeMemberRole(MemberInfo, false);
            });
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using MFPS.ULogin;

namespace MFPS.Addon.Clan
{
    public class bl_MyClan : bl_ClanBase
    {
        [SerializeField] private GameObject[] windows;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if (!bl_DataBase.IsUserLogged)
            {
                foreach (var g in windows) g.SetActive(false);
                return;
            }

            foreach (var item in windows) item.SetActive(false);
            if (bl_DataBase.LocalUserInstance.HaveAClan())
            {
                windows[1].SetActive(true);
            }
            else
            {
                windows[0].SetActive(true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void KickMember(bl_ClanInfo.ClanMember member, bool self)
        {
            string line = "";
            List<bl_ClanInfo.ClanMember> list = bl_ClanManager.Instance.ClanInfo.Members;
            int idx = list.FindIndex(x => x.ID == member.ID);
            list.RemoveAt(idx);
            for (int i = 0; i < list.Count; i++)
            {
                line += string.Format("{0}-{1},", list[i].ID, (int)list[i].Role);
            }

            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.KICK_MEMBER);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", bl_ClanManager.Instance.ClanInfo.ID);
            wf.AddField("settings", line);
            wf.AddField("userID", member.ID);
#if CLANS
            int dc = 0;
            if (ClanSettings.autoDeleteEmptyClans)
            {
                dc = member.Role == ClanMemberRole.Leader ? 1 : 0;//if this is the leader, that means that the clan will be empty so we have to delete it from DB
                wf.AddField("desc", dc);
            }
#endif

            ShowLoading(true);
            WebRequest.POST(ClanApiUrl, wf, (r) =>
            {
                ShowLoading(false);
                if (r.isError) { r.PrintError(); return; }

                string t = r.Text;
                if (t.Contains("yes"))
                {
                    if (self)
                    {
#if CLANS
                        bl_DataBase.Instance.LocalUser.Clan = new bl_ClanInfo();
#endif
                        bl_ClanManager.Instance.ClanInfo = new bl_ClanInfo();
                        bl_ClanManager.Instance.Initialize();//reset the menu
                    }
                    else
                    {
                        PlayerClan.Score -= member.Score;
                        bl_ClanManager.Instance.RemoveMember(member.ID);
                        bl_MembersWindow.Instance?.InstanceMembers();
                        ClanEvents.DispatchClanUpdate(PlayerClan);
                    }
                }
                else Debug.LogWarning($"Unexpected response: {r.Text}");
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="ascend"></param>
        public void ChangeMemberRole(bl_ClanInfo.ClanMember member, bool ascend)
        {
            int roleID = (int)member.Role;
            roleID = ascend ? roleID + 1 : roleID - 1;
            string line = "";
            List<bl_ClanInfo.ClanMember> list = bl_ClanManager.Instance.ClanInfo.Members;
            int idx = list.FindIndex(x => x.ID == member.ID);
            list[idx].Role = (ClanMemberRole)roleID;
            for (int i = 0; i < list.Count; i++)
            {
                line += string.Format("{0}-{1},", list[i].ID, (int)list[i].Role);
            }

            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.CHANGE_MEMBER_ROLE);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", bl_ClanManager.Instance.ClanInfo.ID);
            wf.AddField("settings", line);

            WebRequest.POST(ClanApiUrl, wf, (r) =>
            {
                if (r.isError)
                {
                    r.PrintError();
                    return;
                }

                if (r.HTTPCode == 202)
                {
                    PlayerClan.SourceMembers = line;
                    var localMember = PlayerClan.GetMember(member.ID);
                    if (localMember != null) localMember.Role = (ClanMemberRole)roleID;
                    bl_MembersWindow.Instance?.InstanceMembers();
                    ClanEvents.DispatchClanUpdate(PlayerClan);
                }
                else Debug.LogWarning($"Unexpected response: {r.Text}");
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="ascend"></param>
        public void TransferLeadership(bl_ClanInfo.ClanMember member)
        {
            string line = "";
            List<bl_ClanInfo.ClanMember> list = bl_ClanManager.Instance.ClanInfo.Members;

            int idx = list.FindIndex(x => x.Role == ClanMemberRole.Leader);
            list[idx].Role = ClanMemberRole.Officer;
            int oldLeaderId = list[idx].ID;

            idx = list.FindIndex(x => x.ID == member.ID);
            list[idx].Role = ClanMemberRole.Leader;

            for (int i = 0; i < list.Count; i++)
            {
                line += string.Format("{0}-{1},", list[i].ID, (int)list[i].Role);
            }

            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.CHANGE_MEMBER_ROLE);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", bl_ClanManager.Instance.ClanInfo.ID);
            wf.AddField("settings", line);

            WebRequest.POST(ClanApiUrl, wf, (r) =>
            {
                if (r.isError)
                {
                    r.PrintError();
                    return;
                }

                if (r.HTTPCode == 202)
                {
                    PlayerClan.SourceMembers = line;
                    var localMember = PlayerClan.GetMember(member.ID);
                    if (localMember != null) localMember.Role = ClanMemberRole.Leader;
                    localMember = PlayerClan.GetMember(oldLeaderId);
                    if (localMember != null) localMember.Role = ClanMemberRole.Officer;

                    bl_MembersWindow.Instance?.InstanceMembers();
                    ClanEvents.DispatchClanUpdate(PlayerClan);
                }
                else Debug.LogWarning($"Unexpected response: {r.Text}");
            });
        }
    }
}
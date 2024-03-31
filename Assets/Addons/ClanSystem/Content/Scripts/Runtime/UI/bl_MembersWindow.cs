using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Addon.Clan
{
    public class bl_MembersWindow : bl_ClanBase
    {
        public bl_MyClan myClan;
        public GameObject memberTemplate;
        public RectTransform membersPanel;

        private List<GameObject> cacheInstanceUI = new List<GameObject>();

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            memberTemplate.SetActive(false);
            if (!bl_DataBase.IsUserLogged || IsPlayerClanNull) return;

            if(PlayerClan.AreMembersFetched)
            {
                InstanceMembers();
            }
            else
            {
                GetClanMembers();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetClanMembers()
        {
            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.GET_CLAN_MEMBERS);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", PlayerClan.ID);

            WebRequest.POST(ClanApiUrl, wf, (r) =>
              {
                  if (r.isError)
                  {
                      r.PrintError();
                      return;
                  }

                  string[] split = r.Text.Split("-"[0]);
                  for (int i = 0; i < split.Length; i++)
                  {
                      if (string.IsNullOrEmpty(split[i])) continue;
                      string[] info = split[i].Split("|"[0]);
                      int id = 0;
                      if (!int.TryParse(info[0], out id))
                      {
                          Debug.Log("corrupted member data: " + info[0]); continue;
                      }
                      if (info.Length < 2) continue;

                      var member = PlayerClan.Members.Find(x => x.ID == id);
                      if (member != null)
                      {
                          member.Name = info[1];
                          member.Score = info[2].ToInt();
                      }
                      else
                      {
                          PlayerClan.Members.Add(new bl_ClanInfo.ClanMember()
                          {
                              Name = info[1],
                              ID = id,
                              Score = info[2].ToInt(),
                              Role = ClanMemberRole.Member,
                          });
                      }
                  }
                  PlayerClan.AreMembersFetched = true;
                  if (VerifyMembers())
                  {
                      VerifyScore();
                      InstanceMembers();
                      ClanEvents.DispatchClanUpdate(PlayerClan);
                  }
              });
        }

        /// <summary>
        /// Check that all members exist
        /// A common problem among developers is when they delete an account from the database manually
        /// and this account player was in a clan, the clan doesn't get automatically updated.
        /// </summary>
        private bool VerifyMembers()
        {
            if (PlayerClan.Members.Count <= 0) return false;
            if (!PlayerClan.AreMembersFetched) return false;

            bool hasChanges = false;
            var members = new List<bl_ClanInfo.ClanMember>();
            foreach (var member in PlayerClan.Members)
            {
                if (!string.IsNullOrEmpty(member.Name))
                {
                    members.Add(member);
                }
                else
                {
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                UpdateClanMembers(members);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        private void VerifyScore()
        {
            int localScore = PlayerClan.GetTotalMembersScore();
            int dbScore = PlayerClan.Score;
            if (localScore > dbScore)
            {
                var wf = new WWWForm();
                wf.AddField("type", ClanCommands.UPDATE_CLAN_SCORE);
                wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
                wf.AddField("clanID", PlayerClan.ID);
                wf.AddField("param", localScore);

                WebRequest.POST(ClanApiUrl, wf, (r) =>
                {
                    if (r.isError)
                    {
                        r.PrintError();
                        return;
                    }

                    if (r.HTTPCode == 202)
                    {
                        PlayerClan.Score = localScore;
                        ClanEvents.DispatchClanUpdate(PlayerClan);
                       if(ULoginSettings.FullLogs) Debug.Log($"Updated clan score from {dbScore} to {localScore}");
                    }
                    else Debug.LogWarning($"Unexpected response: {r.Text}");
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void InstanceMembers()
        {
            if (PlayerClan.Members.Count <= 0) return;

            ClearCacheUI();
            bl_ClanInfo.ClanMember[] list = PlayerClan.Members.ToArray();
            for (int i = 0; i < list.Length; i++)
            {
                GameObject m = Instantiate(memberTemplate);
                m.SetActive(true);
                m.GetComponent<bl_MemberListUI>().Set(list[i], this);
                m.transform.SetParent(membersPanel, false);
                cacheInstanceUI.Add(m);
            }
            memberTemplate.SetActive(false);
        }

        /// <summary>
        /// Update the local player clan member list with the given list
        /// </summary>
        private void UpdateClanMembers(List<bl_ClanInfo.ClanMember> members)
        {
           if(members.Count <= 0)
            {
                Debug.LogWarning("Can't update members with a empty list.");
                return;
            }

            string param = "";
            for (int i = 0; i < members.Count; i++)
            {
                param += $"{members[i].ID}-{(int)members[i].Role},";
            }

            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.UPDATE_CLAN_MEMBERS);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", PlayerClan.ID);
            wf.AddField("param", param);
            wf.AddField("settings", members.Count);

            WebRequest.POST(ClanApiUrl, wf, (r) =>
            {
                if (r.isError)
                {
                    r.PrintError();
                    return;
                }

                if(r.HTTPCode == 202)
                {
                    Debug.Log("Clan members were cleaned automatically!");
                    PlayerClan.Members = members;
                    VerifyScore();
                    InstanceMembers();
                    ClanEvents.DispatchClanUpdate(PlayerClan);
                }
                else
                {
                    r.Print(true);
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveMember(int userID)
        {
            bl_ClanManager.Instance.RemoveMember(userID);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearCacheUI()
        {
            foreach (var item in cacheInstanceUI)
            {
                if (item != null)
                    Destroy(item);
            }
            cacheInstanceUI.Clear();
        }

        public void Kick(bl_ClanInfo.ClanMember member, bool self) => myClan.KickMember(member, self);
        public void ChangeMemberRole(bl_ClanInfo.ClanMember member, bool ascend) => myClan.ChangeMemberRole(member, ascend);

        private static bl_MembersWindow _instance;
        public static bl_MembersWindow Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_MembersWindow>(); }
                return _instance;
            }
        }
    }
}
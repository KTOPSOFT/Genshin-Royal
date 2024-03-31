using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MFPS.Addon.Clan
{
    public class bl_PlayerClanInvitations : bl_ClanBase
    {
        [SerializeField] private GameObject invitationTemplate = null;
        public RectTransform listPanel = null;

        private List<GameObject> cacheUI = new List<GameObject>();
        private bool isBussy = false;
        private float timeSince = 0;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            invitationTemplate.SetActive(false);
            if (!bl_DataBase.IsUserLogged) return;

            LoadInvitations();
        }

        /// <summary>
        /// 
        /// </summary>
        public void LoadInvitations()
        {
            //Allow one request each 5 seconds as minimum interval.
            if (Time.time - timeSince < 5) return;
            timeSince = Time.time;

            if (isBussy) return;

            isBussy = true;
#if CLANS
            if (LocalUserInfo.HaveAClan())
            {
                if(cacheUI.Count > 0)
                {
                    ClearCache();
                }
                return;
            }

            string[] ids = LocalUserInfo.Clan.MyInvitations.Select(x => x.ToString()).ToArray();
            string line = string.Join(",", ids);
            if (line.Length > 0) { line += ","; }

            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.GET_PLAYER_INVITATIONS);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", line);

            WebRequest.POST(ClanApiUrl, wf, (r) =>
              {
                  if (r.isError) { r.PrintError(); return; }

                  string[] split = r.Text.Split("|"[0]);
                  if (split[0].Contains("yes"))
                  {
                      ClearCache();
                      for (int i = 0; i < split.Length; i++)
                      {
                          if (string.IsNullOrEmpty(split[i])) continue;
                          if (i == 0) continue;

                          string[] info = split[i].Split(","[0]);

                          var g = Instantiate(invitationTemplate);
                          g.SetActive(true);
                          g.name = info[2].ToString();
                          g.GetComponent<bl_ClanInvitationUIBinding>().SetClanRequest(info[0], info[1], info[2], this);
                          g.transform.SetParent(listPanel, false);
                          cacheUI.Add(g);
                      }
                  }
                  else
                  {
                         if(r.HTTPCode != 204)
                          Debug.LogWarning($"Unexpected response: {r.Text}");
                  }
              });
#endif
            isBussy = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="updateClanInfo"></param>
        public void AcceptClanInvitation(bl_ClanInfo info)
        {
            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.ACCEPT_INVITATION_TO_CLAN);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", info.ID);
            wf.AddField("userID", LocalUserInfo.ID);
            wf.AddField("settings", LocalUserInfo.Score);
            wf.AddField("param", bl_ClanSettings.Instance.maxClanMembers);

            ShowLoading(true);
            WebRequest.POST(ClanApiUrl, wf, (r) =>
            {
                ShowLoading(false);
                if (r.isError) { r.PrintError(); return; }

                string t = r.Text;
                if (t.Contains("done"))
                {
#if CLANS
                    bl_DataBase.Instance.LocalUser.Clan = info;
#endif
                    ShowNotification($"Joined to the clan <b>{info.Name}</b>!");
                    ClearCache();
                    bl_ClanManager.Instance.GetUserClan(() =>
                    {
                        bl_UserProfile.Instance?.OnLogin();
                        bl_EventHandler.DispatchCoinUpdate(null);
                        bl_ClanManager.Instance.OpenWindow("user-clan");
                    });
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
        /// <param name="clanID"></param>
        /// <param name="callback"></param>
        public void DenyClanInvitation(bl_ClanInfo clan, Action callback = null)
        {
            List<int> ids = new List<int>();
#if CLANS
            ids.AddRange(LocalUserInfo.Clan.MyInvitations.ToArray());
#endif
            ids.Remove(clan.ID);
            string[] all = ids.Select(x => x.ToString()).ToArray();
            string line = string.Join(",", all);
            if (line.Length > 0) { line += ","; }

            WWWForm wf = new WWWForm();
            wf.AddField("type", ClanCommands.DENY_INVITATION_TO_CLAN);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("settings", line);
            wf.AddField("userID", LocalUserInfo.ID);

            WebRequest.POST(ClanApiUrl, wf, (r) =>
            {
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
                    callback?.Invoke();
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        private void ClearCache()
        {
            cacheUI.ForEach(x => { if (x != null) Destroy(x); });
            cacheUI.Clear();
        }
    }
}
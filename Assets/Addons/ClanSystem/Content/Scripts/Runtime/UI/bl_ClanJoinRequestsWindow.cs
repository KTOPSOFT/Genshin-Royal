using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MFPS.Addon.Clan
{
    public class bl_ClanJoinRequestsWindow : bl_ClanBase
    {
        [SerializeField] private GameObject invitationTemplate = null;
        [SerializeField] private RectTransform listPanel = null;

        private List<PlayerRequestData> requestsDatas = new List<PlayerRequestData>();
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

            LoadRequests();
        }

        /// <summary>
        /// 
        /// </summary>
        public void LoadRequests()
        {
            //Allow one request each 5 seconds as minimum interval.
            if (Time.time - timeSince < 5) return;
            timeSince = Time.time;

            if (isBussy) return;

            isBussy = true;
            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.GET_CLAN_JOIN_REQUESTS);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            string[] array = PlayerClan.ClanJoinRequests.Select(x => x.ToString()).ToArray();
            string line = string.Join(",", array);
            wf.AddField("userID", line);

            WebRequest.POST(ClanApiUrl, wf, (r) =>
              {
                  if (r.isError) { r.PrintError(); return; }

                  string[] split = r.Text.Split("|"[0]);
                  if (split[0].Contains("yes"))
                  {                    
                      requestsDatas.Clear();
                      for (int i = 0; i < split.Length; i++)
                      {
                          if (string.IsNullOrEmpty(split[i])) continue;
                          if (i == 0) continue;

                          string[] info = split[i].Split(',');

                          var requestData = new PlayerRequestData();
                          requestData.Parse(info);
                          requestsDatas.Add(requestData);
                      }
                      InstantiateRequests();
                  }
                  else
                  {
                      if (r.HTTPCode == 204)
                      {
                          // No content

                      }
                      else
                      {
                          Debug.LogWarning($"Unexpected response: {r.Text}");
                      }
                  }
              });
            isBussy = false;
        }

        /// <summary>
        /// 
        /// </summary>
        private void InstantiateRequests()
        {
            ClearCache();
            for (int i = 0; i < requestsDatas.Count; i++)
            {
                var request = requestsDatas[i];
                GameObject g = Instantiate(invitationTemplate);
                g.SetActive(true);
                g.GetComponent<bl_ClanInvitationUIBinding>().SetPlayerRequest(request.PlayerName, request.PlayerID, this);
                g.transform.SetParent(listPanel, false);
                cacheUI.Add(g);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        public void AcceptJoinRequest(int id)
        {
            int[] requests = PlayerClan.ClanJoinRequests.Where(x => x != id).ToArray();
            string[] str = requests.Select(x => x.ToString()).ToArray();
            string line = string.Join(",", str);
            if (line.Length > 0) { line += ","; }

            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.ACCEPT_PLAYER_CLAN_JOIN_REQUEST);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", PlayerClan.ID);
            wf.AddField("userID", id);
            wf.AddField("settings", line);
            wf.AddField("param", bl_ClanSettings.Instance.maxClanMembers);

            ShowLoading(true);
            WebRequest.POST(ClanApiUrl, wf, (r) =>
            {
                ShowLoading(false);
                if (r.isError) { r.PrintError(); return; }

                string t = r.Text;
                if (t.Contains("done"))
                {
                    PlayerClan.ClanJoinRequests.Clear();
                    PlayerClan.ClanJoinRequests.AddRange(requests);
                    //let the member list that the data is incomplete
                    PlayerClan.AreMembersFetched = false;
                    int rid = requestsDatas.FindIndex(x => x.PlayerID == id);
                    if(rid != -1) { requestsDatas.RemoveAt(rid); }
                    InstantiateRequests();
                }
                else
                {
                    Debug.LogWarning($"Unexpected response: {r.Text}");
                }
            });
        }

        public void DenyUserJoinRequest(int id, Action callBack)
        {
            int[] array = PlayerClan.ClanJoinRequests.Where(x => x != id).ToArray();
            string[] str = array.Select(x => x.ToString()).ToArray();
            string line = string.Join(",", str);
            if (line.Length > 0) { line += ","; }

            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.DENY_PLAYER_CLAN_JOIN_REQUEST);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", PlayerClan.ID);
            wf.AddField("userID", line);

            WebRequest.POST(ClanApiUrl, wf, (r) =>
            {
                if (r.isError) { r.PrintError(); return; }

                string t = r.Text;
                if (t.Contains("done"))
                {
                    PlayerClan.ClanJoinRequests.Clear();
                    PlayerClan.ClanJoinRequests.AddRange(array);
                    int rid = requestsDatas.FindIndex(x => x.PlayerID == id);
                    if (rid != -1) { requestsDatas.RemoveAt(rid); }
                    InstantiateRequests();
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
        private void ClearCache()
        {
            cacheUI.ForEach(x => { if (x != null) Destroy(x); });
            cacheUI.Clear();
        }

        public class PlayerRequestData
        {
            public string PlayerName;
            public int PlayerID;

            public void Parse(string[] data)
            {
                PlayerName = data[0];
                PlayerID = int.Parse(data[1]);
            }
        }
    }
}
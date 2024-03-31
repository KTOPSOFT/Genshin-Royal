using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Addon.Clan
{
    public class bl_TopClansWindow : bl_ClanBase
    {
        [SerializeField] private GameObject clanInfoTemplate = null;
        [SerializeField] private RectTransform listPanel = null;
        [SerializeField] private GameObject loadingUI = null;

        private List<GameObject> cacheUI = new List<GameObject>();
        private bool infoFetchet = false;
        private float timeSince = 0;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            clanInfoTemplate.SetActive(false);
            loadingUI.SetActive(false);
            if (!infoFetchet)
            {
                GetClans();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetClans()
        {
            if (bl_DataBase.Instance == null) return;

            //Allow one request each 15 seconds as minimum interval.
            if ((Time.time - timeSince < 15) && infoFetchet) return;
            timeSince = Time.time;

            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.GET_TOP_CLANS);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("param", bl_ClanSettings.Instance.topClansFetchLimit);

            loadingUI.SetActive(true);
            WebRequest.POST(ClanApiUrl, wf, (r) =>
              {
                  loadingUI.SetActive(false);
                  if (r.isError)
                  {
                      r.PrintError();
                      return;
                  }

                  string[] split = r.Text.Split("\n"[0]);
                  if (split.Length > 0 && !split[0].Contains("empty"))
                  {
                      cacheUI.ForEach(x => { if (x != null) Destroy(x); });
                      cacheUI.Clear();
                      for (int i = 0; i < split.Length; i++)
                      {
                          if (string.IsNullOrEmpty(split[i]) || !split[i].Contains("|")) continue;
                          string[] info = split[i].Split("|"[0]);
                          GameObject g = Instantiate(clanInfoTemplate) as GameObject;
                          g.SetActive(true);
                          g.GetComponent<bl_TopClanUI>().Set(info[0], info[2], info[1]);
                          g.transform.SetParent(listPanel, false);
                          cacheUI.Add(g);
                      }
                  }
                  infoFetchet = true;
              });
        }
    }
}
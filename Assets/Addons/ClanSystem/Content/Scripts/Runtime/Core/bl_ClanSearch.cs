using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Text.RegularExpressions;

namespace MFPS.Addon.Clan
{
    public class bl_ClanSearch : MonoBehaviour
    {
        public TextMeshProUGUI SearchLogText;
        public TMP_InputField SearchClanInput;
        [HideInInspector] public bl_ClanInfo LastSearchInfo = null;
        [SerializeField] private GameObject loadingUI = null;
        private bool isSearching = false;

        /// <summary>
        /// 
        /// </summary>
        public void SearchClan()
        {
            string clanName = SearchClanInput.text;
            if (string.IsNullOrEmpty(clanName))
            {
                SearchLogText.text = "Clan Name can't be empty";
                return;
            }
            if (!Regex.IsMatch(clanName, @"^[a-zA-Z0-9_ ]+$"))
            {
                SearchLogText.text = "Clan Name contain no allowed characters.";
                return;
            }
            SearchLogText.text = string.Empty;
            StartCoroutine(Search(clanName, true));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clanName"></param>
        /// <param name="display"></param>
        public void DoSearch(string clanName, bool display)
        {
            if (isSearching) return;
            StartCoroutine(Search(clanName, display));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clanName"></param>
        /// <param name="display"></param>
        /// <returns></returns>
        public IEnumerator Search(string clanName, bool display)
        {
            isSearching = true;
            loadingUI.SetActive(true);
            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.GET_CLAN_INFO_FROM_NAME);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", clanName);

            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
            {
                yield return w.SendWebRequest();

                if (!bl_UtilityHelper.IsNetworkError(w))
                {
                    string t = w.downloadHandler.text;
                    string[] split = t.Split("|"[0]);
                    if (split[0].Contains("yes"))
                    {
                        var ci = new bl_ClanInfo();
                        ci.Name = clanName;
                        ci.ID = int.Parse(split[1]);
                        ci.Date = split[2];
                        ci.DecompileMembers(split[3]);
                        ci.Score = int.Parse(split[4]);
                        ci.Description = split[5];
                        ci.Tag = split[7];
                        ci.DecompileSettings(split[6]);
                        if (display)
                        {
                            bl_ClanDisplayer.Instance.Display(ci);
                        }
                        LastSearchInfo = ci;
                    }
                    else
                    {
                        SearchLogText.text = t;
                        Debug.Log(t);
                    }
                }
                else
                {
                    Debug.LogError(w.error);
                }
                loadingUI.SetActive(false);
            }
            isSearching = false;
        }

        private static bl_ClanSearch _instance;
        public static bl_ClanSearch Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_ClanSearch>(); }
                return _instance;
            }
        }
    }
}
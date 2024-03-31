using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace MFPS.Addon.Clan
{
    public class bl_ClanManager : bl_ClanBase
    {
        [Header("Windows")]
        public List<MenuWindow> windows;

        public bl_ClanInfo ClanInfo = new bl_ClanInfo();

        public int CurrentWindow { get; set; } = -1;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            if (!bl_DataBase.IsUserLogged)
            {
                Debug.Log("You need an account access to the clan data.");
                return;
            }
            Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
            if (bl_DataBase.LocalUserInstance.HaveAClan())
            {
                if (IsPlayerClanNull)
                {
                    GetUserClan(() => { OpenWindow("user-clan"); });
                }
                else OpenWindow("user-clan");
            }
            else
            {
                OpenWindow("clan-home");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowName"></param>
        public void OpenWindow(string windowName)
        {
            var id = windows.FindIndex(x => x.Name == windowName);
            if (id == -1)
            {
                Debug.LogWarning($"Clan window {windowName} doesn't exist.");
                return;
            }
            OpenWindow(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowName"></param>
        public void OpenWindow(int windowIndex)
        {
            if (windowIndex == CurrentWindow) return;

            CurrentWindow = windowIndex;

            windows.ForEach(x => x.SetActive(false));

            var window = windows[CurrentWindow];
            window.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void GetUserClan(Action callback = null)
        {
            int clanID = 0;
#if CLANS
            clanID = bl_DataBase.Instance.LocalUser.Clan.ID;
#endif

            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.GET_CLAN_INFO);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", clanID);

            ShowOverAllLoading(true);
            WebRequest.POST(ClanApiUrl, wf, (r) =>
            {
                ShowOverAllLoading(false);
                if (r.isError)
                {
                    r.PrintError();
                    callback?.Invoke();
                    return;
                }

                string[] split = r.Text.Split("|"[0]);
                if (split[0].Contains("yes"))
                {
                    if (ClanInfo == null) { ClanInfo = new bl_ClanInfo(); }
                    ClanInfo.Name = split[1];
                    ClanInfo.Date = split[2];
                    DecompileMembers(split[3]);
                    DecompileClanRequests(split[4]);
                    ClanInfo.Score = int.Parse(split[5]);
                    ClanInfo.ID = clanID;
                    DecompileSettings(split[6]);
                    ClanInfo.Description = split[7];
                    ClanInfo.Tag = split[8];
                    ClanEvents.DispatchClanUpdate(ClanInfo);
                }
                else
                {
                    Debug.LogWarning($"Unexpected response: {r.Text}");
                }
                callback?.Invoke();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        private void DecompileMembers(string line)
        {
            ClanInfo.Members = new List<bl_ClanInfo.ClanMember>();
            ClanInfo.SourceMembers = line;
            string[] split = line.Split(","[0]);
            for (int i = 0; i < split.Length; i++)
            {
                if (string.IsNullOrEmpty(split[i])) continue;
                string[] info = split[i].Split("-"[0]);
                if (info.Length < 2) continue;
                bl_ClanInfo.ClanMember member = new bl_ClanInfo.ClanMember();
                member.ID = int.Parse(info[0]);
                member.Role = (ClanMemberRole)int.Parse(info[1]);
               ClanInfo.Members.Add(member);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        private void DecompileClanRequests(string line)
        {
            ClanInfo.ClanJoinRequests = new List<int>();
            string[] split = line.Split(","[0]);
            for (int i = 0; i < split.Length; i++)
            {
                if (string.IsNullOrEmpty(split[i])) continue;
                ClanInfo.ClanJoinRequests.Add(int.Parse(split[i]));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        void DecompileSettings(string line)
        {
            bl_ClanInfo.DecompileClanSettings(line, ClanInfo);
        }    
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID"></param>
        public void RemoveMember(int userID)
        {
            int index = ClanInfo.Members.FindIndex(x => x.ID == userID);
            if (index == -1) return;

            ClanInfo.Members.RemoveAt(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="action"></param>
        public static void AskComfirmationFor(string text, Action action)
        {
            bl_LobbyUI.ShowConfirmationWindow(text, action);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public static void ShowOverAllLoading(bool active) => bl_LobbyUI.ShowEmptyLoading(active);

        [Serializable]
        public class MenuWindow
        {
            public string Name;
            public GameObject Window;
            public Button OpenButton;
            public bl_EventHandler.UEvent onOpen;

            public void SetActive(bool active)
            {
                if (Window != null) Window.SetActive(active);
                if (OpenButton != null) OpenButton.interactable = !active;
                onOpen?.Invoke();
            }
        }

        private static bl_ClanManager _instance;
        public static bl_ClanManager Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_ClanManager>(); }
                return _instance;
            }
        }
    }
}
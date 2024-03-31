using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

namespace MFPS.Addon.Clan
{
    [Serializable]
    public class bl_ClanInfo
    {
        #region Public members
        public int ID = -1;
        public string Name = "";
        public string Tag = "";
        public int Score = 0;
        public string Date = "";
        public string Description = "";
        public bool AllCanInvite = true;
        public bool isPublic = true;
        public string SourceMembers = string.Empty;
        public List<ClanMember> Members = new List<ClanMember>();
        public List<int> ClanJoinRequests = new List<int>();
        public List<int> MyInvitations = new List<int>();
        #endregion


        #region Private members
        private int[] Settings;
        private string _clanTagPrefix = ""; 
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public bl_ClanInfo()
        {
            ID = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ClanMemberRole PlayerRole()
        {
            if (Members.Count > 0 && bl_DataBase.Instance.LocalUser != null)
            {
                var member = Members.Find(x => x.ID == bl_DataBase.Instance.LocalUser.ID);
                if (member != null)
                    return member.Role;
            }
            return ClanMemberRole.Member;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="split"></param>
        public void GetSplitInfo(string[] split)
        {
            ID = int.Parse(split[12]);
            string[] splitInvitations = split[13].Split(","[0]);
            MyInvitations.Clear();
            for (int i = 0; i < splitInvitations.Length; i++)
            {
                if (string.IsNullOrEmpty(splitInvitations[i])) continue;
                MyInvitations.Add(int.Parse(splitInvitations[i]));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="split"></param>
        public void GetSplitInfo(Dictionary<string, string> split)
        {
            ID = int.Parse(split["clan"]);
            string[] splitInvitations = split["clan_invitations"].Split(", "[0]);
            MyInvitations.Clear();
            for (int i = 0; i < splitInvitations.Length; i++)
            {
                if (string.IsNullOrEmpty(splitInvitations[i])) continue;
                MyInvitations.Add(int.Parse(splitInvitations[i]));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetClanBasicInfo(Action callback = null)
        {
            if (ID == -1)
            {
                callback?.Invoke();
                yield break;
            }

            var wf = new WWWForm();
            wf.AddField("type", ClanCommands.GET_CLAN_BASIC_INFO);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", ID);

            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
            {
                yield return w.SendWebRequest();

                if (!bl_UtilityHelper.IsNetworkError(w))
                {
                    string t = w.downloadHandler.text;
                    string[] split = t.Split('|');
                    if (split[0].Contains("yes"))
                    {
                        for (int i = 0; i < split.Length; i++)
                        {
                            if (string.IsNullOrEmpty(split[i])) continue;

                            Name = split[1];
                            SourceMembers = split[2];
                            DecompileMembers(split[2]);
                            Tag = split[3];
                            DecompileSettings(split[4]);
                        }
                    }
                    else
                    {
                        Debug.Log(t);
                    }
                }
                else
                {
                    Debug.LogError(w.error);
                }
                callback?.Invoke();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        public void DecompileMembers(string line)
        {
            Members = new List<ClanMember>();
            string[] split = line.Split(","[0]);
            for (int i = 0; i < split.Length; i++)
            {
                if (string.IsNullOrEmpty(split[i])) continue;
                string[] info = split[i].Split("-"[0]);
                ClanMember member = new ClanMember();
                member.ID = int.Parse(info[0]);
                member.Role = (ClanMemberRole)int.Parse(info[1]);
                Members.Add(member);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        public void DecompileSettings(string line)
        {
            DecompileClanSettings(line, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="clan"></param>
        public static void DecompileClanSettings(string line, bl_ClanInfo clan)
        {
            string[] split = line.Split(',');
            clan.AllCanInvite = (int.Parse(split[(int)ClanSettingID.SendInvitationsPermision]) == 1 ? true : false);
            clan.isPublic = (int.Parse(split[(int)ClanSettingID.ClanAccess]) == 1 ? true : false);

            clan.Settings = new int[split.Length];
            for (int i = 0; i < split.Length; i++)
            {
                if (!string.IsNullOrEmpty(split[i]))
                    clan.Settings[i] = int.Parse(split[i]);
            }

            string tagColor = ColorUtility.ToHtmlStringRGB(clan.GetTagColor());
            clan._clanTagPrefix = $"[<color=#{tagColor}>{clan.Tag}</color>]";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ClanMember Leader()
        {
            return Members.Find(x => x.Role == ClanMemberRole.Leader);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settingID"></param>
        /// <returns></returns>
        public int GetSetting(ClanSettingID settingID)
        {
            int id = (int)settingID;
            if (id >= Settings.Length) return -1;

            return Settings[id];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Color GetTagColor()
        {
            if (Settings == null || Settings.Length <= 0) return Color.white;
            return bl_ClanSettings.Instance.GetTagColor(Settings[(int)ClanSettingID.ClanTagColorID]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetTagPrefix() => _clanTagPrefix;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public ClanMember GetMember(int userID)
        {
            return Members.Find(x => x.ID == userID);
        }

        /// <summary>
        /// 
        /// </summary>
        public int MembersCount { get { return Members.Count; } }

        /// <summary>
        /// Sum all the members score
        /// </summary>
        /// <returns></returns>
        public int GetTotalMembersScore()
        {
            int score = 0;
            for (int i = 0; i < Members.Count; i++)
            {
                score += Members[i].Score;
            }
            return score;
        }

        /// <summary>
        /// 
        /// </summary>
        private bool _membersFetched = false;
        public bool AreMembersFetched
        {
            get => _membersFetched;
            set => _membersFetched = value;
        }

        [Serializable]
        public class ClanMember
        {
            public int ID = 0;
            public string Name = "";
            public int Score = 0;
            public ClanMemberRole Role = ClanMemberRole.Member;

            public string GetNameWithRole() { return string.Format("{0} <size=9><b>[{1}]</b></size>", Name, Role.ToString().ToUpper()); }
        }
    }

    [Serializable]
    public class bl_ClanRequestStatus
    {
        public int RequestID = 0;
        public int Status = 0;
    }
}
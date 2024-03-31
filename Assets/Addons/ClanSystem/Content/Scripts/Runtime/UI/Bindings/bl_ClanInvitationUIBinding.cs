using UnityEngine;
using TMPro;

namespace MFPS.Addon.Clan
{
    public class bl_ClanInvitationUIBinding : bl_ClanBase
    {
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI ScoreText;
        public GameObject Buttons;

        private int userID = 0;
        private bl_ClanInfo clanInfo;
        private bl_PlayerClanInvitations playerClanInvitations;
        private bl_ClanJoinRequestsWindow clanJoinRequests;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="ID"></param>
        public void SetPlayerRequest(string Name, int ID, bl_ClanJoinRequestsWindow manager)
        {
            clanJoinRequests = manager;
            userID = ID;
            NameText.text = Name;
#if CLANS
        Buttons.SetActive((int)PlayerClan.PlayerRole() > 0);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="score"></param>
        /// <param name="_clanID"></param>
        public void SetClanRequest(string Name, string score, string _clanID, bl_PlayerClanInvitations manager)
        {
            playerClanInvitations = manager;
            clanInfo = new bl_ClanInfo();
            clanInfo.ID = int.Parse(_clanID);
            clanInfo.Name = Name;
            clanInfo.Score = int.Parse(score);
            NameText.text = Name;
            if (ScoreText != null)
                ScoreText.text = string.Format("SCORE: <b>{0}</b>", score);
        }

        public void AcceptClanInvitation()
        {
            playerClanInvitations.AcceptClanInvitation(clanInfo);
        }

        public void DenyClanInvitation()
        {
            playerClanInvitations.DenyClanInvitation(clanInfo, OnFetch);
        }

        public void AcceptPlayerRequest()
        {
            clanJoinRequests.AcceptJoinRequest(userID);
        }

        public void DenyPlayerRequests()
        {
            clanJoinRequests.DenyUserJoinRequest(userID, OnFetch);
        }

        void OnFetch()
        {
            Destroy(gameObject);
        }
    }
}
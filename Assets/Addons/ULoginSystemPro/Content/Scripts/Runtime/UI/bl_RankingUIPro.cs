using TMPro;
using UnityEngine;

namespace MFPS.ULogin
{
    public class bl_RankingUIPro : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Color AdminColor;
        [SerializeField] private Color ModColor;
        [Header("References")]
        [SerializeField] private TextMeshProUGUI RankText = null;
        [SerializeField] private TextMeshProUGUI PlayerNameText = null;
        [SerializeField] private TextMeshProUGUI ScoreText = null;
        [SerializeField] private TextMeshProUGUI KillsText = null;
        [SerializeField] private TextMeshProUGUI DeathsText = null;

        public void SetInfo(LoginUserInfo info, int rank)
        {
            RankText.text = rank.ToString();
            PlayerNameText.text = info.NickName;
            ScoreText.text = info.Score.ToString();
            KillsText.text = info.Kills.ToString();
            DeathsText.text = info.Deaths.ToString();
            CheckStatus(info.UserStatus);
        }

        /// <summary>
        /// 
        /// </summary>
        void CheckStatus(LoginUserInfo.Status status)
        {
            if (status == LoginUserInfo.Status.Admin)
            {
                PlayerNameText.text += " [Admin]";
                PlayerNameText.color = AdminColor;
            }
            else if (status == LoginUserInfo.Status.Moderator)
            {
                PlayerNameText.text += " [Moderator]";
                PlayerNameText.color = ModColor;
            }
        }
    }
}
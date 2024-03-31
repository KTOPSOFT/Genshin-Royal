using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

namespace MFPS.GameModes.CoverPoint
{
    public class bl_CovertPointUI : MonoBehaviour
    {
        public GameObject CoverBarUI;
        public Image BarImage;
        public TextMeshProUGUI BarText;
        public TextMeshProUGUI Team1ScoreText, Team2ScoreText;
        public TextMeshProUGUI Team1NameText, Team2NameText;
        public TextMeshProUGUI TimeText;
        public Image Team1Bar, Team2Bar;
        public Graphic[] Team1UI;
        public Graphic[] Team2UI;
        int maxGoal = 1000;

        /// <summary>
        /// 
        /// </summary>
        public void SetUp()
        {
            foreach (Graphic g in Team1UI) { g.color = Team.Team1.GetTeamColor(); }
            foreach (Graphic g in Team2UI) { g.color = Team.Team2.GetTeamColor(); }
            Team1NameText.text = Team.Team1.GetTeamName().ToUpper();
            Team2NameText.text = Team.Team2.GetTeamName().ToUpper();
            Team1Bar.fillAmount = 0;
            Team2Bar.fillAmount = 0;
            maxGoal = (int)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.RoomGoal];
            bl_UIReferences.Instance.PlayerUI.MaxKillsUI.SetActive(false);
            bl_UIReferences.Instance.PlayerUI.TimeUIRoot.SetActive(false);
            bl_MatchTimeManagerBase.Instance.SetTimeTextRender(TimeText);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            bl_EventHandler.onUIMaskChanged += OnUIMaskChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            bl_EventHandler.onUIMaskChanged -= OnUIMaskChanged;
        }

        void OnUIMaskChanged(RoomUILayers layers)
        {
            bl_UIReferences.Instance.PlayerUI.TimeUIRoot.SetActive(false);
            bl_UIReferences.Instance.PlayerUI.MaxKillsUI.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ShowBar(bool show, float percent = 0, int time = 7)
        {
            BarImage.fillAmount = percent;
            BarText.text = string.Format(bl_GameTexts.CoverPointTime, (time + 1));
            CoverBarUI.SetActive(show);
        }

        public void UpdateScore(int team1, int team2)
        {
            team1 = Mathf.Clamp(team1, 0, maxGoal);
            team2 = Mathf.Clamp(team2, 0, maxGoal);
            Team1ScoreText.text = team1.ToString();
            Team2ScoreText.text = team2.ToString();
            if (team1 > 0)
                Team1Bar.fillAmount = ((float)team1 / (float)maxGoal);
            if (team2 > 0)
                Team2Bar.fillAmount = ((float)team2 / (float)maxGoal);
        }

        private static bl_CovertPointUI _instance;
        public static bl_CovertPointUI Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_CovertPointUI>(); }
                return _instance;
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MFPS.Addon.PlayerSelector
{
    public class bl_PSTeamUI : MonoBehaviour
    {
        public TextMeshProUGUI TeamNameText;
        public TextMeshProUGUI OperatorNameText;
        public Image PreviewImage;
        public GameObject FavUI;
        public GameObject FavButton;
        private Team UITeam = Team.All;

        public void SetUp(bl_PlayerSelectorInfo info, Team team)
        {
            UITeam = team;
            TeamNameText.text = team.GetTeamName().ToUpper();
            OperatorNameText.text = info.Name.ToUpper();
            PreviewImage.sprite = info.Preview;
            bool fav = (team == bl_PlayerSelectorData.GetFavoriteTeam());

            FavButton.SetActive(!fav);
            FavUI.SetActive(fav);
        }

        public void SetTeamFav(Team team)
        {
            bool fav = (team == UITeam);

            FavButton.SetActive(!fav);
            FavUI.SetActive(fav);
        }
    }
}
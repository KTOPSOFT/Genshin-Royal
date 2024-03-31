using UnityEngine;
using TMPro;

namespace MFPS.Addon.Clan
{
    public class bl_TopClanUI : MonoBehaviour
    {
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI MembersText;
        public TextMeshProUGUI ScoreText;

        public void Set(string Name, string Members, string Score)
        {
            NameText.text = Name;
            MembersText.text = string.Format("<b>{0}</b> / {1} MEMBERS", Members, bl_ClanSettings.Instance.maxClanMembers);
            ScoreText.text = Score;
        }

        public void OnClick()
        {
            bl_ClanSearch.Instance?.DoSearch(NameText.text, true);
        }
    }
}
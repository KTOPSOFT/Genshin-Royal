using UnityEngine;
using TMPro;

namespace MFPS.Addon.Clan
{
    public class bl_ClanNameDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            Show();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Show()
        {
            if (nameText == null) return;
#if ULSP && CLANS
            if (!bl_DataBase.IsUserLogged || !bl_DataBase.LocalUserInstance.HaveAClan())
            {
                nameText.text = "";
                return;
            }

            string acronym = bl_DataBase.LocalUserInstance.Clan.Tag;
            string fullName = bl_DataBase.LocalUserInstance.Clan.Name;
            string tagColor = ColorUtility.ToHtmlStringRGB(bl_DataBase.LocalUserInstance.Clan.GetTagColor());
            nameText.text = $"<color=#{tagColor}>[{acronym}]</color> {fullName}";
#endif
        }
    }
}
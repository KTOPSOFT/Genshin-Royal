using UnityEngine.UI;
using TMPro;

namespace MFPS.Addon.LevelManager
{
    public class bl_LevelRender : bl_LevelRenderBase
    {
        [LovattoToogle] public bool AutoFetchLocal = false;
        public Image levelIcon;
        public TextMeshProUGUI levelNameText;
        public TextMeshProUGUI levelNumberText;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if (AutoFetchLocal) FetchLocal();
        }

        /// <summary>
        /// 
        /// </summary>
        public void FetchLocal()
        {
            Render(bl_LevelManager.Instance.GetLevel());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        public override void Render(LevelInfo level)
        {
            if(levelIcon != null) levelIcon.sprite = level.Icon;
            if (levelNameText != null) levelNameText.text = level.Name;
            if (levelNumberText != null) levelNumberText.text = level.LevelID.ToString();
        }
    }
}
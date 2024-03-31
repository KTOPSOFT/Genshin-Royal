using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.LevelManager
{
    public class bl_LevelUnlockRender : MonoBehaviour
    {
        public Image levelIconImg;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI levelText;
        public CanvasGroup canvasGroup;
        public GameObject lockUI;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public void SetUp(UnlockInfo info, int currentLevel)
        {
            levelIconImg.sprite = info.Preview;
            if (nameText != null) nameText.text = info.Name;
            if (levelText != null) levelText.text = $"LEVEL {info.UnlockLevel}";

            bool unlock = currentLevel >= info.UnlockLevel;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = unlock ? 1 : 0.33f;
            }
            if (lockUI != null) lockUI.SetActive(!unlock);
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MFPS.Addon.LevelManager
{
    public class bl_LevelListRender : MonoBehaviour
    {
        [SerializeField] private Image Icon = null;
        [SerializeField] private TextMeshProUGUI NameText = null;
        [SerializeField] private TextMeshProUGUI XpText = null;
        [SerializeField] private CanvasGroup canvasGroup = null;
        public GameObject CurrentMarkUI;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="score"></param>
        /// <param name="manager"></param>
        public void Set(LevelInfo info, int score, bl_LevelList manager)
        {
            Icon.sprite = info.Icon;
            NameText.text = info.Name.ToUpper();
            XpText.text = info.ScoreNeeded.ToString() + "XP";
            gameObject.name = info.Name;

            if (info.IsReached(score))
            {
                canvasGroup.alpha = 1;
            }
            else
            {
                canvasGroup.alpha = 0.3f;
            }

            bool isCurrent = info.IsCurrentLevel(score);
            if (isCurrent)
            {
                manager.SnapTo((RectTransform)transform);             
            }
            if (CurrentMarkUI != null) CurrentMarkUI.SetActive(isCurrent);
        }
    }
}
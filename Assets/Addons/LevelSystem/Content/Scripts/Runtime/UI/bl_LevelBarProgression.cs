using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.LevelManager
{
    public class bl_LevelBarProgression : MonoBehaviour
    {
        [LovattoToogle] public bool showRelativeScore = false;
        public Image barImage;
        public TextMeshProUGUI requiredScoreText;
        public TextMeshProUGUI currentScoreText;

        /// <summary>
        /// 
        /// </summary>
        public void Build(LevelInfo from, LevelInfo to, int score)
        {
            float progress = from.GetProgressTo(to, score);

            if (barImage != null) barImage.fillAmount = progress;

            if(requiredScoreText != null)
            {
                int relativeScore = score;
                int scoreRequired = to.ScoreNeeded;

                if (showRelativeScore)
                {
                     relativeScore = from.GetRelativeScore(score);
                     scoreRequired = from.ScoreDifferenceWith(to);
                }
                requiredScoreText.text = $"{relativeScore} / {scoreRequired} XP Required to Rank Up";
            }

            if (currentScoreText != null) currentScoreText.text = $"{score} XP";
        }
    }
}
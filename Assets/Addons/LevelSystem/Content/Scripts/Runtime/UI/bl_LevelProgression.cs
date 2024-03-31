using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MFPS.Addon.LevelManager
{
    public class bl_LevelProgression : MonoBehaviour
    {
        public bl_LevelRenderBase currentLevelRender;
        public bl_LevelRenderBase nextLevelRender;
        public bl_LevelBarProgression barProgression;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            CalculateLevel();
        }

        /// <summary>
        /// 
        /// </summary>
        void CalculateLevel()
        {
            int score = bl_MFPS.LocalPlayer.Stats.GetAllTimeScore();
            var cl = bl_LevelManager.Instance.GetLevel(score);
            var nl = bl_LevelManager.Instance.GetNextLevel(score);

            currentLevelRender.Render(cl);
            nextLevelRender.Render(nl);

            if(barProgression != null)
            {
                barProgression.Build(cl, nl, score);
            }
        }
    }
}
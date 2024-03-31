using System;
using UnityEngine;
using MFPSEditor;

namespace MFPS.Addon.LevelManager
{
    [Serializable]
    public class LevelInfo
    {
        public string Name = "Level";
        public int ScoreNeeded = 0;
        [SpritePreview(50)] public Sprite Icon;
        [HideInInspector] public int LevelID;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetRelativeScoreNeeded()
        {
            if (LevelID <= 1) return ScoreNeeded;

            int index = LevelID - 1;
            return bl_LevelManager.Instance.GetLevelByID(index).ScoreNeeded - bl_LevelManager.Instance.GetLevelByID(index - 1).ScoreNeeded;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsReached(int score)
        {
            return score >= ScoreNeeded;
        }

        /// <summary>
        /// Calculate the progress from this level to the given level
        /// with the given score as reference
        /// </summary>
        /// <param name="level">Level target</param>
        /// <param name="score"></param>
        /// <returns>0 to 1 progress</returns>
        public float GetProgressTo(LevelInfo level, int score)
        {
            int levelScoreDiff = ScoreDifferenceWith(level);
            int relativeScore = score - ScoreNeeded;

            float p = (float)relativeScore / (float)levelScoreDiff;
            return p;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        public int GetRelativeScore(int score)
        {
            return score - ScoreNeeded;
        }

        /// <summary>
        /// Returns the score difference from this level to the given level
        /// </summary>
        /// <param name="compareLevel"></param>
        /// <returns></returns>
        public int ScoreDifferenceWith(LevelInfo compareLevel)
        {
            return compareLevel.ScoreNeeded - ScoreNeeded;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsCurrentLevel(int score)
        {
            var currentLevel = bl_LevelManager.Instance.GetLevelID(score);
            return (currentLevel + 1) == LevelID;
        }
    }
}
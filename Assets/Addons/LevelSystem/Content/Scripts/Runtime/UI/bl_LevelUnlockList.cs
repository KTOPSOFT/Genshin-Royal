using MFPS.Internal;
using UnityEngine;

namespace MFPS.Addon.LevelManager
{
    public class bl_LevelUnlockList : MonoBehaviour
    {
        public UIListHandler listHandler;
        public int fetchCount = 10;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            listHandler.Initialize();
            InstanceList();
        }

        /// <summary>
        /// 
        /// </summary>
        private void InstanceList(bool force = false)
        {

            if (force) listHandler.Clear();

            if (listHandler.IsInitialize) return;

            var all = bl_LevelManager.Instance.GetFullUnlockList();
            int currentLevel = bl_LevelManager.Instance.GetLevelID(bl_MFPS.LocalPlayer.Stats.GetAllTimeScore()) + 1;

            for (int i = 0; i < all.Count; i++)
            {
                var script = listHandler.InstatiateAndGet<bl_LevelUnlockRender>();
                script.SetUp(all[i], currentLevel);
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.LevelManager
{
    public class bl_LevelList : MonoBehaviour
    {
        public GameObject levelUITemplate;
        public RectTransform panel;
        public ScrollRect scrollRect;

        private List<bl_LevelListRender> cachedUI = new List<bl_LevelListRender>();

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            InstanceLevels();
        }

        /// <summary>
        /// 
        /// </summary>
        public void InstanceLevels(bool force = false)
        {

            if (force)
            {
                foreach (var item in cachedUI)
                {
                    Destroy(item.gameObject);
                }
                cachedUI.Clear();
            }

            if (cachedUI.Count > 0) return;

            int localScore = bl_MFPS.LocalPlayer.Stats.GetAllTimeScore();

            var all = bl_LevelManager.Instance.Levels;
            for (int i = 0; i < all.Count; i++)
            {
                var go = Instantiate(levelUITemplate);
                go.transform.SetParent(panel, false);
                go.SetActive(true);
                var script = go.GetComponent<bl_LevelListRender>();
                script.Set(all[i], localScore, this);
                cachedUI.Add(script);
            }
            levelUITemplate.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        public void SnapTo(RectTransform target)
        {
            if (scrollRect == null || panel.childCount <= 0) return;

            Canvas.ForceUpdateCanvases();
            int ti = target.GetSiblingIndex();
            float v = (float)ti / (float)panel.childCount;

            float xSize = target.sizeDelta.x + 5;
            float totalSize = xSize * panel.childCount;
            float rowP = xSize / totalSize;

            scrollRect.horizontalNormalizedPosition = v + (rowP * 4);
        }
    }
}
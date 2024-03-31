using UnityEngine;

namespace MFPS.Shop
{
    public class bl_CoinsWindow : MonoBehaviour
    {
        [SerializeField] private GameObject content = null;
        public bl_ShopCoinPackUI coinPackTemplate;
        [SerializeField] private RectTransform listPanel = null;
        private bool initialized = false;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            InstancePacks();
        }

        /// <summary>
        /// 
        /// </summary>
        void InstancePacks()
        {
            if (initialized) return;

            var packs = bl_ShopData.Instance.CoinsPacks;
            for (int i = 0; i < packs.Count; i++)
            {
                var go = Instantiate(coinPackTemplate.gameObject) as GameObject;
                go.transform.SetParent(listPanel, false);
                var script = go.GetComponent<bl_ShopCoinPackUI>();
                script.Init(packs[i], i);
            }
            coinPackTemplate.gameObject.SetActive(false);
            initialized = true;
        }

        public void SetActive(bool active) => content.SetActive(active);

        private static bl_CoinsWindow _Instance;
        public static bl_CoinsWindow Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<bl_CoinsWindow>();
                }
                return _Instance;
            }
        }
    }
}
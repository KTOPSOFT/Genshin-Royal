using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Shop
{
    public class bl_ShopAfterPurchaseWindow : MonoBehaviour
    {
        [SerializeField] private GameObject content = null;
        [SerializeField] private Image itemImg = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemIcon"></param>
        public void Show(ShopProductData item)
        {
            if (itemImg != null) itemImg.sprite = item.GetIcon();
            SetActive(true);
        }

        public void SetActive(bool active) => content.SetActive(active);

        private static bl_ShopAfterPurchaseWindow _Instance;
        public static bl_ShopAfterPurchaseWindow Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<bl_ShopAfterPurchaseWindow>();
                }
                return _Instance;
            }
        }
    }
}
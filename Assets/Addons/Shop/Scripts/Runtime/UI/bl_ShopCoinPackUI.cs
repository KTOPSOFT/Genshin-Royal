using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MFPS.Shop
{
    public class bl_ShopCoinPackUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText = null;
        [SerializeField] private TextMeshProUGUI priceText = null;
        [SerializeField] private RawImage iconImage = null;
        [SerializeField] private TextMeshProUGUI bonusText = null;
        [SerializeField] private GameObject[] highLightUI;

        private int packID = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinPack"></param>
        public void Init(bl_ShopData.ShopVirtualCoins coinPack, int id)
        {
            packID = id;
            nameText.text = coinPack.Name;
            priceText.text = $"{bl_ShopData.Instance.PricePrefix}{coinPack.Price}";
            nameText.text = coinPack.Name;
            iconImage.texture = coinPack.PackIcon;

            if (coinPack.GetBonusCoins() > 0)
            {
                bonusText.text = $"+{coinPack.GetBonusCoins()} BONUS";
            }
            else bonusText.gameObject.SetActive(false);

            foreach (var item in highLightUI)
            {
                if (item == null) continue;
                item.SetActive(coinPack.HighlightPack);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void Buy()
        {
            bl_ShopManager.Instance.BuyCoinPack(packID);
        }
    }
}
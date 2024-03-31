using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MFPS.Internal.Structures;
using UnityEngine.UI;
using MFPSEditor;
using MFPS.Internal.Scriptables;

namespace MFPS.Shop
{
    public class bl_PriceUI : MonoBehaviour
    {
        [LovattoToogle] public bool isBuyButton;
        public PriceUI[] prices;

        [Header("Button References")]
        public bl_CheckoutBuyButton buttonTemplate;
        public RectTransform buttonPanel;

        private List<bl_CheckoutBuyButton> buyButtons = new List<bl_CheckoutBuyButton>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemInfo"></param>
        public void ShowPrices(MFPSItemUnlockability itemInfo)
        {
            if (isBuyButton)
            {
                ShowPricesButtons(itemInfo);
                return;
            }

            var allowedCoins = itemInfo.GetAllowedCoins();
            foreach (var pui in prices)
            {
                if (!allowedCoins.Contains(pui.GetCoin()))
                {
                    pui.SetActive(false);
                    continue;
                }
                pui.SetUpCoinPrice(itemInfo.Price);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemInfo"></param>
        public void ShowPricesButtons(MFPSItemUnlockability itemInfo, Action<int> callback = null)
        {
            foreach (var button in buyButtons) button.gameObject.SetActive(false);

            var allowedCoins = itemInfo.GetAllowedCoins();
            foreach (var coin in allowedCoins)
            {
                int coinID = coin;
                while(buyButtons.Count - 1 < coinID)
                {
                    var go = Instantiate(buttonTemplate.gameObject);
                    go.SetActive(false);
                    go.transform.SetParent(buttonPanel, false);
                    buyButtons.Add(go.GetComponent<bl_CheckoutBuyButton>());
                }

                buyButtons[coinID].Init(itemInfo, coin, callback);
            }
            buttonTemplate.gameObject.SetActive(false);
        }

        [Serializable]
        public class PriceUI
        {
            [MFPSCoinID] public int Coin;
            public TextMeshProUGUI PriceText;
            public Image IconImg;
            public bl_CheckoutBuyButton BuyButton;

            public MFPSCoin GetCoin() => bl_MFPS.Coins.GetCoinData(Coin);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="itemPrice"></param>
            public void SetUpCoinPrice(int itemPrice)
            {
                var coin = bl_MFPS.Coins.GetCoinData(Coin);
                if (coin == null) return;

                if(IconImg != null) IconImg.sprite = coin.CoinIcon;
                if (PriceText != null) PriceText.text = coin.DoConversion(itemPrice).ToString();
                SetActive(true);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="active"></param>
            public void SetActive(bool active)
            {
                if (IconImg != null) IconImg.gameObject.SetActive(active);
                if (PriceText != null) PriceText.gameObject.SetActive(active);
            }
        }
    }
}
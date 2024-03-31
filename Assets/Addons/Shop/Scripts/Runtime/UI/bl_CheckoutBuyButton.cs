using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MFPS.Internal.Scriptables;
using MFPS.Internal.Structures;

namespace MFPS.Shop
{
    public class bl_CheckoutBuyButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI priceText = null;
        [SerializeField] private Image coinIconImg = null;
        public CanvasGroup canvasGroup = null;

        private Action<int> PurchaseCallback;
        private MFPSCoin ThisCoin;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="coin"></param>
        public void Init(MFPSItemUnlockability item, MFPSCoin coin, Action<int> callBack = null)
        {
            PurchaseCallback = callBack;
            ThisCoin = coin;
            int coinPrice = coin.DoConversion(item.Price);
            priceText.text = $"<b>{coinPrice}</b> <size=10>{coin.Acronym}</size>";
            coinIconImg.sprite = coin.CoinIcon;

#if ULSP
            if (!bl_UserWallet.HasFundsFor(item.Price, coin))
            {
                canvasGroup.interactable = false;
                canvasGroup.alpha = 0.33f;
            }
            else
            {
                canvasGroup.interactable = true;
                canvasGroup.alpha = 1f;
            }
#endif
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void PurchaseWith()
        {
            PurchaseCallback?.Invoke(ThisCoin);
        }
    }
}
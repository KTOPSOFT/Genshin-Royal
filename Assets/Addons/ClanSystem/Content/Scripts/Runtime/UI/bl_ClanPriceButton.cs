using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MFPS.Internal.Scriptables;
using MFPS.Internal.Structures;

namespace MFPS.Addon.Clan
{
    public class bl_ClanPriceButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI priceText = null;
        [SerializeField] private Image coinIconImg = null;
        [SerializeField] private CanvasGroup canvasGroup = null;

        private Action<int> PurchaseCallback;
        private MFPSCoin ThisCoin;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="coin"></param>
        public void Init(int price, MFPSCoin coin, Action<int> callBack = null)
        {
            PurchaseCallback = callBack;
            ThisCoin = coin;
            int coinPrice = coin.DoConversion(price);
            priceText.text = $"<b>{coinPrice}</b> <size=10>{coin.Acronym}</size>";
            coinIconImg.sprite = coin.CoinIcon;

            if (!bl_UserWallet.HasFundsFor(price, coin))
            {
                canvasGroup.interactable = false;
                canvasGroup.alpha = 0.33f;
            }
            else
            {
                canvasGroup.interactable = true;
                canvasGroup.alpha = 1f;
            }
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
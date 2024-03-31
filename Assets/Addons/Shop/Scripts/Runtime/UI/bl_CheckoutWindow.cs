using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;

namespace MFPS.Shop
{
    public class bl_CheckoutWindow : MonoBehaviour
    {
        [SerializeField] private GameObject content = null;
        [SerializeField] private TextMeshProUGUI itemNameText = null;
        [SerializeField] private Image iconImage = null;
        [SerializeField] private bl_PriceUI priceUI = null;
        [SerializeField] private GameObject[] windows = null;

        public ShopProductData ItemInCart { get; private set; }

        private bool isBusy = false;
        private Action onCompletePurchase;

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            onCompletePurchase = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Checkout(ShopProductData item, Action onComplete = null)
        {
            ItemInCart = item;
            onCompletePurchase = onComplete;
            itemNameText.text = item.Name.ToUpper();
            iconImage.sprite = item.GetIcon();
            priceUI.ShowPricesButtons(item.UnlockabilityInfo, ConfimrPurchase);
            OpenWindow(0);
            content.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator ProcessBuy(int coinID)
        {
#if ULSP && SHOP
            isBusy = true;
            bl_LobbyUI.Instance.overAllEmptyLoading.SetActive(true);
            var wf = bl_DataBaseUtils.CreateWWWForm(ULogin.FormHashParm.Name, true);
            wf.AddSecureField("type", 0);
            wf.AddSecureField("id", bl_DataBase.Instance.LocalUser.ID);
            wf.AddSecureField("name", bl_DataBase.Instance.LocalUser.LoginName);
            //calculate the price with the coin conversion value.
            var priceForCoin = bl_MFPS.Coins.GetCoinData(coinID).DoConversion(ItemInCart.Price);
            wf.AddSecureField("coins", priceForCoin);
            wf.AddSecureField("coinid", coinID);

            //temp add the purchase
            List<bl_ShopPurchase> plist = bl_DataBase.Instance.LocalUser.ShopData.ShopPurchases;
            var sp = new bl_ShopPurchase();
            sp.ID = ItemInCart.ID;
            sp.TypeID = (int)ItemInCart.Type;
            plist.Add(sp);
            wf.AddSecureField("line", bl_ShopData.CompilePurchases(plist));

            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Shop), wf))
            {
                yield return w.SendWebRequest();

                if (!bl_UtilityHelper.IsNetworkError(w))
                {
                    string result = w.downloadHandler.text;
                    if (result.Contains("done"))
                    {
                        bl_DataBase.Instance.LocalUser.ShopData.ShopPurchases = plist;
                        bl_DataBase.Instance.LocalUser.Coins[coinID] -= priceForCoin;
                        bl_EventHandler.DispatchCoinUpdate(null);
                        bl_ShopData.onItemPurchased?.Invoke();
                        OnPurchaseComplete();
                        Debug.Log("Purchase successfully");
                    }
                    else
                    {
                        Debug.LogWarning(result);
                    }
                }
                else
                {
                    Debug.LogError(w.error);
                }
            }
            bl_LobbyUI.Instance.overAllEmptyLoading.SetActive(false);
            isBusy = false;
#else
        yield break;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public void ConfimrPurchase(int coinID)
        {
            if (isBusy || ItemInCart == null) return;
#if ULSP
            if (!bl_DataBase.IsUserLogged)
            {
                OpenWindow(2);
                Debug.Log("You have to log in with an account in order to make a purchase.");
                return;
            }

            if (!bl_UserWallet.HasFundsFor(ItemInCart.Price))
            {
                OpenWindow(1);
                return;
            }
#endif
            StartCoroutine(ProcessBuy(coinID));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        private void OpenWindow(int id)
        {
            foreach (GameObject w in windows) w.SetActive(false);
            windows[id].SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OpenCoinWindow()
        {
            OpenWindow(0);
            bl_CoinsWindow.Instance?.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnPurchaseComplete()
        {
            onCompletePurchase?.Invoke();
            SetActive(false);
            bl_ShopAfterPurchaseWindow.Instance?.Show(ItemInCart);
        }

        /// <summary>
        /// 
        /// </summary>
        public void CreateAccount()
        {
            bl_UtilityHelper.LoadLevel("Login");
        }

        public void SetActive(bool active) => content.SetActive(active);

        private static bl_CheckoutWindow _Instance;
        public static bl_CheckoutWindow Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<bl_CheckoutWindow>();
                }
                return _Instance;
            }
        }
    }
}
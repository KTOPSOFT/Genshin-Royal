using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MFPS.Shop;
using TMPro;
using MFPS.Internal.Structures;

public class bl_ShopManager : MonoBehaviour
{
    #region Public members
    [Header("References")]
    public GameObject ContentUI;
    public GameObject ItemPrefab;
    public GameObject NoCoinsWindow;
    public GameObject BuyPreviewButton;
    public GameObject InfoPanel;
    public Transform ListPanel;
    public TextMeshProUGUI PreviewNameText;
    public bl_PriceUI previewPriceUI;
    public TMP_Dropdown catDropDown;
    public Image[] PreviewBars;
    public Image[] PreviewIcons;
    public AnimationCurve ScaleCurve;
    #endregion

    #region Private members
    private List<ShopProductData> Items = new List<ShopProductData>();
    List<bl_GunInfo> Weapons = new List<bl_GunInfo>();
    private ShopProductData infoPreviewData = null;
    private List<GameObject> cacheUI = new List<GameObject>();
    [HideInInspector] public bl_ShopData.ShopVirtualCoins coinPack;
    private ShopItemType sortByType = ShopItemType.None;
#if SHOP_UIAP
    private bl_UnityIAPShopHandler UnityIAPHandler;
#endif 
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator Start()
    {
        if (!bl_GameData.isDataCached)
        {
            while (!bl_GameData.isDataCached) { yield return null; }
        }
        if (Items == null || Items.Count <= 0) { BuildData(); }
        InstanceItems();
        SetUpUI();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
#if SHOP_UIAP
        if (UnityIAPHandler == null) { UnityIAPHandler = new bl_UnityIAPShopHandler(); }
        bl_UnityIAP.Instance.InitializeIfNeeded();
        bl_ShopData.Instance.onPurchaseComplete += UnityIAPHandler.OnPurchaseResult;
#endif
        bl_ShopData.Instance.onPurchaseFailed += OnPurchaseFailed;
        bl_ShopData.onItemPurchased += OnItemPurchased;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
#if SHOP_UIAP
        bl_ShopData.Instance.onPurchaseComplete -= UnityIAPHandler.OnPurchaseResult;
#endif
        bl_ShopData.Instance.onPurchaseFailed -= OnPurchaseFailed;
        bl_ShopData.onItemPurchased -= OnItemPurchased;
    }

    /// <summary>
    /// 
    /// </summary>
    void BuildData()
    {
        Items.Clear();
        Weapons = bl_GameData.Instance.AllWeapons;
        for (int i = 0; i < Weapons.Count; i++)
        {
            var data = new ShopProductData();
            data.ID = i;
            data.Name = Weapons[i].Name;
            data.Type = ShopItemType.Weapon;
            data.GunInfo = Weapons[i];
            data.UnlockabilityInfo = Weapons[i].Unlockability;
            Items.Add(data);
        }

#if PSELECTOR
        var allPlayers = bl_PlayerSelector.Data.AllPlayers;
        for (int i = 0; i < allPlayers.Count; i++)
        {
            var pinfo = allPlayers[i];
            var data = new ShopProductData();
            data.ID = i;
            data.Name = pinfo.Name;
            data.Type = ShopItemType.PlayerSkin;
            data.PlayerSkinInfo = pinfo;
            data.UnlockabilityInfo = pinfo.Unlockability;
            Items.Add(data);
        }
#endif

#if CUSTOMIZER
        for (int i = 0; i < bl_CustomizerData.Instance.GlobalCamos.Count; i++)
        {
            if (i == 0) continue;//skip the default camo

            var gc = bl_CustomizerData.Instance.GlobalCamos[i];
            var data = new ShopProductData();
            data.ID = i;
            data.Name = gc.Name;
            data.Type = ShopItemType.WeaponCamo;
            data.UnlockabilityInfo = gc.Unlockability;
            data.camoInfo = gc;
            Items.Add(data);
        }
#endif

        // [START] SHOW EMBLEMS AND CALLING CARDS IN THE SHOP WINDOW
#if EACC
        if (bl_ShopData.Instance.showEmeblemsInShop)
        {
            for (int i = 0; i < bl_EmblemsDataBase.Instance.emblems.Count; i++)
            {
                var source = bl_EmblemsDataBase.Instance.emblems[i];
                var data = new ShopProductData();
                data.ID = i;
                data.Name = "Emblem";
                data.Type = ShopItemType.Emblem;
                data.UnlockabilityInfo = source.Unlockability;
                data.SetIcon(source.Emblem);
                Items.Add(data);
            }

            for (int i = 0; i < bl_EmblemsDataBase.Instance.callingCards.Count; i++)
            {
                var source = bl_EmblemsDataBase.Instance.callingCards[i];
                var data = new ShopProductData();
                data.ID = i;
                data.Name = source.Name;
                data.Type = ShopItemType.CallingCard;
                data.UnlockabilityInfo = source.Unlockability;
                data.SetIcon(source.Card);
                Items.Add(data);
            }
        }
#endif
        // [END] SHOW EMBLEMS AND CALLING CARDS IN THE SHOP WINDOW
    }

    /// <summary>
    /// 
    /// </summary>
    public void InstanceItems()
    {
        CleanPanel();
        if (sortByType == ShopItemType.None && bl_ShopData.Instance.randomizeItemsInShop)
        {
            Shuffle(Items);
        }
        bool showLevelBlocked = bl_ShopData.Instance.showBlockedByLevelOnly;
        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            if ((item.Price <= 0 || !item.UnlockabilityInfo.CanBePurchased()) && !bl_ShopData.Instance.ShowFreeItems 
                || item.UnlockabilityInfo.UnlockMethod == MFPSItemUnlockability.UnlockabilityMethod.Hidden ||
                (!showLevelBlocked && item.UnlockabilityInfo.UnlockMethod == MFPSItemUnlockability.UnlockabilityMethod.LevelUpOnly)) continue;

            if (sortByType != ShopItemType.None)
            {
                //sort items
                if (Items[i].Type != sortByType) continue;
            }

            GameObject g = Instantiate(ItemPrefab) as GameObject;
            g.SetActive(true);
            g.GetComponent<bl_ShopItemUIBase>().Setup(item);
            g.transform.SetParent(ListPanel, false);
            if (i == 0)
            {
                Preview(item, false, false);
            }
            cacheUI.Add(g);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowInventory()
    {
        CleanPanel();
        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            if (!item.UnlockabilityInfo.IsUnlocked(item.ID) || item.UnlockabilityInfo.UnlockMethod == MFPSItemUnlockability.UnlockabilityMethod.Hidden) continue;

            GameObject g = Instantiate(ItemPrefab) as GameObject;
            g.SetActive(true);
            g.GetComponent<bl_ShopItemUIBase>().Setup(item);
            g.transform.SetParent(ListPanel, false);
            if (i == 0)
            {
                Preview(item, false);
            }
            cacheUI.Add(g);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PreviewItem(ShopProductData info, Vector3 origin)
    {
        bl_CheckoutWindow.Instance.Checkout(info);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowCurrentPreview()
    {
        if (infoPreviewData == null) return;
#if ULSP
        if (!bl_DataBase.IsUserLogged)
        {
            bl_ShopNotification.Instance?.Show("You need an account to make purchases.").Hide(3);
            Debug.LogWarning("You has to be login in order to make a purchase.");
            return;
        }
#endif
        PreviewItem(infoPreviewData, BuyPreviewButton.GetComponent<RectTransform>().position);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void BuyCoinPack(int id)
    {
        coinPack = bl_ShopData.Instance.CoinsPacks[id];

        //add your payment system process here
        //use the 'coinPack' info like
        // coinPack.Price
        // coinPack.Name

        switch (bl_ShopData.Instance.ShopPayment)
        {
            case bl_ShopData.ShopPaymentTypes.UnityIAP:
                //Unity IAP integration here
                //check this: https://docs.unity3d.com/Manual/UnityIAP.html
#if SHOP_UIAP
                if (bl_UnityIAP.Instance == null)
                {
                    Debug.LogWarning("Unity IAP addon has not been integrated.");
                    return;
                }
                bl_UnityIAP.Instance.BuyProductID(coinPack.ID);
#endif
                break;
            case bl_ShopData.ShopPaymentTypes.Paypal:
#if SHOP_PAYPAL && SHOP
                if(bl_Paypal.Instance == null)
                {
                    Debug.LogWarning("Paypal addon has not been integrated.");
                    return;
                }
                bl_Paypal.Instance.PurchaseCoinPack(coinPack);
#endif
                break;
            case bl_ShopData.ShopPaymentTypes.Steam:
                //Steam IAP integration here
                //check this: https://partner.steamgames.com/doc/features/microtransactions
#if STEAM_MICROTXM
               if(bl_SteamPayment.Instance == null)
                {
                    Debug.LogWarning("Steam Payment addon has not been integrated.");
                    return;
                }

                bl_SteamPayment.Instance.PurchaseCoinPack(coinPack);
#endif
                break;
            case bl_ShopData.ShopPaymentTypes.Other:
                //Your own payment API

                break;
            default:
                Debug.LogWarning("Payment not defined");
                break;
        }

        //once the payment is confirmed add the new coins to the player data using:
        //bl_DataBase.Instance.SaveNewCoins(coinPack.Amount);
        //that's

        bl_CoinsWindow.Instance.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    /// <param name="isOwned"></param>
    public void Preview(ShopProductData info, bool isOwned, bool canPurchase = true)
    {
        foreach (Image i in PreviewIcons) { i.gameObject.SetActive(false); }
        InfoPanel.SetActive(true);
        infoPreviewData = info;
        PreviewNameText.text = info.Name.ToUpper();
        previewPriceUI.ShowPrices(info.UnlockabilityInfo);
        if (info.Type == ShopItemType.Weapon)
        {
            PreviewIcons[0].gameObject.SetActive(true);
            PreviewIcons[0].sprite = info.GunInfo.GunIcon;
            PreviewBars[0].transform.parent.parent.GetComponentInChildren<TextMeshProUGUI>().text = "DAMAGE:";
            PreviewBars[0].fillAmount = (float)info.GunInfo.Damage / 100f;
            PreviewBars[1].transform.parent.parent.GetComponentInChildren<TextMeshProUGUI>().text = "FIRE RATE:";
            PreviewBars[1].fillAmount = info.GunInfo.FireRate / 1f;
            PreviewBars[2].transform.parent.parent.GetComponentInChildren<TextMeshProUGUI>().text = "ACCURACY:";
            PreviewBars[2].fillAmount = (float)info.GunInfo.Accuracy / 5f;
            PreviewBars[3].transform.parent.parent.GetComponentInChildren<TextMeshProUGUI>().text = "WEIGHT:";
            PreviewBars[3].fillAmount = (float)info.GunInfo.Weight / 4f;
        }
        else if (info.Type == ShopItemType.PlayerSkin)
        {
#if PSELECTOR
            PreviewIcons[1].gameObject.SetActive(true);
            PreviewIcons[1].sprite = info.PlayerSkinInfo.Preview;
            var pdm = info.PlayerSkinInfo.Prefab.GetComponent<bl_PlayerHealthManager>();
            var fpc = info.PlayerSkinInfo.Prefab.GetComponent<bl_FirstPersonControllerBase>();
            PreviewBars[0].transform.parent.parent.GetComponentInChildren<TextMeshProUGUI>().text = "HEALTH:";
            PreviewBars[0].fillAmount = pdm.health / 125;
            PreviewBars[1].transform.parent.parent.GetComponentInChildren<TextMeshProUGUI>().text = "SPEED:";
            PreviewBars[1].fillAmount = fpc.GetSpeedOnState(PlayerState.Walking) / 5;
            PreviewBars[2].transform.parent.parent.GetComponentInChildren<TextMeshProUGUI>().text = "REGENERATION:";
            PreviewBars[2].fillAmount = pdm.RegenerationSpeed / 5;
            PreviewBars[3].transform.parent.parent.GetComponentInChildren<TextMeshProUGUI>().text = "NOISE:";
            PreviewBars[3].fillAmount = 0.9f;
#endif
        }
        else if (info.Type == ShopItemType.WeaponCamo)
        {
#if CUSTOMIZER
            PreviewIcons[2].gameObject.SetActive(true);
            PreviewIcons[2].sprite = info.camoInfo.spritePreview();
            InfoPanel.SetActive(false);
#endif
        }
        else if (info.Type == ShopItemType.Emblem)
        {
            PreviewIcons[1].gameObject.SetActive(true);
            PreviewIcons[1].sprite = info.GetIcon();
            InfoPanel.SetActive(false);
        }
        else if (info.Type == ShopItemType.CallingCard)
        {
            PreviewIcons[0].gameObject.SetActive(true);
            PreviewIcons[0].sprite = info.GetIcon();
            InfoPanel.SetActive(false);
        }

        BuyPreviewButton.SetActive(!isOwned && canPurchase);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeCategory(int categoryID)
    {
        if (categoryID == 0) { sortByType = ShopItemType.None; }
        else
        {
            categoryID = categoryID - 1;
            sortByType = bl_ShopData.Instance.categorys[categoryID].itemType;
        }
        InstanceItems();
    }

    /// <summary>
    /// 
    /// </summary>
    void SetUpUI()
    {
        catDropDown.ClearOptions();
        List<TMP_Dropdown.OptionData> cats = new List<TMP_Dropdown.OptionData>();
        List<ShopCategoryInfo> allcats = bl_ShopData.Instance.categorys;
        cats.Add(new TMP_Dropdown.OptionData() { text = "ALL" });
        for (int i = 0; i < allcats.Count; i++)
        {
            TMP_Dropdown.OptionData od = new TMP_Dropdown.OptionData();
            od.text = allcats[i].Name.ToUpper();
            cats.Add(od);
        }
        catDropDown.AddOptions(cats);
        ItemPrefab.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void CleanPanel()
    {
        for (int i = 0; i < cacheUI.Count; i++)
        {
            if (cacheUI[i] == null) continue;
            Destroy(cacheUI[i]);
        }
        cacheUI.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OpenBuyCoinsWindow()
    {
        bl_CoinsWindow.Instance.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnItemPurchased()
    {
        InstanceItems();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnPurchaseFailed()
    {

    }

    private static System.Random rng = new System.Random();
    public void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private static bl_ShopManager _instance = null;
    public static bl_ShopManager Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_ShopManager>(); }
            if (_instance == null && bl_LobbyUI.Instance != null)
            {
                _instance = bl_LobbyUI.Instance.GetComponentInChildren<bl_ShopManager>(true);
            }
            return _instance;
        }
    }
}
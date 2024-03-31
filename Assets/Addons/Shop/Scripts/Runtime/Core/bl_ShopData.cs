using System;
using System.Collections.Generic;
using UnityEngine;
using MFPSEditor;
using MFPS.Shop;

public class bl_ShopData : ScriptableObject
{
    public ShopPaymentTypes ShopPayment = ShopPaymentTypes.UnityIAP;
    [MFPSCoinID] public int CoinToApplyPurchases;
    public List<ShopVirtualCoins> CoinsPacks = new List<ShopVirtualCoins>();
    public List<ShopCategoryInfo> categorys = new List<ShopCategoryInfo>();

    [Header("Settings")]
    public string PricePrefix = "$";
    [LovattoToogle] public bool ShowFreeItems = true;
    [LovattoToogle] public bool showBlockedByLevelOnly = false;
    [LovattoToogle] public bool randomizeItemsInShop = true;
    [LovattoToogle(20)] public bool allowPurchasesWithoutAccount = true;
#if EACC
    [LovattoToogle] public bool showEmeblemsInShop = false;
#endif

    public Action<string, string> onPurchaseComplete;
    public Action onPurchaseFailed;
    public static Action onItemPurchased;

    /// <summary>
    /// Parse the current local purchases into a string that can be stored in the database
    /// </summary>
    /// <param name="purchases"></param>
    /// <returns></returns>
    public static string CompilePurchases(List<bl_ShopPurchase> purchases)
    {
        string line = "";
        for (int i = 0; i < purchases.Count; i++)
        {
            line += string.Format("{0},{1}-", purchases[i].TypeID, purchases[i].ID);
        }
        return line;
    }

    /// <summary>
    /// Parse the purchases from a string into a list of <see cref="bl_ShopPurchase"/>
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public static List<bl_ShopPurchase> DecompilePurchases(string line)
    {
        string[] split = line.Split("-"[0]);
        List<bl_ShopPurchase> list = new List<bl_ShopPurchase>();
        for (int i = 0; i < split.Length; i++)
        {
            if (string.IsNullOrEmpty(split[i])) continue;
            string[] info = split[i].Split(","[0]);

            bl_ShopPurchase sp = new bl_ShopPurchase();
            sp.TypeID = int.Parse(info[0]);
            sp.ID = int.Parse(info[1]);
            list.Add(sp);
        }
        return list;
    }

    /// <summary>
    /// 
    /// </summary>
    public void FireOnPurchaseComplete(string productID, string receipt)
    {
        if (onPurchaseComplete != null) { onPurchaseComplete.Invoke(productID, receipt); }
    }

    /// <summary>
    /// Get the coin pack list as a list of <see cref="ShopProductData"/>
    /// </summary>
    /// <returns></returns>
    public static List<ShopProductData> GetCoinsPackProductList()
    {
        var list = new List<ShopProductData>();
        for (int i = 0; i < Instance.CoinsPacks.Count; i++)
        {
            var pack = Instance.CoinsPacks[i];
            var product = new ShopProductData()
            {
                Name = pack.Name,
                ID = i,
                Type = ShopItemType.CoinPack
            };
            product.SetIcon(pack.PackIcon);

            list.Add(product);
        }
        return list;
    }

    /// <summary>
    /// 
    /// </summary>
    public void FirePurchaseFailed() { if (onPurchaseFailed != null) { onPurchaseFailed.Invoke(); } }

    private static bl_ShopData m_Data;
    public static bl_ShopData Instance
    {
        get
        {
            if (m_Data == null)
            {
                m_Data = Resources.Load("ShopData", typeof(bl_ShopData)) as bl_ShopData;
            }
            return m_Data;
        }
    }

    [System.Serializable]
    public class ShopVirtualCoins
    {
        public string Name;
        public string ID;
        [SerializeField] private int Coins;
        [SerializeField] private int BonusCoins;
        public float Price;
        public Texture2D PackIcon;
        [LovattoToogle] public bool HighlightPack = false;

        /// <summary>
        /// Return all the coins that this package include (Coins + Bonus Coins)
        /// </summary>
        /// <returns></returns>
        public int GetCoins() => Coins + BonusCoins;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetBonusCoins() => BonusCoins;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetPackIndex()
        {
            var all = Instance.CoinsPacks;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].ID == ID) return i;
            }

            Debug.LogWarning($"CoinPack {Name} not listed in ShopData.");
            return -1;
        }


        private string priceString = string.Empty;
        public string PriceString { get { if (string.IsNullOrEmpty(priceString)) { return Price.ToString(); } else { return priceString; } } set { priceString = value; } }
    }

    [System.Serializable]
    public enum ShopPaymentTypes
    {
        Paypal = 0,
        UnityIAP = 1,
        Steam = 2,
        Other = 3,
    }
}
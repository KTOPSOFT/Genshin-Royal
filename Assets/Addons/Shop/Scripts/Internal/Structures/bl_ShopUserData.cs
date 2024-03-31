using System.Collections.Generic;
using System;

namespace MFPS.Shop
{
    [Serializable]
    public class bl_ShopUserData
    {
        public string PurchasesSource;
        public List<bl_ShopPurchase> ShopPurchases = new List<bl_ShopPurchase>();

        /// <summary>
        /// Called from ULogin to set the purchases raw data from the database
        /// </summary>
        /// <param name="data"></param>
        public void SetRawData(Dictionary<string, string> data)
        {
            PurchasesSource = data["purchases"];
            //Debug.Log($"Purchases: {data["purchases"]}");
            ShopPurchases = bl_ShopData.DecompilePurchases(PurchasesSource);
        }

        /// <summary>
        /// Returns true if the given item has been purchased by the local player
        /// </summary>
        /// <param name="typeID">The ShopItemType</param>
        /// <param name="ID">The unique ID of the item</param>
        /// <returns></returns>
        public bool isItemPurchase(int typeID, int ID) => isItemPurchase((ShopItemType)typeID, ID);

        /// <summary>
        /// Returns true if the given item has been purchased by the local player
        /// </summary>
        /// <param name="typeID">The ShopItemType</param>
        /// <param name="ID">The unique ID of the item</param>
        /// <returns></returns>
        public bool isItemPurchase(ShopItemType typeID, int ID)
        {
            return ShopPurchases.Exists(x => x.TypeID == (int)typeID && x.ID == ID);
        }

        /// <summary>
        /// Returns true if the given item has been purchased by the local player
        /// </summary>
        /// <returns></returns>
        public bool isItemPurchase(ShopProductData data)
        {
            return ShopPurchases.Exists(x => x.TypeID == (int)data.Type && x.ID == data.ID);
        }

        /// <summary>
        /// Add purchase locally
        /// This wont send any data to the database
        /// </summary>
        /// <param name="purchase"></param>
        public void AddPurchase(bl_ShopPurchase purchase)
        {
            ShopPurchases.Add(purchase);
        }
    }
}
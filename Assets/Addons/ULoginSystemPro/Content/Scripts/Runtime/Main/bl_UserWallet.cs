using System.Collections.Generic;
using MFPS.Internal.Scriptables;
using UnityEngine;

public static class bl_UserWallet
{

    /// <summary>
    /// Check if the local user has funds for the given price
    /// This will check the funds in all the game coins.
    /// </summary>
    /// <param name="price"></param>
    /// <param name="coins"></param>
    /// <returns></returns>
    public static bool HasFundsFor(int price, MFPSCoin coins)
    {
        var list = new List<MFPSCoin>() { coins };
        return HasFundsFor(price, list);
    }

    /// <summary>
    /// Check if the local user has funds for the given price
    /// This will check the funds in all the game coins.
    /// </summary>
    /// <param name="price"></param>
    /// <returns></returns>
    public static bool HasFundsFor(int price, List<MFPSCoin> coins = null)
    {
        if (coins == null) coins = bl_MFPS.Coins.GetAllCoins();
#if ULSP
        if (!bl_DataBase.IsUserLogged)
        {
            Debug.Log("You need an account to check funds.");
            return false;
        }

        foreach (var coin in coins)
        {
            int coinPrice = coin.DoConversion(price);
            if (coin.GetCoins() >= coinPrice) return true;
        }

        return false;
#else
            foreach (var coin in coins)
            {
                int coinPrice = coin.DoConversion(price);
                if (coin.GetCoins(bl_PhotonNetwork.NickName) >= coinPrice) return true;
            }
            return false;
#endif
    }
}
using System;
using UnityEngine;
using MFPS.ULogin;

public static class bl_ULoginMFPS
{
    /// <summary>
    /// Save the Kills, Deaths and Score of the local player
    /// </summary>
    public static void SaveLocalPlayerKDS(Action<bool> callback = null, int overrideScore = -1)
    {
        if (!bl_DataBase.IsUserLogged)
        {
            Debug.Log("Player has to be logged in order to save data");
            return;
        }

        var lp = bl_PhotonNetwork.LocalPlayer;
        var lu = bl_DataBase.Instance.LocalUser;
        int scoreToAdd = overrideScore == -1 ? lp.GetPlayerScore() : overrideScore;

        if(!lu.SendNewData(lp.GetKills(), lp.GetDeaths(), scoreToAdd))
        {
            // if there's no data to update
            return;
        }

        var fields = new ULoginUpdateFields();
        fields.AddField("kills", lu.Kills);
        fields.AddField("deaths", lu.Deaths);
        fields.AddField("score", lu.Score);

        bl_DataBase.Instance.UpdateUserData(fields, callback);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="friends"></param>
    public static void SaveFriendList(string friends, Action callback = null)
    {
        bl_DataBase.Instance.SaveValue("friends", friends, callback, true);
    }

    /// <summary>
    /// 
    /// </summary>
    public static bool SendNewData(this LoginUserInfo userInfo, int kills, int deaths, int score)
    {
        if (kills == 0 && deaths == 0 && score == 0) return false;

        int finalScore = Mathf.Max(1, score);
        userInfo.Score += finalScore;
        userInfo.Kills += kills;
        userInfo.Deaths += deaths;

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string GetLocalUserInitialCoins()
    {
        string line = "";
        var all = bl_MFPS.Coins.GetAllCoins();
        for (int i = 0; i < all.Count; i++)
        {
            var coin = all[i];
            line += $"{coin.InitialCoins}";
            if (i != all.Count - 1) { line += "&"; }
        }
        return line;
    }
}
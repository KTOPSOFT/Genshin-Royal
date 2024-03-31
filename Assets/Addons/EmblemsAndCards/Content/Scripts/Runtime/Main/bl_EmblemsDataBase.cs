using System;
using System.Collections.Generic;
using UnityEngine;
using MFPS.Addon.Avatars;

[CreateAssetMenu(fileName = "AvatarDataBase", menuName = "MFPS/Addons/Avatars/Database")]
public class bl_EmblemsDataBase : ScriptableObject
{
    public List<EmblemData> emblems = new List<EmblemData>();
    public List<CallingCardData> callingCards = new List<CallingCardData>();

    [Header("Selector Settings")]
    [LovattoToogle] public bool showOwnedFirst = true;

    [Header("References")]
    public Texture2D vignetteTexture;

    public static Action onBadgeUpdate;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="avatar"></param>
    public static void EquipEmblem(EmblemData avatar, Action onEquipped = null)
    {
#if !ULSP
            var key = PropertiesKeys.GetUniqueKeyForPlayer("avatarid", bl_PhotonNetwork.NickName);
            PlayerPrefs.SetInt(key, avatar.GetID());
            onEquipped?.Invoke();
#else
        if (bl_DataBase.IsUserLogged)
        {
            bl_DataBase.GetLocalUserMetaData().rawData.Avatar = avatar.GetID();
            bl_DataBase.Instance.SaveUserMetaData(onEquipped);
        }
        else
        {
            onEquipped?.Invoke();
            Debug.LogWarning("Can't equip Calling Card without an account, need to login.");
        }
#endif
        onBadgeUpdate?.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static EmblemData GetUserEmblem()
    {
#if !ULSP
            var key = PropertiesKeys.GetUniqueKeyForPlayer("avatarid", bl_PhotonNetwork.NickName);
            return Instance.emblems[PlayerPrefs.GetInt(key, 0)];
#else
        if (bl_DataBase.IsUserLogged)
        {
            return Instance.emblems[bl_DataBase.GetLocalUserMetaData().rawData.Avatar];
        }

        return Instance.emblems[0];
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="avatar"></param>
    /// <returns></returns>
    public static int GetEmblemID(EmblemData avatar)
    {
        var all = Instance.emblems;
        for (int i = 0; i < all.Count; i++)
        {
            if (avatar == all[i]) return i;
        }

        return -1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static EmblemData GetEmblem(int id)
    {
        if (id < 0 || id >= Instance.emblems.Count) return Instance.emblems[0];
        return Instance.emblems[id];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="card"></param>
    public static void EquipCallingCard(CallingCardData card, Action onEquipped = null)
    {
#if !ULSP
            var key = PropertiesKeys.GetUniqueKeyForPlayer("callingcardid", bl_PhotonNetwork.NickName);
            PlayerPrefs.SetInt(key, card.GetID());
            onEquipped?.Invoke();
#else
        if (bl_DataBase.IsUserLogged)
        {
            bl_DataBase.GetLocalUserMetaData().rawData.CallingCard = card.GetID();
            bl_DataBase.Instance.SaveUserMetaData(onEquipped);
        }
        else
        {
            onEquipped?.Invoke();
            Debug.LogWarning("Can't equip Calling Card without an account, need to login.");
        }

#endif
        onBadgeUpdate?.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static CallingCardData GetUserCallingCard()
    {
#if !ULSP
            var key = PropertiesKeys.GetUniqueKeyForPlayer("callingcardid", bl_PhotonNetwork.NickName);
            return Instance.callingCards[PlayerPrefs.GetInt(key, 0)];
#else
        if (bl_DataBase.IsUserLogged)
        {
            return Instance.callingCards[bl_DataBase.GetLocalUserMetaData().rawData.CallingCard];
        }

        return Instance.callingCards[0];
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    public static int GetCallingCardID(CallingCardData card)
    {
        var all = Instance.callingCards;
        for (int i = 0; i < all.Count; i++)
        {
            if (card == all[i]) return i;
        }

        return -1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static CallingCardData GetCallingCard(int id)
    {
        if (id < 0 || id >= Instance.callingCards.Count) return Instance.callingCards[0];
        return Instance.callingCards[id];
    }

    private static bl_EmblemsDataBase m_Data;
    public static bl_EmblemsDataBase Instance
    {
        get
        {
            if (m_Data == null)
            {
                m_Data = Resources.Load("EmblemsDataBase", typeof(bl_EmblemsDataBase)) as bl_EmblemsDataBase;
            }
            return m_Data;
        }
    }
}
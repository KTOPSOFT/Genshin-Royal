using System.Collections.Generic;
using UnityEngine;
using MFPS.Addon.Clan;

[CreateAssetMenu( fileName = "ClanSettings", menuName = "MFPS/Addons/Clan/Settings")]
public class bl_ClanSettings : ScriptableObject
{
    //In order to set a value > 50 you'll need to modify the database clan table row 'members' to set a higher chars limit.
    [Range(3,50)] public int maxClanMembers = 20;
    public int clanCreationPrice = 400;
    [Range(5, 20)] public int clanNameCharLimit = 16;
    [Range(3, 8)] public int clanTagCharLimit = 5;
    [Range(3, 25)] public int topClansFetchLimit = 10;
    //In order to set a value > 16 you'll need to modify the database clan table row 'requests' to set a higher chars limit.
    [Range(3, 16)] public int maxClanJoinRequests = 10;
    [LovattoToogle] public bool autoDeleteEmptyClans = true;
    [LovattoToogle] public bool onlyLeaderCanKickMembers = false;
    [LovattoToogle] public bool onlyLeaderCanPromoteMembers = false;
    [LovattoToogle] public bool allowTransferLeadership = true;
    [SerializeField]private Color[] tagColors;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="colorID"></param>
    /// <returns></returns>
    public Color GetTagColor(int colorID)
    {
        return tagColors[colorID];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Color[] GetAllTagColors() => tagColors;

    private static bl_ClanSettings m_Data;
    public static bl_ClanSettings Instance
    {
        get
        {
            if (m_Data == null)
            {
                m_Data = Resources.Load("ClanSettings", typeof(bl_ClanSettings)) as bl_ClanSettings;
            }
            return m_Data;
        }
    }
}
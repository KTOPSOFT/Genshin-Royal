using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Addon.Clan
{
    public static class ClanEvents
    {
        public static Action<bl_ClanInfo> onClanUpdate;
        public static void DispatchClanUpdate(bl_ClanInfo info) => onClanUpdate?.Invoke(info);
    }
}
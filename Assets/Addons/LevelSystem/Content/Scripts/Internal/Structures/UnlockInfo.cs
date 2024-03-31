using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Addon.LevelManager
{
    [Serializable]
    public class UnlockInfo
    {
        public string Name;
        public int UnlockLevel;
        public Sprite Preview;
    }
}
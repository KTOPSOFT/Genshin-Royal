using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.ULogin
{
    [Serializable]
    public class bl_UserMetaData
    {
        public RawData rawData = new RawData();

        public override string ToString()
        {
            string data = JsonUtility.ToJson(this);
            return data;
        }

        [Serializable]
        public class RawData
        {
            public string WeaponsLoadouts;
            public int ClassKit = 0;
            public int Avatar = 0;
            public int CallingCard = 0;
        }
    }
}
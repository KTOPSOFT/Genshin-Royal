using UnityEngine;
using MFPS.Internal.Structures;
using MFPSEditor;

namespace MFPS.Addon.PlayerSelector
{
    [System.Serializable]
    public class bl_PlayerSelectorInfo
    {
        public string Name;
        [SpritePreview(AutoScale = true)] public Sprite Preview;
        public GameObject Prefab;
        public MFPSItemUnlockability Unlockability;

        public int Price
        {
            get => Unlockability.Price;
        }

        [HideInInspector] public int ID;
        [HideInInspector] public Team team;

        public bool isEquipedOne()
        {
            return bl_PlayerSelectorData.GetTeamOperatorID(team) == ID;
        }
    }
}
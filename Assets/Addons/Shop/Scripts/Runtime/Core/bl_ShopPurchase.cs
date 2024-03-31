using System;

[Serializable]
public class bl_ShopPurchase
{
    public int TypeID = 0;
    public int ID = 0;
}

[Serializable]
public enum ShopItemType
{
    Weapon = 0,
    WeaponCamo = 1,
    PlayerSkin = 2,
    PlayerAccesory = 3,
    Emblem = 4,
    CallingCard = 5,
    Emote = 6,
    SeasonPass = 7,
    LootBox = 8,
    Bundle = 9,
    CoinPack = 10,
    None = 99,
}
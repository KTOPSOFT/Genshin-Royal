using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Shop
{
    public abstract class bl_ShopItemUIBase : MonoBehaviour
    {
        public abstract void Setup(ShopProductData data);
    }
}
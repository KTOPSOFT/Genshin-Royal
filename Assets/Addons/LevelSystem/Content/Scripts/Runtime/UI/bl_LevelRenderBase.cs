using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Addon.LevelManager
{
    public abstract class bl_LevelRenderBase : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        public abstract void Render(LevelInfo level);
    }
}
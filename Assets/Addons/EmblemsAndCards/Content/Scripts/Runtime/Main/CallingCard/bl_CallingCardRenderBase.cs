using UnityEngine;

namespace MFPS.Addon.Avatars
{
    public abstract class bl_CallingCardRenderBase : MonoBehaviour
    {

        public CallingCardData CallingCard { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="avatar"></param>
        public abstract void Render(CallingCardData card);
    }
}
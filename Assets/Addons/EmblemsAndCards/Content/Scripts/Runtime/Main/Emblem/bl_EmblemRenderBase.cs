using UnityEngine;

namespace MFPS.Addon.Avatars
{
    public abstract class bl_EmblemRenderBase : MonoBehaviour
    {

        public EmblemData Avatar { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="avatar"></param>
        public abstract void Render(EmblemData avatar);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public abstract void SetActive(bool active);
    }
}
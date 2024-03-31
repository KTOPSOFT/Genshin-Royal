using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.Avatars
{
    public class bl_EmblemRender : bl_EmblemRenderBase
    {
        [LovattoToogle] public bool AutoFetchLocal = false;
        [SerializeField] private Image backgroundImg = null;
        [SerializeField] private RawImage avatarImg = null;
        [SerializeField] private Image outlineImg = null;
        [SerializeField] private RawImage vignetteImg = null;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if (AutoFetchLocal) FetchLocal();
        }

        /// <summary>
        /// 
        /// </summary>
        public void FetchLocal()
        {
            Render(bl_EmblemsDataBase.GetUserEmblem());
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Render(EmblemData avatar)
        {
            Avatar = avatar;

            if (backgroundImg != null) backgroundImg.color = avatar.BackgroundColor;
            if (outlineImg != null)
            {
                outlineImg.color = avatar.OutlineColor;
                outlineImg.gameObject.SetActive(avatar.OutlineColor.a > 0);
            }
            if (vignetteImg != null)
            {
                vignetteImg.color = avatar.VignetteColor;
                vignetteImg.gameObject.SetActive(avatar.VignetteColor.a > 0);
            }
            if (avatarImg != null)
            {
                avatarImg.texture = avatar.Emblem;
                avatarImg.GetComponent<RectTransform>().localScale = Vector3.one * avatar.Size;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public override void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}
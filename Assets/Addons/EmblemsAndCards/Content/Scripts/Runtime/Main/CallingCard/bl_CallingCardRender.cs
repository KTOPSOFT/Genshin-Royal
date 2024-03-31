using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.Avatars
{
    public class bl_CallingCardRender : bl_CallingCardRenderBase
    {
        [LovattoToogle] public bool AutoFetchLocal = false;
        [SerializeField] private RawImage cardImg = null;
        [SerializeField] private Image outlineImg = null;

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
            Render(bl_EmblemsDataBase.GetUserCallingCard());
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Render(CallingCardData card)
        {
            CallingCard = card;
            if (outlineImg != null)
            {
                outlineImg.color = card.OutlineColor;
                outlineImg.gameObject.SetActive(card.OutlineColor.a > 0);
            }
            if (cardImg != null)
            {
                cardImg.texture = card.Card;
            }
        }
    }
}
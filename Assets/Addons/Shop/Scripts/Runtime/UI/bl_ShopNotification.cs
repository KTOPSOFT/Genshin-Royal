using UnityEngine;
using System.Collections;
using TMPro;

namespace MFPS.Shop
{
    public class bl_ShopNotification : MonoBehaviour
    {
        public GameObject content;
        public TextMeshProUGUI notificationText;
        public Animator m_animator;

        public bl_ShopNotification Show(string text)
        {
            notificationText.text = text.ToUpper();
            content.SetActive(true);
            StopAllCoroutines();
            m_animator?.Play("show", 0, 0);
            return this;
        }

        public void Hide(float delay)
        {
            StopAllCoroutines();
            StartCoroutine(DoHide(delay));
        }

        IEnumerator DoHide(float delay)
        {
            yield return new WaitForSeconds(delay);
            m_animator?.Play("hide", 0, 0);
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(m_animator.GetCurrentAnimatorStateInfo(0).length);
            content.SetActive(false);
        }

        private static bl_ShopNotification _Instance;
        public static bl_ShopNotification Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<bl_ShopNotification>();
                }
                return _Instance;
            }
        }
    }
}
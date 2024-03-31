using System.Collections;
using TMPro;
using UnityEngine;

namespace MFPS.ULogin
{
    public class bl_ULoginLoadingWindow : MonoBehaviour
    {
        public bool animatedHide = false;
        public GameObject content;
        [SerializeField] private TextMeshProUGUI middleText = null;

        public void SetActive(bool active)
        {
            if (!active && animatedHide)
            {
                if (!content.activeInHierarchy) return;
                StopAllCoroutines();
                StartCoroutine(DoHide());
            }
            else
            {
                content.SetActive(active);
            }
        }

        IEnumerator DoHide()
        {
            var anim = content.GetComponent<Animator>();
            anim.Play("hide", 0, 0);
            yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);
            content.SetActive(false);
        }

        public void SetText(string text)
        {
            middleText.text = text;
        }

        public void SetText(string text, bool active)
        {
            middleText.text = text;
            SetActive(active);
        }

        public bool isShowing => content.activeInHierarchy;

        private static bl_ULoginLoadingWindow _instance;
        public static bl_ULoginLoadingWindow Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_ULoginLoadingWindow>(); }
                return _instance;
            }
        }
    }
}
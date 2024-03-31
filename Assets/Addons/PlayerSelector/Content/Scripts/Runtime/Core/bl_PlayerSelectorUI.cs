using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace MFPS.Addon.PlayerSelector
{
    public class bl_PlayerSelectorUI : MonoBehaviour
    {
        [SerializeField] private Image PlayerPreview = null;
        [SerializeField] private TextMeshProUGUI PlayerNameText = null;
        [SerializeField] private Image HealthText = null;
        [SerializeField] private Image SpeedText = null;
        [SerializeField] private Image RegenerationText = null;
        [SerializeField] private Image stealthBar = null;
        public GameObject LockedUI;
        public AnimationCurve VerticalCurve;

        private bl_PlayerSelector Selector;
        private bl_PlayerSelectorInfo Info;
        private Animator Anim;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="script"></param>
        public void Set(bl_PlayerSelectorInfo info, bl_PlayerSelector script)
        {
            Info = info;
            PlayerPreview.sprite = info.Preview;
            PlayerNameText.text = info.Name.ToUpper();
            Selector = script;
            if (info.Prefab != null)
            {
                var pdm = info.Prefab.GetComponent<bl_PlayerHealthManager>();
                var fpc = info.Prefab.GetComponent<bl_FirstPersonControllerBase>();

                HealthText.fillAmount = (float)pdm.health / 125f;
                SpeedText.fillAmount = fpc.GetSpeedOnState(PlayerState.Walking) / 5;
                RegenerationText.fillAmount = pdm.RegenerationSpeed / 5;
                stealthBar.fillAmount = (1 - ((bl_FirstPersonController)fpc).stealthSpeed / 5);
            }

            int pID = bl_PlayerSelectorData.Instance.GetPlayerID(info.Name);
            if (!info.Unlockability.IsUnlocked(pID))
            {
                LockedUI.SetActive(true);
            }
            else
            {
                LockedUI.SetActive(false);
            }
        }

        public void Select()
        {
            if (Anim != null) return;

            Transform parent = transform.parent;
            parent.GetComponent<HorizontalLayoutGroup>().enabled = false;
            parent.GetComponent<ContentSizeFitter>().enabled = false;
            Selector.DeleteAllBut(gameObject);
            StartCoroutine(ShowUp());
        }

        IEnumerator ShowUp()
        {
            Anim = GetComponent<Animator>();
            Anim.SetTrigger("play");
            float time = 2;
            float d = 0;
            Vector3 origin = transform.position;
            while (d < 1)
            {
                d += Time.deltaTime / (time * 0.5f);
                Vector3 v = Selector.CenterReference.position;
                v.y = v.y + (VerticalCurve.Evaluate(d) * 275);
                transform.position = Vector3.Lerp(origin, v, d);
                yield return null;
            }
            yield return new WaitForSeconds(time * 0.5f);
            Selector.SelectPlayer(Info);
            Destroy(gameObject);
        }
    }
}
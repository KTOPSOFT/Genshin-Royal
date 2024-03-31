using UnityEngine;
using TMPro;

namespace MFPS.Addon.LevelManager
{
    public class bl_LevelChecker : MonoBehaviour
    {
        public float showTime = 5;

        [Header("References")]
        public GameObject ContentUI;
        public TextMeshProUGUI levelText;
        public Animator animator;
        public AnimationClip showClip;
        public AudioClip newLevelSound;

        private int actualLevel = 0;
        private AudioSource aSource;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            bl_EventHandler.onLocalKill += OnLocalKill;
            actualLevel = bl_LevelManager.Instance.GetLevelID();
            ContentUI.SetActive(false);
            aSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            bl_EventHandler.onLocalKill -= OnLocalKill;
        }

        /// <summary>
        /// Check the score each time that the local player kill someone
        /// </summary>
        void OnLocalKill(KillInfo info)
        {
            Check();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Check()
        {
            //get the current level taking in account the score earning in this match.
            int levelID = bl_LevelManager.Instance.GetRuntimeLevelID();
            //if this is a new level
            if (levelID > actualLevel)
            {
                CancelInvoke();
                levelText.text = levelID.ToString();
                ContentUI.SetActive(true);
                animator.Play("show", 0, 0);
                actualLevel = levelID;
                Invoke("Hide", showClip.length + showTime);
                if (newLevelSound != null)
                {
                    aSource.clip = newLevelSound;
                    aSource.Play();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void Hide()
        {
            animator.Play("hide", 0, 0);
            Invoke("Disable", showClip.length);
        }

        /// <summary>
        /// 
        /// </summary>
        void Disable()
        {
            ContentUI.SetActive(false);
        }
    }
}
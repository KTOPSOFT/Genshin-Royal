using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.Compass
{
    public class bl_Compass : MonoBehaviour
    {
        [Header("SETTINGS")]
        public CompassType m_CompassType = CompassType.Horizontal;
        [SerializeField] private bool FadeEdges = true;
        [Range(0.1f, 5)] public float Space = 2;
        [Range(0.1f, 1)] public float FadeAmount = 0.5f;
        [Range(1, 10)] public int UpdateRate = 5;
        [SerializeField] private AnimationCurve FadeCurve;
        [Header("REFERENCES")]
        [SerializeField] private Transform Panel;
        [SerializeField] private GameObject MarkPrefab;
        [SerializeField] private Text DegreeText;
        [SerializeField] private RectTransform[] UIDregres;

        private float CircularRadious = 114;
        private CanvasGroup[] Alphas;
        private Transform CameraView;
        public float Angle { get; set; }
        public List<CompassMark> Marks = new List<CompassMark>();
        int currentFrame = -1;
        private Vector3 cameraAngle;
        private readonly Vector3 FarForward = Vector3.forward * 100000;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            Alphas = new CanvasGroup[UIDregres.Length];
            for (int i = 0; i < UIDregres.Length; i++)
            {
                Alphas[i] = UIDregres[i].GetComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            CompassMarkEvent.CompassMarkAction += OnMarkEvent;
            CompassMarkEvent.ChangeCompassCameraAction += OnChangeCamera;
            CompassMarkEvent.ActionDestroyMark += OnDestroyMark;
            CompassMarkEvent.ActionShowMark += OnShowMark;
            StartCoroutine(Loop());
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            CompassMarkEvent.CompassMarkAction -= OnMarkEvent;
            CompassMarkEvent.ChangeCompassCameraAction -= OnChangeCamera;
            CompassMarkEvent.ActionDestroyMark -= OnDestroyMark;
            CompassMarkEvent.ActionShowMark -= OnShowMark;
            StopAllCoroutines();
        }

        /// <summary>
        /// 
        /// </summary>
        IEnumerator Loop()
        {
            while (true)
            {
                if (CameraView != null)
                {
                    OnUpdate();
                }
                yield return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void OnUpdate()
        {
            if (CameraView == null)
                return;
            currentFrame = (currentFrame + 1) % UpdateRate;
            if (currentFrame != 0) return;

            CalculateAngle();
            ControlledUI();
            ControlledMarks();
        }

        /// <summary>
        /// 
        /// </summary>
        void CalculateAngle()
        {
            if (CameraView == null) { CameraView = Camera.main.transform; }
            //get the 2D position of the player camera
            cameraAngle = CameraView.forward;
            cameraAngle.y = 0;
            //get the angle between the player camera position and an north fixed position
            Angle = Vector3.Angle(FarForward, cameraAngle);

            float Degree = Angle;
            Vector3 cross = Vector3.Cross(FarForward, cameraAngle);
            if (cross.y < 0) { Angle = -Angle; Degree = 360 - Degree; }
            if (DegreeText != null) { DegreeText.text = Degree.ToString("F0"); }
        }

        /// <summary>
        /// 
        /// </summary>
        void ControlledUI()
        {
            RectTransform r = null;
            Vector2 aPosition;
            float RowDegre = 360 / UIDregres.Length;
            int inv = 1;
            float fadeAmount = 180 * (1 - FadeAmount);
            for (int i = 0; i < UIDregres.Length; i++)
            {
                r = UIDregres[i];
                if (r == null) continue;
                aPosition = r.anchoredPosition;
                int half = UIDregres.Length / 2;
                float defaultDegre;
                float alpha;
                if (i > half)
                {
                    defaultDegre = ((-RowDegre * inv) + -Angle);
                    if (FadeEdges)
                    {
                        alpha = (1 - (Mathf.Abs(defaultDegre) / fadeAmount));
                        alpha = FadeCurve.Evaluate(alpha);
                        Alphas[i].alpha = alpha;
                    }
                    if (defaultDegre < -180)
                    {
                        defaultDegre = defaultDegre + 360;
                        if (FadeEdges)
                        {
                            alpha = (1 - (defaultDegre / fadeAmount));
                            alpha = FadeCurve.Evaluate(alpha);
                            Alphas[i].alpha = alpha;
                        }
                    }
                    inv++;
                }
                else
                {
                    defaultDegre = ((RowDegre * i) + -Angle);
                    if (FadeEdges)
                    {
                        alpha = (1 - (Mathf.Abs(defaultDegre) / fadeAmount));
                        alpha = FadeCurve.Evaluate(alpha);
                        Alphas[i].alpha = alpha;
                    }
                    if (defaultDegre > 180)
                    {
                        defaultDegre = defaultDegre - 360;
                        if (FadeEdges)
                        {
                            alpha = (1 - Mathf.Abs(defaultDegre) / fadeAmount);
                            alpha = FadeCurve.Evaluate(alpha);
                            Alphas[i].alpha = alpha;
                        }
                    }
                }
                if (m_CompassType == CompassType.Horizontal)
                {
                    aPosition.x = defaultDegre;
                }
                else if (m_CompassType == CompassType.Vertical)
                {
                    aPosition.y = defaultDegre;
                }
                r.anchoredPosition = aPosition * Space;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void CircularUI()
        {
            RectTransform r = null;
            float RowDegre = 360 / UIDregres.Length;
            int inv = 1;
            for (int i = 0; i < UIDregres.Length; i++)
            {
                r = UIDregres[i];
                if (r == null) continue;
                Vector2 v = r.anchoredPosition;
                float half = UIDregres.Length / 2;
                float defaultDegre = 0;

                if (i > half)
                {
                    defaultDegre = ((-RowDegre * inv) + -(Angle + 90)) / (CircularRadious * 0.5f);
                    inv++;
                }
                else
                {
                    defaultDegre = ((RowDegre * i) + -(Angle + 90)) / (CircularRadious * 0.5f);
                }
                defaultDegre = defaultDegre * -1;
                v.x = CircularRadious * Mathf.Cos(defaultDegre);
                v.y = CircularRadious * Mathf.Sin(defaultDegre);
                r.anchoredPosition = v * Space;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void ControlledMarks()
        {
            float fa = 180 * (1 - FadeAmount);
            for (int i = 0; i < Marks.Count; i++)
            {
                CompassMark m = Marks[i];
                if (m == null) continue;
                if (m.Target == null || m.MarkUI == null) { Marks.RemoveAt(i); continue; }
                if (!m.MarkUI.gameObject.activeSelf) continue;

                Vector2 v = m.MarkUI.anchoredPosition;
                Vector3 rhs = m.Target.position - CameraView.position;
                rhs.y = 0;
                rhs.Normalize();
                Vector3 forward = CameraView.forward;
                float dot = Vector3.Dot(forward, rhs);
                Vector3 cross = Vector3.Cross(forward, rhs);
                float angle = (1 - dot) * 90;
                if (FadeEdges)
                {
                    float alpha = (1 - (angle / fa));
                    alpha = FadeCurve.Evaluate(alpha);
                    m.Alpha.alpha = alpha;
                }
                if (cross.y < 0)
                {
                    angle = -angle;
                }
                if (m_CompassType == CompassType.Horizontal)
                {
                    v.x = angle;
                }
                else if (m_CompassType == CompassType.Vertical)
                {
                    v.y = angle;
                }
                m.MarkUI.anchoredPosition = v * Space;
            }
        }

        /// <summary>
        /// Call this to hide or show again any mark on compass.
        /// </summary>
        public void ShowMark(bool show, Transform Target)
        {
            CompassMark mark = GetTargetMark(Target);
            if (mark == null) { Debug.Log("This transform doesn't have any mark created."); return; }
            mark.MarkUI.gameObject.SetActive(show);
        }

        /// <summary>
        /// 
        /// </summary>
        void OnMarkEvent(CompassMark mark)
        {
            if (MarkPrefab == null || Panel == null)
                return;
            if (Marks.Exists(x => x.Target == mark.Target))
                return;

            CompassMark NewMark = new CompassMark();
            NewMark.Icon = mark.Icon;
            NewMark.IconColor = mark.IconColor;
            NewMark.Target = mark.Target;

            GameObject p = Instantiate(MarkPrefab) as GameObject;
            p.transform.SetParent(Panel, false);
            NewMark.Alpha = p.GetComponent<CanvasGroup>();
            Image img = p.GetComponent<Image>();
            img.sprite = mark.Icon;
            img.color = mark.IconColor;
            NewMark.MarkUI = p.GetComponent<RectTransform>();
            Marks.Add(NewMark);
        }

        /// <summary>
        /// 
        /// </summary>
        void OnDestroyMark(Transform target)
        {
            if (Marks.Exists(x => x.Target == target))
            {
                int id = Marks.FindIndex(x => x.Target == target);
                Destroy(Marks[id].MarkUI);
                Marks.RemoveAt(id);
            }
            else
            {
                Debug.LogWarning("This target: " + target.name + " doesn't have a mark.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void OnShowMark(Transform target, bool show)
        {
            if (Marks.Exists(x => x.Target == target))
            {
                int id = Marks.FindIndex(x => x.Target == target);
                Marks[id].MarkUI.gameObject.SetActive(show);
            }
            else
            {
                Debug.LogWarning("This target: " + target.name + " doesn't have a mark.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void OnChangeCamera(Transform camera)
        {
            CameraView = camera;
        }

        /// <summary>
        /// try to get the compass mark of a target
        /// </summary>
        /// <returns></returns>
        public CompassMark GetTargetMark(Transform target)
        {
            if (Marks.Exists(x => x.Target == target))
            {
                return Marks.Find(x => x.Target == target);
            }
            return null;
        }

        [System.Serializable]
        public enum CompassType
        {
            Horizontal,
            Vertical,
            //Circular,
        }

        /*  [ContextMenu("Set")]
          void Set()
          {
              CameraView = transform;
              currentFrame = UpdateRate;
              Alphas = new CanvasGroup[UIDregres.Length];
              for (int i = 0; i < UIDregres.Length; i++)
              {
                  Alphas[i] = UIDregres[i].GetComponent<CanvasGroup>();
              }
              OnUpdate();
          }*/
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

namespace MFPS.GameModes.CoverPoint
{
    public class bl_CoverPointBase : bl_PhotonHelper
    {
        public int PointID = 0;

        [Header("HUD Icon")]
        public Texture2D Icon;
        public Vector3 Offset;
        public float size = 25;
        public Color iconColor = Color.white;

        [Header("References")]
        public MeshRenderer BaseRenderer;
        public Image BaseIcon;
        public Image BackgroundImg;

        //Private
        private int cacheDomineIn = 0;
        public bool isLocalIn { get; set; }
        private bl_CPBaseInfo info
        {
            get { return bl_CoverPoint.Instance.objetivePoints[PointID]; }
        }
        public float takingProgress { get; set; }

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            cacheDomineIn = bl_CoverPoint.Instance.TimeToDominatePoint;
        }

        /// <summary>
        /// 
        /// </summary>
        void OnTriggerEnter(Collider c)
        {
            if (c.isLocalPlayerCollider())
            {
                isLocalIn = true;
                bl_CoverPoint.Instance.OnLocalPlayerEnterInPoint(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void OnTriggerExit(Collider c)
        {
            if (c.isLocalPlayerCollider())
            {
                bl_CoverPoint.Instance.OnLocalPlayerExitPoint(this);
                isLocalIn = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartTakingThisPoint(Team team)
        {
            StartCoroutine("FillIcon", team);
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopTaking()
        {
            StopCoroutine("FillIcon");
        }

        /// <summary>
        /// 
        /// </summary>
        public void CancelTaking()
        {
            StopCoroutine("FillIcon");
            takingProgress = 0;
            BaseIcon.fillAmount = 1;
            Color c = info.TeamOwner.GetTeamColor(0.2f);
            BackgroundImg.color = c;
            BaseRenderer.material.SetColor("_TintColor", c);
        }

        /// <summary>
        /// 
        /// </summary>
        IEnumerator FillIcon(Team team)
        {
            Team playerTeam = team;
            while (takingProgress < 1)
            {
                takingProgress += Time.deltaTime / cacheDomineIn;
                BaseIcon.fillAmount = 1 - takingProgress;
                Color c = Color.Lerp(info.TeamOwner.GetTeamColor(), playerTeam.GetTeamColor(), takingProgress);
                c.a = 0.2f;
                BaseRenderer.material.SetColor("_TintColor", c);
                if (isLocalIn)
                {
                    bl_CovertPointUI.Instance.ShowBar(true, takingProgress, Mathf.FloorToInt(cacheDomineIn - (cacheDomineIn * takingProgress)));
                }
                yield return null;
            }
            OnDomine(team);
        }

        /// <summary>
        /// 
        /// </summary>
        void OnDomine(Team team)
        {
            BaseIcon.fillAmount = 1;
            bl_CovertPointUI.Instance.ShowBar(false);
            info.TeamOwner = team;
            info.pointStatus = CovertPointStatus.Taked;
            takingProgress = 0;
            BackgroundImg.color = team.GetTeamColor(0.2f);
            if (bl_PhotonNetwork.IsMasterClient)
            {
                string hexColor = ColorUtility.ToHtmlStringRGB(team.GetTeamColor());
                bl_KillFeedBase.Instance.SendMessageEvent(string.Format("<color=#{0}>{1}</color> has taken {2} point", hexColor, team.GetTeamName(), info.Name));
            }
            //reward all players that capture this point
            if (info.OwnerTeamContainPlayer(bl_PhotonNetwork.LocalPlayer.ActorNumber))
            {
                bl_PhotonNetwork.LocalPlayer.PostScore(bl_CoverPoint.Instance.PlayerScorePerCapture);
            }
            bl_CoverPoint.Instance.OnCoverPoint();
        }

        /// <summary>
        /// 
        /// </summary>
        public void RefreshInfo()
        {
            BaseIcon.fillAmount = 1;
            Color c = info.TeamOwner.GetTeamColor(0.2f);
            BackgroundImg.color = c;
            BaseRenderer.material.SetColor("_TintColor", c);
        }

        /// <summary>
        /// 
        /// </summary>
        void OnGUI()
        {
            if (!bl_UtilityHelper.GetCursorState)
                return;
            if (bl_GameManager.Instance.CameraRendered == null)
                return;
            if (bl_RoomMenu.Instance.isPaused)
                return;

            Vector3 position = transform.position + Offset;
            Plane plane = new Plane(bl_GameManager.Instance.CameraRendered.transform.forward, bl_GameManager.Instance.CameraRendered.transform.position);

            //If the object is behind the camera, then don't draw it
            if (plane.GetSide(position) == false)
            {
                return;
            }

            //Calculate the 2D position of the position where the icon should be drawn
            Vector3 viewportPoint = bl_GameManager.Instance.CameraRendered.WorldToViewportPoint(position);

            //The viewportPoint coordinates are between 0 and 1, so we have to convert them into screen space here
            Vector2 drawPosition = new Vector2(viewportPoint.x * Screen.width, Screen.height * (1 - viewportPoint.y));

            float clampBorder = 12;

            //Clamp the position to the edge of the screen in case the icon would be drawn outside the screen
            drawPosition.x = Mathf.Clamp(drawPosition.x, clampBorder, Screen.width - clampBorder);
            drawPosition.y = Mathf.Clamp(drawPosition.y, clampBorder, Screen.height - clampBorder);

            GUI.color = iconColor;
            GUI.DrawTexture(new Rect(drawPosition.x - size * 0.5f, drawPosition.y - size * 0.5f, size, size), Icon);
            GUI.color = Color.white;
        }
    }
}
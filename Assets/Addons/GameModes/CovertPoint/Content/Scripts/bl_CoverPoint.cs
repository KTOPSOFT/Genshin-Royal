using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;
using Photon.Realtime;
using MFPS.GameModes.CoverPoint;

public class bl_CoverPoint : bl_PhotonHelper, IGameMode
{
    [Header("Settings")]
    public int TimeToDominatePoint = 7;
    public int PlayerScorePerCapture = 250;
    public int TicketsPerBase = 5;
    public int TicketsEachSeconds = 3;

    [Header("Points")]
    public List<bl_CPBaseInfo> objetivePoints = new List<bl_CPBaseInfo>();

    [Header("References")]
    public GameObject CPObjects = null;
    public AudioClip CoverPointSound;
    private AudioSource Source;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        Source = GetComponent<AudioSource>();
        Initialize();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        bl_PhotonCallbacks.MasterClientSwitched += OnMasterClientSwitched;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        bl_PhotonCallbacks.MasterClientSwitched -= OnMasterClientSwitched;
        CancelInvoke();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnLocalPlayerEnterInPoint(bl_CoverPointBase point)
    {
        photonView.RPC(nameof(RpcChangePointState), RpcTarget.All, point.PointID, CovertPointStatus.Taking);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnLocalPlayerExitPoint(bl_CoverPointBase point)
    {
        photonView.RPC(nameof(RpcChangePointState), RpcTarget.All, point.PointID, CovertPointStatus.Eviction);
        bl_CovertPointUI.Instance.ShowBar(false);
    }

    [PunRPC]
    public void RpcChangePointState(int pointID, CovertPointStatus newState, PhotonMessageInfo info)
    {
        Team team = info.Sender.GetPlayerTeam();
        if (newState == CovertPointStatus.Taking)
        {
            objetivePoints[pointID].AddPlayerInPoint(info.Sender);//add this player at this point
            if (!objetivePoints[pointID].CanTakePoint(team))//if the point is from the enemy team and it doesn't have any member of the that team
            {
                newState = CovertPointStatus.Contested;//members of both team inside
            }
            objetivePoints[pointID].CheckNewEntrance(info.Sender, newState);//check how the point will react with this new player entrance
        }
        else if (newState == CovertPointStatus.Eviction)//player leave or die inside of point
        {
            if (objetivePoints[pointID].RemovePlayerFromPoint(info.Sender))//if no one remain inside of the point
            {
                if (objetivePoints[pointID].TeamOwner == Team.None)//if no one have controlled this point before
                {
                    newState = CovertPointStatus.Untaked;
                }
                else { newState = CovertPointStatus.Taked; }
            }
            objetivePoints[pointID].CheckExit(info.Sender);//check how the point will react with this new player exit
        }
        objetivePoints[pointID].SetStatus(newState, team);
    }

    /// <summary>
    /// 
    /// </summary>
    void ManageTickets()
    {
        if (!bl_PhotonNetwork.IsMasterClient) return;

        int team1Merits = GetTeamBases(Team.Team1);
        int team2Merits = GetTeamBases(Team.Team2);

        if (team1Merits > 0) { PhotonNetwork.CurrentRoom.SetTeamScore(Team.Team1, team1Merits); }
        if (team2Merits > 0) { PhotonNetwork.CurrentRoom.SetTeamScore(Team.Team2, team2Merits); }
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckScore()
    {
        if (PhotonNetwork.OfflineMode || !bl_RoomSettings.Instance.RoomInfoFetched) return;

        int team1 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team1);
        int team2 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team2);

        bl_CovertPointUI.Instance.UpdateScore(team1, team2);

        if (team1 >= bl_RoomSettings.Instance.GameGoal)
        {
            bl_MatchTimeManagerBase.Instance.FinishRound();
            return;
        }
        if (team2 >= bl_RoomSettings.Instance.GameGoal)
        {
            bl_MatchTimeManagerBase.Instance.FinishRound();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnPlayerDeath(int actorNumber)
    {
        for (int i = 0; i < objetivePoints.Count; i++)
        {
            if (objetivePoints[i].team2ActorsIn.Contains(actorNumber) || objetivePoints[i].team1ActorsIn.Contains(actorNumber))
            {
                photonView.RPC(nameof(RpcChangePointState), RpcTarget.All, i, CovertPointStatus.Eviction);
                objetivePoints[i].pointBase.isLocalIn = false;
                bl_CovertPointUI.Instance.ShowBar(false);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnCoverPoint()
    {
        Source.clip = CoverPointSound;
        Source.Play();
    }

    public int GetTeamBases(Team team)
    {
        int c = 0;
        for (int i = 0; i < objetivePoints.Count; i++)
        {
            if(objetivePoints[i].TeamOwner == team) { c += TicketsPerBase; }
        }
        return c;
    }

    public Team GetWinnerTeam()
    {
        int team1 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team1);
        int team2 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team2);

        Team winner = Team.None;
        if (team1 > team2) { winner = Team.Team1; }
        else if (team1 < team2) { winner = Team.Team2; }
        else { winner = Team.None; }
        return winner;
    }

    [PunRPC]
    void SyncCoverPointGame(string data)
    {
        Debug.Log("Sync Data: " + data);
        string[] pointsInfo = data.Split("\n"[0]);
        for (int i = 0; i < pointsInfo.Length; i++)
        {
            if (string.IsNullOrEmpty(pointsInfo[i])) continue;
            string[] info = pointsInfo[i].Split("|"[0]);

            objetivePoints[i].pointStatus = (CovertPointStatus)(int.Parse(info[0]));
            objetivePoints[i].TeamOwner = (Team)(int.Parse(info[1]));
            if (!string.IsNullOrEmpty(info[2]))
            {
                int[] team1 = info[2].Split(',').Select(n => int.Parse(n)).ToArray();
                objetivePoints[i].team1ActorsIn.AddRange(team1);
            }
            if (!string.IsNullOrEmpty(info[3]))
            {
                int[] team2 = info[3].Split(',').Select(n => int.Parse(n)).ToArray();
                objetivePoints[i].team2ActorsIn.AddRange(team2);
            }
            objetivePoints[i].pointBase.RefreshInfo();
        }
    }

    #region GameMode Interface
    public bool isLocalPlayerWinner { get { return bl_PhotonNetwork.LocalPlayer.GetPlayerTeam() == GetWinnerTeam(); } }

    public void Initialize()
    {
        //check if this is the game mode of this room
        if (bl_GameManager.Instance.IsGameMode(GameMode.CP, this))
        {
            CPObjects.SetActive(true);
            CheckViewAllocation();
            bl_GameManager.Instance.SetGameState(MatchState.Starting);
            bl_CovertPointUI.Instance.SetUp();

            if (bl_PhotonNetwork.IsMasterClient)
            {
                InvokeRepeating(nameof(ManageTickets), TicketsEachSeconds, TicketsEachSeconds);
            }
        }
        else
        {
            CPObjects.SetActive(false);

        }
    }

    public void OnFinishTime(bool gameOver)
    {
        //determine the winner
        string finalText = "";
        if (GetWinnerTeam() != Team.None)
        {
            finalText = GetWinnerTeam().GetTeamName();
        }
        else
        {
            finalText = bl_GameTexts.NoOneWonName;
        }
        
        bl_RoundFinishScreenBase.Instance?.Show(finalText);
    }

    public void OnLocalPoint(int points, Team teamToAddPoint)
    {       
    }

    public void OnLocalPlayerKill()
    {      
    }

    public void OnLocalPlayerDeath()
    {
        OnPlayerDeath(bl_PhotonNetwork.LocalPlayer.ActorNumber);
    }

    public void OnOtherPlayerEnter(Player newPlayer)
    {
        if (bl_PhotonNetwork.IsMasterClient)
        {
            string data = "";
            for (int i = 0; i < objetivePoints.Count; i++)
            {
                int status = (int)objetivePoints[i].pointStatus;
                int team = (int)objetivePoints[i].TeamOwner;
                string team1Players = string.Join(",", objetivePoints[i].team1ActorsIn.Select(x => x.ToString()).ToArray());
                string team2Players = string.Join(",", objetivePoints[i].team2ActorsIn.Select(x => x.ToString()).ToArray());
                data += string.Format("{0}|{1}|{2}|{3}\n", status, team, team1Players, team2Players);
            }
            photonView.RPC(nameof(SyncCoverPointGame), newPlayer, data);
        }
    }

    public void OnOtherPlayerLeave(Player otherPlayer)
    {
        for (int i = 0; i < objetivePoints.Count; i++)
        {
            objetivePoints[i].team1ActorsIn.Remove(otherPlayer.ActorNumber);
            objetivePoints[i].team2ActorsIn.Remove(otherPlayer.ActorNumber);
        }
    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(PropertiesKeys.Team1Score) || propertiesThatChanged.ContainsKey(PropertiesKeys.Team2Score))
        {
            CheckScore();
        }
    }
    #endregion

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        if (bl_PhotonNetwork.IsMasterClient)
        {
            InvokeRepeating(nameof(ManageTickets), TicketsEachSeconds, TicketsEachSeconds);
        }
    }

    private static bl_CoverPoint _instance;
    public static bl_CoverPoint Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_CoverPoint>(); }
            return _instance;
        }
    }
}
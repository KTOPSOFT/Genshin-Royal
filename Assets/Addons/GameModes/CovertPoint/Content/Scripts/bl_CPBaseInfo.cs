using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

namespace MFPS.GameModes.CoverPoint
{
    [System.Serializable]
    public class bl_CPBaseInfo
    {
        public string Name;
        public string Letter;
        public CovertPointStatus pointStatus = CovertPointStatus.Untaked;
        public Team TeamOwner = Team.None;

        [Header("References")]
        public bl_CoverPointBase pointBase;

        public List<int> team1ActorsIn = new List<int>();
        public List<int> team2ActorsIn = new List<int>();

        public void SetStatus(CovertPointStatus newStatus, Team team)
        {
            if (pointStatus == CovertPointStatus.Taked && newStatus == CovertPointStatus.Taking && team == TeamOwner) return;//don't change the taken status if a player from the dominate team entry in the point

            pointStatus = newStatus;
        }

        public void CheckNewEntrance(Player player, CovertPointStatus newStatus)
        {
            if (pointStatus != CovertPointStatus.Taking && newStatus == CovertPointStatus.Taking)//someone start taking this point
            {
                if (TeamOwner == player.GetPlayerTeam()) return;

                pointBase.StartTakingThisPoint(player.GetPlayerTeam());
            }
            else if (pointStatus == CovertPointStatus.Taking && newStatus == CovertPointStatus.Contested)//someone of the enemy team get in the point.
            {
                pointBase.StopTaking();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void CheckExit(Player player)
        {
            if (team1ActorsIn.Count > team2ActorsIn.Count)//if team 1 has majority
            {
                pointBase.StartTakingThisPoint(Team.Team1);
            }
            else if (team1ActorsIn.Count < team2ActorsIn.Count)//if team 2 has majority
            {
                pointBase.StartTakingThisPoint(Team.Team2);
            }
            else
            {
                if (team1ActorsIn.Count == 0 && team2ActorsIn.Count == 0)//if there is nobody remain inside of the point
                {
                    if (TeamOwner == player.GetPlayerTeam()) return;
                    pointBase.CancelTaking();
                }
                else//if someone exit or death and now both team have the same amount of players inside of the point.
                {
                    pointBase.StopTaking();
                }
            }
        }

        public bool CanTakePoint(Team teamMember)
        {
            if (teamMember == Team.Team1)
            {
                return team1ActorsIn.Count > team2ActorsIn.Count;
            }
            else
            {
                return team2ActorsIn.Count > team1ActorsIn.Count;
            }
        }

        public void AddPlayerInPoint(Player player)
        {
            if (player.GetPlayerTeam() == Team.Team1)
            {
                if (!team1ActorsIn.Contains(player.ActorNumber)) { team1ActorsIn.Add(player.ActorNumber); }
            }
            else
            {
                if (!team2ActorsIn.Contains(player.ActorNumber)) { team2ActorsIn.Add(player.ActorNumber); }
            }
        }

        public bool RemovePlayerFromPoint(Player player)
        {
            if (player.GetPlayerTeam() == Team.Team1)
            {
                team1ActorsIn.Remove(player.ActorNumber);
            }
            else
            {
                team2ActorsIn.Remove(player.ActorNumber);
            }
            return team1ActorsIn.Count <= 0 && team2ActorsIn.Count <= 0;
        }

        public bool OwnerTeamContainPlayer(int actorNumber)
        {
            if (TeamOwner == Team.Team1) { return team1ActorsIn.Contains(actorNumber); }
            else if (TeamOwner == Team.Team2) { return team2ActorsIn.Contains(actorNumber); }
            else { return false; }
        }
    }

    [System.Serializable]
    public enum CovertPointStatus
    {
        Untaked = 0,
        Taking = 1,
        Contested = 2,
        Taked = 3,
        Eviction = 4,
    }
}
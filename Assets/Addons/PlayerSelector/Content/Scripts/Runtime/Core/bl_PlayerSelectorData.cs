using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

namespace MFPS.Addon.PlayerSelector
{
    public class bl_PlayerSelectorData : ScriptableObject
    {
        [Header("Players")]
        public List<bl_PlayerSelectorInfo> AllPlayers = new List<bl_PlayerSelectorInfo>();
        [Header("Player Per Team")]
        [FormerlySerializedAs("DeltaPlayers")]
        [PlayerSelectorID] public List<int> Team1Players = new List<int>();
        [FormerlySerializedAs("ReconPlayers")]
        [PlayerSelectorID] public List<int> Team2Players = new List<int>();
        [PlayerSelectorID] public List<int> FFAPlayers = new List<int>();

        [Header("Bots")]
        public List<bl_AIShooter> Team1Bots;
        public List<bl_AIShooter> Team2Bots;
        [LovattoToogle] public bool RandomizeBots = true;

        [Header("Settings")]
        public PSType PlayerSelectorMode = PSType.InLobby;

        [SerializeField] private GameObject inMatchUIPrefab = null;

        public const string OPERATORS_KEY = "mfps.operators";
        public const string FAV_TEAM_KEY = "mfps.operators.favteam";

        /// <summary>
        /// 
        /// </summary>
        public static bl_PlayerSelector InitInMatchUI()
        {
            var instance = FindObjectOfType<bl_PlayerSelector>();
            if (instance != null) return instance;

            var go = Instantiate(Instance.inMatchUIPrefab);
            return go.GetComponentInChildren<bl_PlayerSelector>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bl_PlayerSelectorInfo GetPlayer(Team team, int id)
        {
            var list = FFAPlayers;
            if (team == Team.Team1)
            {
                list = Team1Players;
            }
            else if (team == Team.Team2)
            {
                list = Team2Players;
            }

            if (id >= list.Count)
            {
                Debug.LogWarning($"Player ID {id} in the player list for team {team.ToString()} is not listed.");
                return AllPlayers[0];
            }

            int pid = list[id];
            if (pid >= AllPlayers.Count)
            {
                Debug.LogWarning($"Player ID {pid} is not listed in the All Players List of PlayerSelector.");
                return AllPlayers[0];
            }

            return AllPlayers[pid];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bl_PlayerSelectorInfo GetPlayerByIndex(int id)
        {
            if(id >= AllPlayers.Count)
            {
                Debug.LogWarning($"Player ID {id} is not listed in the All Players List of PlayerSelector.");
                return AllPlayers[0];
            }
            return AllPlayers[id];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        public List<bl_PlayerSelectorInfo> GetPlayerList(Team team)
        {
            List<bl_PlayerSelectorInfo> list = new List<bl_PlayerSelectorInfo>();
            List<int> ids = team == Team.Team1 ? Team1Players : Team2Players;
            for (int i = 0; i < ids.Count; i++)
            {
                bl_PlayerSelectorInfo info = GetPlayerByIndex(ids[i]);
                info.ID = ids[i];
                info.team = team;
                list.Add(info);
            }
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        public GameObject GetRandomBotForTeam(Team team)
        {
            var list = Team1Bots;
            if (team == Team.Team2) list = Team2Bots;

            bl_AIShooter bot;
            int interations = 0;
            do
            {
                bot = list[Random.Range(0, list.Count)];
                interations++;
            } while (bot == null && interations < list.Count);


            return bot.gameObject;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetPlayerID(string name)
        {
            return AllPlayers.FindIndex(x => x.Name == name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string[] AllPlayerStringList()
        {
            return AllPlayers.Select(x => x.Name).ToList().ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        public static int GetTeamOperatorID(Team team)
        {
            return PlayerPrefs.GetInt(OPERATORS_KEY + team.ToString(), 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operatorID"></param>
        /// <param name="team"></param>
        public static void SetTeamOperator(int operatorID, Team team)
        {
            PlayerPrefs.SetInt(OPERATORS_KEY + team.ToString(), operatorID);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        public bl_PlayerSelectorInfo GetSelectedPlayerFromTeam(Team team)
        {
            int id = GetTeamOperatorID(team);
            if (team == Team.All || team == Team.None)
            {
                id = GetTeamOperatorID(GetFavoriteTeam());
            }
            return GetPlayerByIndex(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static Team GetFavoriteTeam()
        {
            int id = PlayerPrefs.GetInt(FAV_TEAM_KEY, (int)Team.Team1);
            return (Team)id;
        }

        public void SetFavoriteTeam(Team team)
        {
            PlayerPrefs.SetInt(FAV_TEAM_KEY, (int)team);
        }

        [System.Serializable]
        public enum PSType
        {
            InMatch,
            InLobby,
        }

        private static bl_PlayerSelectorData m_Data;
        public static bl_PlayerSelectorData Instance
        {
            get
            {
                if (m_Data == null)
                {
                    m_Data = Resources.Load("PlayerSelector", typeof(bl_PlayerSelectorData)) as bl_PlayerSelectorData;
                }
                return m_Data;
            }
        }
    }
}
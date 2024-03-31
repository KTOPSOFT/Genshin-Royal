using UnityEngine;
using UnityEditor;

namespace MFPS.Addon.Compass
{
    public class UCompassBarInitializer
    {

        [MenuItem("MFPS/Addons/CompassBar/Setup Players")]
        private static void SetupPlayers()
        {
            bool sucess = true;
            GameObject playerPrefab = bl_GameData.Instance.Player1.gameObject;
            SetUpPlayer(playerPrefab);
            playerPrefab = bl_GameData.Instance.Player2.gameObject;
            SetUpPlayer(playerPrefab);

#if PSELECTOR
            foreach (var p in bl_PlayerSelector.Data.AllPlayers)
            {
                if (p.Prefab == null) continue;
                playerPrefab = p.Prefab.gameObject;
                SetUpPlayer(playerPrefab);
            }
#endif

            if (sucess)
            {
                Debug.Log("Player Prefabs has been setup for UCompass Bar");
            }
        }

        static void SetUpPlayer(GameObject playerPrefab)
        {
            if (playerPrefab == null) return;

            GameObject c = playerPrefab.GetComponentInChildren<bl_CameraShakerBase>().gameObject;
            if (c.GetComponent<bl_CompassMagnet>() == null)
            {
                c.AddComponent<bl_CompassMagnet>();
                EditorUtility.SetDirty(playerPrefab);
            }
        }
    }
}
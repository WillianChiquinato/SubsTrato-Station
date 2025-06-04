using UnityEngine;
using Fusion;

public class PlayersSpawn : SimulationBehaviour, IPlayerJoined
{
    public GameObject playerPrefab;
    public GameObject SpawnPlayers;

    public void PlayerJoined(PlayerRef player)
    {
        if (Runner.LocalPlayer == player)
        {
            // Spawns the player at the default spawn point
            Runner.Spawn(playerPrefab, SpawnPlayers.transform.position, Quaternion.identity, player);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;
using static Player;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private InputField sessionText;
    private NetworkRunner _runner;
    [SerializeField] private GameObject hostMenu;
    [SerializeField] private GameObject[] playerPrefab;
    [Networked] private Vector3 spawnPosition { get; set; }

    public async void Host()
    {
        hostMenu.SetActive(false);
        Spawner.canSpawnEnemies = true;

        _runner = GetComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = sessionText.text,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public async void Shared()
    {
        hostMenu.SetActive(false);

        _runner = GetComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = sessionText.text,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { 
        NetworkObject playerObject = runner.GetPlayerObject(player);

    if (playerObject != null)
    {
        runner.Despawn(playerObject);
        Debug.Log($"Player {player} left the game. Object despawned.");
    }
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInputData data = new NetworkInputData
        {
            moveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"))
        };
        input.Set(data);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { 
        SceneManager.LoadScene("Menu");
    }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"Disconnect: {reason}");
        SceneManager.LoadScene("Menu");
    }
    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log($"Connection succesful");
    }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log($"Connection failed: {reason}");
    }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, System.ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            int randomIndex = UnityEngine.Random.Range(0, 4);
            spawnPosition = new Vector3(UnityEngine.Random.Range(4, 14), 2, UnityEngine.Random.Range(-5, 5));
            var playerObject = runner.Spawn(playerPrefab[randomIndex], spawnPosition, Quaternion.identity, player);
            Debug.Log($"Player spawned at {spawnPosition} (Host: {runner.IsServer}, PlayerRef: {player})");

            if (player == runner.LocalPlayer)
            {
                Player playerElement = playerObject.GetComponent<Player>();
                Camera.main.GetComponent<PlayerCamera>().target = playerObject.transform;
            }
        }
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug.Log("Connection requested");
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject networkObject, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject networkObject, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey reliableKey, float x) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

}

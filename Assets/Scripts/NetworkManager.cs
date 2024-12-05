using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;
    [SerializeField] private GameObject hostMenu;

    public async void Host()
    {
        hostMenu.SetActive(false);

        _runner = GetComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = "Coon And Friends",
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
            GameMode = GameMode.Shared,
            SessionName = "Coon And Friends",
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, System.Collections.Generic.List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, System.ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"{player} joined the game");
    }  

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug.Log("Connection requested");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log("Disconnected from server");
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject networkObject, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject networkObject, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey reliableKey, float x) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

}

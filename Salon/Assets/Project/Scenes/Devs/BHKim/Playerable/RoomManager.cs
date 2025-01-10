using Firebase.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Salon.Firebase.Database;
using Salon.Firebase;

public class RoomManager : MonoBehaviour
{
    private DatabaseReference dbReference;
    private Dictionary<string, GameObject> instantiatedPlayers = new Dictionary<string, GameObject>();
    public GameObject playerPrefab;
    public Transform spawnParent;

    private string currentRoom = "Room1"; // ���� ä��/�� �̸�

    private async void OnEnable()
    {
        try
        {
            if (dbReference == null)
                dbReference = FirebaseManager.Instance.DbReference;

            await SubscribeToPlayerChanges(currentRoom);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Firebase �ʱ�ȭ ����: {ex.Message}");
        }
    }

    private async Task SubscribeToPlayerChanges(string roomName)
    {
        var playersReference = dbReference.Child("Channels").Child(roomName).Child("Players");

        // ���� �÷��̾� �ε�
        var snapshot = await playersReference.GetValueAsync();
        if (snapshot.Exists)
        {
            foreach (var child in snapshot.Children)
            {
                var displayName = child.Key;
                var playerData = JsonConvert.DeserializeObject<GamePlayerData>(child.GetRawJsonValue());
                if (!instantiatedPlayers.ContainsKey(displayName))
                {
                    InstantiatePlayer(displayName, playerData);
                }
            }
        }

        // �̺�Ʈ ���
        playersReference.ChildAdded += OnPlayerAdded;
        playersReference.ChildRemoved += OnPlayerRemoved;
        playersReference.ChildChanged += OnPlayerUpdated; // �÷��̾� ������ ������Ʈ ó��
    }

    private void OnPlayerAdded(object sender, ChildChangedEventArgs e)
    {
        if (e.Snapshot.Exists)
        {
            var displayName = e.Snapshot.Key;
            var playerData = JsonConvert.DeserializeObject<GamePlayerData>(e.Snapshot.GetRawJsonValue());

            if (!instantiatedPlayers.ContainsKey(displayName))
            {
                InstantiatePlayer(displayName, playerData);
            }
        }
    }

    private void OnPlayerRemoved(object sender, ChildChangedEventArgs e)
    {
        if (e.Snapshot.Exists)
        {
            var displayName = e.Snapshot.Key;

            Debug.Log($"�÷��̾� ����: {displayName}");
            if (instantiatedPlayers.ContainsKey(displayName))
            {
                Destroy(instantiatedPlayers[displayName]);
                instantiatedPlayers.Remove(displayName);
            }
        }
    }

    private void OnPlayerUpdated(object sender, ChildChangedEventArgs e)
    {
        if (e.Snapshot.Exists)
        {
            var displayName = e.Snapshot.Key;
            var playerData = JsonConvert.DeserializeObject<GamePlayerData>(e.Snapshot.GetRawJsonValue());

            if (instantiatedPlayers.ContainsKey(displayName))
            {
                instantiatedPlayers[displayName].GetComponent<PlayerController>()
                    ?.UpdatePosition(new Vector3(playerData.Position.X, playerData.Position.Y, playerData.Position.Z));
            }
        }
    }

    public void InstantiatePlayer(string displayName, GamePlayerData playerData, bool isLocalPlayer = false)
    {
        Vector3 spawnPosition = playerData.Position != null
            ? new Vector3(playerData.Position.X, playerData.Position.Y, playerData.Position.Z)
            : Vector3.zero;

        var playerObject = Instantiate(playerPrefab, spawnPosition, Quaternion.identity, spawnParent);
        playerObject.name = displayName;

        var playerController = playerObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.Initialize(displayName, isLocalPlayer);
        }

        instantiatedPlayers[displayName] = playerObject;

        if (isLocalPlayer)
        {
            // ���� ĳ������ �߰� �ʱ�ȭ �۾� (ī�޶�, �Է� ��ũ��Ʈ ��)
            var inputController = playerObject.GetComponent<PlayerInputController>();
            if (inputController != null)
            {
                inputController.enabled = true;
            }
        }
    }

    private void UpdatePlayerPosition(GameObject playerObject, PositionData position)
    {
        if (position != null)
        {
            playerObject.transform.position = new Vector3(position.X, position.Y, position.Z);
        }
    }

    private void OnDisable()
    {
        if (dbReference != null)
        {
            var playersReference = dbReference.Child("Channels").Child(currentRoom).Child("Players");
            playersReference.ChildAdded -= OnPlayerAdded;
            playersReference.ChildRemoved -= OnPlayerRemoved;
            playersReference.ChildChanged -= OnPlayerUpdated;
        }
    }
}


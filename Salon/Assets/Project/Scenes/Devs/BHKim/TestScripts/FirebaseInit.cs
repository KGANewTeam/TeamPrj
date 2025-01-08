using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections;
using System.Runtime.InteropServices;
using Firebase.Extensions;

[System.Serializable]
public class RoomData
{
    public Dictionary<string, MessageData> Messages { get; set; }
    public Dictionary<string, PlayerData> Players { get; set; }
    public int UserCount;
    public bool isFull;

    public RoomData()
    {
        Messages = new Dictionary<string, MessageData>
        {
            { "welcome", new MessageData("system", "Welcome to the room!", DateTimeOffset.UtcNow.ToUnixTimeSeconds()) }
        };
        Players = null;
        UserCount = 0;
        isFull = false;
    }
    public bool AddPlayer(PlayerData player)
    {
        if (true == isFull)
            return false;

        if (false == Players.ContainsKey(player.PlayerId))
        {
            Players[player.PlayerId] = player;
            UserCount++;

            if (UserCount >= 10)
                isFull = true;
            return true;
        }
        return false;
    }
}

[System.Serializable]
public class MessageData
{
    public string SenderId { get; set; }
    public string Content { get; set; }
    public long Timestamp { get; set; }

    public MessageData(string senderId, string content, long timestamp)
    {
        SenderId = senderId;
        Content = content;
        Timestamp = timestamp;
    }
}

[System.Serializable]
public class PlayerData
{
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public bool IsOnline { get; set; }

    public PlayerData(string playerId, string playerName, bool isOnline)
    {
        PlayerId = playerId;
        PlayerName = playerName;
        IsOnline = isOnline;
    }
}

public class FirebaseInit : MonoBehaviour
{
    private DatabaseReference dbReference;
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log($"Firebase �ʱ�ȭ ����");
                InitializeFirebase();
            }
            else
                Debug.LogError($"Firebase �ʱ�ȭ ����: {dependencyStatus}");
        });
    }

    private void InitializeFirebase()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        ExistRooms();
    }

    private void ExistRooms()
    {
        Debug.Log("Firebase �� ���� ����");

        // Firebase���� ���� �����ϴ� �� ��� Ȯ��
        dbReference.Child("Rooms").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                // �̹� �����ϴ� �� �̸� ����
                HashSet<string> existingRooms = new HashSet<string>();
                foreach (DataSnapshot room in snapshot.Children)
                {
                    existingRooms.Add(room.Key);
                }

                CreateMissingRooms(existingRooms);
            }
            else
                Debug.LogError("�� ������ Ȯ�� ����: " + task.Exception);
        });
    }

    private void CreateMissingRooms(HashSet<string> existingRooms)
    {
        Debug.Log("Firebase ������ �� ���� ����");
        Dictionary<string, RoomData> rooms = new Dictionary<string, RoomData>();

        for (int i = 1; i <= 10; i++)
        {
            string roomName = $"Room{i}";

            if (!existingRooms.Contains(roomName))
            {
                RoomData roomData = new RoomData();
                rooms[roomName] = roomData;
            }
        }

        if (rooms.Count > 0)
        {
            string jsonData = JsonConvert.SerializeObject(rooms, Formatting.Indented);
            Debug.Log($"����ȭ�� JSON ������: {jsonData}");
            Debug.Log($"JSON ������ ũ��: {jsonData.Length} bytes");

            dbReference.Child("Rooms").SetRawJsonValueAsync(jsonData).ContinueWith(task =>
            {
                if (task.IsFaulted)
                    Debug.LogError("�� ���� ����: " + task.Exception);
                else if (task.IsCompleted)
                    Debug.Log("������ �� ���� �Ϸ�!");
            });
        }
        else
        {
            Debug.Log("��� ���� �̹� �����մϴ�. �߰� �۾��� �ʿ����� �ʽ��ϴ�.");
            TestAddPlayersToRoom();
        }
    }
    public void AddPlayerToRoom(string roomName, string playerId, string playerName)
    {
        dbReference.Child("Rooms").Child(roomName).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            try {
            if (task.IsFaulted)
            {
                Debug.LogError($"�� {roomName} ������ �������� ����: {task.Exception}");
                return;
            }

                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    if (!snapshot.Exists)
                    {
                        Debug.LogError($"�� {roomName}�� �������� �ʽ��ϴ�.");
                        return;
                    }

                    string json = snapshot.GetRawJsonValue();
                    RoomData roomData = JsonConvert.DeserializeObject<RoomData>(json);

                    if (roomData.isFull)
                    {
                        Debug.Log($"�� {roomName}�� �̹� ���� á���ϴ�. �÷��̾� �߰� �Ұ�.");
                        return;
                    }

                    PlayerData newPlayer = new PlayerData(playerId, playerName, true);
                    bool added = roomData.AddPlayer(newPlayer);

                    if (added)
                    {
                        string updatedJson = JsonConvert.SerializeObject(roomData, Formatting.Indented);
                        dbReference.Child("Rooms").Child(roomName).SetRawJsonValueAsync(updatedJson).ContinueWithOnMainThread(updateTask =>
                        {
                            try
                            {
                                if (updateTask.IsFaulted)
                                    Debug.LogError($"�÷��̾� �߰� ����: {updateTask.Exception}");
                                else if (updateTask.IsCompleted)
                                    Debug.Log($"�÷��̾� {playerName}�� �� {roomName}�� �߰��Ǿ����ϴ�.");
                            }
                            catch (Exception ex)
                            {
                                Debug.Log(ex.Message);
                            }
                        });
                    }
                    else
                        Debug.Log($"�÷��̾� {playerName} �߰� ����: �̹� �����ϰų� ���� ���� á���ϴ�.");
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        });
    }
    public void RemovePlayerFromRoom(string roomName, string playerId)
    {
        dbReference.Child("Rooms").Child(roomName).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"�� {roomName} ������ �������� ����: {task.Exception}");
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (!snapshot.Exists)
                {
                    Debug.LogError($"�� {roomName}�� �������� �ʽ��ϴ�.");
                    return;
                }

                string json = snapshot.GetRawJsonValue();
                RoomData roomData = JsonConvert.DeserializeObject<RoomData>(json);

                if (roomData.Players.ContainsKey(playerId))
                {
                    roomData.Players.Remove(playerId);
                    roomData.UserCount--;

                    if (roomData.isFull && roomData.UserCount < 10)
                        roomData.isFull = false;

                    string updatedJson = JsonConvert.SerializeObject(roomData, Formatting.Indented);
                    dbReference.Child("Rooms").Child(roomName).SetRawJsonValueAsync(updatedJson).ContinueWithOnMainThread(updateTask =>
                    {
                        if (updateTask.IsFaulted)
                            Debug.LogError($"�÷��̾� ���� ����: {updateTask.Exception}");
                        else if (updateTask.IsCompleted)
                            Debug.Log($"�÷��̾� {playerId}�� �� {roomName}���� ���ŵǾ����ϴ�.");
                    });
                }
                else
                    Debug.Log($"�÷��̾� {playerId}�� �� {roomName}�� �������� �ʽ��ϴ�.");
            }
        });
    }
    void TestAddPlayersToRoom()
    {
        string roomName = "Room1";

        // 10���� �÷��̾ �����Ͽ� �߰�
        for (int i = 1; i <= 10; i++)
        {
            string playerId = $"player{i}";
            string playerName = $"Player{i}";

            AddPlayerToRoom(roomName, playerId, playerName);
        }

        // �� ������ Ȯ��
        dbReference.Child("Rooms").Child(roomName).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"�� {roomName} ������ �������� ����: {task.Exception}");
                return;
            }

            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    string json = snapshot.GetRawJsonValue();
                    RoomData roomData = JsonConvert.DeserializeObject<RoomData>(json);

                    Debug.Log($"�� �̸�: {roomName}");
                    Debug.Log($"���� ���� ��: {roomData.UserCount}");
                    Debug.Log($"���� ���� á�°�: {roomData.isFull}");
                }
                else
                {
                    Debug.LogError($"�� {roomName}�� �������� �ʽ��ϴ�.");
                }
            }
        });
    }
}
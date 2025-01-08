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
using System.Threading.Tasks;

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
        Players = new Dictionary<string, PlayerData>();
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

[System.Serializable]
public class PlayerMapping
{
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }

    public PlayerMapping(string playerId, string playerName)
    {
        PlayerId = playerId;
        PlayerName = playerName;
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

    private async void InitializeFirebase()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;

        await ExistRooms();
        await InitPlayerMappings();
    }

    private async Task InitPlayerMappings()
    {
        try
        {
            // Rooms ������ ��������
            DataSnapshot roomsSnapshot = await dbReference.Child("Rooms").GetValueAsync();

            if (!roomsSnapshot.Exists)
            {
                Debug.LogWarning("Rooms �����Ͱ� �����ϴ�. ������ �������� �ʽ��ϴ�.");
                return;
            }

            DataSnapshot mappingSnapshot = await dbReference.Child("PlayerNameToIdMapping").GetValueAsync();

            HashSet<string> existingNames = new HashSet<string>();
            if (mappingSnapshot.Exists)
            {
                foreach (DataSnapshot entry in mappingSnapshot.Children)
                {
                    existingNames.Add(entry.Key); // ���� ���ο��� �̸� ��������
                }
            }

            List<Task> mappingTasks = new List<Task>();
            foreach (DataSnapshot roomSnapshot in roomsSnapshot.Children)
            {
                DataSnapshot playersSnapshot = roomSnapshot.Child("Players");

                if (playersSnapshot.Exists)
                {
                    foreach (DataSnapshot playerSnapshot in playersSnapshot.Children)
                    {
                        string playerId = playerSnapshot.Key; // Player ID
                        string playerName = playerSnapshot.Child("PlayerName").Value?.ToString(); // Player Name

                        if (!string.IsNullOrEmpty(playerName) && !existingNames.Contains(playerName))
                        {
                            mappingTasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    await dbReference.Child("PlayerNameToIdMapping").Child(playerName).SetValueAsync(playerId);
                                    Debug.Log($"�÷��̾� �̸� {playerName}�� ID {playerId} ���� �Ϸ�");
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError($"�÷��̾� {playerName} ���� �� ���� �߻�: {ex.Message}");
                                }
                            }));
                        }
                    }
                }
            }

            await Task.WhenAll(mappingTasks);

            Debug.Log("Firebase �̸�-���̵� ���� �ʱ�ȭ �Ϸ�");
        }
        catch (Exception ex)
        {
            Debug.LogError($"�̸�-���̵� ���� �ʱ�ȭ �� ���� �߻�: {ex.Message}");
        }
    }
    public async Task<PlayerMapping> GetPlayerMappingByName(string playerName)
    {
        try
        {
            DataSnapshot snapshot = await dbReference.Child("PlayerNameToIdMapping").Child(playerName).GetValueAsync();

            if (snapshot.Exists)
            {
                string playerId = snapshot.Value.ToString();
                return new PlayerMapping(playerId, playerName);
            }
            else
            {
                Debug.LogError($"�÷��̾� �̸� {playerName}�� ã�� �� �����ϴ�.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"ID ��ȸ �� ���� �߻�: {ex.Message}");
            return null;
        }
    }
    public async Task InvitePlayerToRoomByName(string roomName, string playerName)
    {
        PlayerMapping mapping = await GetPlayerMappingByName(playerName);

        if (mapping != null)
        {
            await AddPlayerToRoom(roomName, mapping.PlayerId, mapping.PlayerName);
            Debug.Log($"�÷��̾� {playerName}�� �� {roomName}���� �ʴ�Ǿ����ϴ�.");
        }
        else
        {
            Debug.LogError($"�÷��̾� {playerName} �ʴ� ����: �̸��� ã�� �� �����ϴ�.");
        }
    }

    private async Task ExistRooms()
    {
        Debug.Log("Firebase �� ���� ����");

        try
        {
            DataSnapshot snapshot = await dbReference.Child("Rooms").GetValueAsync();

            // �̹� �����ϴ� �� �̸� ����
            HashSet<string> existingRooms = new HashSet<string>();
            if (snapshot.Exists)
            {
                foreach (DataSnapshot room in snapshot.Children)
                {
                    existingRooms.Add(room.Key);
                }
            }

            await CreateMissingRooms(existingRooms);
        }
        catch (Exception ex)
        {
            Debug.LogError($"�� ��� Ȯ�� ����: {ex.Message}");
        }
    }

    private async Task CreateMissingRooms(HashSet<string> existingRooms)
    {
        Debug.Log("Firebase ������ �� ���� ����");

        for (int i = 1; i <= 10; i++)
        {
            string roomName = $"Room{i}";

            if (false == existingRooms.Contains(roomName)) // ������ ���� �游 �߰�
            {
                RoomData roomData = new RoomData();
                string roomJson = JsonConvert.SerializeObject(roomData, Formatting.Indented);

                try
                {
                    await dbReference.Child("Rooms").Child(roomName).SetRawJsonValueAsync(roomJson);
                    Debug.Log($"�� {roomName} ���� �Ϸ�!");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"�� {roomName} ���� �� ���� �߻�: {ex.Message}");
                }
            }
            else
            {
                Debug.Log($"�� {roomName}�� �̹� �����մϴ�. �������� �ʽ��ϴ�.");
            }
        }
        Debug.Log("Firebase �� ���� �Ϸ�");

        //await AddPlayerToRoom("Room1", "player1", "player1");
        await AddPlayerToRoom("Room2", "player12", "player1");
        await AddPlayerToRoom("Room2", "player_hero", "player_hero1");
        //await TestAddPlayersToRoom();
    }
    public async Task AddPlayerToRoom(string roomName, string playerId, string playerName)
    {
        try
        {
            // �÷��̾ �ٸ� �濡 �����ϴ��� Ȯ��
            string existingRoom = await FindPlayerInRooms(playerId);

            if (existingRoom != null && existingRoom != roomName)
            {
                Debug.LogWarning($"�÷��̾� {playerName}�� �̹� �� {existingRoom}�� �����մϴ�. �� {roomName}�� �߰����� �ʽ��ϴ�.");
                return;
            }

            // Firebase���� ���� �� ������ ��������
            DataSnapshot snapshot = await dbReference.Child("Rooms").Child(roomName).GetValueAsync();

            RoomData roomData;

            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                roomData = JsonConvert.DeserializeObject<RoomData>(json);
            }
            else
            {
                Debug.LogError($"�� {roomName}�� �������� �ʽ��ϴ�. ���� �����ؾ� �մϴ�.");
                return;
            }

            // Players�� null�� ��� �ʱ�ȭ
            if (roomData.Players == null)
                roomData.Players = new Dictionary<string, PlayerData>();

            // ���� ���� á���� Ȯ��
            if (roomData.isFull)
            {
                Debug.Log($"�� {roomName}�� �̹� ���� á���ϴ�. �÷��̾� �߰� �Ұ�.");
                return;
            }

            // �÷��̾� �߰�
            if (false == roomData.Players.ContainsKey(playerId))
            {
                PlayerData newPlayer = new PlayerData(playerId, playerName, true);
                roomData.Players[playerId] = newPlayer;
                roomData.UserCount++;

                // ���� ���� á���� Ȯ��
                if (roomData.UserCount >= 10)
                    roomData.isFull = true;

                Debug.Log($"�÷��̾� {playerName}�� �� {roomName}�� �߰��Ǿ����ϴ�.");
            }
            else
            {
                Debug.Log($"�÷��̾� {playerName}�� �̹� �� {roomName}�� �����մϴ�.");
                return;
            }

            // Firebase�� ������ ������ ����
            string updatedJson = JsonConvert.SerializeObject(roomData, Formatting.Indented);
            await dbReference.Child("Rooms").Child(roomName).SetRawJsonValueAsync(updatedJson);

            Debug.Log($"�� {roomName} ������Ʈ �Ϸ�. ���� ���� ��: {roomData.UserCount}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"�÷��̾� �߰� �� ���� �߻�: {ex.Message}");
        }
    }
    private async Task<string> FindPlayerInRooms(string playerId)
    {
        try
        {
            DataSnapshot roomsSnapshot = await dbReference.Child("Rooms").GetValueAsync();

            if (roomsSnapshot.Exists)
            {
                foreach (DataSnapshot roomSnapshot in roomsSnapshot.Children)
                {
                    DataSnapshot playersSnapshot = roomSnapshot.Child("Players");

                    if (playersSnapshot.Exists && playersSnapshot.HasChild(playerId))
                    {
                        return roomSnapshot.Key; // �÷��̾ �̹� �����ϴ� �� �̸� ��ȯ
                    }
                }
            }

            return null; // �÷��̾ � �濡�� �������� ����
        }
        catch (Exception ex)
        {
            Debug.LogError($"�÷��̾� �˻� �� ���� �߻�: {ex.Message}");
            return null;
        }
    }

    public async Task RemovePlayerFromRoom(string roomName, string playerId)
    {
        try
        {
            DataSnapshot snapshot = await dbReference.Child("Rooms").Child(roomName).GetValueAsync();

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
                await dbReference.Child("Rooms").Child(roomName).SetRawJsonValueAsync(updatedJson);

                Debug.Log($"�÷��̾� {playerId}�� �� {roomName}���� ���ŵǾ����ϴ�.");
            }
            else
                Debug.Log($"�÷��̾� {playerId}�� �� {roomName}�� �������� �ʽ��ϴ�.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"�÷��̾� ���� �� ���� �߻�: {ex.Message}");
        }
    }

    public async Task TestAddPlayersToRoom()
    {
        string roomName = "Room1";

        // 10���� �÷��̾ �����Ͽ� �߰�
        for (int i = 1; i <= 10; i++)
        {
            string playerId = $"player{i}";
            string playerName = $"Player{i}";

            await AddPlayerToRoom(roomName, playerId, playerName);
        }

        // �� ������ Ȯ��
        try
        {
            DataSnapshot snapshot = await dbReference.Child("Rooms").Child(roomName).GetValueAsync();

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
        catch (Exception ex)
        {
            Debug.LogError($"�� ���� Ȯ�� �� ���� �߻�: {ex.Message}");
        }
    }

}
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

public class FirebaseManager : MonoBehaviour
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
        }
    }
}
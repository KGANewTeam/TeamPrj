using UnityEngine;
using Firebase.Database;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Salon.Firebase.Database;
using System.Threading.Tasks;
using Salon.System;
using UnityEngine.SceneManagement;
using System.Threading;

namespace Salon.Firebase
{
    public class ChannelManager : Singleton<ChannelManager>
    {
        private DatabaseReference dbReference;
        private DatabaseReference channelsRef;
        private DatabaseReference currentChannelRef;
        private DatabaseReference currentChannelPlayersRef;
        private DatabaseReference currentChannelDataRef;
        private DatabaseReference connectedRef;
        public string CurrentChannel { get; private set; }
        private string currentUserName;
        private const float DISCONNECT_TIMEOUT = 5f;
        private EventHandler<ValueChangedEventArgs> disconnectHandler;

        // 매니저들의 초기화 상태를 추적
        private bool isChatManagerInitialized;
        private bool isRoomManagerInitialized;

        private bool isQuitting = false;

        void Start()
        {
            _ = Initialize();
        }

        public async Task Initialize()
        {
            try
            {
                Debug.Log("[ChannelManager] Initialize 시작");
                if (dbReference == null)
                {
                    dbReference = await GetDbReference();
                    channelsRef = dbReference.Child("Channels");
                    Debug.Log("[ChannelManager] 데이터베이스 참조 설정 완료");
                    SetupDisconnectHandlers();
                }

                await InitializeManagers();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChannelManager] 초기화 실패: {ex.Message}");
            }
        }

        private async Task InitializeManagers()
        {
            try
            {
                // ChatManager 초기화
                if (!isChatManagerInitialized && ChatManager.Instance != null)
                {
                    await ChatManager.Instance.Initialize();
                    isChatManagerInitialized = true;
                }

                // RoomManager 초기화
                if (!isRoomManagerInitialized && RoomManager.Instance != null)
                {
                    await RoomManager.Instance.Initialize();
                    isRoomManagerInitialized = true;
                }

                Debug.Log("[ChannelManager] 모든 매니저 초기화 완료");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChannelManager] 매니저 초기화 실패: {ex.Message}");
                throw;
            }
        }

        private async Task<DatabaseReference> GetDbReference()
        {
            int maxRetries = 5;
            int currentRetry = 0;
            int delayMs = 1000;

            while (currentRetry < maxRetries)
            {
                if (FirebaseManager.Instance.DbReference != null)
                {
                    return FirebaseManager.Instance.DbReference;
                }

                Debug.Log($"[ChannelManager] Firebase 데이터베이스 참조 대기 중... (시도 {currentRetry + 1}/{maxRetries})");
                await Task.Delay(delayMs);
                currentRetry++;
                delayMs *= 2;
            }

            throw new Exception("[ChannelManager] Firebase 데이터베이스 참조를 가져올 수 없습니다.");
        }

        private void UpdateChannelReferences(string channelName)
        {
            if (string.IsNullOrEmpty(channelName)) return;

            currentChannelRef = channelsRef.Child(channelName);
            currentChannelPlayersRef = currentChannelRef.Child("Players");
            currentChannelDataRef = currentChannelRef.Child("CommonChannelData");
            CurrentChannel = channelName;
            Debug.Log($"[ChannelManager] 채널 레퍼런스 업데이트 완료: {channelName}");
        }

        private void ClearChannelReferences()
        {
            currentChannelRef = null;
            currentChannelPlayersRef = null;
            currentChannelDataRef = null;
            CurrentChannel = null;
            Debug.Log("[ChannelManager] 채널 레퍼런스 초기화 완료");
        }

        public void SetCurrentUserName(string userName)
        {
            currentUserName = userName;
            Debug.Log($"[ChannelManager] 현재 사용자 이름 설정: {currentUserName}");
        }

        public async Task SendChat(string message)
        {
            if (string.IsNullOrEmpty(CurrentChannel)) return;
            await ChatManager.Instance.SendChat(message, CurrentChannel, FirebaseManager.Instance.GetCurrentDisplayName());
        }

        private void SetupDisconnectHandlers()
        {
            if (connectedRef != null && disconnectHandler != null)
            {
                connectedRef.ValueChanged -= disconnectHandler;
            }

            connectedRef = FirebaseDatabase.DefaultInstance.GetReference(".info/connected");
            disconnectHandler = async (sender, args) =>
            {
                try
                {
                    if (args.Snapshot.Value == null) return;
                    bool isConnected = (bool)args.Snapshot.Value;

                    if (!isConnected && !string.IsNullOrEmpty(CurrentChannel) &&
                        FirebaseManager.Instance.CurrentUserName != null)
                    {
                        Debug.Log("[ChannelManager] 연결 끊김 감지, 정리 작업 시작");
                        await LeaveChannel(false);
                        Debug.Log("[ChannelManager] 연결 끊김 정리 작업 완료");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ChannelManager] 연결 상태 처리 중 오류 발생: {ex.Message}");
                }
            };

            connectedRef.ValueChanged += disconnectHandler;
            Debug.Log("[ChannelManager] 연결 해제 핸들러 설정 완료");
        }

        public async Task ExistRooms()
        {
            try
            {
                var snapshot = await channelsRef.GetValueAsync();
                var existingRooms = new HashSet<string>();

                if (snapshot.Exists)
                {
                    foreach (var room in snapshot.Children)
                    {
                        existingRooms.Add(room.Key);
                    }
                }

                await CreateMissingRooms(existingRooms);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChannelManager] 채널 목록 확인 실패: {ex.Message}");
            }
        }

        private async Task CreateMissingRooms(HashSet<string> existingRooms)
        {
            for (int i = 1; i <= 10; i++)
            {
                string roomName = $"Channel{i:D2}";
                if (!existingRooms.Contains(roomName))
                {
                    try
                    {
                        var roomData = new ChannelData();
                        string roomJson = JsonConvert.SerializeObject(roomData, Formatting.Indented);
                        await channelsRef.Child(roomName).SetRawJsonValueAsync(roomJson);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ChannelManager] 방 {roomName} 생성 중 오류: {ex.Message}");
                    }
                }
            }
        }

        public async Task JoinChannel(string channelName)
        {
            try
            {
                Debug.Log($"[ChannelManager] {channelName} 채널 입장 시도 시작");

                var channelSnapshot = await channelsRef.Child(channelName).GetValueAsync();
                if (!channelSnapshot.Exists)
                {
                    Debug.LogError($"[ChannelManager] 채널 {channelName}이 존재하지 않음");
                    throw new Exception($"채널 {channelName}이 존재하지 않습니다.");
                }

                if (CurrentChannel != null)
                {
                    Debug.Log("[ChannelManager] 현재 채널에서 나가기 시도...");
                    await LeaveChannel(true);
                    Debug.Log("[ChannelManager] 현재 채널에서 나가기 완료");
                }

                UpdateChannelReferences(channelName);

                await ValidateAndUpdateUserCount();

                await SetupPlayerData();

                await Task.WhenAll(
                    ChatManager.Instance.StartListeningToMessages(channelName),
                    RoomManager.Instance.JoinChannel(channelName)
                );

                Debug.Log($"[ChannelManager] {channelName} 채널 입장 완료");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChannelManager] 채널 입장 실패: {ex.Message}\n스택 트레이스: {ex.StackTrace}");
                throw;
            }
        }

        private async Task ValidateAndUpdateUserCount()
        {
            var currentCount = await GetChannelUserCount(CurrentChannel);
            if (currentCount >= 10)
            {
                throw new Exception("채널이 가득 찼습니다.");
            }
        }

        private async Task SetupPlayerData()
        {
            string currentUserName = FirebaseManager.Instance.CurrentUserName;
            if (string.IsNullOrEmpty(currentUserName))
            {
                throw new Exception("[ChannelManager] 현재 사용자 이름이 설정되지 않았습니다.");
            }

            var playerData = new GamePlayerData(currentUserName);
            await AddPlayerToChannel(CurrentChannel, currentUserName, playerData);
        }

        public async Task LeaveChannel(bool isNormalDisconnect = true)
        {
            try
            {
                string channelName = CurrentChannel;
                Debug.Log($"[ChannelManager] 채널 나가기 시작 - Channel: {channelName}, User: {currentUserName}, Normal: {isNormalDisconnect}");

                if (string.IsNullOrEmpty(channelName) || string.IsNullOrEmpty(currentUserName))
                {
                    Debug.Log("[ChannelManager] 채널 또는 유저 정보가 없어 종료");
                    return;
                }

                // 1. 채팅 처리
                if (ChatManager.Instance != null)
                {
                    ChatManager.Instance.StopListeningToMessages();
                }

                // 2. 룸 처리
                if (RoomManager.Instance != null)
                {
                    RoomManager.Instance.UnsubscribeFromChannel();
                    RoomManager.Instance.DestroyAllPlayers();
                }

                // 3. 채널 데이터 정리
                try
                {
                    await RemovePlayerFromChannel(channelName, currentUserName);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ChannelManager] 플레이어 제거 실패: {ex.Message}");
                }

                // 4. 이벤트 핸들러 정리
                CleanupResources();

                ClearChannelReferences();
                Debug.Log($"[ChannelManager] 채널 나가기 완료 - Channel: {channelName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChannelManager] 채널 나가기 실패: {ex.Message}");
                throw;
            }
        }

        private async void OnDestroy()
        {
            if (!Application.isPlaying || isQuitting) return;

            try
            {
                Debug.Log("[ChannelManager] OnDestroy 시작");
                await LeaveChannel(false);
                Debug.Log("[ChannelManager] OnDestroy 완료");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChannelManager] OnDestroy 처리 실패: {ex.Message}");
            }
        }

        private async void OnApplicationQuit()
        {
            if (isQuitting) return;
            isQuitting = true;

            try
            {
                Debug.Log("[ChannelManager] OnApplicationQuit 시작");
                await LeaveChannel(false);
                Debug.Log("[ChannelManager] OnApplicationQuit 완료");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChannelManager] OnApplicationQuit 처리 실패: {ex.Message}");
            }
        }

        private void CleanupResources()
        {
            try
            {
                if (connectedRef != null && disconnectHandler != null)
                {
                    connectedRef.ValueChanged -= disconnectHandler;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChannelManager] 리소스 정리 실패: {ex.Message}");
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            Application.quitting += OnApplicationQuit;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Application.quitting -= OnApplicationQuit;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"[ChannelManager] 씬 로드됨: {scene.name}");

            if (!FirebaseManager.Instance.IsInitialized)
            {
                Debug.Log("[ChannelManager] Firebase 재초기화 필요");
                _ = Initialize();
            }
        }

        public async Task<Dictionary<string, ChannelData>> WaitForChannelData()
        {
            try
            {
                var snapshot = await channelsRef.GetValueAsync();
                if (!snapshot.Exists) return null;

                var channelData = new Dictionary<string, ChannelData>();
                foreach (var channelSnapshot in snapshot.Children)
                {
                    channelData[channelSnapshot.Key] = JsonConvert.DeserializeObject<ChannelData>(channelSnapshot.GetRawJsonValue());
                }
                return channelData;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChannelManager] 채널 데이터 로드 실패: {ex.Message}");
                return null;
            }
        }

        public async Task<int> GetChannelUserCount(string channelName)
        {
            try
            {
                var userCountRef = channelsRef.Child(channelName).Child("CommonChannelData").Child("UserCount");
                var snapshot = await userCountRef.GetValueAsync();
                return snapshot.Value != null ? Convert.ToInt32(snapshot.Value) : 0;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChannelManager] 유저 수 조회 실패: {ex.Message}");
                return 0;
            }
        }

        public async Task UpdateChannelUserCount(string channelName, int countDelta)
        {
            try
            {
                var channelDataRef = channelsRef.Child(channelName).Child("CommonChannelData");
                var playersRef = channelsRef.Child(channelName).Child("Players");

                var playersSnapshot = await playersRef.GetValueAsync();
                int actualPlayerCount = playersSnapshot.Exists ? (int)playersSnapshot.ChildrenCount : 0;

                int newCount = countDelta > 0 ? actualPlayerCount : Math.Max(0, actualPlayerCount - 1);

                Debug.Log($"[ChannelManager] 채널 {channelName}의 유저 수 업데이트 - 실제: {actualPlayerCount}, 델타: {countDelta}, 새값: {newCount}");

                var updates = new Dictionary<string, object>
                {
                    ["UserCount"] = newCount,
                    ["isFull"] = newCount >= 10
                };

                await channelDataRef.UpdateChildrenAsync(updates);
                Debug.Log($"[ChannelManager] 채널 {channelName}의 유저 수 업데이트 완료: {newCount}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChannelManager] 유저 수 업데이트 실패: {ex.Message}");
                throw;
            }
        }

        public async Task RemovePlayerFromChannel(string channelName, string playerName)
        {
            try
            {
                // 플레이어 데이터 삭제
                var playerRef = channelsRef.Child(channelName).Child("Players").Child(playerName);
                await playerRef.RemoveValueAsync();
                Debug.Log($"[ChannelManager] 플레이어 {playerName} 데이터 제거 완료");

                // 유저 수 업데이트 (-1)
                await UpdateChannelUserCount(channelName, -1);
                Debug.Log($"[ChannelManager] 플레이어 {playerName} 제거 및 유저 수 업데이트 완료");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChannelManager] 플레이어 제거 실패: {ex.Message}");
                throw;
            }
        }

        public async Task AddPlayerToChannel(string channelName, string playerName, GamePlayerData playerData)
        {
            try
            {
                var currentCount = await GetChannelUserCount(channelName);
                if (currentCount >= 10)
                {
                    throw new Exception("채널이 가득 찼습니다.");
                }

                // 플레이어 데이터 추가
                var playerRef = channelsRef.Child(channelName).Child("Players").Child(playerName);
                await playerRef.SetRawJsonValueAsync(JsonConvert.SerializeObject(playerData));

                // 유저 수 업데이트
                await UpdateChannelUserCount(channelName, 1);
                Debug.Log($"[ChannelManager] 플레이어 {playerName} 추가 및 유저 수 업데이트 완료");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ChannelManager] 플레이어 추가 실패: {ex.Message}");
                throw;
            }
        }
    }
}
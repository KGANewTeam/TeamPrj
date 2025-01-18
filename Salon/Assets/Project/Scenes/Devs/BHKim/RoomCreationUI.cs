using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using Salon.Firebase;
using TMPro;

public class RoomCreationUI : Panel
{
    [Header("UI Elements")]
    public TMP_InputField roomNameInput;
    public Button createRoomButton;
    public Button closeButton;
    public Text errorMessage;

    [Header("Scroll View")]
    public Transform roomListContent;
    public GameObject roomItemPrefab;

    private string currentChannelId;
    public override void Open()
    {
        base.Open();
        Initialize();
    }

    private void Initialize()
    {
        
    }

    public void SetCurrentChannel(string channelId)
    {
        currentChannelId = channelId;
        LoadRoomList(); // ä�ο� ���� �� ��� �ε�
    }
    public async void LoadRoomList()
    {
        // ���� ����Ʈ �ʱ�ȭ
        foreach (Transform child in roomListContent)
        {
            Destroy(child.gameObject);
        }

        // GameRoomManager���� �� ��� ��������
        var roomIds = await GameRoomManager.Instance.GetRoomList(currentChannelId);

        // �� ������ ����
        foreach (var roomId in roomIds)
        {
            GameObject roomItem = Instantiate(roomItemPrefab, roomListContent);
            var roomItemUI = roomItem.GetComponent<RoomItemUI>();

            if (roomItemUI != null)
            {
                roomItemUI.SetRoomInfo(roomId, OnJoinRoomClick);
            }
        }
    }
    private void OnJoinRoomClick(string roomId)
    {
        Debug.Log($"Joining room: {roomId}");

        GameRoomManager.Instance.JoinRoom(currentChannelId, roomId, "PlayerID", "PlayerName");
    }
    public async void OnCreateRoomClick()
    {
        string roomName = roomNameInput.text.Trim();

        if (string.IsNullOrEmpty(roomName))
        {
            ShowError("�� �̸��� �Է��ϼ���.");
            return;
        }

        if (string.IsNullOrEmpty(currentChannelId))
        {
            ShowError("ä�� ID�� ã�� �� �����ϴ�.");
            return;
        }

        string roomId = await GameRoomManager.Instance.CreateRoomInChannel(currentChannelId, roomName);
        if (!string.IsNullOrEmpty(roomId))
        {
            Debug.Log($"[RoomCreationUI] �� ���� ����: {roomName} (Room ID: {roomId})");
            roomNameInput.text = "";
            LoadRoomList();
        }
        else
        {
            ShowError("�� ������ �����߽��ϴ�. �ٽ� �õ��ϼ���.");
        }
    }

    public void OnCancelClicked()
    {
        createRoomButton.onClick.RemoveAllListeners();
        //rankingButton.onClick.RemoveAllListeners();
        closeButton.onClick.RemoveAllListeners();
        base.Close();
    }

    private void ShowError(string message)
    {
        errorMessage.text = message;
        errorMessage.gameObject.SetActive(true);
    }
}

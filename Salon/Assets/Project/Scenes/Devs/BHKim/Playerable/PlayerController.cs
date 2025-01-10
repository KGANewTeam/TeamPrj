using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private string displayName;
    private bool isLocalPlayer;

    public void Initialize(string displayName, bool isLocalPlayer)
    {
        this.displayName = displayName;
        this.isLocalPlayer = isLocalPlayer;

        if (!isLocalPlayer)
        {
            // ��Ʈ��ũ�θ� ����ȭ�Ǵ� �÷��̾�� �Է� ��Ʈ�ѷ��� ��Ȱ��ȭ
            var inputController = GetComponent<PlayerInputController>();
            if (inputController != null)
            {
                inputController.enabled = false;
            }
        }
    }

    public void UpdatePosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }
}

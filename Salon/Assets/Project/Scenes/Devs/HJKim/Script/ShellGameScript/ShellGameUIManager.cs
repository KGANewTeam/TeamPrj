using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShellGameUIManager : MonoBehaviour
{
    [SerializeField]
    private Button[] difficulty_Button;
    public GameObject difficult_Panel;
    public GameObject betting_Panel;
    public Button start_button;
    private ShuffleManager shuffleManager;

    private void ShowBettingUI()
    {
        difficult_Panel.SetActive(false);
        betting_Panel.SetActive(true);

    }

    private void Start()
    {
        difficult_Panel.SetActive(true);
        betting_Panel.SetActive(false);

        shuffleManager = FindObjectOfType<ShuffleManager>();

        // ���̵� ��ư ����
        difficulty_Button[0].onClick.AddListener(() => {
            shuffleManager.SetDifficulty(SHELLDIFFICULTY.Easy);
            ShowBettingUI();
        });

        difficulty_Button[1].onClick.AddListener(() => {
            shuffleManager.SetDifficulty(SHELLDIFFICULTY.Nomal);
            ShowBettingUI();
        });

        difficulty_Button[2].onClick.AddListener(() => {
            shuffleManager.SetDifficulty(SHELLDIFFICULTY.Hard);
            ShowBettingUI();
        });

        // ���� ��ư ����
        start_button.onClick.AddListener(() => {
            betting_Panel.SetActive(false);  // ���� UI �����
            shuffleManager.StartGame();      // ���� ����
        });
    }


}

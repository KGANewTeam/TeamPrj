using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShellUIManager : MonoBehaviour
{
    // UI GameObjects
    public GameObject shell_Difficult_UI;
    public GameObject shell_Betting_UI;
    public GameObject shell_GameOver_UI;

    // Buttons
    public Button easy_Button;
    public Button normal_Button; 
    public Button hard_Button;


    private void Start()
    {//���ۻ��� �ʱ�ȭ
        Show_Shell_Difficult();
        Hide_Shell_Betting_UI();
    }
    // ù��° ���̵� ���� UI
        private void Show_Shell_Difficult()
    {
        shell_Difficult_UI.SetActive(true);
    }
        private void Hide_Shell_Difficult()
    {
        shell_Difficult_UI.SetActive(false);
    }

    private void Show_Shell_Betting_UI()
    {
        shell_Betting_UI.SetActive(true);
    }

    private void Hide_Shell_Betting_UI()
    {
        shell_Betting_UI.SetActive(false);
    }
}

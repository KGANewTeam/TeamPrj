using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class ShuffleManager : MonoBehaviour
{
    //[SerializeField]
    //public GameObject spinner_Prefab;
    [SerializeField]
    private List<Cup> cups = new List<Cup>();
    [SerializeField]
    private float spinDuration = 1f;//ȸ�� �ӵ� 
    private GameObject spinner;//�󲮵��� ���ǳ�

       
    private int cupCount;


    private SHELLDIFFICULTY shellDifficulty;

    private void Start()
    {//������ 1��(��� ��)�� ���� ����������.
        cups[1].hasBall = true;

        foreach (Cup cup in cups)
        {
            cup.gameObject.SetActive(false);
        }

        //TODO:���̵� ���� ��ư �Ҵ��ؾ� ��.
        shellDifficulty = SHELLDIFFICULTY.Easy;
        cupCount = (int)SHELLDIFFICULTY.Easy;

        //if (shellDifficulty == SHELLDIFFICULTY.Easy)
        //{
        //    cups[0].gameObject.SetActive(true);
        //    cups[1].gameObject.SetActive(true);
        //    cups[2].gameObject.SetActive(true);
        //}
        //else { }

        for (int i = 0; i < cupCount; i++)
        {
            cups[i].gameObject.SetActive(true);
        }

        CupShuffle();
    }

    private void CupShuffle()
    {//�� �ΰ��̱� 
        int firstCup = Random.Range(0,cupCount);
        int secondCup = Random.Range(0, cupCount);
        while (firstCup == secondCup)
        {
            firstCup = Random.Range(0, cupCount);
        }

        if (cups[firstCup].hasBall == true)
        {
            print("�������� ���ԵǾ��ֽ��ϴ�.");
        }

        //�� �����̱�
        Vector3 spinnerPos = (cups[firstCup].transform.position + cups[secondCup].transform.position) / 2f;
        //������ ������ ȸ�� �ʱ�ȭ

        spinner = new GameObject("Spinner");
        spinner.transform.position = spinnerPos;

        //�ڽ����� ����
        cups[firstCup].transform.SetParent(spinner.transform);
        cups[secondCup].transform.SetParent(spinner.transform);

        //ȸ�� ����

        //spinner.transform.rotation = Quaternion.Euler(0f, 180f, 0f);


    }
}


public enum SHELLDIFFICULTY
{
    Easy= 3,
    Nomal,
    Hard,
}

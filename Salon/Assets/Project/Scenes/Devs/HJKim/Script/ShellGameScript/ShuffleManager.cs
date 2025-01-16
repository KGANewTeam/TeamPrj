using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShuffleManager : MonoBehaviour
{
    //[SerializeField]
    //public GameObject spinner_Prefab;
    [SerializeField]
    private List<Cup> cups = new List<Cup>();
    [Header("Controller")]
    [SerializeField]
    private float spinSpeed =5f;//ȸ�� �ӵ� 
    [SerializeField]
    private float shuffleDuration = 5;
    [Header("Anime")]
    //�ʹ� �ִϸ��̼ǿ� �Ű� ����
    public GameObject anime_Cup;
    public GameObject anime_Ball;
    private GameObject spinner;//�󲮵��� ���ǳ�
    [SerializeField]
    private Transform table_pos;

    private int cupCount;
    private bool isStart = false;
    private bool isCanSelect = false;
    private float cupDis;

    private SHELLDIFFICULTY shellDifficulty;


    private void Update()
    {
        if (spinner != null)
        {
            CupShuffle();
        }
      else if (spinner ==null&& isStart==true)
        {
            SpawnSpinner();
        }
    }
    private void SpawnSpinner()
    {//�� �ΰ��̱� 
        int firstCup = Random.Range(0, cupCount);
        int secondCup = Random.Range(0, cupCount);
        while (firstCup == secondCup)
        {
            firstCup = Random.Range(0, cupCount);
        }

        //if (cups[firstCup].hasBall == true)
        //{
        //    print("�������� ���ԵǾ��ֽ��ϴ�.");
        //}

        //�� �����̱�
        Vector3 spinnerPos = (cups[firstCup].transform.position + cups[secondCup].transform.position) / 2f;
        cupDis = Vector3.Distance(cups[firstCup].transform.position , cups[secondCup].transform.position);
        cupDis = Mathf.Min(cupDis, 5f);
        //���ǳ� ����� ����
        spinner = new GameObject("Spinner");
        spinner.transform.position = spinnerPos;

        //�ڽ����� ����
        cups[firstCup].transform.SetParent(spinner.transform);
        cups[secondCup].transform.SetParent(spinner.transform);

        //ȸ�� ����

        //spinner.transform.rotation = Quaternion.Euler(0f, 180f, 0f);


    }
    private void CupShuffle()
    {
        spinner.transform.rotation = Quaternion.Lerp
            (spinner.transform.rotation,
            Quaternion.Euler(0f, 180f, 0f),
            Time.deltaTime * spinSpeed/cupDis);
        if (Quaternion.Angle(spinner.transform.rotation,
            Quaternion.Euler(0f, -180f, 0f)) < 0.05f){
            while (spinner.transform.childCount > 0)
            {
                spinner.transform.GetChild(0).SetParent(table_pos);
            }

            Destroy(spinner);
        }
    }

    private IEnumerator ShuffleStart()
    {
        yield return new WaitForSeconds(shuffleDuration);

        isStart=false;
        isCanSelect = true;
    }

    public void OnCupSelected(Cup cup)
    {
        if (isCanSelect == false)
        {
            return;
        }
        if (cup.hasBall == true)
        { 
            print("�¸�");
        }
        else if (cup.hasBall == false)
        {
            print("�й�");
        }
        isCanSelect = false;
    } 


    public void StartGame()//���Ⱑ ���� �����ϴ� ���� �ִ� ������

    {
        isStart = true;
        cups[1].hasBall = true;//������ �ִ����� 3��°��(�߾�)


        foreach (Cup cup in cups)
        {
            cup.Initialize(this);
            cup.gameObject.SetActive(false);
        }

        //���̵��� ������ŭ �� �ѱ�
        for (int i = 0; i < cupCount; i++)
        {
            cups[i].gameObject.SetActive(true);
        }
        StartCoroutine(ShuffleStart());
    }

    public void SetDifficulty(SHELLDIFFICULTY difficulty)
    {
        shellDifficulty = difficulty;
        cupCount = (int)shellDifficulty;
    }

    public void StartAnime()
    {//������ ���ؼ� �ϴ��� ����� ������
        cups[1].gameObject.SetActive(false);

    }
}


public enum SHELLDIFFICULTY
{
    Easy = 3,
    Nomal,
    Hard,
}
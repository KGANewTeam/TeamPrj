using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Dart : MonoBehaviour
{
    [SerializeField]
    private JoyStick joyStick;

    [SerializeField, Header("��Ʈ�� �߾�")]
    private Transform center;

    [SerializeField,Header("���� ������� ��ġ")]
    private ScroeSetPoint scroeMultiplyPoint = new ScroeSetPoint();

    private Transform[] transforms;

    //�߾ӿ��� ���� �浹��ġ ���� �� ����Լ�
    public void SetShootPoint(Vector3 Point)
    {
        float arrowDistance = Vector3.Magnitude(center.position - Point);

        float arrowAngle= Mathf.Atan2(Point.y - center.position.y, Point.x - center.position.x) * Mathf.Rad2Deg;

        //print(Point);
        //print(arrowDistance);
        //print(arrowAngle);

        int scores = GetScoreFromAngle(arrowAngle);

        scores = GetScoreMultiplier(arrowDistance,scores);

        print(scores);
    }

    //���� �������� �������
    public int GetScoreFromAngle(float arrowAngle)
    {
        int[] scores = new int[]
        {
            6, 13, 4, 18, 1, 20, 5, 12, 9, 14,
            11, 8, 16, 7, 19, 3, 17, 2, 15, 10
        };

        arrowAngle += 9f;
        if (arrowAngle < 0)
        {
            arrowAngle += 360f;
        }

        int section = Mathf.FloorToInt(arrowAngle / 18f);

        return scores[section];
    }

    //�߾ӿ��� �Ÿ��������� �߰����� ���
    public int GetScoreMultiplier(float arrowDistance,int scores)
    {
        if(arrowDistance < GetScoreDistance(scroeMultiplyPoint.FiftyPoint))
        {
            return 50;
        }
        if(arrowDistance < GetScoreDistance(scroeMultiplyPoint.TwentyfivePoint))
        {
            return 25;
        }
        if(arrowDistance > GetScoreDistance(scroeMultiplyPoint.ThreetimesPoint1) 
            && arrowDistance < GetScoreDistance(scroeMultiplyPoint.ThreetimesPoint2))
        {
            return scores * 3;
        }
        if(arrowDistance > GetScoreDistance(scroeMultiplyPoint.TwotimesPoint1)
            && arrowDistance < GetScoreDistance(scroeMultiplyPoint.TwotimesPoint2))
        {
            return scores * 2;
        }
        if (arrowDistance > GetScoreDistance(scroeMultiplyPoint.TwotimesPoint2))
        {
            return 0;
        }
        
        return scores;
    }

    //
    public float GetScoreDistance(Transform Point)
    {
        float Distance = Vector3.Magnitude(center.position - Point.position);

        return Distance;
    }
}

[Serializable]
public struct ScroeSetPoint
{
    public Transform FiftyPoint;
    public Transform TwentyfivePoint;
    public Transform ThreetimesPoint1;
    public Transform ThreetimesPoint2;
    public Transform TwotimesPoint1;
    public Transform TwotimesPoint2;
}
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;


public class Dart : MonoBehaviour
{
    [SerializeField]
    private JoyStick joyStick;

    [SerializeField, Header("��Ʈ�� �߾�")]
    private Transform center;

    [SerializeField,Header("���� ������� ��ġ")]
    private ScroeSetPoint scroeMultiplyPoint = new ScroeSetPoint();

    [SerializeField,Header("���ؿ� ������")]
    private AimingRing aimingRing;

    private AimingRing targetAiming;

    [SerializeField,Header("������Ʈ �ƿ�����")]
    private Transform outerLine;
   
    private void Start()
    {
        SpawnAimingRing();
    }

    private void Update()
    {
        AimingMove();
    }

    public void AimingMove()
    {
        if (targetAiming == null)
        {
            SpawnAimingRing();
        }
        Vector2 direction = joyStick.GetDirection();
        float  magnitude = joyStick.GetMagnitude();
        //print(magnitude);
        if (magnitude < 1f) return;

        Vector3 newPosition = targetAiming.transform.position +
            new Vector3(direction.x, direction.y, 0) * Time.deltaTime * 0.3f;

        float distanceFromCenter = Vector3.Distance(newPosition ,center.position);
        float maxAllowedDistance = Vector3.Distance(center.position, outerLine.position)
            - Vector3.Distance(targetAiming.transform.position, targetAiming.outerLine.position);

        if (distanceFromCenter > maxAllowedDistance)
        {
            Vector3 directionFromCenter = (newPosition -center.position).normalized;

            newPosition = center.position + directionFromCenter * maxAllowedDistance;
        }

        targetAiming.transform.position = new Vector3(newPosition.x, newPosition.y, targetAiming.transform.position.z);
    }

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


    public void SpawnAimingRing()
    {
        AimingRing aiming = Instantiate(aimingRing);

        float dartDistance = Vector3.Magnitude(center.position - outerLine.position);
        float aminingDistance = Vector3.Magnitude(aiming.transform.position - aiming.outerLine.position);

       //print(dartDistance);
       //print(aminingDistance);

        float maxSpawnDistance = dartDistance - aminingDistance;
            
       //print(maxSpawnDistance);
        Vector2 ranPos = Random.insideUnitCircle;

        ranPos = ranPos * maxSpawnDistance;

        Vector3 spawnPosition = center.position + new Vector3(ranPos.x, ranPos.y, -0.01f);

        aiming.transform.position = spawnPosition;

        targetAiming = aiming;
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
        print($"��Ʈ : {arrowDistance}");
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

    
    public float GetScoreDistance(Transform Point)
    {
        float Distance = Vector3.Magnitude(center.position - Point.position);

        print(Distance);
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

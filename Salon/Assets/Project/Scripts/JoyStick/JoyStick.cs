using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class JoyStick : MonoBehaviour, IEndDragHandler, IDragHandler, IPointerClickHandler
{
    [SerializeField, Header("���̽�ƽ �ڵ�")]
    private RectTransform handle;
    //��ġ�� ��ġ�� ���� ����
    private Vector2 touch;
    //�������� ���� ����
    private float widthHalf;
    //�ڵ� ��������
    private Vector2 startHandlePoint;

    [SerializeField, Header("�ڵ� ������ �ӵ�")]
    private float speed = 0.35f;

    //�ڵ� ��Ŀ�� �ΰ���
    private float joyStickSensitive = 0.3f;

    [SerializeField, Header("�ڵ� �̵����� ��Ŀ�� ������Ʈ")]
    private JoyStickFocus[] handleFocus;

    private Dictionary<FocusType,JoyStickFocus> handleFocusDict =
        new Dictionary<FocusType,JoyStickFocus>();

    [SerializeField, Header("�ڵ� �ִ� �̵��Ÿ�")]
    private float length = 0.8f;

    private void Start()
    {
        initialize();
    }

    public void initialize()
    {
        widthHalf = GetComponent<RectTransform>().sizeDelta.x * 0.5f;

        startHandlePoint = handle.localPosition;

        foreach (JoyStickFocus joyStickFocus in handleFocus)
        {
            handleFocusDict.Add(joyStickFocus.focusType,joyStickFocus);
            joyStickFocus.gameObject.SetActive(false);
        }


    }

    public void OnPointerClick(PointerEventData eventData)
    {
        print("Ŭ����");
    }


    public void OnDrag(PointerEventData eventData)
    {
        Vector2 touchPos = ((eventData.position - (Vector2)transform.position) * speed) / widthHalf;

        if (touchPos.magnitude > length)
        {
            touchPos = touchPos.normalized * length;
        }

        touch = touchPos / speed;
        handle.anchoredPosition = touchPos * widthHalf;
        FocusSetActive();
    }

    private void FocusSetActive()
    {
        if (touch.x > joyStickSensitive)
        {
            if (handleFocusDict.TryGetValue(FocusType.RIGHT,out JoyStickFocus joyStickFocus))
            {
                joyStickFocus.gameObject.SetActive(true);
            } 
        }
        else
        {
            if (handleFocusDict.TryGetValue(FocusType.RIGHT, out JoyStickFocus joyStickFocus))
            {
                joyStickFocus.gameObject.SetActive(false);
            }
        }
        if (touch.x < -joyStickSensitive)
        {
            if (handleFocusDict.TryGetValue(FocusType.LEFT, out JoyStickFocus joyStickFocus))
            {
                joyStickFocus.gameObject.SetActive(true);
            }
        }
        else
        {
            if (handleFocusDict.TryGetValue(FocusType.LEFT, out JoyStickFocus joyStickFocus))
            {
                joyStickFocus.gameObject.SetActive(false);
            }
        }
        if (touch.y > joyStickSensitive)
        {
            if (handleFocusDict.TryGetValue(FocusType.TOP, out JoyStickFocus joyStickFocus))
            {
                joyStickFocus.gameObject.SetActive(true);
            }
        }
        else
        {
            if (handleFocusDict.TryGetValue(FocusType.TOP, out JoyStickFocus joyStickFocus))
            {
                joyStickFocus.gameObject.SetActive(false);
            }
        }
        if (touch.y < -joyStickSensitive)
        {
            if (handleFocusDict.TryGetValue(FocusType.BOTTOM, out JoyStickFocus joyStickFocus))
            {
                joyStickFocus.gameObject.SetActive(true);
            }
        }
        else
        {
            if (handleFocusDict.TryGetValue(FocusType.BOTTOM, out JoyStickFocus joyStickFocus))
            {
                joyStickFocus.gameObject.SetActive(false);
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        handle.anchoredPosition = startHandlePoint;
        touch = Vector2.zero;
        foreach (JoyStickFocus handle in handleFocus)
        {
            handle.gameObject.SetActive(false);
        }
    }

    public Vector2 GetDirection()
    {
        return touch.normalized;
    }

    public float GetMagnitude()
    {
        return touch.magnitude / length;
    }

}

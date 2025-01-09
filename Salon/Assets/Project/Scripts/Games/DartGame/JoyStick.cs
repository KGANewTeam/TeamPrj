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

    [SerializeField, Header("�ڵ� �ִ� �̵��Ÿ�")]
    private float length = 50f;
    
    [SerializeField,Header("�ڵ� ��Ŀ�� �ΰ���")]
    private float joyStickSensitive = 10f;

    [SerializeField, Header("�ڵ� �̵����� ��Ŀ�� ������Ʈ")]
    private JoyStickFocus[] handleFocus;

    private Dictionary<FOCUSTYPE,JoyStickFocus> handleFocusDict =
        new Dictionary<FOCUSTYPE,JoyStickFocus>();


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

    //��ġ�ϴ� ���� �޼��� �Լ�
    public void OnPointerClick(PointerEventData eventData)
    {
        print("Ŭ����");
    }

    //�巡���� �޼��� �Լ�
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

    //���⿡���� ��Ŀ�� ��������
    private void FocusSetActive()
    {

        Dictionary<(float value, float threshold), FOCUSTYPE> directionMap = new Dictionary<(float value, float threshold), FOCUSTYPE>
    {
        { (touch.x, joyStickSensitive), FOCUSTYPE.LEFT },
        { (-touch.x, joyStickSensitive), FOCUSTYPE.RIGHT },
        { (touch.y, joyStickSensitive), FOCUSTYPE.BOTTOM },
        { (-touch.y, joyStickSensitive), FOCUSTYPE.TOP }
    };


        foreach (var focus in handleFocusDict.Values)
        {
            focus.gameObject.SetActive(false);
        }


        foreach (var direction in directionMap)
        {
            if (direction.Key.value > direction.Key.threshold)
            {
                if (handleFocusDict.TryGetValue(direction.Value, out JoyStickFocus focus))
                {
                    focus.gameObject.SetActive(true);
                }
            }
        }
    }

    //�巡�׳� �޼��� �Լ�
    public void OnEndDrag(PointerEventData eventData)
    {
        handle.anchoredPosition = startHandlePoint;
        touch = Vector2.zero;
        foreach (JoyStickFocus handle in handleFocus)
        {
            handle.gameObject.SetActive(false);
        }
    }

    //���̽�ƽ�� ���ϰ��ִ� ����
    public Vector2 GetDirection()
    {
        return touch.normalized;
    }

    //���̽�ƽ�� ����
    public float GetMagnitude()
    {
        return touch.magnitude / length;
    }

}

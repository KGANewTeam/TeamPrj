using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class JoyStick : MonoBehaviour, IEndDragHandler, IDragHandler, IPointerClickHandler
{
    [SerializeField, Header("조이스틱 핸들")]
    private RectTransform handle;
    //터치한 위치를 담을 변수
    private Vector2 touch;
    //반지름을 담을 변수
    private float widthHalf;
    //핸들 시작지점
    private Vector2 startHandlePoint;

    [SerializeField, Header("핸들 움직임 속도")]
    private float speed = 0.35f;

    //핸들 포커스 민감도
    private float joyStickSensitive = 0.3f;

    [SerializeField, Header("핸들 이동방향 포커스 오브젝트")]
    private JoyStickFocus[] handleFocus;

    private Dictionary<FOCUSTYPE,JoyStickFocus> handleFocusDict =
        new Dictionary<FOCUSTYPE,JoyStickFocus>();

    [SerializeField, Header("핸들 최대 이동거리")]
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

    //터치하는 순간 메세지 함수
    public void OnPointerClick(PointerEventData eventData)
    {
        print("클릭함");
    }

    //드래그중 메세지 함수
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

    //방향에따른 포커스 꺼짐켜짐
    private void FocusSetActive()
    {
        if (touch.x > joyStickSensitive)
        {
            if (handleFocusDict.TryGetValue(FOCUSTYPE.RIGHT,out JoyStickFocus joyStickFocus))
            {
                joyStickFocus.gameObject.SetActive(true);
            } 
        }
        else
        {
            if (handleFocusDict.TryGetValue(FOCUSTYPE.RIGHT, out JoyStickFocus joyStickFocus))
            {
                joyStickFocus.gameObject.SetActive(false);
            }
        }
        if (touch.x < -joyStickSensitive)
        {
            if (handleFocusDict.TryGetValue(FOCUSTYPE.LEFT, out JoyStickFocus joyStickFocus))
            {
                joyStickFocus.gameObject.SetActive(true);
            }
        }
        else
        {
            if (handleFocusDict.TryGetValue(FOCUSTYPE.LEFT, out JoyStickFocus joyStickFocus))
            {
                joyStickFocus.gameObject.SetActive(false);
            }
        }
        if (touch.y > joyStickSensitive)
        {
            if (handleFocusDict.TryGetValue(FOCUSTYPE.TOP, out JoyStickFocus joyStickFocus))
            {
                joyStickFocus.gameObject.SetActive(true);
            }
        }
        else
        {
            if (handleFocusDict.TryGetValue(FOCUSTYPE.TOP, out JoyStickFocus joyStickFocus))
            {
                joyStickFocus.gameObject.SetActive(false);
            }
        }
        if (touch.y < -joyStickSensitive)
        {
            if (handleFocusDict.TryGetValue(FOCUSTYPE.BOTTOM, out JoyStickFocus joyStickFocus))
            {
                joyStickFocus.gameObject.SetActive(true);
            }
        }
        else
        {
            if (handleFocusDict.TryGetValue(FOCUSTYPE.BOTTOM, out JoyStickFocus joyStickFocus))
            {
                joyStickFocus.gameObject.SetActive(false);
            }
        }
    }

    //드래그끝 메세지 함수
    public void OnEndDrag(PointerEventData eventData)
    {
        handle.anchoredPosition = startHandlePoint;
        touch = Vector2.zero;
        foreach (JoyStickFocus handle in handleFocus)
        {
            handle.gameObject.SetActive(false);
        }
    }

    //조이스틱이 향하고있는 방향
    public Vector2 GetDirection()
    {
        return touch.normalized;
    }

    //조이스틱의 강도
    public float GetMagnitude()
    {
        return touch.magnitude / length;
    }

}

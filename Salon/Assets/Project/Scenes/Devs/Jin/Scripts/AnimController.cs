using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;


//TODO: �ִ�Ŭ����ü ���߰�����
public class AnimController : MonoBehaviour 
{
    public List<AnimationClip> clipList = new List<AnimationClip>();
    
    public AnimationClip TestClip;
    public AnimationClip TestClip2;

    public Animator animator;

    private AnimatorOverrideController currentController;

    [SerializeField,Header("�ٲܷ����ϴ� Animator�� State�̸� �ٸ��� ������")]
    private string targetClipName = "ActionState";

    private string originalClipName = "";

    private void Start()
    {
       GetOriginalClipName();
       ClipChange(TestClip);
       ClipChange(TestClip2);
       animator.SetTrigger("ASTrigger");
    }
 
    public void Initialize()
    {
        animator = GetComponent<Animator>();

        currentController = SetupOverrideController();

        GetOriginalClipName();
    }

    private AnimatorOverrideController SetupOverrideController()
    {
        RuntimeAnimatorController Controller = animator.runtimeAnimatorController;
        AnimatorOverrideController overrideController = new AnimatorOverrideController(Controller);
        animator.runtimeAnimatorController = overrideController;
        return overrideController;
    }

    private void GetOriginalClipName()
    {
        if(animator.runtimeAnimatorController is AnimatorController controller)
        {
             ChildAnimatorState[] states = controller.layers[0].stateMachine.states;

           foreach(ChildAnimatorState state in states)
            {
                if(state.state.name == targetClipName &&
                    state.state.motion is AnimationClip clip)
                {
                    originalClipName = clip.name;
                    print(originalClipName);
                }
            }
        }


    }

    public void ClipChange(string clipname)
    {
        if (currentController == null)       
        {
            currentController = SetupOverrideController();
        }

        //Ŭ�� string���� ã�� ����
        AnimationClip targetClip = clipList.Find(clip => clip.name == clipname);
        if(targetClip != null)
        {
            currentController[originalClipName] = targetClip;
        }
        else
        {
            print($"����Ʈ�� '{clipname}' �̰ž���");
        }
    }

    public void ClipChange(AnimationClip animationClip)
    {
        if (currentController == null)
        {
            currentController = SetupOverrideController();
        }

        if (animationClip != null)
        {
            currentController[originalClipName] = animationClip;
        }
    }

    [Tooltip("Animator�� �����Ҷ� ��� �����ҷ��� State�̸� �����ϰ� �ؾ���")]
    public void AnimatorChange(RuntimeAnimatorController controller)
    {
        animator.runtimeAnimatorController = controller;

        currentController = SetupOverrideController();
        GetOriginalClipName();
    }

}

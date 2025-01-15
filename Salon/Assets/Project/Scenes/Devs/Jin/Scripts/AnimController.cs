using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;


//TODO: �ִ�Ŭ����ü ���߰�����
public class AnimController : MonoBehaviour 
{
    public List<AnimationClip> clipList = new List<AnimationClip>();
    
    public AnimationClip TestClip;

    public Animator animator;

    private AnimatorOverrideController currentController;

    private void Start()
    {
        ClipChange(TestClip);
        animator.SetTrigger("ASTrigger");
    }
 
    public void Initialize()
    {
        animator = GetComponent<Animator>();

        currentController = SetupOverrideController();

    }

    private AnimatorOverrideController SetupOverrideController()
    {
        RuntimeAnimatorController Controller = animator.runtimeAnimatorController;
        AnimatorOverrideController overrideController = new AnimatorOverrideController(Controller);
        animator.runtimeAnimatorController = overrideController;
        return overrideController;
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
            currentController["ActionState"] = targetClip;
        }
        else
        {
            print($"Animation clip '{clipname}' not found in clipList");
        }
    }

    public void ClipChange(AnimationClip animationClip)
    {
        if (currentController == null)
        {
            currentController = SetupOverrideController();
        }

        //Ŭ�� ��ü�� ����(Ŭ�� �̸��� ������ �ٲܼ�����)
        if (animationClip != null)
        {
            currentController["IdleArmSwing"] = animationClip;
        }
    }

    public void AnimatorChange(RuntimeAnimatorController controller)
    {
        animator.runtimeAnimatorController = controller;

        currentController = SetupOverrideController();
    }

}

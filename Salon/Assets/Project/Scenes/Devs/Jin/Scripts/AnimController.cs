using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO: ���߰�����
public class AnimController : MonoBehaviour 
{
    public List<AnimationClip> clipList = new List<AnimationClip>();

    private Animator animator;
    
    public void FindAnimaClip(string clipname)
    {
        animator = gameObject.GetComponent<Animator>();

        AnimatorOverrideController anima = new AnimatorOverrideController(animator.runtimeAnimatorController);

        animator.runtimeAnimatorController = anima;
    }

}

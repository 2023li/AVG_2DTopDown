using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class AIDecisionWaitingAnimationTime : AIDecision
{
    public override bool Decide()
    {
        return EvaluateTime();
    }


    public float AddTime;
    public string AnimName;
    [MMReadOnly]
    public float CurrentAnimationTime;

  

    public override void Initialization()
    {
        base.Initialization();
        RuntimeAnimatorController controller = gameObject.GetComponent<Character>().CharacterAnimator.runtimeAnimatorController;
        foreach (AnimationClip clip in controller.animationClips)
        {
          
            if (clip.name == AnimName)
            {
                CurrentAnimationTime = clip.length;
                break;
            }
        }

    }
    protected virtual bool EvaluateTime()
    {
        if (_brain == null) { return false; }

        return (_brain.TimeInThisState >= CurrentAnimationTime+ AddTime);
    }

}

using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using UnityEngine;

public class AIActionPlayAim : AIAction
{
    public string AniName;

    public override void OnEnterState()
    {
        base.OnExitState();
     
        gameObject.GetComponent<Character>().CharacterAnimator.SetBool(AniName, true);
    }
    public override void OnExitState()
    {
        base.OnExitState();
   
        gameObject.GetComponent<Character>().CharacterAnimator.SetBool(AniName, false);
    }
    public override void PerformAction()
    {
    
    }

   
}

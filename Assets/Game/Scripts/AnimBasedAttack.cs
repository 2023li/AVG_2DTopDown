using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

public class AnimBasedAttack : MonoBehaviour
{

    [MMLabel("伤害区域碰撞体")]
    [SerializeField]
    public Collider2D _damageAreaCollider2D;
    [MMLabel("伤害区激活时间")]
    public float ActiveDuration = 0.2f;

    //攻击是否正在进行
    private bool _attackInProgress = false;



  

    protected virtual IEnumerator MeleeWeaponAttack()
    {
      
        if (_attackInProgress) { yield break; }

        _attackInProgress = true;
        
        EnableDamageArea();
        yield return new WaitForSeconds(ActiveDuration);
        DisableDamageArea();
        _attackInProgress = false;
    }



    /// <summary>
    /// Enables the damage area.
    /// </summary>
    protected virtual void EnableDamageArea()
    {
    
        Debug.Log(_damageAreaCollider2D == null);
        if (_damageAreaCollider2D != null)
        {
           
            _damageAreaCollider2D.enabled = true;
        }
       
    }


    /// <summary>
    /// Disables the damage area.
    /// </summary>
    protected virtual void DisableDamageArea()
    {
        if (_damageAreaCollider2D != null)
        {
            _damageAreaCollider2D.enabled = false;
        }
       
    }

    public void ActivateDamageArea()
    {
        StartCoroutine(MeleeWeaponAttack());
    }
}

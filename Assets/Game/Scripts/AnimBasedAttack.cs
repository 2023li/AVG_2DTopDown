using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

public class AnimBasedAttack : MonoBehaviour
{

    [MMLabel("�˺�������ײ��")]
    [SerializeField]
    public Collider2D _damageAreaCollider2D;
    [MMLabel("�˺�������ʱ��")]
    public float ActiveDuration = 0.2f;

    //�����Ƿ����ڽ���
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

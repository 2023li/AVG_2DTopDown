using System.Collections;
using System.Collections.Generic;
using MoreMountains.TopDownEngine;
using UnityEngine;
using UnityEngine.SceneManagement;


//��MMCheckoutPoint����չ ����Ҫ����LO�浵ϵͳ��һЩ����
public class LOCheckoutPoint : CheckPoint
{



    protected override void TriggerEnter(GameObject collider)
    {
        base.TriggerEnter(collider);
        if (PersistenceManager.Instance != null)
        {
            PersistenceManager.Instance.SetLastScene(SceneManager.GetActiveScene().name);
            //Debug.Log("���������"+ SceneManager.GetActiveScene().name);
            PersistenceManager.Instance.SetLastPoint(this.CheckPointOrder);
            //�־û��洢
            PersistenceManager.Instance.SaveGameToFile();

        }

    }



}

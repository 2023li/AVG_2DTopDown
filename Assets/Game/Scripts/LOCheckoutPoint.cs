using System.Collections;
using System.Collections.Generic;
using MoreMountains.TopDownEngine;
using UnityEngine;
using UnityEngine.SceneManagement;


//对MMCheckoutPoint的扩展 还需要处理LO存档系统的一些数据
public class LOCheckoutPoint : CheckPoint
{



    protected override void TriggerEnter(GameObject collider)
    {
        base.TriggerEnter(collider);
        if (PersistenceManager.Instance != null)
        {
            PersistenceManager.Instance.SetLastScene(SceneManager.GetActiveScene().name);
            //Debug.Log("保存的名字"+ SceneManager.GetActiveScene().name);
            PersistenceManager.Instance.SetLastPoint(this.CheckPointOrder);
            //持久化存储
            PersistenceManager.Instance.SaveGameToFile();

        }

    }



}

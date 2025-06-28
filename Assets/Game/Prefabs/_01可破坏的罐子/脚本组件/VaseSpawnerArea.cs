using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VaseSpawnerArea : SpawnerArea
{


    private List<GO_Vase> CurrentVaseList;





    public override List<GameObject> SpawnObjects()
    {
        if (CurrentVaseList == null) 
        { 
            CurrentVaseList = new List<GO_Vase>();
        }
        else
        {

        }



        List<GameObject> go_vase = base.SpawnObjects();
        


        foreach (GameObject go in go_vase)
        {


        }


        return go_vase;
    }
}

using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using Sirenix.OdinInspector;
using UnityEngine;

public class GO_Vase : MonoBehaviour,ISceneAddedListener
{

    [SerializeField]
    private SO_Vase vaseData;

   

    [MMReadOnly]
    [SerializeField]
    private SO_Vase.VaseColor color = SO_Vase.VaseColor._×ØÉ«;

    [MMReadOnly]
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private bool isFirstAddToScenes = true;


    private Health health;
    void Reset()
    {
        spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }


    private void Start()
    {
        health = transform.GetChild(0).GetComponent<Health>();
        health.OnDeath += BeBroken;
    }
   

    public void OnAddedToScene()
    {
        if (isFirstAddToScenes)
        {
            RandomVase();
            isFirstAddToScenes=false;


        }
    }
    
    public void RandomVase()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        }

        color = vaseData.GetRandomVaseColor();
        spriteRenderer.sprite = vaseData.GetRandomVase(color);
    }

    [Button("Ë¢ÐÂ")]
    public void Refresh()
    {
        health.Revive();
        RandomVase();
    }

    public void BeBroken()
    {
        spriteRenderer.sprite = vaseData.GetRandomBrokenVase(color);
    }


}

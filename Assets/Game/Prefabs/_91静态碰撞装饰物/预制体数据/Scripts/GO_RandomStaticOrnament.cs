using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;


namespace LO
{
    public class GO_RandomStaticOrnament : MonoBehaviour,ISceneAddedListener
    {

        [SerializeField]
        [MMLabel("是否第一次添加到场景")]
       
        private bool isFristAddToScenes = true;

        [SerializeField]
        [MMLabel("添加时是否随机缩放")]
        private bool isRandomScaleOnAddScene = false;

        [ShowIf("isRandomScaleOnAddScene", false)]
        [SerializeField]
        private float minScale = 2;

        [ShowIf("isRandomScaleOnAddScene", false)]
        [SerializeField]
        private float maxScale = 2.5f;
        private void RandomScale()
        {
            if (isRandomScaleOnAddScene)
            {
                float scale = Random.Range(minScale, maxScale);
                transform.localScale = new Vector3(scale, scale, scale);
            }
        }


        [MMReadOnly]
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private SO_RandomSpritePool pool;




        void Reset()
        {
            spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        }


        public void OnAddedToScene()
        {
            if (!isFristAddToScenes) { return; }
            isFristAddToScenes = false;
            if (spriteRenderer != null)
            {
                spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = pool.GetRandomSprite();
            RandomScale();
        }


        [Button("刷新精灵")]
        public void RefreshSprite()
        {
            isFristAddToScenes = true;
            OnAddedToScene();
        }



    }
}
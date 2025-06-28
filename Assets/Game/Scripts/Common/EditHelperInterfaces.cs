using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 需要添加到场景中执行某些逻辑
/// </summary>
public interface ISceneAddedListener
{
#if UNITY_EDITOR
    void OnAddedToScene();
#endif
}


public interface ISceneSaveListener
{
    void OnSaveScene();
}
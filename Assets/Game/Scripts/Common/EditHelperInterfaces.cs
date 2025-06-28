using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ��Ҫ��ӵ�������ִ��ĳЩ�߼�
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
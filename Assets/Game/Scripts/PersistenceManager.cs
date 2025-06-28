using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.PixelFantasy.PixelHeroes.Common.Scripts.CharacterScripts;
using JetBrains.Annotations;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using Sirenix.OdinInspector;
using UnityEngine;


public enum LOGameLanguage
{
    Engilsh=0,
    简体中文 = 1,
}
public static class GameHelper
{

    public static void ClearAllChild(Transform transform)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }

    public static LOGameLanguage Int2LOGameLanguage(int i)
    {
        switch (i)
        {
            case 0: return LOGameLanguage.Engilsh;
            case 1: return LOGameLanguage.简体中文;
            default:
                Debug.LogWarning($"{i} 没有对应的语言类型");
                return LOGameLanguage.Engilsh;
                
        }
    }
}



public enum LOGameEventType
{
    AppInitComplete,

    SetLanguage,

    LoadSaveComplete,
}

public struct LOGameEvent
{
    public LOGameEventType eventTpye;
    public LOGameEvent(LOGameEventType eventTpye)
    {
        this.eventTpye = eventTpye; 
    }

    public static void Tigger(LOGameEvent e)
    {
        MMEventManager.TriggerEvent(e);
    }

}




/// <summary>
/// 处理跨场景相关数据
/// </summary>
public class PersistenceManager : MMPersistentSingleton<PersistenceManager>, MMEventListener<LOGameEvent>
{
    private static class Appkey
    {
       
        //第一次启动
        public const string FirstLaunch = "FirstLaunch";
        public const string Language = "Language";
        public const string Volume_BGM = "Volume_BGM";
        public const string Volume_SFX = "Volume_SFX";
     

    }

    [ReadOnly][SerializeField] private bool _firstLaunch = true;
    [ReadOnly][SerializeField] private LOGameLanguage _appLanguage = LOGameLanguage.简体中文;
    [ReadOnly][SerializeField] private float _volume_BGM = 1;
    [ReadOnly][SerializeField] private float _volume_SFX = 1;

    public bool FirstLaunch { get { return _firstLaunch; } }
    public void SetFirstLaunch()
    {
        _firstLaunch = false;
        ES3.Save(Appkey.FirstLaunch, _appLanguage);
    }



    public LOGameLanguage AppLanguage {  get { return _appLanguage; } }
    public void SetLanguage(LOGameLanguage newLanguage)
    {
        _appLanguage = newLanguage;
        ES3.Save(Appkey.Language, _appLanguage);
    }
    public float Volume_BGM { get { return _volume_BGM; } } 
    public float Volume_SFX { get { return _volume_SFX; } }

    private void TryFixAppData()
    {

    }
  
   
    private void LoadAppDataIfNullCreate()
    {
        // 直接使用 persistentDataPath，不再添加额外的 GameName 文件夹
        string fullPath = Path.Combine(Application.persistentDataPath, "AppData.loappsave");

        // 确保文件夹存在（虽然 Unity 的 persistentDataPath 目录通常已存在）
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        // 检查文件是否存在
        if (ES3.FileExists(fullPath))
        {

            LoadAppData();

        }
        else
        {
            CreateDefaultAppData();
            SaveAppData();
        }
    }
   
    private  void LoadAppData()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, "AppData.loappsave");
        _firstLaunch = ES3.Load<bool>(Appkey.FirstLaunch, fullPath);
        _appLanguage = ES3.Load<LOGameLanguage>(Appkey.Language, fullPath);
        _volume_SFX = ES3.Load<float>(Appkey.Volume_SFX, fullPath);
        _volume_BGM = ES3.Load<float>(Appkey.Volume_BGM, fullPath);
    }


    private void SaveAppData()
    {
        string path = Path.Combine(Application.persistentDataPath, "AppData.loappsave");

        ES3.Save(Appkey.FirstLaunch, FirstLaunch, path);
        ES3.Save<LOGameLanguage>(Appkey.Language, AppLanguage, path);
        ES3.Save(Appkey.Volume_SFX, 1f, path);
        ES3.Save(Appkey.Volume_BGM, 1f, path);
    }


    private void CreateDefaultAppData()
    {
        _firstLaunch = true;
        _appLanguage = LOGameLanguage.Engilsh;
        _volume_SFX = 1;
        _volume_BGM = 1;

     
    }




    //----------------------------存档数据------------------------------------
  
    private static class GDkey
    {
        public const string PlayerName = "PlayerName";
        public const string SaveID = "SaveID";
        public const string LastSceneName = "LastSceneName";
        public const string LastCheckoutPoint = "LastCheckoutPoint";
        public const string CharDress = "CharDress";

    }

    public string PlayerName { get { return _playerName; } }
    public string SaveOnlyID { get { return _saveOnlyID; } }
    public string SaveFolderPath { get { return _saveFolderPath; } }
    public string LastScene { get { return _lastScene; } }
    public int LastPoint { get { return _lastPoint; } }


    public string SetPlayerName(string name){ return _playerName = name;}
    public string SetLastScene(string scene){ return _lastScene = scene;}
    public int SetLastPoint(int point){ return _lastPoint = point;}
    

    [ReadOnly][SerializeField] private string _saveFolderPath = "";
    [ReadOnly][SerializeField] private string _saveOnlyID = "t1";
    [ReadOnly][SerializeField] private string _playerName = "p1";
    [ReadOnly][SerializeField] private string _lastScene = "_00启动场景";
    [ReadOnly][SerializeField] private int _lastPoint = 0;
    Dictionary<CharacterBuilderPartEnum, string> _dic_CharDress;
    public void CreateNewGameSave(string playerName)
    {
        _saveFolderPath = GetNextAvailableSavePath();
        _saveOnlyID = Guid.NewGuid().ToString();
        _playerName = playerName;
        _lastScene = "_00启动场景";
        _lastPoint = 0;
        _dic_CharDress = new Dictionary<CharacterBuilderPartEnum, string>();    




        //还要存玩家数据
        Debug.LogWarning("玩家数据部分未完成");
        SaveGameToFile();

    }


    private void TryFixGameData()
    {

    }

    [Button("测试保存")]
    public void SaveGameToFile()
    {
       
        if (!Directory.Exists(SaveFolderPath))
        {
            Directory.CreateDirectory(SaveFolderPath);
        }
        string fullPath = Path.Combine(SaveFolderPath, "GameData.logamesave");

        ES3.Save(GDkey.PlayerName, PlayerName, fullPath);
        ES3.Save(GDkey.SaveID, SaveOnlyID, fullPath);
        ES3.Save(GDkey.LastSceneName, LastScene, fullPath);
        ES3.Save(GDkey.LastCheckoutPoint, LastPoint, fullPath);
        ES3.Save(GDkey.CharDress, _dic_CharDress, fullPath);


        DateTime currentTime = DateTime.Now;
        string timeString = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
        ES3.Save(LOGameConstant.str_LastSavedDate, timeString, fullPath);

    }

    public void LoadSave(string fullPath)
    {

        _saveFolderPath = Path.GetDirectoryName(fullPath);
        _saveOnlyID = ES3.Load<string>(GDkey.SaveID, fullPath);
        _playerName = ES3.Load<string>(GDkey.PlayerName, fullPath);
        _lastScene = ES3.Load<string>(GDkey.LastSceneName, fullPath);
        _lastPoint = ES3.Load<int>(GDkey.LastCheckoutPoint, fullPath);
        _dic_CharDress = ES3.Load<Dictionary<CharacterBuilderPartEnum, string>>(GDkey.CharDress, fullPath);

        Debug.Log("数据加载完成");
        //SceneHelp会监听这个事件 并且去跳转场景
        LOGameEvent e = new LOGameEvent(LOGameEventType.LoadSaveComplete);
        LOGameEvent.Tigger(e);

    }


    public struct SaveGameInfo
    {
        public string PlayerName;
        public string SaveTime;
        public string SavePath; // 新增字段
    }
    public List<SaveGameInfo> GetAllSaveGames()
    {
        List<SaveGameInfo> saveGames = new List<SaveGameInfo>();
        string basePath = Application.persistentDataPath;

        // 获取所有以"Save"开头的文件夹
        string[] saveFolders = Directory.GetDirectories(basePath, "Save*");

        foreach (string folder in saveFolders)
        {
            string saveFile = Path.Combine(folder, "GameData.logamesave");

            if (File.Exists(saveFile))
            {
                try
                {
                    // 从存档文件读取数据
                    string playerName = ES3.Load<string>("PlayerName", saveFile);
                    string saveTime = ES3.Load<string>(LOGameConstant.str_LastSavedDate, saveFile);

                    saveGames.Add(new SaveGameInfo
                    {
                        PlayerName = playerName,
                        SaveTime = saveTime,
                        SavePath = saveFile // 添加保存路径
                    });
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"读取存档失败: {folder}\n错误信息: {e.Message}");
                }
            }
        }

        return saveGames;
    }






   
    public Dictionary<CharacterBuilderPartEnum, string> GetCharDressDic()
    {
        return _dic_CharDress;
    }
    public void SaveCharDress(CharacterBuilder character)
    {
        if (_dic_CharDress == null)
        { _dic_CharDress = new Dictionary<CharacterBuilderPartEnum, string>(); }

        CharacterBuilderHelper.CharacterDressToDic(character, ref _dic_CharDress);

    }



    void Start()
    {
        LoadAppDataIfNullCreate();

        LOGameEvent e = new LOGameEvent(LOGameEventType.AppInitComplete);
        Debug.Log("App数据加载完成");
        MMEventManager.TriggerEvent(e);
        
    }



   





    /// 返回下一个可用的路径
    private string GetNextAvailableSavePath()
    {
        // 基础路径：Application.persistentDataPath
        string basePath = Application.persistentDataPath;

        // 从 1 开始检查每个可能的保存文件夹
        int saveNumber = 1;

        while (true)
        {
            // 构建当前检查的路径（如 "Save1", "Save2" 等）
            string savePath = Path.Combine(basePath, $"Save{saveNumber}");

            // 检查该文件夹是否存在
            if (!Directory.Exists(savePath))
            {
                // 如果不存在，返回这个路径
                return savePath;
            }

            // 如果存在，继续检查下一个编号
            saveNumber++;
        }
    }

    public void OnMMEvent(LOGameEvent e)
    {
        switch (e.eventTpye)
        {
            case LOGameEventType.AppInitComplete:
                break;
            case LOGameEventType.SetLanguage:
               


                break;
            default:
                break;
        }
    }
}

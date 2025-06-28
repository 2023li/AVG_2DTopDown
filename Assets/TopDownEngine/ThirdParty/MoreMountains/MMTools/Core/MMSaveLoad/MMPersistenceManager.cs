using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Add this component to a scene and it'll let you save and load the state of objects that implement the IMMPersistent interface
    /// You can create your own classes that implement this interface, or use the MMPersistent class that comes with this package
    /// It will save their transform data (position, rotation, scale) and their active state
    /// Triggering save and load is done via events, and the manager also emits events every time data is loaded or saved
    /// 
    /// 
    /// 将此组件添加到场景中，它将允许你保存和加载实现 IMMPersistent 接口的对象的状态。
    /// 你可以创建自己的实现此接口的类，或者使用此包中自带的 MMPersistent 类。
    /// 它将保存对象的变换数据（位置、旋转、缩放）及其激活状态。
    /// 触发保存和加载是通过事件完成的，并且管理器每次加载或保存数据时也会发出事件。
    /// </summary>
    public class MMPersistenceManager : MMPersistentSingleton<MMPersistenceManager>, MMEventListener<MMGameEvent>
	{
        [Header("持久化设置")]
        /// A persistence ID used to identify...
        [MMLabel("持久化ID")]
        [Tooltip("管理器的唯一标识ID（通常保留默认值）")]
        public string PersistenceID = "MMPersistency";

        [Header("事件设置")]
        /// whether or not this manager should listen...
        [MMLabel("监听保存事件")]
        [Tooltip("是否响应全局保存事件（否则需手动调用SaveToMemory/SaveFromMemoryToFile）")]
        public bool ListenForSaveEvents = true;

        [MMLabel("监听加载事件")]
        [Tooltip("是否响应全局加载事件（否则需手动调用LoadFromMemory/LoadFromFileToMemory）")]
        public bool ListenForLoadEvents = true;

        [MMLabel("监听内存保存")]
        [Tooltip("是否响应内存保存事件（否则需手动调用SaveToMemory）")]
        public bool ListenForSaveToMemoryEvents = true;

        [MMLabel("监听内存加载")]
        [Tooltip("是否响应内存加载事件（否则需手动调用LoadFromMemory）")]
        public bool ListenForLoadFromMemoryEvents = true;

        [MMLabel("监听文件保存")]
        [Tooltip("是否响应文件保存事件（否则需手动调用SaveFromMemoryToFile）")]
        public bool ListenForSaveToFileEvents = true;

        [MMLabel("监听文件加载")]
        [Tooltip("是否响应文件加载事件（否则需手动调用LoadFromFileToMemory）")]
        public bool ListenForLoadFromFileEvents = true;

        [MMLabel("保存时存文件")]
        [Tooltip("响应保存事件时是否自动将数据保存到文件")]
        public bool SaveToFileOnSaveEvents = true;

        [MMLabel("加载时读文件")]
        [Tooltip("响应加载事件时是否自动从文件加载数据")]
        public bool LoadFromFileOnLoadEvents = true;

        [Header("调试按钮（仅运行时）")]
        [MMLabel("内存保存")]
        [MMInspectorButton("SaveToMemory")]
        public bool SaveToMemoryButton;

        [MMLabel("内存加载")]
        [MMInspectorButton("LoadFromMemory")]
        public bool LoadFromMemoryButton;

        [MMLabel("保存到文件")]
        [MMInspectorButton("SaveFromMemoryToFile")]
        public bool SaveToFileButton;

        [MMLabel("从文件加载")]
        [MMInspectorButton("LoadFromFileToMemory")]
        public bool LoadFromFileButton;

        [MMLabel("删除持久文件")]
        [MMInspectorButton("DeletePersistenceFile")]
        public bool DeletePersistenceFileButton;

        public DictionaryStringSceneData SceneDatas;
		
		public static string _resourceItemPath = "Persistence/";
		public static string _saveFolderName = "MMTools/";
		public static string _saveFileExtension = ".persistence";

		protected string _currentSceneName;

		#region INITIALIZATION
			/// <summary>
			/// Statics initialization to support enter play modes
			/// </summary>
			[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
			protected static void InitializeStatics()
			{
				_instance = null;
			}
		
			/// <summary>
			/// On Awake we initialize our dictionary
			/// </summary>
			protected override void Awake()
			{
				base.Awake();
				SceneDatas = new DictionaryStringSceneData();
			}

		#endregion

		#region SAVE_AND_LOAD

			/// <summary>
			/// Saves data from objects that need saving to memory
			/// </summary>
			public virtual void SaveToMemory()
			{
				ComputeCurrentSceneName();

				SceneDatas.Remove(_currentSceneName);
				
				MMPersistenceSceneData sceneData = new MMPersistenceSceneData();
				sceneData.ObjectDatas = new DictionaryStringString();
				
				IMMPersistent[] persistents = FindAllPersistentObjects();
				
				foreach (IMMPersistent persistent in persistents)
				{
					if (persistent.ShouldBeSaved())
					{
						sceneData.ObjectDatas.Add(persistent.GetGuid(), persistent.OnSave());	
					}
				}

				SceneDatas.Add(_currentSceneName, sceneData);

				MMPersistenceEvent.Trigger(MMPersistenceEventType.DataSavedToMemory, PersistenceID);
			}

			/// <summary>
			/// Loads data from memory and applies it to all objects that need it
			/// </summary>
			public virtual void LoadFromMemory()
			{
				ComputeCurrentSceneName();
				
				if (!SceneDatas.TryGetValue(_currentSceneName, out MMPersistenceSceneData sceneData))
				{
					return;
				}
				
				if (sceneData.ObjectDatas == null)
				{
					return;
				}
				
				IMMPersistent[] persistents = FindAllPersistentObjects();
				foreach (IMMPersistent persistent in persistents)
				{
					if (sceneData.ObjectDatas.TryGetValue(persistent.GetGuid(), out string data))
					{
						persistent.OnLoad(sceneData.ObjectDatas[persistent.GetGuid()]);
					}
				}
				
				MMPersistenceEvent.Trigger(MMPersistenceEventType.DataLoadedFromMemory, PersistenceID);
			}

			/// <summary>
			/// Saves data from memory to a file
			/// </summary>
			public virtual void SaveFromMemoryToFile()
			{
				MMPersistenceManagerData saveData = new MMPersistenceManagerData();
				saveData.PersistenceID = PersistenceID;
				saveData.SaveDate = DateTime.Now.ToString();
				saveData.SceneDatas = SceneDatas;
				MMSaveLoadManager.Save(saveData, DetermineSaveName(), _saveFolderName);
				
				MMPersistenceEvent.Trigger(MMPersistenceEventType.DataSavedFromMemoryToFile, PersistenceID);
			}

			/// <summary>
			/// Loads data from file and stores it in memory
			/// </summary>
			public virtual void LoadFromFileToMemory()
			{
				MMPersistenceManagerData saveData = (MMPersistenceManagerData)MMSaveLoadManager.Load(typeof(MMPersistenceManagerData), DetermineSaveName(), _saveFolderName);
				if ((saveData != null) && (saveData.SceneDatas != null))
				{
					SceneDatas = new DictionaryStringSceneData();
					SceneDatas = saveData.SceneDatas;	
				}
				MMPersistenceEvent.Trigger(MMPersistenceEventType.DataLoadedFromFileToMemory, PersistenceID);
			}
			
			/// <summary>
			/// On Save, we save to memory and to file if needed
			/// </summary>
			public virtual void Save()
			{
				SaveToMemory();
				if (SaveToFileOnSaveEvents)
				{
					SaveFromMemoryToFile();
				}
			}

			/// <summary>
			/// On Load, we load from memory and from file if needed
			/// </summary>
			public virtual void Load()
			{
				if (LoadFromFileOnLoadEvents)
				{
					LoadFromFileToMemory();
				}
				LoadFromMemory();
			}

		#endregion

		#region RESET

			/// <summary>
			/// Deletes all persistence data for the specified scene
			/// </summary>
			/// <param name="sceneName"></param>
			public virtual void DeletePersistencyMemoryForScene(string sceneName)
			{
				if (!SceneDatas.TryGetValue(_currentSceneName, out MMPersistenceSceneData sceneData))
				{
					return;
				}
				SceneDatas.Remove(sceneName);
			}

			/// <summary>
			/// Deletes persistence data from memory and on file for this persistence manager
			/// </summary>
			public virtual void ResetPersistence()
			{
				DeletePersistenceMemory();
				DeletePersistenceFile();
			}

			/// <summary>
			/// Deletes all persistence data stored in this persistence manager's memory
			/// </summary>
			public virtual void DeletePersistenceMemory()
			{
				SceneDatas = new DictionaryStringSceneData();
			}
				
			/// <summary>
			/// Deletes the save file for this persistence manager
			/// </summary>
			public virtual void DeletePersistenceFile()
			{
				MMSaveLoadManager.DeleteSave(DetermineSaveName(), _saveFolderName);
				Debug.LogFormat("Persistence save file deleted");
			}

		#endregion

		#region HELPERS

			/// <summary>
			/// Finds all objects in the scene that implement IMMPersistent and may need saving
			/// </summary>
			/// <returns></returns>
			protected virtual IMMPersistent[] FindAllPersistentObjects()
			{
				return FindObjectsOfType<MonoBehaviour>(true).OfType<IMMPersistent>().ToArray();
			}

			/// <summary>
			/// Grabs the current scene's name and stores it 
			/// </summary>
			protected virtual void ComputeCurrentSceneName()
			{
				_currentSceneName = SceneManager.GetActiveScene().name;
			}

			/// <summary>
			/// Determines the name of the file to write to store persistence data
			/// </summary>
			/// <returns></returns>
			protected virtual string DetermineSaveName()
			{
				return gameObject.name + "_" + PersistenceID + _saveFileExtension;
			}

		#endregion

		#region EVENTS

			/// <summary>
			/// When we get a MMEvent, we filter on its name and invoke the appropriate methods if needed
			/// </summary>
			/// <param name="gameEvent"></param>
			public virtual void OnMMEvent(MMGameEvent gameEvent)
			{
				if ((gameEvent.EventName == "Save") && ListenForSaveEvents)
				{
					Save();
				}
				if ((gameEvent.EventName == "Load") && ListenForLoadEvents)
				{
					Load();
				}
				if ((gameEvent.EventName == "SaveToMemory") && ListenForSaveToMemoryEvents)
				{
					SaveToMemory();
				}
				if ((gameEvent.EventName == "LoadFromMemory") && ListenForLoadFromMemoryEvents)
				{
					LoadFromMemory();
				}
				if ((gameEvent.EventName == "SaveToFile") && ListenForSaveToFileEvents)
				{
					SaveFromMemoryToFile();
				}
				if ((gameEvent.EventName == "LoadFromFile") && ListenForLoadFromFileEvents)
				{
					LoadFromFileToMemory();
				}
			}
			
			/// <summary>
			/// On enable, we start listening for MMGameEvents
			/// </summary>
			protected virtual void OnEnable()
			{
				this.MMEventStartListening<MMGameEvent>();
			}
			
			/// <summary>
			/// On enable, we stop listening for MMGameEvents
			/// </summary>
			protected virtual void OnDisable()
			{
				this.MMEventStopListening<MMGameEvent>();
			}

		#endregion
	}	
}


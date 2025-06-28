using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;


namespace MoreMountains.TopDownEngine
{	
	/// <summary>
	/// Spawns the player, handles checkpoints and respawn
	/// </summary>
	[AddComponentMenu("TopDown Engine/Managers/Level Manager")]
	public class LevelManager : MMSingleton<LevelManager>, MMEventListener<TopDownEngineEvent>
	{
        /// the prefab you want for your player
        [Header("角色实例化")]
        [MMInformation("LevelManager负责处理生成/重生、检查点管理和关卡边界。在此可定义关卡的可用角色。", MMInformationAttribute.InformationType.Info, false)]
        /// should the player IDs be auto attributed (usually yes)
        [MMLabel("是否自动分配玩家ID")]
        [Tooltip("should the player IDs be auto attributed (usually yes)")]
        public bool AutoAttributePlayerIDs = true;
        /// the list of player prefabs to instantiate
        [MMLabel("玩家预制体列表")]
        [Tooltip("The list of player prefabs this level manager will instantiate on Start")]
        public Character[] PlayerPrefabs;

        [Header("场景中已有角色")]
        [MMInformation("推荐通过LevelManager实例化角色，但若需使用场景中已有角色，请绑定下方列表。", MMInformationAttribute.InformationType.Info, false)]
        /// a list of Characters already present in the scene before runtime. If this list is filled, PlayerPrefabs will be ignored
        [MMLabel("场景中存在的角色")]
        [Tooltip("场景中预先存在的角色列表（若使用则忽略PlayerPrefabs）")]
        public List<Character> SceneCharacters;

        [Header("检查点设置")]
        /// the checkpoint to use as initial spawn point if no point of entry is specified
        [MMLabel("初始生成点")]
        [Tooltip("未指定入口点时使用的初始生成点")]
        public CheckPoint InitialSpawnPoint;
        /// the currently active checkpoint (the last checkpoint passed by the player)
        [MMLabel("当前检查点")]
        [Tooltip("当前激活的检查点（玩家最后通过的检查点）")]
        public CheckPoint CurrentCheckpoint;

        [Header("关卡入口点")]
        /// A list of this level's points of entry, which can be used from other levels as initial targets
        [MMLabel("入口列表")]
        [Tooltip("本关卡入口点列表（可从其他关卡指定进入位置）")]
        public Transform[] PointsOfEntry;

        [Space(10)]
        [Header("开场/结束动画时长")]
        [MMInformation("设置关卡开始/结束时的淡入淡出时长，以及重生前的延迟时间。", MMInformationAttribute.InformationType.Info, false)]
        /// duration of the initial fade in (in seconds)
        [MMLabel("开场淡入时长")]
        [Tooltip("the duration of the initial fade in (in seconds)")]
        public float IntroFadeDuration = 1f;

        public float SpawnDelay = 0f;
        /// duration of the fade to black at the end of the level (in seconds)
        [MMLabel("结束淡出时长（秒）")]
        [Tooltip("关卡结束时的淡出黑屏时间（秒）")]
        public float OutroFadeDuration = 1f;
        /// the ID to use when triggering the event (should match the ID on the fader you want to use)
        [MMLabel("淡出事件ID")]
        [Tooltip("淡入淡出事件使用的ID（需与Fader组件匹配）")]
        public int FaderID = 0;

        /// the curve to use for in and out fades
        [Tooltip("淡出曲线类型")]
        public MMTweenType FadeCurve = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);
        /// duration between a death of the main character and its respawn
        [Tooltip("the duration between a death of the main character and its respawn")]
        [MMLabel("玩家重生倒计时")]
        public float RespawnDelay = 2f;

        [Header("重生流程")]
        /// the delay, in seconds, before displaying the death screen once the player is dead
        [MMLabel("死亡界面延迟")]
        [Tooltip("玩家死亡后显示死亡界面的延迟时间（秒）")]
        public float DelayBeforeDeathScreen = 1f;

        [Header("关卡边界")]
        /// if this is true, this level will use the level bounds defined on this LevelManager. Set it to false when using the Rooms system.
        [MMLabel("是否使用Level Manager定义边界")]
        [Tooltip("是否使用本LevelManager定义的边界（使用房间系统时应关闭）")]
        public bool UseLevelBounds = true;

        [Header("场景加载设置")]

        /// the method to use to load the destination level
        [MMLabel("场景加载模式")]
        [Tooltip("Unity原生API 或是 MM")]
        public MMLoadScene.LoadingSceneModes LoadingSceneMode = MMLoadScene.LoadingSceneModes.MMSceneLoadingManager;

        /// the name of the MMSceneLoadingManager scene you want to use
        [Tooltip("the name of the MMSceneLoadingManager scene you want to use")]
        [MMEnumCondition("LoadingSceneMode", (int)MMLoadScene.LoadingSceneModes.MMSceneLoadingManager)]
        public string LoadingSceneName = "LoadingScreen";

        /// the settings to use when loading the scene in additive mode
        [MMLabel("加载参数")]
        [Tooltip("附加场景加载模式的设置参数")]
        [MMEnumCondition("LoadingSceneMode", (int)MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager)]
        public MMAdditiveSceneLoadingManagerSettings AdditiveLoadingSettings;

        [Header("Feedbacks")]

        /// if this is true, an event will be triggered on player instantiation to set the range target of all feedbacks to it
        [MMLabel("是否在生成玩家时将其设为反馈范围中心")]
        [Tooltip("如果这为true，在玩家实例化时将触发一个事件，以将所有反馈的范围目标设置为它。")]
        public bool SetPlayerAsFeedbackRangeCenter = false;

    
        
		/// the level limits, camera and player won't go beyond this point.
		public virtual Bounds LevelBounds {  get { return (_collider==null)? new Bounds(): _collider.bounds; } }
		public virtual Collider BoundsCollider { get; protected set; }
		public virtual Collider2D BoundsCollider2D { get; protected set; }

		/// the elapsed time since the start of the level
		public virtual TimeSpan RunningTime { get { return DateTime.UtcNow - _started ;}}
        
		// private stuff
		public virtual List<CheckPoint> Checkpoints { get; protected set; }
		public virtual List<Character> Players { get; protected set; }

		protected DateTime _started;
		protected int _savedPoints;
		protected Collider _collider;
		protected Collider2D _collider2D;
		protected Vector3 _initialSpawnPointPosition;
		
		/// <summary>
		/// Statics initialization to support enter play modes
		/// </summary>
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			_instance = null;
		}
		
		/// <summary>
		/// On awake, instantiates the player
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			_collider = this.GetComponent<Collider>();
			_collider2D = this.GetComponent<Collider2D>();
		}

		/// <summary>
		/// On Start we grab our dependencies and initialize spawn
		/// </summary>
		protected virtual void Start()
		{
			StartCoroutine(InitializationCoroutine());
		}

		protected virtual IEnumerator InitializationCoroutine()
		{
			if (SpawnDelay > 0f)
			{
				yield return MMCoroutine.WaitFor(SpawnDelay);    
			}

			BoundsCollider = _collider;
			BoundsCollider2D = _collider2D;
			InstantiatePlayableCharacters();

			if (UseLevelBounds)
			{
				MMCameraEvent.Trigger(MMCameraEventTypes.SetConfiner, null, BoundsCollider, BoundsCollider2D);
			}            
            
			if (Players == null || Players.Count == 0) { yield break; }

			Initialization();

			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnCharacterStarts, null);

			// we handle the spawn of the character(s)
			if (Players.Count == 1)
			{
				SpawnSingleCharacter();
			}
			else
			{
				SpawnMultipleCharacters ();
			}

			CheckpointAssignment();

			// we trigger a fade
			MMFadeOutEvent.Trigger(IntroFadeDuration, FadeCurve, FaderID);

			// we trigger a level start event
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelStart, null);
			MMGameEvent.Trigger("Load");

			if (SetPlayerAsFeedbackRangeCenter)
			{
				MMSetFeedbackRangeCenterEvent.Trigger(Players[0].transform);
			}

			MMCameraEvent.Trigger(MMCameraEventTypes.SetTargetCharacter, Players[0]);
			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);
			MMGameEvent.Trigger("CameraBound");
		}

		/// <summary>
		/// A method meant to be overridden by each multiplayer level manager to describe how to spawn characters
		/// </summary>
		protected virtual void SpawnMultipleCharacters()
		{

		}

		/// <summary>
		/// Instantiate playable characters based on the ones specified in the PlayerPrefabs list in the LevelManager's inspector.
		/// </summary>
		protected virtual void InstantiatePlayableCharacters()
		{
			_initialSpawnPointPosition = (InitialSpawnPoint == null) ? Vector3.zero : InitialSpawnPoint.transform.position;
			
			Players = new List<Character> ();

			if (GameManager.Instance.PersistentCharacter != null)
			{
				Players.Add(GameManager.Instance.PersistentCharacter);
				return;
			}
			
			// we check if there's a stored character in the game manager we should instantiate
			if (GameManager.Instance.StoredCharacter != null)
			{
				Character newPlayer = Instantiate(GameManager.Instance.StoredCharacter, _initialSpawnPointPosition, Quaternion.identity);
				newPlayer.name = GameManager.Instance.StoredCharacter.name;
				Players.Add(newPlayer);
				return;
			}

			if ((SceneCharacters != null) && (SceneCharacters.Count > 0))
			{
				foreach (Character character in SceneCharacters)
				{
					Players.Add(character);
				}
				return;
			}

			if (PlayerPrefabs == null) { return; }

			// player instantiation
			if (PlayerPrefabs.Length != 0)
			{ 
				foreach (Character playerPrefab in PlayerPrefabs)
				{
					Character newPlayer = Instantiate (playerPrefab, _initialSpawnPointPosition, Quaternion.identity);
					newPlayer.name = playerPrefab.name;
					Players.Add(newPlayer);

					if (playerPrefab.CharacterType != Character.CharacterTypes.Player)
					{
						Debug.LogWarning ("LevelManager : The Character you've set in the LevelManager isn't a Player, which means it's probably not going to move. You can change that in the Character component of your prefab.");
					}
				}
			}


		}

		/// <summary>
		/// Assigns all respawnable objects in the scene to their checkpoint
		/// </summary>
		protected virtual void CheckpointAssignment()
		{
			// we get all respawnable objects in the scene and attribute them to their corresponding checkpoint
			IEnumerable<Respawnable> listeners = FindObjectsOfType<MonoBehaviour>(true).OfType<Respawnable>();
			AutoRespawn autoRespawn;
			foreach (Respawnable listener in listeners)
			{
				for (int i = Checkpoints.Count - 1; i >= 0; i--)
				{
					autoRespawn = (listener as MonoBehaviour).GetComponent<AutoRespawn>();
					if (autoRespawn == null)
					{
						Checkpoints[i].AssignObjectToCheckPoint(listener);
						continue;
					}
					else
					{
						if (autoRespawn.IgnoreCheckpointsAlwaysRespawn)
						{
							Checkpoints[i].AssignObjectToCheckPoint(listener);
							continue;
						}
						else
						{
							if (autoRespawn.AssociatedCheckpoints.Contains(Checkpoints[i]))
							{
								Checkpoints[i].AssignObjectToCheckPoint(listener);
								continue;
							}
							continue;
						}
					}
				}
			}
		}


		/// <summary>
		/// Gets current camera, points number, start time, etc.
		/// </summary>
		protected virtual void Initialization()
		{
			Checkpoints = FindObjectsOfType<CheckPoint>().OrderBy(o => o.CheckPointOrder).ToList();
			_savedPoints =GameManager.Instance.Points;
			_started = DateTime.UtcNow;
		}

        /// <summary>
        /// Spawns a playable character into the scene
        /// 在场景中生成一个可操控角色
        /// </summary>
        protected virtual void SpawnSingleCharacter()
		{
			PointsOfEntryStorage point = GameManager.Instance.GetPointsOfEntry(SceneManager.GetActiveScene().name);
			if ((point != null) && (PointsOfEntry.Length >= (point.PointOfEntryIndex + 1)))
			{
				Players[0].RespawnAt(PointsOfEntry[point.PointOfEntryIndex], point.FacingDirection);
				TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnComplete, Players[0]);
				return;
			}

			if (InitialSpawnPoint != null)
			{
				InitialSpawnPoint.SpawnPlayer(Players[0]);
				TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnComplete, Players[0]);
				return;
			}

		}

		/// <summary>
		/// Gets the player to the specified level
		/// </summary>
		/// <param name="levelName">Level name.</param>
		public virtual void GotoLevel(string levelName)
		{
			//触发结束事件
			TriggerEndLevelEvents();
			StartCoroutine(GotoLevelCo(levelName));
		}

		/// <summary>
		/// Triggers end of level events
		/// </summary>
		public virtual void TriggerEndLevelEvents()
		{
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelEnd, null);
			MMGameEvent.Trigger("Save");
		}

		/// <summary>
		/// Waits for a short time and then loads the specified level
		/// </summary>
		/// <returns>The level co.</returns>
		/// <param name="levelName">Level name.</param>
		protected virtual IEnumerator GotoLevelCo(string levelName)
		{
			if (Players != null && Players.Count > 0)
			{ 
				foreach (Character player in Players)
				{
					player.Disable ();	
				}	    		
			}

			MMFadeInEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID);
            
			if (Time.timeScale > 0.0f)
			{ 
				yield return new WaitForSeconds(OutroFadeDuration);
			}
			// we trigger an unPause event for the GameManager (and potentially other classes)
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LoadNextScene, null);

			string destinationScene = (string.IsNullOrEmpty(levelName)) ? "StartScreen" : levelName;

			switch (LoadingSceneMode)
			{
				case MMLoadScene.LoadingSceneModes.UnityNative:
					SceneManager.LoadScene(destinationScene);			        
					break;
				case MMLoadScene.LoadingSceneModes.MMSceneLoadingManager:
					MMSceneLoadingManager.LoadScene(destinationScene, LoadingSceneName);
					break;
				case MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager:
					MMAdditiveSceneLoadingManager.LoadScene(levelName, AdditiveLoadingSettings);
					break;
			}
		}

		/// <summary>
		/// Kills the player.
		/// </summary>
		public virtual void PlayerDead(Character playerCharacter)
		{
			if (Players.Count < 2)
			{
				StartCoroutine (PlayerDeadCo ());
			}
		}

		/// <summary>
		/// Triggers the death screen display after a short delay
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator PlayerDeadCo()
		{
			yield return new WaitForSeconds(DelayBeforeDeathScreen);

			GUIManager.Instance.SetDeathScreen(true);
		}

		/// <summary>
		/// Initiates the respawn
		/// </summary>
		protected virtual void Respawn()
		{
			if (Players.Count < 2)
			{
				StartCoroutine(SoloModeRestart());
			}
		}

		/// <summary>
		/// Coroutine that kills the player, stops the camera, resets the points.
		/// </summary>
		/// <returns>The player co.</returns>
		protected virtual IEnumerator SoloModeRestart()
		{
			if ((PlayerPrefabs.Length <= 0) && (SceneCharacters.Count <= 0))
			{
				yield break;
			}

			// if we've setup our game manager to use lives (meaning our max lives is more than zero)
			if (GameManager.Instance.MaximumLives > 0)
			{
				// we lose a life
				GameManager.Instance.LoseLife();
				// if we're out of lives, we check if we have an exit scene, and move there
				if (GameManager.Instance.CurrentLives <= 0)
				{
					TopDownEngineEvent.Trigger(TopDownEngineEventTypes.GameOver, null);
					if ((GameManager.Instance.GameOverScene != null) && (GameManager.Instance.GameOverScene != ""))
					{
						MMSceneLoadingManager.LoadScene(GameManager.Instance.GameOverScene);
					}
				}
			}

			MMCameraEvent.Trigger(MMCameraEventTypes.StopFollowing);

			MMFadeInEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID, true, Players[0].transform.position);
			yield return new WaitForSeconds(OutroFadeDuration);

			yield return new WaitForSeconds(RespawnDelay);
			GUIManager.Instance.SetPauseScreen(false);
			GUIManager.Instance.SetDeathScreen(false);
			MMFadeOutEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID, true, Players[0].transform.position);

			if (CurrentCheckpoint == null)
			{
				CurrentCheckpoint = InitialSpawnPoint;
			}

			if (Players[0] == null)
			{
				InstantiatePlayableCharacters();
			}

			if (CurrentCheckpoint != null)
			{
				CurrentCheckpoint.SpawnPlayer(Players[0]);
			}
			else
			{
				Debug.LogWarning("LevelManager : no checkpoint or initial spawn point has been defined, can't respawn the Player.");
			}

			_started = DateTime.UtcNow;
			
			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);

			// we send a new points event for the GameManager to catch (and other classes that may listen to it too)
			TopDownEnginePointEvent.Trigger(PointsMethods.Set, 0);
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.RespawnComplete, Players[0]);
			yield break;
		}


		/// <summary>
		/// Toggles Character Pause
		/// </summary>
		public virtual void ToggleCharacterPause()
		{
			foreach (Character player in Players)
			{
				CharacterPause characterPause = player.FindAbility<CharacterPause>();
				if (characterPause == null)
				{
					break;
				}

				if (GameManager.Instance.Paused)
				{
					characterPause.PauseCharacter();
				}
				else
				{
					characterPause.UnPauseCharacter();
				}
			}
		}

		/// <summary>
		/// Freezes the character(s)
		/// </summary>
		public virtual void FreezeCharacters()
		{
			foreach (Character player in Players)
			{
				player.Freeze();
			}
		}

		/// <summary>
		/// Unfreezes the character(s)
		/// </summary>
		public virtual void UnFreezeCharacters()
		{
			foreach (Character player in Players)
			{
				player.UnFreeze();
			}
		}

		/// <summary>
		/// Sets the current checkpoint with the one set in parameter. This checkpoint will be saved and used should the player die.
		/// </summary>
		/// <param name="newCheckPoint"></param>
		public virtual void SetCurrentCheckpoint(CheckPoint newCheckPoint)
		{
			if (newCheckPoint.ForceAssignation)
			{
				CurrentCheckpoint = newCheckPoint;
				return;
			}

			if (CurrentCheckpoint == null)
			{
				CurrentCheckpoint = newCheckPoint;
				return;
			}
			if (newCheckPoint.CheckPointOrder >= CurrentCheckpoint.CheckPointOrder)
			{
				CurrentCheckpoint = newCheckPoint;
			}
		}

		/// <summary>
		/// Catches TopDownEngineEvents and acts on them, playing the corresponding sounds
		/// </summary>
		/// <param name="engineEvent">TopDownEngineEvent event.</param>
		public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
				case TopDownEngineEventTypes.PlayerDeath:
					PlayerDead(engineEvent.OriginCharacter);
					break;
				case TopDownEngineEventTypes.RespawnStarted:
					Respawn();
					break;
			}
		}

		/// <summary>
		/// OnDisable, we start listening to events.
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
		}

		/// <summary>
		/// OnDisable, we stop listening to events.
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}
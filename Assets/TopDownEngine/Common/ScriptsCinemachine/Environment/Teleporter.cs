﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{	
	[AddComponentMenu("TopDown Engine/Environment/Teleporter")]
	/// <summary>
	/// Add this script to a trigger collider2D or collider to teleport objects from that object to its destination
	/// </summary>
	public class Teleporter : ButtonActivated 
	{
		/// the possible modes the teleporter can interact with the camera system on activation, either doing nothing, teleporting the camera to a new position, or blending between Cinemachine virtual cameras
		public enum CameraModes { DoNothing, TeleportCamera, CinemachinePriority }
		/// the possible teleportation modes (either 1-frame instant teleportation, or tween between this teleporter and its destination)
		public enum TeleportationModes { Instant, Tween }
		/// the possible time modes 
		public enum TimeModes { Unscaled, Scaled }

		[MMInspectorGroup("Teleporter", true, 18)]

        /// if true, this won't teleport non player characters
        [MMLabel("仅影响玩家")]
        [Tooltip("是否只传送玩家角色（非玩家角色不受影响）")]
        public bool OnlyAffectsPlayer = true;

        /// the offset to apply when exiting this teleporter
        [MMLabel("出口偏移")]
        [Tooltip("传送后应用的偏移量")]
        public Vector3 ExitOffset;


        /// the selected teleportation mode 
        [MMLabel("传送模式")]
        [Tooltip("选择传送实现方式（瞬间完成/缓动过渡）")]
        public TeleportationModes TeleportationMode = TeleportationModes.Instant;

        /// the curve to apply to the teleportation tween 
        [MMLabel("缓动曲线")]
        [MMEnumCondition("TeleportationMode", (int)TeleportationModes.Tween)]
        [Tooltip("传送缓动动画的过渡曲线")]
        public MMTween.MMTweenCurve TweenCurve = MMTween.MMTweenCurve.EaseInCubic;

        /// whether or not to maintain the x value of the teleported object on exit
        [MMLabel("保持X位置")]
        [Tooltip("传送后是否保持物体原始的X坐标")]
        public bool MaintainXEntryPositionOnExit = false;

        [MMLabel("保持Y位置")]
        [Tooltip("传送后是否保持物体原始的Y坐标")]
        public bool MaintainYEntryPositionOnExit = false;

        [MMLabel("保持Z位置")]
        [Tooltip("传送后是否保持物体原始的Z坐标")]
        public bool MaintainZEntryPositionOnExit = false;

        [MMInspectorGroup("Destination", true, 19)]

        [MMLabel("目标传送器")]
        [Tooltip("此传送器的目标位置")]
        public Teleporter Destination;
        /// if this is true, the teleported object will be put on the destination's ignore list, to prevent immediate re-entry. If your 
        /// destination's offset is far enough from its center, you can set that to false
        [MMLabel("添加忽略列表")]
        [Tooltip("防止传送后立即返回（目标偏移足够大时可关闭）")]
        public bool AddToDestinationIgnoreList = true;

        [MMInspectorGroup("Rooms", true, 20)]

        [MMLabel("摄像机模式")]
        [Tooltip("传送时摄像机的处理方式")]
        public CameraModes CameraMode = CameraModes.TeleportCamera;
        /// the room this teleporter belongs to
        [MMLabel("当前房间")]
        [Tooltip("此传送器所属的房间")]
        public Room CurrentRoom;

        /// the target room
        [MMLabel("目标房间")]
        [Tooltip("传送后进入的目标房间")]
        public Room TargetRoom;

        [MMInspectorGroup("MMFader Transtitions", true, 21)]

        [MMLabel("触发渐隐")]
        [Tooltip("传送时是否触发渐隐到黑屏效果")]
        public bool TriggerFade = false;

        [MMLabel("渐隐器ID")]
        [MMCondition("TriggerFade", true)]
        [Tooltip("目标渐隐效果的标识ID")]
        public int FaderID = 0;
        /// the curve to use to fade to black
        [MMLabel("渐隐曲线")]
        [Tooltip("渐隐效果使用的过渡曲线")]
        public MMTweenType FadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
        /// if this is true, fade events will ignore timescale
        [MMLabel("忽略时间缩放")]
        [Tooltip("渐隐效果是否忽略游戏时间缩放")]
        public bool FadeIgnoresTimescale = false;


        [MMInspectorGroup("Mask", true, 22)]

        /// whether or not we should ask to move a MMSpriteMask on activation
        [MMLabel("移动遮罩")]
        [Tooltip("传送时是否移动精灵遮罩")]
        public bool MoveMask = true;
        [MMLabel("遮罩移动曲线")]
        [MMCondition("MoveMask", true)]
        [Tooltip("遮罩移动的过渡曲线")]
        public MMTween.MMTweenCurve MoveMaskCurve = MMTween.MMTweenCurve.EaseInCubic;



        [MMLabel("遮罩移动方式")]
        [MMCondition("MoveMask", true)]
        [Tooltip("遮罩移动的方法类型")]
        public MMSpriteMaskEvent.MMSpriteMaskEventTypes MoveMaskMethod = MMSpriteMaskEvent.MMSpriteMaskEventTypes.ExpandAndMoveToNewPosition;

        /// the duration of the mask movement (usually the same as the DelayBetweenFades
        [MMLabel("遮罩移动时长")]
        [MMCondition("MoveMask", true)]
        [Tooltip("遮罩移动持续时间（通常等于渐隐间隔）")]
        public float MoveMaskDuration = 0.2f;

        [MMInspectorGroup("Freeze", true, 23)]

        [MMLabel("冻结时间")]
        [Tooltip("传送过程中是否冻结游戏时间")]
        public bool FreezeTime = false;

        [MMLabel("冻结角色")]
        [Tooltip("传送过程中是否冻结角色输入")]
        public bool FreezeCharacter = true;

        [MMInspectorGroup("Teleport Sequence", true, 24)]
        /// the timescale to use for the teleport sequence
        [MMLabel("时间模式")]
        [Tooltip("传送序列使用的时间缩放模式")]
        public TimeModes TimeMode = TimeModes.Unscaled;

        [MMLabel("初始延迟")]
        [Tooltip("传送序列开始前的延迟时间（秒）")]
        public float InitialDelay = 0.1f;

        /// the duration (in seconds) after the initial delay covering for the fade out of the scene
        [MMLabel("渐出时长")]
        [Tooltip("场景渐出效果的持续时间（秒）")]
        public float FadeOutDuration = 0.2f;

        [MMLabel("渐隐间隔")]
        [Tooltip("渐出和渐入效果间的等待时间（秒）")]
        public float DelayBetweenFades = 0.3f;

        [MMLabel("渐入时长")]
        [Tooltip("场景渐入效果的持续时间（秒）")]
        public float FadeInDuration = 0.2f;

        /// the duration (in seconds) to apply after the fade in of the scene
        [MMLabel("结束延迟")]
        [Tooltip("渐入效果后的延迟时间（秒）")]
        public float FinalDelay = 0.1f;

        public virtual float LocalTime => (TimeMode == TimeModes.Unscaled) ? Time.unscaledTime : Time.time;
		public virtual float LocalDeltaTime => (TimeMode == TimeModes.Unscaled) ? Time.unscaledDeltaTime : Time.deltaTime;

		protected Character _player;
		protected Character _characterTester;
		protected CharacterGridMovement _characterGridMovement;
		protected List<Transform> _ignoreList;

		protected Vector3 _entryPosition;
		protected Vector3 _newPosition;

		/// <summary>
		/// On start we initialize our ignore list
		/// </summary>
		protected virtual void Awake()
		{
			InitializeTeleporter();
		}

		/// <summary>
		/// Grabs the current room in the parent if needed
		/// </summary>
		protected virtual void InitializeTeleporter()
		{
			_ignoreList = new List<Transform>();
			if (CurrentRoom == null)
			{
				CurrentRoom = this.gameObject.GetComponentInParent<Room>();
			}
		}

		/// <summary>
		/// Triggered when something enters the teleporter
		/// </summary>
		/// <param name="collider">Collider.</param>
		protected override void TriggerEnter(GameObject collider)
		{
			// if the object that collides with the teleporter is on its ignore list, we do nothing and exit.
			if (_ignoreList.Contains(collider.transform))
			{
				return;
			}

			_characterTester = collider.GetComponent<Character>();

			if (_characterTester != null)
			{
				if (RequiresPlayerType)
				{
					if (_characterTester.CharacterType != Character.CharacterTypes.Player)
					{
						return;
					}
				}

				_player = _characterTester;
				_characterGridMovement = _player.GetComponent<CharacterGridMovement>();
			}
            
			// if the teleporter is supposed to only affect the player, we do nothing and exit
			if (OnlyAffectsPlayer || !AutoActivation)
			{
				base.TriggerEnter(collider);
			}
			else
			{
				base.TriggerButtonAction();
				Teleport(collider);
			}
		}

		/// <summary>
		/// If we're button activated and if the button is pressed, we teleport
		/// </summary>
		public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				return;
			}
			base.TriggerButtonAction();
			Teleport(_player.gameObject);
		}

		/// <summary>
		/// Teleports whatever enters the portal to a new destination
		/// </summary>
		protected virtual void Teleport(GameObject collider)
		{
			_entryPosition = collider.transform.position;
			// if the teleporter has a destination, we move the colliding object to that destination
			if (Destination != null)
			{
				StartCoroutine(TeleportSequence(collider));         
			}
		}
        
		/// <summary>
		/// Handles the teleport sequence (fade in, pause, fade out)
		/// </summary>
		/// <param name="collider"></param>
		/// <returns></returns>
		protected virtual IEnumerator TeleportSequence(GameObject collider)
		{
			SequenceStart(collider);

			for (float timer = 0, duration = InitialDelay; timer < duration; timer += LocalDeltaTime) { yield return null; }
            
			AfterInitialDelay(collider);

			for (float timer = 0, duration = FadeOutDuration; timer < duration; timer += LocalDeltaTime) { yield return null; }

			AfterFadeOut(collider);
            
			for (float timer = 0, duration = DelayBetweenFades; timer < duration; timer += LocalDeltaTime) { yield return null; }

			AfterDelayBetweenFades(collider);

			for (float timer = 0, duration = FadeInDuration; timer < duration; timer += LocalDeltaTime) { yield return null; }

			AfterFadeIn(collider);

			for (float timer = 0, duration = FinalDelay; timer < duration; timer += LocalDeltaTime) { yield return null; }

			SequenceEnd(collider);
		}

		/// <summary>
		/// Describes the events happening before the initial fade in
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void SequenceStart(GameObject collider)
		{
			if (CameraMode == CameraModes.TeleportCamera)
			{
				MMCameraEvent.Trigger(MMCameraEventTypes.StopFollowing);
			}

			if (FreezeTime)
			{
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
			}

			if (FreezeCharacter && (_player != null))
			{
				_player.Freeze();
			}
		}

		/// <summary>
		/// Describes the events happening after the initial delay has passed
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void AfterInitialDelay(GameObject collider)
		{            
			if (TriggerFade)
			{
				MMFadeInEvent.Trigger(FadeOutDuration, FadeTween, FaderID, FadeIgnoresTimescale, LevelManager.Instance.Players[0].transform.position);
			}
		}

		/// <summary>
		/// Describes the events happening once the initial fade in is complete
		/// </summary>
		protected virtual void AfterFadeOut(GameObject collider)
		{   
			#if MM_CINEMACHINE || MM_CINEMACHINE3         
			TeleportCollider(collider);

			if (AddToDestinationIgnoreList)
			{
				Destination.AddToIgnoreList(collider.transform);
			}            
            
			if (CameraMode == CameraModes.CinemachinePriority)
			{
				MMCameraEvent.Trigger(MMCameraEventTypes.ResetPriorities);
				MMCinemachineBrainEvent.Trigger(MMCinemachineBrainEventTypes.ChangeBlendDuration, DelayBetweenFades);
			}

			if (CurrentRoom != null)
			{
				CurrentRoom.PlayerExitsRoom();
			}
            
			if (TargetRoom != null)
			{
				TargetRoom.PlayerEntersRoom();
				#if MM_CINEMACHINE || MM_CINEMACHINE3 
				if (TargetRoom.VirtualCamera != null)
				{
					TargetRoom.VirtualCamera.Priority = 10;	
				}
				#endif
				MMSpriteMaskEvent.Trigger(MoveMaskMethod, (Vector2)TargetRoom.RoomColliderCenter, TargetRoom.RoomColliderSize, MoveMaskDuration, MoveMaskCurve);
			}
			#endif
		}

		/// <summary>
		/// Teleports the object going through the teleporter, either instantly or by tween
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void TeleportCollider(GameObject collider)
		{
			_newPosition = Destination.transform.position + Destination.ExitOffset;
			if (MaintainXEntryPositionOnExit)
			{
				_newPosition.x = _entryPosition.x;
			}
			if (MaintainYEntryPositionOnExit)
			{
				_newPosition.y = _entryPosition.y;
			}
			if (MaintainZEntryPositionOnExit)
			{
				_newPosition.z = _entryPosition.z;
			}

			switch (TeleportationMode)
			{
				case TeleportationModes.Instant:
					collider.transform.position = _newPosition;
					_ignoreList.Remove(collider.transform);
					break;
				case TeleportationModes.Tween:
					StartCoroutine(TeleportTweenCo(collider, collider.transform.position, _newPosition));
					break;
			}
		}

		/// <summary>
		/// Tweens the object from origin to destination
		/// </summary>
		/// <param name="collider"></param>
		/// <param name="origin"></param>
		/// <param name="destination"></param>
		/// <returns></returns>
		protected virtual IEnumerator TeleportTweenCo(GameObject collider, Vector3 origin, Vector3 destination)
		{
			float startedAt = LocalTime;
			while (LocalTime - startedAt < DelayBetweenFades)
			{
				float elapsedTime = LocalTime - startedAt;
				collider.transform.position = MMTween.Tween(elapsedTime, 0f, DelayBetweenFades, origin, destination, TweenCurve);
				yield return null;
			}
			_ignoreList.Remove(collider.transform);
		}

		/// <summary>
		/// Describes the events happening after the pause between the fade in and the fade out
		/// </summary>
		protected virtual void AfterDelayBetweenFades(GameObject collider)
		{
			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);

			if (TriggerFade)
			{
				MMFadeOutEvent.Trigger(FadeInDuration, FadeTween, FaderID, FadeIgnoresTimescale, LevelManager.Instance.Players[0].transform.position);
			}
		}

		/// <summary>
		/// Describes the events happening after the fade in of the scene
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void AfterFadeIn(GameObject collider)
		{

		}

		/// <summary>
		/// Describes the events happening after the fade out is complete, so at the end of the teleport sequence
		/// </summary>
		protected virtual void SequenceEnd(GameObject collider)
		{
			if (FreezeCharacter && (_player != null))
			{
				_player.UnFreeze();
			}

			if (_characterGridMovement != null)
			{
				_characterGridMovement.SetCurrentWorldPositionAsNewPosition();
			}

			if (FreezeTime)
			{
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
			}
		}

		/// <summary>
		/// When something exits the teleporter, if it's on the ignore list, we remove it from it, so it'll be considered next time it enters.
		/// </summary>
		/// <param name="collider">Collider.</param>
		public override void TriggerExitAction(GameObject collider)
		{
			if (_ignoreList.Contains(collider.transform))
			{
				_ignoreList.Remove(collider.transform);
			}
			base.TriggerExitAction(collider);
		}

		/// <summary>
		/// Adds an object to the ignore list, which will prevent that object to be moved by the teleporter while it's in that list
		/// </summary>
		/// <param name="objectToIgnore">Object to ignore.</param>
		public virtual void AddToIgnoreList(Transform objectToIgnore)
		{
			if (!_ignoreList.Contains(objectToIgnore))
			{
				_ignoreList.Add(objectToIgnore);
			}            
		}
        
		/// <summary>
		/// On draw gizmos, we draw arrows to the target destination and target room if there are any
		/// </summary>
		protected virtual void OnDrawGizmos()
		{
			if (Destination != null)
			{
				// draws an arrow from this teleporter to its destination
				MMDebug.DrawGizmoArrow(this.transform.position, (Destination.transform.position + Destination.ExitOffset) - this.transform.position, Color.cyan, 1f, 25f);
				// draws a point at the exit position 
				MMDebug.DebugDrawCross(this.transform.position + ExitOffset, 0.5f, Color.yellow);
				MMDebug.DrawPoint(this.transform.position + ExitOffset, Color.yellow, 0.5f);
			}

			if (TargetRoom != null)
			{
				// draws an arrow to the destination room
				MMDebug.DrawGizmoArrow(this.transform.position, TargetRoom.transform.position - this.transform.position, MMColors.Pink, 1f, 25f);
			}
		}
	}
}
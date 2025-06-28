using System;
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
	[RequireComponent(typeof(Weapon))]
	public abstract class WeaponAim : TopDownMonoBehaviour, MMEventListener<TopDownEngineEvent>
	{
		/// the list of possible control modes
		public enum AimControls { Off, PrimaryMovement, SecondaryMovement, Mouse, Script, SecondaryThenPrimaryMovement, PrimaryThenSecondaryMovement, CharacterRotateCameraDirection }
		/// the list of possible rotation modes
		public enum RotationModes { Free, Strict2Directions, Strict4Directions, Strict8Directions }
		/// the possible types of reticles
		public enum ReticleTypes { None, Scene, UI }

        [MMInspectorGroup("Control Mode", true, 5)]
        [MMInformation("添加此组件到武器可实现瞄准（旋转）功能。支持三种控制模式：鼠标（瞄准指针方向）、主移动（瞄准当前移动方向）、副移动（双摇杆射击模式）。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        
		/// the aim control mode
        [MMLabel("瞄准模式")]
        [Tooltip("选择武器瞄准控制方式")]
        public AimControls AimControl = AimControls.SecondaryMovement;
        /// if this is true...
        [MMLabel("启用瞄准控制")]
        [Tooltip("是否启用当前瞄准模式的输入读取")]
        public bool AimControlActive = true;


        [MMInspectorGroup("Weapon Rotation", true, 10)]
        [MMInformation("定义旋转模式：自由旋转、4方向（上下左右）或8方向（含对角线）。可设置旋转速度、最小/最大角度（如限制角色不能向后瞄准可设-90到90度）。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// the rotation mode
        [MMLabel("旋转模式")]
        [Tooltip("选择武器旋转约束方式")]
        public RotationModes RotationMode = RotationModes.Free;
        /// the the speed at which the weapon reaches its new position. Set it to zero if you want movement to directly follow input
        [MMLabel("旋转速度")]
        [Tooltip("武器到达新位置的旋转速度（0表示即时跟随）")]
        public float WeaponRotationSpeed = 1f;
        /// the minimum angle at which the weapon's rotation will be clamped
        [MMLabel("最小角度")]
        [Tooltip("武器旋转的最小限制角度")]
        [Range(-180, 180)]
        public float MinimumAngle = -180f;
        /// the maximum angle at which the weapon's rotation will be clamped
        [MMLabel("最大角度")]
        [Tooltip("武器旋转的最大限制角度")]
        [Range(-180, 180)]
        public float MaximumAngle = 180f;
        /// the minimum threshold at which the weapon's rotation magnitude will be considered 
        [MMLabel("最小幅度阈值")]
        [Tooltip("触发武器旋转的最小输入幅度")]
        public float MinimumMagnitude = 0.2f;


        [MMInspectorGroup("Reticle", true, 11)]
        [MMInformation("可显示屏幕准星：设为0时跟随光标，否则以武器为中心显示。可配置准星旋转、替换鼠标指针等行为。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// Defines whether the reticle...
        [MMLabel("准星类型")]
        [Tooltip("准星显示位置（场景中/UI界面）")]
        public ReticleTypes ReticleType = ReticleTypes.None;
        /// the gameobject to display...
        [MMLabel("准星预制体")]
        [Tooltip("准星/十字线显示对象（空表示不使用准星）")]
        [MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
        public GameObject Reticle;
        /// the distance at which...
        [MMLabel("准星距离")]
        [Tooltip("准星与武器的显示距离（场景模式）")]
        [MMEnumCondition("ReticleType", (int)ReticleTypes.Scene)]
        public float ReticleDistance;
        /// the height at which the reticle should position itself above the ground, when in Scene mode
        [MMLabel("准星高度")]
        [Tooltip("场景模式下准星离地高度")]
        [MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
        public float ReticleHeight;
        /// if set to true, the reticle will be placed at the mouse's position (like a pointer)
        [MMLabel("跟随鼠标位置")]
        [Tooltip("准星是否跟随鼠标位置（类似指针）")]
        [MMEnumCondition("ReticleType", (int)ReticleTypes.Scene)]
        public bool ReticleAtMousePosition;
        /// if set to true...
        [MMLabel("旋转准星")]
        [Tooltip("准星是否随武器旋转（否则保持固定方向）")]
        [MMEnumCondition("ReticleType", (int)ReticleTypes.Scene)]
        public bool RotateReticle = false;
        /// if set to true, the reticle will replace the mouse pointer
        [MMLabel("替换鼠标指针")]
        [Tooltip("是否用准星替换系统鼠标指针")]
        [MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
        public bool ReplaceMousePointer = true;
        /// the radius around...
        [MMLabel("鼠标死区半径")]
        [Tooltip("武器中心忽略鼠标的半径（防止抖动）")]
        [MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
        public float MouseDeadZoneRadius = 0.5f;
        /// if set to false, the reticle won't be added and displayed
        [MMLabel("显示准星")]
        [Tooltip("是否在屏幕上显示准星")]
        [MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
        public bool DisplayReticle = true;

        [MMInspectorGroup("Camera Target", true, 12)]
        /// whether the camera target...
        [MMLabel("移动摄像机目标")]
        [Tooltip("摄像机目标是否向准星移动（无准星时向瞄准方向移动）")]
        public bool MoveCameraTargetTowardsReticle = false;
        /// the offset to apply...
        [MMLabel("摄像机目标偏移")]
        [Tooltip("沿武器/准星连线的偏移比例（0-1）")]
        [Range(0f, 1f)]
        public float CameraTargetOffset = 0.3f;
        /// the maximum distance at which...
        [MMLabel("最大移动距离")]
        [Tooltip("摄像机目标离武器的最大距离")]
        [MMCondition("MoveCameraTargetTowardsReticle", true)]
        public float CameraTargetMaxDistance = 10f;
        /// the speed at which...
        [MMLabel("移动速度")]
        [Tooltip("摄像机目标移动的平滑速度")]
        [MMCondition("MoveCameraTargetTowardsReticle", true)]
        public float CameraTargetSpeed = 5f;

        public virtual float CurrentAngleAbsolute { get; protected set; }
		/// the weapon's current rotation
		public virtual Quaternion CurrentRotation { get { return transform.rotation; } }
		/// the weapon's current direction
		public virtual Vector3 CurrentAim { get { return _currentAim; } }
		/// the weapon's current direction, absolute (flip independent)
		public virtual Vector3 CurrentAimAbsolute { get { return _currentAimAbsolute; } }
		/// the current angle the weapon is aiming at
		public virtual float CurrentAngle { get; protected set; }
		/// the current angle the weapon is aiming at, adjusted to compensate for the current orientation of the character
		public virtual float CurrentAngleRelative
		{
			get
			{
				if (_weapon != null)
				{
					if (_weapon.Owner != null)
					{
						return CurrentAngle;
					}
				}
				return 0;
			}
		}

		public virtual Weapon TargetWeapon => _weapon;
        
		protected Camera _mainCamera;
		protected Vector2 _lastNonNullMovement;
		protected Weapon _weapon;
		protected Vector3 _currentAim = Vector3.zero;
		protected Vector3 _currentAimAbsolute = Vector3.zero;
		protected Quaternion _lookRotation;
		protected Vector3 _direction;
		protected float[] _possibleAngleValues;
		protected Vector3 _mousePosition;
		protected Vector3 _lastMousePosition;
		protected float _additionalAngle;
		protected Quaternion _initialRotation;
		protected Plane _playerPlane;
		protected GameObject _reticle;
		protected Vector3 _reticlePosition;
		protected Vector3 _newCamTargetPosition;
		protected Vector3 _newCamTargetDirection;
		protected bool _initialized = false;
        
		/// <summary>
		/// On Start(), we trigger the initialization
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Grabs the weapon component, initializes the angle values
		/// </summary>
		protected virtual void Initialization()
		{
			_weapon = GetComponent<Weapon>();
			_mainCamera = Camera.main;

			if (RotationMode == RotationModes.Strict4Directions)
			{
				_possibleAngleValues = new float[5];
				_possibleAngleValues[0] = -180f;
				_possibleAngleValues[1] = -90f;
				_possibleAngleValues[2] = 0f;
				_possibleAngleValues[3] = 90f;
				_possibleAngleValues[4] = 180f;
			}
			if (RotationMode == RotationModes.Strict8Directions)
			{
				_possibleAngleValues = new float[9];
				_possibleAngleValues[0] = -180f;
				_possibleAngleValues[1] = -135f;
				_possibleAngleValues[2] = -90f;
				_possibleAngleValues[3] = -45f;
				_possibleAngleValues[4] = 0f;
				_possibleAngleValues[5] = 45f;
				_possibleAngleValues[6] = 90f;
				_possibleAngleValues[7] = 135f;
				_possibleAngleValues[8] = 180f;
			}
			_initialRotation = transform.rotation;
			InitializeReticle();
			_playerPlane = new Plane(Vector3.up, Vector3.zero);
			_initialized = true;
		}
        
		public virtual void ApplyAim()
		{
			Initialization();
			GetCurrentAim();
			DetermineWeaponRotation();
		}

		/// <summary>
		/// Aims the weapon towards a new point
		/// </summary>
		/// <param name="newAim">New aim.</param>
		public virtual void SetCurrentAim(Vector3 newAim, bool setAimAsLastNonNullMovement = false)
		{
			_currentAim = newAim;
		}

		protected virtual void GetCurrentAim()
		{

		}

		/// <summary>
		/// Every frame, we compute the aim direction and rotate the weapon accordingly
		/// </summary>
		protected virtual void Update()
		{

		}

		/// <summary>
		/// On LateUpdate, resets any additional angle
		/// </summary>
		protected virtual void LateUpdate()
		{
			ResetAdditionalAngle();
		}
        
		/// <summary>
		/// Determines the weapon's rotation
		/// </summary>
		protected virtual void DetermineWeaponRotation()
		{

		}

		/// <summary>
		/// Moves the weapon's reticle
		/// </summary>
		protected virtual void MoveReticle()
		{

		}

		/// <summary>
		/// Returns the position of the reticle
		/// </summary>
		/// <returns></returns>
		public virtual Vector3 GetReticlePosition()
		{
			return _reticle.transform.position;
		}

		/// <summary>
		/// Returns the current mouse position
		/// </summary>
		public virtual Vector3 GetMousePosition()
		{
			return _mainCamera.ScreenToWorldPoint(_mousePosition);
		}

		/// <summary>
		/// Rotates the weapon, optionnally applying a lerp to it.
		/// </summary>
		/// <param name="newRotation">New rotation.</param>
		protected virtual void RotateWeapon(Quaternion newRotation, bool forceInstant = false)
		{
			if (GameManager.Instance.Paused)
			{
				return;
			}
			// if the rotation speed is == 0, we have instant rotation
			if ((WeaponRotationSpeed == 0f) || forceInstant)
			{
				transform.rotation = newRotation;
			}
			// otherwise we lerp the rotation
			else
			{
				transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, WeaponRotationSpeed * Time.deltaTime);
			}
		}

		protected Vector3 _aimAtDirection;
		protected Quaternion _aimAtQuaternion;
        
		protected virtual void AimAt(Vector3 target)
		{
		}

		/// <summary>
		/// If a reticle has been set, instantiates the reticle and positions it
		/// </summary>
		protected virtual void InitializeReticle()
		{
           
		}

		/// <summary>
		/// This method defines how the character's camera target should move
		/// </summary>
		protected virtual void MoveTarget()
		{

		}

		/// <summary>
		/// Removes any remaining reticle
		/// </summary>
		public virtual void RemoveReticle()
		{
			if (_reticle != null)
			{
				Destroy(_reticle.gameObject);
			}
		}

		/// <summary>
		/// Hides (or shows) the reticle based on the DisplayReticle setting
		/// </summary>
		protected virtual void HideReticle()
		{
			if (_reticle != null)
			{
				if (GameManager.Instance.Paused)
				{
					_reticle.gameObject.SetActive(false);
					return;
				}
				_reticle.gameObject.SetActive(DisplayReticle);
			}
		}
        
		/// <summary>
		/// Hides or show the mouse pointer based on the settings
		/// </summary>
		protected virtual void HideMousePointer()
		{
			if (AimControl != AimControls.Mouse)
			{
				return;
			}
			if (GameManager.Instance.Paused)
			{
				Cursor.visible = true;
				return;
			}
			if (ReplaceMousePointer)
			{
				Cursor.visible = false;
			}
			else
			{
				Cursor.visible = true;
			}
		}

		/// <summary>
		/// On Destroy, we reinstate our cursor if needed
		/// </summary>
		protected void OnDestroy()
		{
			if (ReplaceMousePointer)
			{
				Cursor.visible = true;
			}
		}


		/// <summary>
		/// Adds additional angle to the weapon's rotation
		/// </summary>
		/// <param name="addedAngle"></param>
		public virtual void AddAdditionalAngle(float addedAngle)
		{
			_additionalAngle += addedAngle;
		}

		/// <summary>
		/// Resets the additional angle
		/// </summary>
		protected virtual void ResetAdditionalAngle()
		{
			_additionalAngle = 0;
		}

		protected virtual void AutoDetectWeaponMode()
		{
			if (_weapon.Owner.LinkedInputManager != null)
			{
				if ((_weapon.Owner.LinkedInputManager.ForceWeaponMode) && (AimControl != AimControls.Off))
				{
					AimControl = _weapon.Owner.LinkedInputManager.WeaponForcedMode;
				}

				if ((!_weapon.Owner.LinkedInputManager.ForceWeaponMode) && (_weapon.Owner.LinkedInputManager.IsMobile) && (AimControl == AimControls.Mouse))
				{
					AimControl = AimControls.PrimaryMovement;
				}
			}
		}

		public void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
				case TopDownEngineEventTypes.LevelStart:
					_initialized = false;
					Initialization();
					break;
			}
		}
        
		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
		}

		/// <summary>
		/// On disable we stop listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}
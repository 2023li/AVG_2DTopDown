﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// This Action will make the Character patrol along the defined path (see the MMPath inspector for that) until it hits a wall or a hole while following a path.
	/// </summary>
	[AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Move Patrol 2D")]
	//[RequireComponent(typeof(MMPath))]
	//[RequireComponent(typeof(Character))]
	//[RequireComponent(typeof(CharacterOrientation2D))]
	//[RequireComponent(typeof(CharacterMovement))]
	public class AIActionMovePatrol2D : AIAction
	{
        [Header("障碍物检测")]

        /// If set to true, the agent will change direction when hitting an obstacle
        [MMLabel("遇到障碍物转向")]
        [Tooltip("若启用，代理在遇到障碍物时将改变方向")]
        public bool ChangeDirectionOnObstacle = true;

        /// the layermask to look for obstacles on
        [MMLabel("障碍物图层掩码")]
        [Tooltip("用于检测障碍物的图层掩码")]
        public LayerMask ObstaclesLayerMask = LayerManager.ObstaclesLayerMask;

        /// the length of the raycast used to detect obstacles
        [MMLabel("障碍物检测射线长度")]
        [Tooltip("用于检测障碍物的射线长度")]
        public float ObstaclesDetectionRaycastLength = 1f;

        /// the frequency (in seconds) at which to check for obstacles
        [MMLabel("每秒障碍物检测频率")]
        [Tooltip("检测障碍物的频率(秒)")]
        public float ObstaclesCheckFrequency = 1f;
		/// the coordinates of the last patrol point
		public virtual Vector3 LastReachedPatrolPoint { get; set; }

		// private stuff
		protected TopDownController _controller;
		protected Character _character;
		protected CharacterOrientation2D _orientation2D;
		protected CharacterMovement _characterMovement;
		protected Health _health;
		protected Vector2 _direction;
		protected Vector2 _startPosition;
		protected Vector3 _initialScale;
		protected MMPath _mmPath;
		protected float _lastObstacleDetectionTimestamp = 0f;
		protected float _lastPatrolPointReachedAt = 0f;

		protected int _currentIndex = 0;
		protected int _indexLastFrame = -1;
		protected float _waitingDelay = 0f;

		/// <summary>
		/// On init we grab all the components we'll need
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
			InitializePatrol();
		}
        
		/// <summary>
		/// On init we grab all the components we'll need
		/// </summary>
		protected virtual void InitializePatrol()
		{
			_controller = this.gameObject.GetComponentInParent<TopDownController>();
			_character = this.gameObject.GetComponentInParent<Character>();
			_orientation2D = _character?.FindAbility<CharacterOrientation2D>();
			_characterMovement = _character?.FindAbility<CharacterMovement>();
			_health = _character?.CharacterHealth;
			_mmPath = this.gameObject.GetComponentInParent<MMPath>();
			// initialize the start position
			_startPosition = transform.position;
			// initialize the direction
			_direction = _orientation2D.IsFacingRight ? Vector2.right : Vector2.left;
			_initialScale = transform.localScale;
			_currentIndex = 0;
			_indexLastFrame = -1;
			_waitingDelay = 0;
			_initialized = true;
			_lastPatrolPointReachedAt = Time.time;
		}


		/// <summary>
		/// On PerformAction we patrol
		/// </summary>
		public override void PerformAction()
		{
			Patrol();
		}

		/// <summary>
		/// This method initiates all the required checks and moves the character
		/// </summary>
		protected virtual void Patrol()
		{
			if (_character == null)
			{
				return;
			}
			if ((_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
			    || (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen))
			{
				return;
			}
			
			if ((_mmPath.CycleOption == MMPath.CycleOptions.OnlyOnce) && _mmPath.EndReached)
			{
				StopMovement();
				return;
			}

			if (Time.time - _lastPatrolPointReachedAt < _waitingDelay)
			{
				StopMovement();
				return;
			}

			// moves the agent in its current direction
			CheckForObstacles();

			_currentIndex = _mmPath.CurrentIndex();
			if (_currentIndex != _indexLastFrame)
			{
				LastReachedPatrolPoint = _mmPath.CurrentPoint();
				_lastPatrolPointReachedAt = Time.time;
				DetermineDelay();
			}

			_direction = _mmPath.CurrentPoint() - this.transform.position;
			_direction = _direction.normalized;

			_characterMovement.SetHorizontalMovement(_direction.x);
			_characterMovement.SetVerticalMovement(_direction.y);

			_indexLastFrame = _currentIndex;
		}
		
		protected virtual void StopMovement()
		{
			_characterMovement.SetHorizontalMovement(0f);
			_characterMovement.SetVerticalMovement(0f);
		}

		protected virtual void DetermineDelay()
		{
			if ( (_mmPath.Direction > 0 && (_currentIndex == 0))
			     || (_mmPath.Direction < 0) && (_currentIndex == _mmPath.PathElements.Count - 1))
			{
				int previousPathIndex = _mmPath.Direction > 0 ? _mmPath.PathElements.Count - 1 : 1;
				_waitingDelay = _mmPath.PathElements[previousPathIndex].Delay;
			}
			else 
			{
				int previousPathIndex = _mmPath.Direction > 0 ? _currentIndex - 1 : _currentIndex + 1;
				_waitingDelay = _mmPath.PathElements[previousPathIndex].Delay; 
			}
		}

		/// <summary>
		/// Draws bounds gizmos
		/// </summary>
		protected virtual void OnDrawGizmosSelected()
		{
			if (_mmPath == null)
			{
				return;
			}
			Gizmos.color = MMColors.IndianRed;
			Gizmos.DrawLine(this.transform.position, _mmPath.CurrentPoint());
		}

		/// <summary>
		/// When exiting the state we reset our movement
		/// </summary>
		public override void OnExitState()
		{
			base.OnExitState();
			_characterMovement?.SetHorizontalMovement(0f);
			_characterMovement?.SetVerticalMovement(0f);
		}

		/// <summary>
		/// Checks for a wall and changes direction if it meets one
		/// </summary>
		protected virtual void CheckForObstacles()
		{
			if (!ChangeDirectionOnObstacle)
			{
				return;
			}

			if (Time.time - _lastObstacleDetectionTimestamp < ObstaclesCheckFrequency)
			{
				return;
			}

			RaycastHit2D raycast = MMDebug.RayCast(_controller.ColliderCenter, _direction, ObstaclesDetectionRaycastLength, ObstaclesLayerMask, MMColors.Gold, true);

			// if the agent is colliding with something, make it turn around
			if (raycast)
			{
				ChangeDirection();
			}

			_lastObstacleDetectionTimestamp = Time.time;
		}
        
		/// <summary>
		/// Changes the current movement direction
		/// </summary>
		public virtual void ChangeDirection()
		{
			_direction = -_direction;
			_mmPath.ChangeDirection();
		}

		/// <summary>
		/// Resets the position of the patrol agent to the start of the path, reinitializes the path
		/// </summary>
		public void ResetPatrol()
		{
			this.transform.position = _startPosition;
			_mmPath.Initialization();
			InitializePatrol();
		}
        
		/// <summary>
		/// When reviving we make sure our directions are properly setup
		/// </summary>
		protected virtual void OnRevive()
		{
			if (!_initialized)
			{
				return;
			}
            
			if (_orientation2D != null)
			{
				_direction = _orientation2D.IsFacingRight ? Vector2.right : Vector2.left;
			}
            
			InitializePatrol();
		}

		/// <summary>
		/// On enable we start listening for OnRevive events
		/// </summary>
		protected virtual void OnEnable()
		{
			if (_health == null)
			{
				_health = (_character != null) ? _character.CharacterHealth : this.gameObject.GetComponent<Health>();
			}

			if (_health != null)
			{
				_health.OnRevive += OnRevive;
			}
		}

		/// <summary>
		/// On disable we stop listening for OnRevive events
		/// </summary>
		protected virtual void OnDisable()
		{
			if (_health != null)
			{
				_health.OnRevive -= OnRevive;
			}
		}
	}
}
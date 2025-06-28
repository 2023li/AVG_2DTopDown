//#define EVENTROUTER_THROWEXCEPTIONS 
#if EVENTROUTER_THROWEXCEPTIONS
//#define EVENTROUTER_REQUIRELISTENER // Uncomment this if you want listeners to be required for sending events.
#endif

using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace MoreMountains.Tools
{
    /// <summary>
    /// MMGameEvents are used throughout the game for general game events (game started, game ended, life lost, etc.)
    /// MM 游戏事件在整个游戏中用于一般游戏事件（游戏开始、游戏结束、生命损失等）。
    /// </summary>
    public struct MMGameEvent
	{
		static MMGameEvent e;
		
		public string EventName;
		public int IntParameter;
		public Vector2 Vector2Parameter;
		public Vector3 Vector3Parameter;
		public bool BoolParameter;
		public string StringParameter;
		
		public static void Trigger(string eventName, int intParameter = 0, Vector2 vector2Parameter = default(Vector2), Vector3 vector3Parameter = default(Vector3), bool boolParameter = false, string stringParameter = "")
		{
			e.EventName = eventName;
			e.IntParameter = intParameter;
			e.Vector2Parameter = vector2Parameter;
			e.Vector3Parameter = vector3Parameter;
			e.BoolParameter = boolParameter;
			e.StringParameter = stringParameter;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// 这个类负责事件管理，可用于在整个游戏中广播事件，告知一个（或多个）类发生了某些事情。
    /// 事件是结构体，你可以定义任何你想要的事件类型。这个管理器自带 MMGameEvents，基本上只是由一个字符串组成，但如果你愿意，也可以使用更复杂的事件。
    ///
    /// 要在任何地方触发一个新事件，执行 YOUR_EVENT.Trigger (YOUR_PARAMETERS)
    /// 例如，MMGameEvent.Trigger ("Save"); 将触发一个名为 Save 的 MMGameEvent 事件
    ///
    /// 你也可以调用 MMEventManager.TriggerEvent (YOUR_EVENT);
    /// 例如：MMEventManager.TriggerEvent (new MMGameEvent ("GameStart")); 将向所有监听器广播一个名为 GameStart 的 MMGameEvent 事件。
    ///
    /// 要从任何类开始监听一个事件，你必须做三件事：
    ///
    /// 1 - 声明你的类为该类型的事件实现了 MMEventListener 接口。
    /// 例如：public class GUIManager : Singleton<GUIManager>, MMEventListener<MMGameEvent>
    /// 你可以有多个这样的声明（每种事件类型一个）。
    ///
    /// 2 - 在启用和禁用时，分别开始和停止监听该事件：
    ///void OnEnable ()
    /// {
    /// this.MMEventStartListening<MMGameEvent>();
    /// }
    /// void OnDisable()
    /// {
    /// this.MMEventStopListening<MMGameEvent>();
    /// }
    ///
    /// 3 - 为该事件实现 MMEventListener 接口。例如：
    ///public void OnMMEvent (MMGameEvent gameEvent)
    /// {
    /// if (gameEvent.EventName == "GameOver")
    /// {
    /// // 执行某些操作
    /// }
    /// }
    /// 这将捕获游戏中任何地方发出的所有 MMGameEvent 类型的事件，如果事件名为 GameOver 则执行某些操作。
    /// </summary>
    [ExecuteAlways]
	public static class MMEventManager 
	{
		private static Dictionary<Type, List<MMEventListenerBase>> _subscribersList;
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		static void InitializeStatics()
		{
			_subscribersList = new Dictionary<Type, List<MMEventListenerBase>>();
		}

		static MMEventManager()
		{
			_subscribersList = new Dictionary<Type, List<MMEventListenerBase>>();
		}

		/// <summary>
		/// Adds a new subscriber to a certain event.
		/// </summary>
		/// <param name="listener">listener.</param>
		/// <typeparam name="MMEvent">The event type.</typeparam>
		public static void AddListener<MMEvent>( MMEventListener<MMEvent> listener ) where MMEvent : struct
		{
			Type eventType = typeof( MMEvent );

			if (!_subscribersList.ContainsKey(eventType))
			{
				_subscribersList[eventType] = new List<MMEventListenerBase>();
			}

			if (!SubscriptionExists(eventType, listener))
			{
				_subscribersList[eventType].Add( listener );
			}
		}

		/// <summary>
		/// Removes a subscriber from a certain event.
		/// </summary>
		/// <param name="listener">listener.</param>
		/// <typeparam name="MMEvent">The event type.</typeparam>
		public static void RemoveListener<MMEvent>( MMEventListener<MMEvent> listener ) where MMEvent : struct
		{
			Type eventType = typeof( MMEvent );

			if( !_subscribersList.ContainsKey( eventType ) )
			{
				#if EVENTROUTER_THROWEXCEPTIONS
					throw new ArgumentException( string.Format( "Removing listener \"{0}\", but the event type \"{1}\" isn't registered.", listener, eventType.ToString() ) );
				#else
				return;
				#endif
			}

			List<MMEventListenerBase> subscriberList = _subscribersList[eventType];

			#if EVENTROUTER_THROWEXCEPTIONS
	            bool listenerFound = false;
			#endif

			for (int i = subscriberList.Count-1; i >= 0; i--)
			{
				if( subscriberList[i] == listener )
				{
					subscriberList.Remove( subscriberList[i] );
					#if EVENTROUTER_THROWEXCEPTIONS
					    listenerFound = true;
					#endif

					if ( subscriberList.Count == 0 )
					{
						_subscribersList.Remove(eventType);
					}						

					return;
				}
			}

			#if EVENTROUTER_THROWEXCEPTIONS
		        if( !listenerFound )
		        {
					throw new ArgumentException( string.Format( "Removing listener, but the supplied receiver isn't subscribed to event type \"{0}\".", eventType.ToString() ) );
		        }
			#endif
		}

		/// <summary>
		/// Triggers an event. All instances that are subscribed to it will receive it (and will potentially act on it).
		/// </summary>
		/// <param name="newEvent">The event to trigger.</param>
		/// <typeparam name="MMEvent">The 1st type parameter.</typeparam>
		public static void TriggerEvent<MMEvent>( MMEvent newEvent ) where MMEvent : struct
		{
			List<MMEventListenerBase> list;
			if( !_subscribersList.TryGetValue( typeof( MMEvent ), out list ) )
				#if EVENTROUTER_REQUIRELISTENER
			            throw new ArgumentException( string.Format( "Attempting to send event of type \"{0}\", but no listener for this type has been found. Make sure this.Subscribe<{0}>(EventRouter) has been called, or that all listeners to this event haven't been unsubscribed.", typeof( MMEvent ).ToString() ) );
				#else
				return;
			#endif
			
			for (int i=list.Count-1; i >= 0; i--)
			{
				( list[i] as MMEventListener<MMEvent> ).OnMMEvent( newEvent );
			}
		}

		/// <summary>
		/// Checks if there are subscribers for a certain type of events
		/// </summary>
		/// <returns><c>true</c>, if exists was subscriptioned, <c>false</c> otherwise.</returns>
		/// <param name="type">Type.</param>
		/// <param name="receiver">Receiver.</param>
		private static bool SubscriptionExists( Type type, MMEventListenerBase receiver )
		{
			List<MMEventListenerBase> receivers;

			if( !_subscribersList.TryGetValue( type, out receivers ) ) return false;

			bool exists = false;

			for (int i = receivers.Count-1; i >= 0; i--)
			{
				if( receivers[i] == receiver )
				{
					exists = true;
					break;
				}
			}

			return exists;
		}
	}

	/// <summary>
	/// Static class that allows any class to start or stop listening to events
	/// 静态扩展类运行任何类去监听一个事件
	/// </summary>
	public static class EventRegister
	{
		public delegate void Delegate<T>( T eventType );

		public static void MMEventStartListening<EventType>( this MMEventListener<EventType> caller ) where EventType : struct
		{
			MMEventManager.AddListener<EventType>( caller );
		}

		public static void MMEventStopListening<EventType>( this MMEventListener<EventType> caller ) where EventType : struct
		{
			MMEventManager.RemoveListener<EventType>( caller );
		}
	}

	/// <summary>
	/// Event listener basic interface
	/// </summary>
	public interface MMEventListenerBase { };

	/// <summary>
	/// A public interface you'll need to implement for each type of event you want to listen to.
	/// </summary>
	public interface MMEventListener<T> : MMEventListenerBase
	{
		void OnMMEvent( T eventType );
	}

	public class MMEventListenerWrapper<TOwner, TTarget, TEvent> : MMEventListener<TEvent>, IDisposable
		where TEvent : struct
	{
		private Action<TTarget> _callback;

		private TOwner _owner;
		public MMEventListenerWrapper(TOwner owner, Action<TTarget> callback)
		{
			_owner = owner;
			_callback = callback;
			RegisterCallbacks(true);
		}

		public void Dispose()
		{
			RegisterCallbacks(false);
			_callback = null;
		}

		protected virtual TTarget OnEvent(TEvent eventType) => default;
		public void OnMMEvent(TEvent eventType)
		{
			var item = OnEvent(eventType);
			_callback?.Invoke(item);
		}

		private void RegisterCallbacks(bool b)
		{
			if (b)
			{
				this.MMEventStartListening<TEvent>();
			}
			else
			{
				this.MMEventStopListening<TEvent>();
			}
		}
	}
}
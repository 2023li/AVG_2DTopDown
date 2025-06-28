using UnityEngine;
using System.Collections;
using System;

namespace MoreMountains.Tools
{
	[Serializable]
	/// <summary>
	/// A serializable class used to store an achievement into a save file
	/// </summary>
	public class SerializedMMAchievement
	{
		/// <summary>
		/// 成就ID
		/// </summary>
		public string AchievementID;
		/// <summary>
		/// 锁定状态
		/// </summary>
		public bool UnlockedStatus;
		/// <summary>
		/// 当前进度
		/// </summary>
		public int ProgressCurrent;

		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.Tools.SerializedMMAchievement"/> class.
		/// </summary>
		/// <param name="achievementID">Achievement I.</param>
		/// <param name="unlockedStatus">If set to <c>true</c> unlocked status.</param>
		/// <param name="progressCurrent">Progress current.</param>
		public SerializedMMAchievement(string achievementID, bool unlockedStatus, int progressCurrent)
		{
			AchievementID = achievementID;
			UnlockedStatus = unlockedStatus;
			ProgressCurrent = progressCurrent;
		}
	}

	[Serializable]
	/// <summary>
	/// Serializable MM achievement manager.
	/// </summary>
	public class SerializedMMAchievementManager 
	{
		public SerializedMMAchievement[] Achievements;
	}
}
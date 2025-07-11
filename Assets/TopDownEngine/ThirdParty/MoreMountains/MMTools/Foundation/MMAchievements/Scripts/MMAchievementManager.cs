﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MoreMountains.Tools
{
	[ExecuteAlways]
    /// <summary>
    /// This static class is in charge of storing the current state of the achievements, unlocking/locking them, and saving them to data files
    /// 这个静态类负责存储成就的当前状态，解锁 / 锁定成就，并将其保存到数据文件中。
    /// </summary>
    public static class MMAchievementManager
	{
		public static List<MMAchievement> AchievementsList { get { return _achievements; }}

		private static List<MMAchievement> _achievements;
		private static MMAchievement _achievement = null;

		public static string _defaultFileName = "Achievements";
		public static string _saveFolderName = "MMAchievements/";
		public static string _saveFileExtension = ".achievements";

		public static string SaveFileName;
		public static string ListID;

		/// <summary>
		/// You'll need to call this method to initialize the manager
		/// </summary>
		public static void LoadAchievementList(MMAchievementList achievementList)
		{
			_achievements = new List<MMAchievement> ();

			if (achievementList == null)
			{
				return;
			}

			// we store the ID for save purposes
			ListID = achievementList.AchievementsListID;

			foreach (MMAchievement achievement in achievementList.Achievements)
			{
				_achievements.Add (achievement.Copy());
			}
		}

		/// <summary>
		/// Unlocks the specified achievement (if found).
		/// </summary>
		/// <param name="achievementID">Achievement I.</param>
		public static void UnlockAchievement(string achievementID)
		{
			_achievement = AchievementManagerContains(achievementID);
			if (_achievement != null)
			{
				_achievement.UnlockAchievement();
			}
		}

		/// <summary>
		/// Locks the specified achievement (if found).
		/// </summary>
		/// <param name="achievementID">Achievement ID.</param>
		public static void LockAchievement(string achievementID)
		{
			_achievement = AchievementManagerContains(achievementID);
			if (_achievement != null)
			{
				_achievement.LockAchievement();
			}
		}

		/// <summary>
		/// Adds progress to the specified achievement (if found).
		/// </summary>
		/// <param name="achievementID">Achievement ID.</param>
		/// <param name="newProgress">New progress.</param>
		public static void AddProgress(string achievementID, int newProgress)
		{
			_achievement = AchievementManagerContains(achievementID);
			if (_achievement != null)
			{
				_achievement.AddProgress(newProgress);
			}
		}

		/// <summary>
		/// Sets the progress of the specified achievement (if found) to the specified progress.
		/// </summary>
		/// <param name="achievementID">Achievement ID.</param>
		/// <param name="newProgress">New progress.</param>
		public static void SetProgress(string achievementID, int newProgress)
		{
			_achievement = AchievementManagerContains(achievementID);
			if (_achievement != null)
			{
				_achievement.SetProgress(newProgress);
			}
		}		

		/// <summary>
		/// Determines if the achievement manager contains an achievement of the specified ID. Returns it if found, otherwise returns null
		/// </summary>
		/// <returns>The achievement corresponding to the searched ID if found, otherwise null.</returns>
		/// <param name="searchedID">Searched I.</param>
		private static MMAchievement AchievementManagerContains(string searchedID)
		{
			if (_achievements.Count == 0)
			{
				return null;
			}
			foreach(MMAchievement achievement in _achievements)
			{
				if (achievement.AchievementID == searchedID)
				{
					return achievement;					
				}
			}
			return null;
		}

        // SAVE ------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Removes saved data and resets all achievements from a list
        /// 从列表中删除已保存的数据并重置所有成就
        /// </summary>
        /// <param name="listID">The ID of the achievement list to reset.</param>
        public static void ResetAchievements(string listID)
		{
			if (_achievements != null)
			{
				foreach(MMAchievement achievement in _achievements)
				{
					achievement.ProgressCurrent = 0;
					achievement.UnlockedStatus = false;
				}	
			}

			DeterminePath (listID);
			MMSaveLoadManager.DeleteSave(SaveFileName + _saveFileExtension, _saveFolderName);
			Debug.LogFormat ("Achievements Reset");
		}

		public static void ResetAllAchievements()
		{
			ResetAchievements (ListID);
		}

        /// <summary>
        /// Loads the saved achievements file and updates the array with its content.
        /// 加载已保存的成就文件，并使用其内容更新数组。
        /// </summary>
        public static void LoadSavedAchievements()
		{
			DeterminePath ();
			SerializedMMAchievementManager serializedMMAchievementManager = (SerializedMMAchievementManager)MMSaveLoadManager.Load(typeof(SerializedMMAchievementManager), SaveFileName+ _saveFileExtension, _saveFolderName);
            //提取 SerializedMMAchievementManager
            ExtractSerializedMMAchievementManager(serializedMMAchievementManager);
		}

        /// <summary>
        /// Saves the achievements current status to a file on disk
        /// 将成就的当前状态保存到磁盘上的文件中
        /// </summary>
        public static void SaveAchievements()
		{
			//确认路径
			DeterminePath();
			SerializedMMAchievementManager serializedMMAchievementManager = new SerializedMMAchievementManager();
			FillSerializedMMAchievementManager(serializedMMAchievementManager);
			MMSaveLoadManager.Save(serializedMMAchievementManager, SaveFileName+_saveFileExtension, _saveFolderName);
		}

        /// <summary>
        /// Determines the path the achievements save file should be saved to.
        /// 确定成就保存文件应保存到的路径。
        /// </summary>
        private static void DeterminePath(string specifiedFileName = "")
		{
			string tempFileName = (!string.IsNullOrEmpty(ListID)) ? ListID : _defaultFileName;
			if (!string.IsNullOrEmpty(specifiedFileName))
			{
				tempFileName = specifiedFileName;
			}

			SaveFileName = tempFileName;
		}

		/// <summary>
		/// Serializes the contents of the achievements array to a serialized, ready to save object
		/// </summary>
		/// <param name="serializedInventory">Serialized inventory.</param>
		public static void FillSerializedMMAchievementManager(SerializedMMAchievementManager serializedAchievements)
		{
			serializedAchievements.Achievements = new SerializedMMAchievement[_achievements.Count];

			for (int i = 0; i < _achievements.Count(); i++)
			{
				SerializedMMAchievement newAchievement = new SerializedMMAchievement (_achievements[i].AchievementID, _achievements[i].UnlockedStatus, _achievements[i].ProgressCurrent);
				serializedAchievements.Achievements [i] = newAchievement;
			}
		}

		/// <summary>
		/// Extracts the serialized achievements into our achievements array if the achievements ID match.
		/// </summary>
		/// <param name="serializedAchievements">Serialized achievements.</param>
		public static void ExtractSerializedMMAchievementManager(SerializedMMAchievementManager serializedAchievements)
		{
			if (serializedAchievements == null)
			{
				return;
			}

			for (int i = 0; i < _achievements.Count(); i++)
			{
				for (int j=0; j<serializedAchievements.Achievements.Length; j++)
				{
					if (_achievements[i].AchievementID == serializedAchievements.Achievements[j].AchievementID)
					{
						_achievements [i].UnlockedStatus = serializedAchievements.Achievements [j].UnlockedStatus;
						_achievements [i].ProgressCurrent = serializedAchievements.Achievements [j].ProgressCurrent;
					}
				}
			}
		}
	}
}
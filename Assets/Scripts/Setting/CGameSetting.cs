using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGameSetting {

	#region CONFIGS

	public static bool GAME_BUSY = false;

	public static int CARD_PER_COLUMN = 8;

	public static Vector2 CARD_SIZE = new Vector2(196f, 260f);
	
	public static int[] CARD_VALUES = new int[] {
			2, 
			4, 
			8, 
			16,
			32,
			64,
			128,
			256,
			512,
			1024,
			2048
		};

	public static Color[] CARD_COLORS = new Color[] {
		new Color32(255, 255, 51,  255),  // 2
		new Color32(251, 153, 0,   255),  // 4
		new Color32(255, 38,  16,  255),  // 8
 		new Color32(134, 2,   175, 255),  // 16
		new Color32(3,   71,  255, 255),  // 32
 		new Color32(101, 175, 51,  255),  // 64
		new Color32(251, 189, 0,   255),  // 128
		new Color32(255, 82,  8,   255),  // 512
		new Color32(167, 26,  76,  255),  // 1024
		new Color32(62,  0,   164, 255),  // 2048
		new Color32(4,   147, 207, 255), 
		new Color32(208, 233, 43,  255)  
	};
	 
	#endregion

	#region LEVEL

	private static string LEVEL_SAVE = "LEVEL_SAVE";
	public static int MAX_GAME_LEVEL = 1;
	public static int GAME_LEVEL
	{
		get 
		{ 
			var level = PlayerPrefs.GetInt(LEVEL_SAVE, 1) % MAX_GAME_LEVEL + 1; 
			return level;
		}
		set 
		{  
			PlayerPrefs.SetInt(LEVEL_SAVE, value);
			PlayerPrefs.Save();
		}
	}

	#endregion

	#region SCORE

	public static float UI_SCORE_TIMER = 1f;
	public static string UI_SCORE = "{0}";

	private static string SCORE_SAVE = "SCORE_SAVE";
	public static int SCORE 
	{
		get 
		{ 
			return PlayerPrefs.GetInt(SCORE_SAVE, 0);
		}
		set 
		{  
			PlayerPrefs.SetInt(SCORE_SAVE, value);
			PlayerPrefs.Save();
		}
	}

	public static string FormatNumber(int num) {
		if (num >= 100000000)
			return (num / 1000000).ToString("#,0M");

		if (num >= 10000000)
			return (num / 1000000).ToString("0.#") + "M";

		if (num >= 100000)
			return (num / 1000).ToString("#,0K");

		if (num >= 10000)
			return (num / 1000).ToString("0.#") + "K";

		return num.ToString("#,0");
	}

	#endregion

	#region SETTING

	public static string SOUND_MUTE = "SOUND_MUTE";

	public static bool SETTING_SOUND_MUTE
	{
		get
		{
			return PlayerPrefs.GetInt(SOUND_MUTE, 1) == 1;
		}
		set
		{
			PlayerPrefs.SetInt(SOUND_MUTE, value ? 1 : 0);
			PlayerPrefs.Save();
		}
	}

	#endregion

	#region GAME

	public static string SAVE_LOAD_NAME = "SAVE_GAME_0001";

	public static bool HasSaveGame()
	{
		return PlayerPrefs.HasKey(SAVE_LOAD_NAME);
	}

	public static void SaveGame(string listToStr)
	{
		// SAVE PREFABS
		PlayerPrefs.SetString(SAVE_LOAD_NAME, listToStr);
		PlayerPrefs.Save();
	}

	public static string LoadGame()
	{
		// LOAD PREFABS
		return PlayerPrefs.GetString(CGameSetting.SAVE_LOAD_NAME);
	}

	public static void DeleteSave()
	{
		PlayerPrefs.DeleteKey(SAVE_LOAD_NAME);
		PlayerPrefs.Save();
		// SCORE
		SCORE = 0;
		// LEVEL
		GAME_LEVEL = 1;
	}

	#endregion

	#region ULTILITIES

	public static Vector2 ConvertScreenToLocal(RectTransform parent, Vector3 position)
	{
		var localPosition = Vector2.zero;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, 
            position, 
            Camera.main, 
            out localPosition);
		return localPosition;
	}

    #endregion

	#region TIMER TO ADS

	public static long DELAY_TO_AD = 60; 
    public static string TIMER_TO_AD = "TIMER_TO_AD";

	public static void InitTimerToAd()
    {
		if (PlayerPrefs.HasKey (TIMER_TO_AD) == false)
		{
			ResetTimerToAd();
		}
    }

    public static bool IsTimerToAd(long delay)
    {
        var timeStr = PlayerPrefs.GetString(TIMER_TO_AD, DateTime.Now.Ticks.ToString());
        var ticks = DateTime.Now.Ticks - long.Parse(timeStr);
        var elapsedSpan = new TimeSpan(ticks);
        return elapsedSpan.TotalSeconds >= delay;
    }

    public static void ResetTimerToAd()
    {
        PlayerPrefs.SetString(TIMER_TO_AD, DateTime.Now.Ticks.ToString());
        PlayerPrefs.Save();
    }

    #endregion
	
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGameSetting {

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
		new Color32(255, 255, 51,  255), 
		new Color32(251, 153, 0,   255),
		new Color32(255, 38,  16,  255), 
		new Color32(134, 2,   175, 255), 
		new Color32(3,   71,  255, 255), 
		new Color32(101, 175, 51,  255), 
		new Color32(251, 189, 0,   255), 
		new Color32(255, 82,  8,   255), 
		new Color32(167, 26,  76,  255), 
		new Color32(62,  0,   164, 255), 
		new Color32(4,   147, 207, 255), 
		new Color32(208, 233, 43,  255)  
	};

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

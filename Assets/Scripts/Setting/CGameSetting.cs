using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGameSetting {
	
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
		new Color32(251, 189, 0,   255), 
		new Color32(251, 153, 0,   255),
		new Color32(255, 82,  8,   255), 
		new Color32(255, 38,  16,  255), 
		new Color32(167, 26,  76,  255), 
		new Color32(134, 2,   175, 255), 
		new Color32(62,  0,   164, 255), 
		new Color32(3,   71,  255, 255), 
		new Color32(4,   147, 207, 255), 
		new Color32(101, 175, 51,  255), 
		new Color32(208, 233, 43,  255)
	};
	
}

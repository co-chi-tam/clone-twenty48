using System;
using UnityEngine;

[Serializable]
public class CBoardData {

	public int saveIndex = -1;
	public int score = 0;
	public string[,] columns; // "0_0"
	public string[] onHands; // "0_0"
	public int removeSize = 0;

	public CBoardData()
	{
		this.saveIndex = -1;
		this.score = 0;
		this.columns = new string[4, 8];
		this.onHands = new string[2];
		this.removeSize = 0;
	}
	
}

using System;
using UnityEngine;

[Serializable]
public class CBoardData {

	public int saveIndex = -1;
	public int[,] columns;
	public int[] onHands;
	public int removeSize = 0;

	public CBoardData()
	{
		this.saveIndex = -1;
		this.columns = new int[4, 8];
		this.onHands = new int[2];
		this.removeSize = 0;
	}
	
}

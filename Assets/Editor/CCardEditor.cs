using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CCard))]
public class CCardEditor: Editor {

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		var card = target as CCard;
		if (GUILayout.Button("Explosion"))
		{
			card.Explosion();
		}
	}

}

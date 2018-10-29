using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CStartScene : MonoBehaviour {

	protected Button m_PlayButton;
	protected Button m_RefeshButton;

	protected CBoard m_Board;

	protected virtual void Start()
	{
		this.Init();
	}

	public virtual void Init()
	{
		// BOARD
		this.m_Board = GameObject.FindObjectOfType<CBoard>();
		// PLAY
		this.m_PlayButton = this.transform.Find("GroupButtons/PlayButton").GetComponent<Button>();
		this.m_PlayButton.onClick.RemoveAllListeners();
		this.m_PlayButton.onClick.AddListener(() => {
			// PlayerPrefs.DeleteAll();
			this.gameObject.SetActive(false);
			this.m_Board.Init();
		});
		// REFRESH
		this.m_RefeshButton = this.transform.Find("GroupButtons/RefreshButton").GetComponent<Button>();
		this.m_RefeshButton.onClick.RemoveAllListeners();
		this.m_RefeshButton.onClick.AddListener(() => {
			this.gameObject.SetActive(false);
			if (this.m_Board != null)
			{
				// this.m_Board.ResetBoard();
				CGameSetting.DeleteSave();
				this.m_Board.Init();
			}
		});
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TinyJSON;

public class CBoard : MonoBehaviour  {

	#region Fields

	[Header("Configs")]
	[SerializeField]	protected bool m_IsInited = false;
	public bool isInited 
	{ 
		get { return this.m_IsInited; }
		set { this.m_IsInited = value; }
	}
	[SerializeField]	protected bool m_CanUndo = true;

	[Header("Menu")]
	[SerializeField]	protected Text m_ScoreText;
	protected int m_CurrentScore = 0;
	public int currentScore 
	{ 
		get { return this.m_CurrentScore; }
		set 
		{ 
			this.m_CurrentScore = value; 
			this.m_ScoreText.text = string.Format(CGameSetting.UI_SCORE, CGameSetting.FormatNumber (value));
		}
	}

	[Header("Columns")]
    [SerializeField]	protected CColumn[] m_Columns;

	[Header("OnHand")]
	[SerializeField]	protected COnHand m_OnHand;

	[Header("Buttons")]
	[SerializeField]	protected CRemoveCard m_RemoveCard;
	// [SerializeField]	protected Button m_RedrawButton;
	[SerializeField]	protected Button m_UndoButton;
	[SerializeField]	protected Button m_ExitButton;


	[Header("Save Load")]
	[SerializeField]	protected CBoardData m_Data = new CBoardData();
	public CBoardData data 
	{ 
		get { return this.m_Data; }
		set { this.m_Data = value; }
	}
	[SerializeField]	protected List<string> m_SaveList = new List<string>();
	public List<string> saveList 
	{ 
		get { return this.m_SaveList; }
		set { this.m_SaveList = value; }
	}

	protected bool m_IsExplosing = false;
	protected CAdsSimple m_AdsSimple;

	#endregion

	#region Implementation Monobehaviour

    protected virtual void Start()
	{
		this.Init();
	}

	#endregion

	#region Main methods

	public virtual void Init()
	{
		// MENU
		this.m_ScoreText = this.transform.Find("MenuPanel/ScoreText").GetComponent<Text>();
		this.currentScore = CGameSetting.SCORE;
		// COLUMNS
        this.m_Columns = this.GetComponentsInChildren<CColumn>();
		for (int i = 0; i < this.m_Columns.Length; i++)
		{
			this.m_Columns[i].Init();
		}
		// ONHAND
		this.m_OnHand = this.GetComponentInChildren<COnHand>();
		this.m_OnHand.Init();
		// REMOVE CARD
		this.m_RemoveCard = this.GetComponentInChildren<CRemoveCard>();
		this.m_RemoveCard.Init();
		// UTILITIES
		// this.m_RedrawButton = this.transform.Find("OnHandPanel/UtilityPanel/RedrawButton").GetComponent<Button>();
		this.m_UndoButton = this.transform.Find("OnHandPanel/UtilityPanel/UndoButton").GetComponent<Button>();
		this.m_UndoButton.onClick.RemoveAllListeners();
		this.m_UndoButton.onClick.AddListener(() => {
			this.UndoBoardWithAbs ();
		});
		this.m_ExitButton = this.transform.Find("MenuPanel/ExitButton").GetComponent<Button>();
		this.m_ExitButton.onClick.RemoveAllListeners();
		this.m_ExitButton.onClick.AddListener(() => {
			this.ResetBoard ();
		});
		// EXPLOSION
		this.m_IsExplosing = false;
		// INDEX
		this.m_Data.saveIndex = -1;
		// UNDO
		this.m_CanUndo = true;
		// LOAD LAST SAVE
		// this.DeleteSave();
		if (CGameSetting.HasSaveGame())
		{
			this.m_IsInited = false;
			this.LoadBoard();
		}
		else
		{
			this.m_IsInited = true;
			this.SaveBoard();
		}
		// ADS
		this.m_AdsSimple = GameObject.FindObjectOfType<CAdsSimple>();
    }

	public virtual void Clear()
	{
		// COLUMNS
		for (int i = 0; i < this.m_Columns.Length; i++)
		{
			this.m_Columns[i].Clear();
		}
		// ONHAND
		this.m_OnHand.Clear();
		// REMOVE CARD
		this.m_RemoveCard.Clear();
		// EXPLOSION
		this.m_IsExplosing = false;
		// UNDO
		this.m_CanUndo = true;
	}

	public virtual void ResetBoard()
	{
		CGameSetting.DeleteSave();
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	public virtual void CheckHand()
	{
		if (this.m_IsInited == false)
			return;
		var carAvailable = this.m_OnHand.GetAvailableCard();
		var result = false;
		for (int i = 0; i < this.m_Columns.Length; i++)
		{
			var column = this.m_Columns[i];
			result |= column.IsAvailable(carAvailable);
		}
		if (result == false)
		{
			Debug.Log ("CHECK HAND " + result);
			CGameSetting.DeleteSave();
			this.ExplosionAllCards (() => {
				Debug.Log ("COMPLETE MAP");
				this.ResetBoard ();
			});
		}
	}

	public virtual void ExplosionAllCards(System.Action callback)
	{
		if (this.m_IsExplosing)
			return;
		this.m_IsExplosing = true;
		StartCoroutine (this.HandleExplosionAllCards(callback));
	}

	protected WaitForSeconds m_WaitSortTime = new WaitForSeconds(0.1f);
	protected IEnumerator HandleExplosionAllCards(System.Action callback)
	{
		for (int i = 0; i < this.m_Columns.Length; i++)
		{
			var cards = this.m_Columns[i].cards;
			for (int x = 0; x < cards.Count; x++)
			{
				yield return this.m_WaitSortTime;
				cards[x].Explosion();
			}
		}
		// EXPLOSION
		this.m_IsExplosing = false;
		// EVENTS
		if(callback != null)
		{
			callback();
		} 
	}

	public virtual void AddScore(int score, int multi)
	{
		// Debug.Log (score + " * " + multi + " = " + (score * multi));
		// UI
		this.currentScore = CGameSetting.SCORE;
		// ADD SCORE
		var totalScore = score * multi;
		CGameSetting.SCORE += totalScore;
		// HANDLE
		StopCoroutine (this.HandleAddScore());
		StartCoroutine (this.HandleAddScore());
	}

	protected WaitForFixedUpdate m_WaitFixedUpdate = new WaitForFixedUpdate();
	protected virtual IEnumerator HandleAddScore()
	{
		var currentTime = 0f;
		for (int i = this.currentScore; i < CGameSetting.SCORE; i++)
		{
			yield return this.m_WaitFixedUpdate;
			currentTime += Time.fixedDeltaTime;
			this.currentScore += 1;
			if (currentTime >= CGameSetting.UI_SCORE_TIMER)
				break;
		}
		this.currentScore = CGameSetting.SCORE;
	}

	#endregion

	#region Save && Load

	public virtual void UndoBoard()
	{
		if (this.m_SaveList.Count < 1)
			return;
		if (this.m_CanUndo == false)
			return;
		this.m_CanUndo = false;
		// REMOVE LAST SAVE => CURRENT SAVE
		if (this.m_SaveList.Count > 1)
		{
			this.m_SaveList.RemoveAt (this.m_SaveList.Count - 1);
		}
		// SAVE
		this.Save (this.m_SaveList);
		// GET NEXT SAVE
		var lastSave = this.m_SaveList[this.m_SaveList.Count - 1];
		// LOAD
		this.Load (lastSave);
		// CHECK
		var countColumn = 0;
		for (int i = 0; i < this.m_Columns.Length; i++)
		{
			this.m_Columns[i].CheckCombineCards(() => {
				countColumn++;
				if (countColumn >= this.m_Columns.Length)
				{
					this.m_CanUndo = true;
				}
			}, false);
		}
	}

	public virtual void UndoBoardWithAbs()
	{
		if (this.m_AdsSimple == null)
			return;
		if (this.m_AdsSimple.canShowAds == false)
			return;
		this.m_AdsSimple.OnFinish.RemoveAllListeners();
		this.m_AdsSimple.OnFinish.AddListener(() => {
			this.UndoBoard();
		});
		this.m_AdsSimple.Show();	
	}

	public virtual void LoadBoard()
	{
		var loadGameStr = CGameSetting.LoadGame();
		this.m_SaveList = JSON.Load(loadGameStr).Make<List<string>>();
		if (this.m_SaveList.Count > 0)
		{
			// GET NEXT SAVE
			var lastSave = this.m_SaveList[this.m_SaveList.Count - 1];
			// LOAD
			this.Load (lastSave);
			// CHECK
			this.m_CanUndo = false;
			var countColumn = 0;
			for (int i = 0; i < this.m_Columns.Length; i++)
			{
				this.m_Columns[i].CheckCombineCardsNonAnim(() => {
					countColumn++;
					if (countColumn >= this.m_Columns.Length)
					{
						this.m_CanUndo = true;
					}
				}, false);
			}
		}
		// INIT
		this.m_IsInited = true;
	}

	public virtual void SaveBoard()
	{
		if (this.m_IsInited == false)
			return;
		// INDEX
		this.m_Data.saveIndex += 1;
		// SCORE
		this.m_Data.score = CGameSetting.SCORE;
		// COLUMNS
		var maxColumns = Mathf.Min(this.m_Columns.Length, this.m_Data.columns.GetLength(0));
		for (int i = 0; i < this.m_Columns.Length; i++)
		{
			var cards = this.m_Columns[i].cards;
			for (int x = 0; x < this.m_Data.columns.GetLength(1); x++)
			{
				if (x < cards.Count)
					this.m_Data.columns[i, x] = cards[x].value;
				else
					this.m_Data.columns[i, x] = 0;
			}
		}
		// ON HAND
		var onHandCards = this.m_OnHand.onHandCard;
		for (int i = 0; i < onHandCards.Count; i++)
		{
			this.m_Data.onHands[i] = onHandCards[i].value;
		}
		// SAVE DATA
		var saveToStr = JSON.Dump(this.m_Data);
		// SAVE TO LIST
		this.m_SaveList.Add (saveToStr);
		this.Save (this.m_SaveList);
	}

	public virtual void Save(List<string> listStr)
	{
		var listToStr = JSON.Dump(listStr);
		// SAVE PREFABS
		CGameSetting.SaveGame (listToStr);
	}

	public virtual void Load(string json)
	{
		// DATA
		this.m_Data = JSON.Load(json).Make<CBoardData>();
		// UI
		CGameSetting.SCORE = this.m_Data.score;
		this.currentScore = this.m_Data.score;
		// Columns
		var columnData = this.m_Data.columns;
		for (int i = 0; i < this.m_Columns.Length; i++)
		{
			var column = this.m_Columns[i];
			column.Clear();
			for (int x = 0; x < columnData.GetLength(1); x++)
			{
				var cardValue = columnData[i, x];
				if (cardValue > 1)
				{
					var card = this.m_OnHand.DrawCard (cardValue, CCard.ECardType.NUMBER);
					column.AddCardImmediate (card);
				}
			}
		}
		// On hand
		var onHandDatas = this.m_Data.onHands;
		this.m_OnHand.Clear();
		for (int i = 0; i < onHandDatas.Length; i++)
		{
			var cardValue = onHandDatas[i];
			if (cardValue > 1)
			{
				var card = this.m_OnHand.DrawCard (cardValue, CCard.ECardType.NUMBER);
				card.Clear();
				this.m_OnHand.OnHandACardImmediate(card);
			}
		}
		// FIRST ACTIVE CARD
		this.m_OnHand.ActiveFirstCard();
		// Remove cards
		var removeSize = this.m_Data.removeSize;
		this.m_RemoveCard.SetSize (removeSize);
	}

	#endregion
	
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CColumn : MonoBehaviour, 
	IDropHandler,
	IPointerEnterHandler, IPointerExitHandler {

	#region Implementation Fields

	[Header("Configs")]
	[SerializeField]	protected float m_HeighOffset = 50f;
	public float heighOffset 
	{ 
		get { return this.m_HeighOffset; }	
	}

	[SerializeField]	protected List<CCard> m_Cards;
	public List<CCard> cards 
	{ 
		get { return this.m_Cards; }
		set { this.m_Cards = value; }
	}
	protected CGroupCard m_Group;
	protected CBoard m_Board;
	protected VerticalLayoutGroup m_LayoutGroup;
	protected Transform m_FirstCard;
	protected Transform m_FirstCardPoint;
	protected bool m_OnAnimatingCard = false;
	public bool onAnimatingCard 
	{ 
		get { return this.m_OnAnimatingCard; }
		set { this.m_OnAnimatingCard = value; }
	}
	protected WaitForFixedUpdate m_WaitFixedUpdate = new WaitForFixedUpdate();
    protected RectTransform m_RectTransform = null;
	public float widthTransform 
	{ 
		get 
		{ 
			if (this.m_RectTransform != null)
				return this.m_RectTransform.sizeDelta.x;
			return -1f; 
		}	
	}
	public float heighTransform 
	{ 
		get 
		{ 
			if (this.m_RectTransform != null)
				return this.m_RectTransform.sizeDelta.y;
			return -1f; 
		}	
	}
	protected bool m_IsExplosing = false;
	
	#endregion

	#region Implementation Monobehaviour

	protected virtual void Start()
	{
		
	}

	protected virtual void OnGUI()
	{
		
	}

	#endregion

	#region Main methods

	public virtual void Init()
	{
		// UI
		this.m_RectTransform = this.transform as RectTransform;
		// CALCULATE HEIGH OFFSET
		var heigh = this.m_RectTransform.sizeDelta.y - CGameSetting.CARD_SIZE.y;
		var perItem = heigh / (CGameSetting.CARD_PER_COLUMN - 1);
		this.m_HeighOffset = Mathf.Max(50f, Mathf.Round (perItem));
		// GROUP
		this.m_Group = GameObject.FindObjectOfType<CGroupCard>();
		this.m_LayoutGroup = this.GetComponent<VerticalLayoutGroup>();
		this.m_LayoutGroup.enabled = false;
		// BOARD
		this.m_Board = GameObject.FindObjectOfType<CBoard>();
		// FIRST CARD
		this.m_FirstCard = this.transform.Find("FirstCard");
		this.m_FirstCardPoint = this.m_FirstCard.Find("FirstCardPoint");
		// CARDS
		this.m_Cards = new List<CCard>();
	}

	public virtual void Clear()
	{
		this.m_OnAnimatingCard = false;
		// CARDS
		for (int i = 0; i < this.m_Cards.Count; i++)
		{
			this.m_Cards[i].SetActive(false);
			this.m_Group.Set(this.m_Cards[i]);
		}
		this.m_Cards = new List<CCard>();
		// UI
		this.m_FirstCard.gameObject.SetActive(true);
	}

	public virtual void AddCard(CCard card)
	{
		if (this.IsAvailable (card) == false)
			return;
		// EXPLOSION
		if (this.m_IsExplosing)
			return;
		// ANIMATING CARDS
		if (this.m_OnAnimatingCard)
			return;
		this.m_OnAnimatingCard = true;
		// TO
		var to = CGameSetting.ConvertScreenToLocal (this.m_RectTransform, card.lastPosition);
		// FROM
		var lastCard = Camera.main.WorldToScreenPoint (this.GetLastCardPosition());
		var from = CGameSetting.ConvertScreenToLocal (this.m_RectTransform, lastCard);
		// UPDATE
		if (this.m_Cards.Count == 0)
		{
			this.AddCardToList (0.1f, 0.3f, card, to, from);
		}
		else if (this.m_Cards.Count > 0 && this.m_Cards.Count < CGameSetting.CARD_PER_COLUMN)
		{
			from.y -= this.m_HeighOffset;
			this.AddCardToList (0.1f, 0.3f, card, to, from);
		}
		else
		{
			this.AddCardToList (0.1f, 0.3f, card, from, from);
		}
		// CLICK SOUND
		CSoundManager.Instance.Play("sfx_drop");
	}

	public virtual void AddCardImmediate(CCard card)
	{
		// UI
		this.m_FirstCard.gameObject.SetActive(false);
		// POSITION
		var lastCard = Camera.main.WorldToScreenPoint (this.GetLastCardPosition());
		var locaPosition = CGameSetting.ConvertScreenToLocal (this.m_RectTransform, lastCard);
		if (this.m_Cards.Count > 0 && this.m_Cards.Count < CGameSetting.CARD_PER_COLUMN)
		{
			locaPosition.y -= this.m_HeighOffset;
		}
		// UPDATE
		card.transform.SetParent (this.m_RectTransform);
		card.SetAnimation(CCard.EAnimation.DROPPED);
		card.transform.localPosition = locaPosition;
		card.transform.localRotation = Quaternion.identity;
		card.transform.localScale = Vector3.one;
		card.gameObject.SetActive(true);
		card.isDropped = true;
		// ADD
		this.m_Cards.Add (card);
	}

	public virtual void AddCardToList(float waitTime, float moveTime, CCard card, Vector3 to, Vector3 from)
	{
		// SETTING
		card.transform.SetParent (this.m_RectTransform);
		card.isDropped = true;
		// MOVE
		card.Move(waitTime, 
				to, from, 
				moveTime, 
				() => {
					this.m_FirstCard.gameObject.SetActive(false);
					// CARD SETTING
					card.transform.SetParent (this.transform);
					card.SetAnimation(CCard.EAnimation.DROPPED);
					// UPDATE
					card.transform.localRotation = Quaternion.identity;
					card.transform.localScale = Vector3.one;
					// ADD
					this.m_Cards.Add (card);
					if (this.m_Cards.Count > 1)
					{
						// CHECK
						this.CheckCombineCards(() => {
							this.Check2048Card();
							// SAVE
							this.m_Board.SaveBoard();
						}, true);
					}
					else
					{
						// UNCHECK CALCULATE
						this.m_OnAnimatingCard = false;
						// 2048
						this.Check2048Card();
						// SAVE
						this.m_Board.SaveBoard();
					}
					// UPDATE
					card.OnHandDropCard();
				});
	}

	public virtual void CheckCombineCards(System.Action complete, bool checkScore = false)
	{
		// IF CARD GREATER 2
		if (this.m_Cards.Count < 2) 
		{
			// UNCHECK CALCULATE
			this.m_OnAnimatingCard = false;
			// EVENTS
			if (complete != null)
			{
				complete();
			}
			return;
		}
		StartCoroutine (this.HandleCheckCombineCards(0.2f, complete, checkScore));
	}

	public virtual IEnumerator HandleCheckCombineCards(float wait, System.Action complete, bool checkScore = false)
	{
		// WAIT
		var counter = 0f;
		while(counter < wait)
		{
			// WAIT
			yield return this.m_WaitFixedUpdate;
			counter += Time.deltaTime;
		}
		int i = this.m_Cards.Count - 1;
		int x = this.m_Cards.Count - 2;
		// SET VALUE
		Vector3 to = Vector3.zero;
		Vector3 from = Vector3.zero;
		var moving = false;
		var multi = 0;
		// CALCUALTE
		while (x >= 0)
		{
			// WAIT
			yield return this.m_WaitFixedUpdate;
			// NUMBER && NUMBER
			if (this.m_Cards[i].cardType == CCard.ECardType.NUMBER 
				&& this.m_Cards[x].cardType == CCard.ECardType.NUMBER)
			{
				// 1. IF SAME VALUE CARD NUMBER
				if (this.m_Cards[i].value == this.m_Cards[x].value)
				{
					// CALCUALTE
					to = this.m_Cards[i].transform.localPosition;
					from = this.m_Cards[x].transform.localPosition;
					moving = false;
					this.m_Cards[i].Move(0.1f, to, from, 0.2f, () => {
						moving = true;
					});
					this.m_Cards[x].SetAnimation(CCard.EAnimation.COMBINE);
					while(moving == false)
					{
						// WAIT
						yield return this.m_WaitFixedUpdate;
					}
					// 2. UPDATE VALUE
					var combineValue = this.m_Cards[i].value + this.m_Cards[x].value;
					this.m_Cards[x].value = combineValue;
					this.m_Cards[i].SetActive(false);
					this.m_Group.Set(this.m_Cards[i]);
					// 3. REMOVE DUPLICATE
					this.m_Cards.Remove (this.m_Cards[i]);
					this.m_Cards.TrimExcess();
					// 4. CONTINUE LOOP
					i = this.m_Cards.Count - 1;
					x = this.m_Cards.Count - 2;
					// 5. CHECK SCORE
					if (checkScore)
					{
						multi += 1;
						this.m_Board.AddScore(combineValue, multi);
					}
				}
				else
				{
					// 6. BREAK LOOP
					break;
				}
			}
			// BOMB && NUMBER || NUMBER && BOMB
			else if (this.m_Cards[i].cardType == CCard.ECardType.BOMB 
				|| this.m_Cards[x].cardType == CCard.ECardType.BOMB)
			{
				// 1. IF SAME VALUE CARD NUMBER
				if (this.m_Cards[i].value == this.m_Cards[x].value)
				{
					// CHECK VALUE
					if (this.m_Cards[i].value == this.m_Cards[x].value)
					{
						for (int index = 0; index < this.m_Cards.Count; index++)
						{
							this.m_Board.AddScore(this.m_Cards[index].value, 1);
						}
						yield return this.HandleExplosionAllCards(null);
						// BREAK LOOP
						break;
					}
				}
				else
				{
					// BREAK LOOP
					break;
				}
			}
		}
		// UNCHECK CALCULATE
		this.m_OnAnimatingCard = false;
		// BOARD CHECK
		this.m_Board.CheckHand();
		// EVENTS
		if (complete != null)
		{
			complete();
		}
	}

	public virtual void Check2048Card()
	{
		bool have2048 = false;
		for (int i = 0; i < this.m_Cards.Count; i++)
		{
			if (this.m_Cards[i].value >= 2048)
			{
				this.ExplosionAllCards(null);
				have2048 = true;
				break;
			}
		}
		// UI
		if (have2048)
			this.m_FirstCard.gameObject.SetActive(true);
		// SAVE BOARD 
		// this.m_Board.SaveBoard();
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
		// CLONE
		var cards = new List<CCard>(this.m_Cards);
		// CLEAR
		this.m_Cards.Clear();
		this.m_Cards.TrimExcess();
		for (int i = 0; i < cards.Count; i++)
		{
			yield return this.m_WaitSortTime;
			cards[i].Explosion();
		}
		// EXPLOSION
		this.m_IsExplosing = false;
		// UI
		this.m_FirstCard.gameObject.SetActive(true);
		// CALL EVENTS
		if (callback != null)
		{
			callback();
		}
	}

	public virtual bool IsAvailable(CCard card)
	{
		if (card == null)
			return true;
		if (this.m_Cards.Count < CGameSetting.CARD_PER_COLUMN)
			return true;
		if (this.m_Cards.Count >= CGameSetting.CARD_PER_COLUMN)
			return this.m_Cards[this.m_Cards.Count - 1].value == card.value;
		return true;
	}

	#endregion

	#region IPointer

	public void OnDrop (PointerEventData eventData)
	{
		// DROPPED
		if (this.m_Group != null 	
			&& this.m_Group.selectCard != null) // HAVE GROUP/ SELECT CARD
		{
			// ADD CARD
			this.AddCard (this.m_Group.selectCard);
			this.m_Group.selectCard = null;
		}
		// HIGHLIGHT
		this.SetHighlight(false);
	}

	public void OnPointerEnter(PointerEventData eventData)
    {
		// GROUP
		if (this.m_Group == null)
			return;
		if (this.m_Group.selectCard == null)
			return;
		this.SetHighlight(true);
	}

	public void OnPointerExit(PointerEventData eventData)
    {
		this.SetHighlight(false);
	}

	#endregion

	#region GETTER && SETTER

	public virtual Vector3 GetLastCardPosition()
	{
		// INDEX = 0
		var lastPosition = this.m_FirstCardPoint.position;
		if (this.m_Cards.Count > 0)
		{
			lastPosition = this.m_Cards[this.m_Cards.Count - 1].transform.position;
		}
		return lastPosition;
	}

	public virtual void SetHighlight(bool value)
	{
		if (this.m_Cards.Count > 0)
		{
			for (int i = 0; i < this.m_Cards.Count; i++)
			{
				if (value)
					this.m_Cards[i].SetHighlight(i == this.m_Cards.Count - 1);	
				else
					this.m_Cards[i].SetHighlight(false);
			}
		}
	}

	#endregion
	
}

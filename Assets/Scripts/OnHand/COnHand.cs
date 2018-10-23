using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class COnHand : MonoBehaviour {

	#region FIELDS

	[Header("Configs")]
	[SerializeField]	protected int m_FirstCardDraw = 2;
	public int firstCardDraw 
	{ 
		get { return this.m_FirstCardDraw; }
		set { this.m_FirstCardDraw = value; }
	}
	[SerializeField]	protected float m_WidthOffset = 140f;
	[SerializeField] 	protected Transform m_CardContainer;
	protected LayoutGroup m_LayoutGroup;
	protected Transform m_FirstCard;
	[SerializeField] 	protected List<CCard> m_OnHandCards;
	public List<CCard> onHandCard 
	{ 
		get { return this.m_OnHandCards; }
		protected set { this.m_OnHandCards = value; }
	}

	[SerializeField] 	protected CCard m_CardPrefab;

	protected CGroupCard m_Group;
	protected CBoard m_Board;

	protected WaitForFixedUpdate m_WaitFixedUpdate = new WaitForFixedUpdate();

	#endregion

	#region Implementation Monobehaviour

	protected virtual void Start()
	{
	
	}

	#endregion

	#region Main methods

	public virtual void Init()
	{
		// UI
		this.m_CardContainer = this.transform.Find("CardContain");
		this.m_LayoutGroup = this.m_CardContainer.GetComponent<LayoutGroup>();
		this.m_FirstCard = this.m_CardContainer.Find("FirstCard");
		// CARDS
		this.m_OnHandCards = new List<CCard>();
		// GROUP
		this.m_Group = GameObject.FindObjectOfType<CGroupCard>();
		// BOARD
		this.m_Board = GameObject.FindObjectOfType<CBoard>();
		// PREFABS
		this.m_CardPrefab = Resources.Load<CCard> ("Card/Card");
		// DRAW
		this.OnDrawACard();
	}

	public virtual void Clear()
	{
		// CARDS
		for (int i = 0; i < this.m_OnHandCards.Count; i++)
		{
			this.m_OnHandCards[i].SetActive(false);
			this.m_OnHandCards[i].activeCard = false;
			this.m_OnHandCards[i].isDropped = false;
			this.m_Group.Set(this.m_OnHandCards[i]);
		}
		this.m_OnHandCards = new List<CCard>();
	}

	public virtual void OnDrawACard()
	{
		// DRAW CARDS
		while (this.m_OnHandCards.Count < this.m_FirstCardDraw)
		{
			var random = Random.Range(0, 6);
			var value = CGameSetting.CARD_VALUES[random];
			var type = CCard.ECardType.NUMBER;
			var card = this.DrawCard (value, type);
			this.OnHandACard(card);
			card.SetActive (true);
		}
		// FIRST ACTIVE CARD
		this.ActiveFirstCard();
		// SAVE
		this.m_Board.SaveBoard();
	}

	public virtual CCard DrawCard(int value, CCard.ECardType type)
	{
		CCard card = this.m_Group.Get();
		if (card == null)
		{
			card = Instantiate(this.m_CardPrefab);
		}
		card.Init();
		card.Clear();
		card.Setup(value, type, this);
		card.gameObject.SetActive(true);
		// UI
		this.m_FirstCard.gameObject.SetActive (false);
		// RETURN
		return card;
	}

	public virtual void OnHandACard(CCard card)
	{
		this.m_LayoutGroup.enabled = false;
		// SET UP
		card.transform.SetParent (this.m_CardContainer);
		card.transform.localPosition = this.m_FirstCard.transform.localPosition;
		card.transform.localRotation = Quaternion.identity;
		card.transform.localScale = Vector3.one;
		card.transform.SetSiblingIndex(0);
		// ANIMATING
		this.AnimatingACard ();
		// ADD
		this.m_OnHandCards.Add (card);
	}

	public virtual void OnHandACardImmediate(CCard card)
	{
		this.m_LayoutGroup.enabled = true;
		// SET UP
		card.transform.SetParent (this.m_CardContainer);
		card.transform.localPosition = this.m_FirstCard.transform.localPosition;
		card.transform.localRotation = Quaternion.identity;
		card.transform.localScale = Vector3.one;
		card.transform.SetSiblingIndex(0);
		card.gameObject.SetActive(true);
		// ADD
		this.m_OnHandCards.Add (card);
	}

	protected void AnimatingACard()
	{
		if (this.m_OnHandCards.Count < 1)
		{
			this.m_LayoutGroup.enabled = true;
			return;
		}
		var card = this.GetLastCard();
		var to = this.m_FirstCard.transform.localPosition;
		var from = to;
		from.x += this.m_WidthOffset;
		card.Move(0.1f, 
			to, from, 
			0.2f, 
			() => {
				this.m_LayoutGroup.enabled = true;
			});
	}

	public virtual void DropCard(CCard card)
	{
		// REMOVE ON HAND
		this.m_OnHandCards.Remove (card);
		// DRAW A NEW CARD
		this.OnDrawACard();
	}

	public virtual void ActiveFirstCard()
	{
		// FIRST ACTIVE CARD
		for (int i = 0; i < this.m_OnHandCards.Count; i++)
		{
			if (i > 0)
			{
				this.m_OnHandCards[i].SetAnimation (CCard.EAnimation.APPEAR);
				this.m_OnHandCards[i].activeCard = false;
			}
			else
			{
				this.m_OnHandCards[i].activeCard = true;
			}
		}
	}

	#endregion

	#region Getter && Setter

	public virtual CCard GetAvailableCard()
	{
		// FIRST ACTIVE CARD
		for (int i = 0; i < this.m_OnHandCards.Count; i++)
		{
			if (this.m_OnHandCards[i].activeCard)
				return this.m_OnHandCards[i];
		}
		return null;
	}

	public virtual CCard GetLastCard()
	{
		if (this.m_OnHandCards.Count < 1)
			return null;
		return this.m_OnHandCards [this.m_OnHandCards.Count - 1];
	}

	#endregion

}

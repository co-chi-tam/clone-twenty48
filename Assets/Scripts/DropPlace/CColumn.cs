using System.Collections;
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
	[SerializeField]	protected int m_MaximumCards = 8;
	public int maximumCards 
	{ 
		get { return this.m_MaximumCards; }
		set { this.m_MaximumCards = value; }
	}
	[SerializeField]	protected List<CCard> m_Cards;
	public List<CCard> cards 
	{ 
		get { return this.m_Cards; }
		set { this.m_Cards = value; }
	}
	[SerializeField]	protected float m_HeighOffset = 50f;
	
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
	protected bool m_IsExplosing = false;
	
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
		this.m_RectTransform = this.transform as RectTransform;
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
		// ENABLE LAYOUT GROUP
		// this.m_LayoutGroup.enabled = true;
		this.m_FirstCard.gameObject.SetActive(true);
	}

	public virtual void AddCard(CCard card)
	{
		if (this.IsAvailable (card) == false)
			return;
		// ANIMATING CARDS
		if (this.m_OnAnimatingCard)
			return;
		this.m_OnAnimatingCard = true;
		// TO
		var to = this.ConvertScreenToLocal (this.m_RectTransform, card.lastPosition);
		// FROM
		var lastCard = Camera.main.WorldToScreenPoint (this.GetLastCardPosition());
		var from = this.ConvertScreenToLocal (this.m_RectTransform, lastCard);
		// UPDATE
		if (this.m_Cards.Count == 0)
		{
			this.AddCardToList (0.2f, 0.2f, card, to, from);
		}
		else if (this.m_Cards.Count > 0 && this.m_Cards.Count < this.m_MaximumCards)
		{
			from.y -= this.m_HeighOffset;
			this.AddCardToList (0.2f, 0.2f, card, to, from);
		}
		else
		{
			this.AddCardToList (0.1f, 0.1f, card, from, from);
		}
	}

	public virtual void AddCardImmediate(CCard card)
	{
		// UI
		// this.m_LayoutGroup.enabled = true;
		this.m_FirstCard.gameObject.SetActive(false);
		// POSITION
		var lastCard = Camera.main.WorldToScreenPoint (this.GetLastCardPosition());
		var locaPosition = this.ConvertScreenToLocal (this.m_RectTransform, lastCard);
		if (this.m_Cards.Count > 0 && this.m_Cards.Count < this.m_MaximumCards)
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
		// DISABLE LAYOUT GROUP
		this.m_LayoutGroup.enabled = false;
		card.transform.SetParent (this.m_RectTransform);
		card.OnDropCard(Vector2.zero);
		// MOVE
		card.Move(waitTime, 
				to, from, 
				moveTime, 
				() => {
					// ENABLE LAYOUT GROUP
					// this.m_LayoutGroup.enabled = this.m_Cards.Count < this.m_MaximumCards;
					this.m_FirstCard.gameObject.SetActive(false);
					// CARD SETTING
					card.transform.SetParent (this.transform);
					card.SetAnimation(CCard.EAnimation.DROPPED);
					// UPDATE
					card.transform.localRotation = Quaternion.identity;
					card.transform.localScale = Vector3.one;
					// ADD
					this.m_Cards.Add (card);
					// CHECK
					this.CheckCombineCards(() => {
						this.Check2048Card();
					});
					// UPDATE
					card.OnHandDropCard();
				});
		// DROPPRED
		card.isDropped = true;
	}

	public virtual void CheckCombineCards(System.Action complete = null)
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
		StartCoroutine (this.HandleCheckCombineCards(0.2f, complete));
	}

	public virtual IEnumerator HandleCheckCombineCards(float wait, System.Action complete)
	{
		// DISABLE LAYOUT GROUP
		this.m_LayoutGroup.enabled = false;
		// WAIT
		var counter = 0f;
		while(counter < wait)
		{
			// WAIT
			yield return this.m_WaitFixedUpdate;
			counter += Time.deltaTime;
		}
		// SET VALUE
		int i = this.m_Cards.Count - 1;
		int x = this.m_Cards.Count - 2;
		Vector3 to = Vector3.zero;
		Vector3 from = Vector3.zero;
		var moving = false;
		// CALCUALTE
		while (x >= 0)
		{
			// WAIT
			yield return this.m_WaitFixedUpdate;
			// 1. IF SAME VALUE
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
				this.m_Cards[x].value = this.m_Cards[i].value + this.m_Cards[x].value;
				this.m_Cards[i].SetActive(false);
				this.m_Group.Set(this.m_Cards[i]);
				// 3. REMOVE DUPLICATE
				this.m_Cards.Remove (this.m_Cards[i]);
				this.m_Cards.TrimExcess();
				// 4. CONTINUE LOOP
				i = this.m_Cards.Count - 1;
				x = this.m_Cards.Count - 2;
			}
			else
			{
				// 5. BREAK LOOP
				break;
			}
		}
		// UNCHECK CALCULATE
		this.m_OnAnimatingCard = false;
		// ENABLE LAYOUT GROUP
		// this.m_LayoutGroup.enabled = true;
		// BOARD CHECK
		this.m_Board.CheckHand();
		// EVENTS
		if (complete != null)
		{
			complete();
		}
	}

	public virtual void CheckCombineCardsNonAnim(System.Action complete = null)
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
		// SET VALUE
		int i = this.m_Cards.Count - 1;
		int x = this.m_Cards.Count - 2;
		Vector3 to = Vector3.zero;
		Vector3 from = Vector3.zero;
		// CALCUALTE
		while (x >= 0)
		{
			// 1. IF SAME VALUE
			if (this.m_Cards[i].value == this.m_Cards[x].value)
			{
				// CALCUALTE
				to = this.m_Cards[i].transform.localPosition;
				from = this.m_Cards[x].transform.localPosition;
				// 2. UPDATE VALUE
				this.m_Cards[x].value = this.m_Cards[i].value + this.m_Cards[x].value;
				this.m_Cards[i].SetActive(false);
				this.m_Group.Set(this.m_Cards[i]);
				// 3. REMOVE DUPLICATE
				this.m_Cards.Remove (this.m_Cards[i]);
				this.m_Cards.TrimExcess();
				// 4. CONTINUE LOOP
				i = this.m_Cards.Count - 1;
				x = this.m_Cards.Count - 2;
			}
			else
			{
				// 5. BREAK LOOP
				break;
			}
		}
		// UNCHECK CALCULATE
		this.m_OnAnimatingCard = false;
		// ENABLE LAYOUT GROUP
		// this.m_LayoutGroup.enabled = true;
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
		for (int i = 0; i < this.m_Cards.Count; i++)
		{
			if (this.m_Cards[i].value >= 2048)
			{
				this.ExplosionAllCards(null);
			}
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
		for (int i = 0; i < this.m_Cards.Count; i++)
		{
			yield return this.m_WaitSortTime;
			this.m_Cards[i].Explosion();
		}
		// EXPLOSION
		this.m_IsExplosing = false;
		// CALL EVENTS
		if (callback != null)
		{
			callback();
		}
	}

	public virtual Vector2 ConvertScreenToLocal(RectTransform parent, Vector3 position)
	{
		var localPosition = Vector2.zero;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, 
            position, 
            Camera.main, 
            out localPosition);
		return localPosition;
	}

	public virtual bool IsAvailable(CCard card)
	{
		if (card == null)
			return true;
		if (this.m_Cards.Count < this.m_MaximumCards)
			return true;
		if (this.m_Cards.Count >= this.m_MaximumCards)
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

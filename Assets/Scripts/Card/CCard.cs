using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CCard : MonoBehaviour, 
	IBeginDragHandler, IDragHandler, IEndDragHandler {

	public enum ECardType: int 
	{
		NUMBER = 0,
		BOMB = 1,
		SPECIAL = 2
	}

	public enum EAnimation: int
	{
		NONE = 0,
		DROPPED = 1,
		COMBINE = 2,
		APPEAR = 3
	}

	#region Fields

	[Header("Configs")]
	[SerializeField]	protected bool m_ActiveCard = false;
	public bool activeCard 
	{ 
		get { return this.m_ActiveCard; }
		set { this.m_ActiveCard = value; }
	}

	[SerializeField]	protected ECardType m_CardType = ECardType.NUMBER;
	public ECardType cardType 
	{ 
		get { return this.m_CardType; }
		set { this.m_CardType = value; }
	}
	[Header("Configs")]
	[SerializeField]	protected bool m_IsDropped = false;
	public bool isDropped 
	{ 
		get { return this.m_IsDropped; }
		set { this.m_IsDropped = value; }
	}
	[SerializeField]	protected int m_Value = 0;
	public int value 
	{ 
		get { return this.m_Value; }
		set 
		{ 
			this.m_Value = value; 
			// UI
			if (this.m_ValueText != null)
			{
				this.m_ValueText.text = this.m_Value.ToString();
			}
			// COLOR
			if (this.m_BGImage != null)
			{
				var index = System.Array.IndexOf (CGameSetting.CARD_VALUES, value);
				if (index > -1)
				{
					var color = CGameSetting.CARD_COLORS[index];
					this.m_BGImage.color = color;
				} 
			}
		}
	}

	[SerializeField]	protected Vector2 m_Size = new Vector2(196f, 260f);
	public Vector2 size 
	{ 
		get { return this.m_Size; }
		set { this.m_Size = value; }
	}

	[SerializeField]	protected Image m_BGImage;
	[SerializeField]	protected RectTransform m_DragObject;
	public RectTransform dragObject 
	{ 
		get { return this.m_DragObject; }
		set { this.m_DragObject = value; }
	}

	[SerializeField]	protected Text m_ValueText;
	[SerializeField]	protected ParticleSystem m_ExplosionSystem;
	public ParticleSystem explosion 
	{ 
		get { return this.m_ExplosionSystem; }
		set { this.m_ExplosionSystem = value; }
	}

	public Vector3 lastPosition;

	protected Vector3 m_FirstStartPosition;
    private Vector2 originalLocalPointerPosition;
    private Vector3 originalPanelLocalPosition;
	private RectTransform dragObjectInternal
    {
        get
        {
            if (this.m_DragObject == null)
            {
                this.m_DragObject = transform as RectTransform;
            }
            return this.m_DragObject;
        }
    }
	
    private RectTransform dragArea = null;
    private RectTransform dragAreaInternal
    {
        get
        {
            if (dragArea == null)
            {
                RectTransform canvas = transform as RectTransform;
                while (canvas.parent != null && canvas.parent is RectTransform)
                {
                    canvas = canvas.parent as RectTransform;
                }
                dragArea = canvas; 
            }
            return dragArea;
        }
    }

	protected CGroupCard m_Group;
	protected COnHand m_OnHand;
	public COnHand onHand 
	{ 
		get { return this.m_OnHand; }
		set { this.m_OnHand = value; }
	}

	protected RectTransform m_RectTransform;

	protected WaitForFixedUpdate m_WaitFixedUpdate = new WaitForFixedUpdate();
	protected bool m_IsMoving = false;

	protected Animator m_Animator;

	protected Transform m_HighlightImage;

	#endregion

	#region Implementation Monobehaviour

	protected virtual void Start()
	{
		
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		this.OnBeginDragCard (eventData.position);
	}

	public void OnDrag(PointerEventData eventData)
	{
		this.OnDragCard (eventData.position);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		this.OnDropCard (eventData.position);
	}

	#endregion

	#region Main methods

	public virtual void Init()
	{
		// UI
		this.m_RectTransform = this.transform as RectTransform;
		this.m_BGImage = this.transform.Find("BGImage").GetComponent<Image>();
		this.m_FirstStartPosition = this.m_BGImage.transform.localPosition;
		this.lastPosition = this.m_FirstStartPosition;
		this.m_DragObject = this.m_BGImage.GetComponent<RectTransform>();
		// VALUE
		this.m_ValueText = this.transform.Find("BGImage/ValueText").GetComponent<Text>();
		// GROUP
		this.m_Group = GameObject.FindObjectOfType<CGroupCard>();
		// DROP
		this.m_IsDropped = false;
		// ANIM
		this.m_IsMoving = false;
		this.m_Animator = this.GetComponent<Animator>();
		this.m_ExplosionSystem = this.transform.Find("Explosion").GetComponent<ParticleSystem>();
		// HIGHLIGHT
		this.m_HighlightImage = this.transform.Find("HighlightImage");
		this.m_HighlightImage.gameObject.SetActive(false);
	}

	public virtual void Clear()
	{
		// UI
		this.m_BGImage.gameObject.SetActive (true);
		this.m_DragObject.gameObject.SetActive (true);
		this.m_ValueText.gameObject.SetActive (true);
		this.m_HighlightImage.gameObject.SetActive(false);
		// DROP
		this.m_IsDropped = false;
		// ANIM
		this.m_IsMoving = false;
	}

	public virtual void Setup(int value, ECardType type, COnHand onHand)
	{
		this.name = string.Format("Card_{0}_{1}", type.ToString(), value);
		// UI
		this.m_ValueText.text = value.ToString();
		// VALUE
		this.value = value;
		// TYPE
		this.m_CardType = type;
		// ONHAND
		this.m_OnHand = onHand;
	}

	public virtual void ResetSize()
	{
		this.m_RectTransform.sizeDelta = this.m_Size;
	}

	public virtual void Move(float wait, Vector3 to, Vector2 from, float time, System.Action complete)
	{
		if (this.m_IsMoving)
			return;
		StartCoroutine (this.HandleMoveTo(wait, to, from, time, complete));
	}

	public virtual IEnumerator HandleMoveTo(float wait, Vector3 to, Vector2 from, float time, System.Action complete)
	{
		// WAIT TO
		var counter = 0f;
		this.m_IsMoving = true;
		// START
		to.x = from.x;
		this.transform.localPosition = to;
		while (counter < wait)
		{
			yield return this.m_WaitFixedUpdate;
			counter += Time.fixedDeltaTime;
		}
		counter = 0f;
		// UPDATE PER FIXED TIMER
		while (counter < time)
		{
			yield return this.m_WaitFixedUpdate;
			var delta = counter / time;
			this.transform.localPosition = Vector3.Lerp (to, from, delta);
			counter += Time.fixedDeltaTime;
		}
		// UPDATE
		this.transform.localPosition = from;
		this.m_IsMoving = false;
		if (complete != null)
		{
			complete();
		}
	}

	public virtual void OnHandDropCard()
	{
		if (this.m_OnHand == null)
			return;
		this.m_OnHand.DropCard(this);
	}

	public virtual void Explosion()
	{
		if (this.m_ExplosionSystem.isPlaying)
			return;
		this.m_ExplosionSystem.Play();

		this.m_BGImage.gameObject.SetActive (false);
		this.m_DragObject.gameObject.SetActive (false);
		this.m_ValueText.gameObject.SetActive (false);
		this.m_HighlightImage.gameObject.SetActive(false);
	}

	#endregion

	#region DRAG

	public virtual void OnBeginDragCard(Vector2 position)
	{
		// ACTIVE
		if (this.m_ActiveCard == false)
			return;
		// GROUP
		if (this.m_Group == null)
			return;
		// UI
		if (this.m_BGImage == null)
			return;
		// IS DROPPED
		if (this.m_IsDropped)
			return;
		this.m_Group.selectCard = this;
		originalPanelLocalPosition = dragObjectInternal.localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragAreaInternal, 
            position, 
            Camera.main, 
            out originalLocalPointerPosition);
	}

	public virtual void OnDragCard(Vector2 position)
	{
		// ACTIVE
		if (this.m_ActiveCard == false)
			return;
		// GROUP
		if (this.m_Group == null)
			return;
		if (this.m_Group.selectCard != this)
			return;
		// IS DROPPED
		if (this.m_IsDropped)
			return;
		// UI
		if (this.m_BGImage == null)
			return;
		Vector2 localPointerPosition;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(dragAreaInternal, Input.mousePosition, Camera.main, out localPointerPosition))
		{
			Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
			var move = originalPanelLocalPosition + offsetToOriginal;
			var delta = Vector3.Lerp(dragObjectInternal.localPosition, move, 0.5f);
			dragObjectInternal.localPosition = delta;
			this.lastPosition = Camera.main.WorldToScreenPoint (dragObjectInternal.position);
		}
	}

	public virtual void OnDropCard(Vector2 position)
	{
		// ACTIVE
		if (this.m_ActiveCard == false)
			return;
		// GROUP
		if (this.m_Group == null)
			return;
		// UI
		if (this.m_BGImage == null)
			return;
		// IS DROPPED
		if (this.m_IsDropped)
			return;
		this.m_Group.selectCard = null;
		dragObjectInternal.localPosition = this.m_FirstStartPosition;
	}

	#endregion

	#region GETTER && SETTER
	
	public virtual void SetAnimation(EAnimation anim)
	{
		if (this.m_Animator == null)
			return;
		switch (anim)
		{
			default:
			case EAnimation.APPEAR:
				this.m_Animator.SetTrigger("IsAppear");
			break;
			case EAnimation.DROPPED:
				this.m_Animator.SetTrigger("IsDropped");
			break;
			case EAnimation.COMBINE:
				this.m_Animator.SetTrigger("IsCombine");
			break;
		}
	}

	public virtual void SetHighlight(bool value)
	{
		this.m_HighlightImage.gameObject.SetActive (value);

		this.m_Animator.SetBool("IsHighlight", value);
	}

	public virtual void SetActive(bool value)
	{
		this.gameObject.SetActive (value);
	}

	#endregion
	
}

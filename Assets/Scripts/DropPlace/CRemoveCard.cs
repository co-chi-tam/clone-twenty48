using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class CRemoveCard : MonoBehaviour, 
	IDropHandler {

	#region Fields

	[SerializeField]	protected int m_CurrentSize = 0;
	[SerializeField]	protected int m_MaximumSize = 2;

	protected RectTransform m_RectTransform;

	protected CGroupCard m_Group;

	protected Image m_FilledImage;

	protected CAdsSimple m_AdsSimple;

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
		// FILLD IMAGE
		this.m_FilledImage = this.transform.Find("FilledImage").GetComponent<Image>();
		this.m_FilledImage.type = Image.Type.Filled;
		this.m_FilledImage.fillMethod = Image.FillMethod.Vertical;
		this.m_FilledImage.fillOrigin = 0;
		this.m_FilledImage.fillAmount = 0f;
		// CARDS
		this.m_CurrentSize = 0;
		// ADS
		this.m_AdsSimple = GameObject.FindObjectOfType<CAdsSimple>();
	}

	public virtual void RemoveCard(CCard card)
	{
		if (this.m_CurrentSize >= this.m_MaximumSize)
			this.RemoveCardWithAds (card);
		else
			this.RemoveCardSimple (card);
	}

	public virtual void RemoveCardSimple(CCard card)
	{
		card.transform.SetParent (this.m_RectTransform);	
		// TO
		var to = CGameSetting.ConvertScreenToLocal (this.m_RectTransform, card.lastPosition);
		// FROM
		var from = this.m_FilledImage.transform.localPosition;
		// MOVE
		card.SetAnimation(CCard.EAnimation.DISAPPEAR);
		card.Move (0.1f, to, from, 0.3f, () => {
			card.OnDropCard(Vector2.zero);
			card.OnHandDropCard();
			card.SetActive (false);
			// RETURN CACHE
			this.m_Group.Set(card);
			// UPDATE SIZE
			this.m_CurrentSize += 1;
			this.m_FilledImage.fillAmount = (float)this.m_CurrentSize / this.m_MaximumSize;
		});
	}

	public virtual void RemoveCardWithAds(CCard card)
	{
		if (this.m_AdsSimple == null)
			return;
		if (this.m_AdsSimple.canShowAds == false)
			return;
		this.m_AdsSimple.OnFinish.RemoveAllListeners();
		this.m_AdsSimple.OnFinish.AddListener(() => {
			this.RemoveCardSimple(card);
		});
		this.m_AdsSimple.Show();
	}

	public virtual void Clear()
	{
		// CARDS
		this.m_CurrentSize = 0;
		this.m_FilledImage.fillAmount = 0f;
	}

	#endregion

	#region IPointer

	public void OnDrop (PointerEventData eventData)
	{
		// DROPPED
		if (this.m_Group != null 	
			&& this.m_Group.selectCard != null) // HAVE GROUP/ SELECT CARD
		{
			// REMOVE CARD
			this.RemoveCard (this.m_Group.selectCard);
		}
	}

	#endregion

	#region Getter && Setter

	public virtual void SetSize(int value)
	{
		// UPDATE SIZE
		this.m_CurrentSize = value;
		this.m_FilledImage.fillAmount = (float)this.m_CurrentSize / this.m_MaximumSize;
	}

	#endregion

}

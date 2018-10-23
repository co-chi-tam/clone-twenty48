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

	protected CGroupCard m_Group;

	protected Image m_FilledImage;

	#endregion

	#region Implementation Monobehaviour

	protected virtual void Start()
	{
		
	}

	#endregion

	#region Main methods

	public virtual void Init()
	{
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
	}

	public virtual void RemoveCard(CCard card)
	{
		if (this.m_CurrentSize >= this.m_MaximumSize)
			return;
		card.OnDropCard(Vector2.zero);
		card.OnHandDropCard();
		card.SetActive (false);
		// RETURN CACHE
		this.m_Group.Set(card);
		// UPDATE SIZE
		this.m_CurrentSize += 1;
		this.m_FilledImage.fillAmount = (float)this.m_CurrentSize / this.m_MaximumSize;
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

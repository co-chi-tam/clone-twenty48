using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CMenu : MonoBehaviour {

	[SerializeField]	protected Button m_ExitButton;
	[SerializeField]	protected Button m_SoundToggleButton;
	protected GameObject m_SoundOnImage;
	protected GameObject m_SoundOffImage;

	protected CBoard m_Board;

	public virtual void Init()
	{
		// BOARD
		this.m_Board = GameObject.FindObjectOfType<CBoard>();
		// EXIT BUTTON
		this.m_ExitButton = this.transform.Find("ExitButton").GetComponent<Button>();
		this.m_ExitButton.onClick.RemoveAllListeners();
		this.m_ExitButton.onClick.AddListener(() => {
			if (this.m_Board != null)
			{
				this.m_Board.ResetBoard ();
			}
			// CLICK SOUND
			CSoundManager.Instance.Play("sfx_click");
		});
		// SOUND
		this.m_SoundToggleButton = this.transform.Find("SoundButton").GetComponent<Button>();
		this.m_SoundOnImage = this.transform.Find("SoundButton/OnImage").gameObject;
		this.m_SoundOffImage = this.transform.Find("SoundButton/OffImage").gameObject;
		this.m_SoundOnImage.SetActive(!CGameSetting.SETTING_SOUND_MUTE);
		this.m_SoundOffImage.SetActive(CGameSetting.SETTING_SOUND_MUTE);
		this.m_SoundToggleButton.onClick.RemoveAllListeners();
		this.m_SoundToggleButton.onClick.AddListener(() => {
			this.SoundToggle();
			// CLICK SOUND
			CSoundManager.Instance.Play("sfx_click");
			CSoundManager.Instance.MuteAll(CGameSetting.SETTING_SOUND_MUTE);
		});
	}
	
	protected virtual void SoundToggle()
	{
		var soundOn = CGameSetting.SETTING_SOUND_MUTE;
		CGameSetting.SETTING_SOUND_MUTE = !soundOn;
		this.m_SoundOnImage.SetActive(!CGameSetting.SETTING_SOUND_MUTE);
		this.m_SoundOffImage.SetActive(CGameSetting.SETTING_SOUND_MUTE);
	}

}

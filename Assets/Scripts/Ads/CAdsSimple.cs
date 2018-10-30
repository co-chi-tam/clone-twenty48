using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.Events;

public class CAdsSimple : MonoBehaviour {

	[Header("Configs")]
   	[SerializeField]	protected string gameId = "2872670";
	[SerializeField]	protected string placementId = "rewardedVideo"; // "video";
	public bool canShowAds 
	{
		get { return Advertisement.IsReady() && CGameSetting.IsTimerToAd(CGameSetting.DELAY_TO_AD); }
	}

	[Header("Events")]
	public UnityEvent OnShow;
	public UnityEvent OnFinish;
	public UnityEvent OnSkip;
	public UnityEvent OnFail;

	protected virtual void Start() {
		this.InitAds ();
	}

	public virtual void InitAds() {
		CGameSetting.InitTimerToAd();
		if (Advertisement.isSupported) {
            Advertisement.Initialize (this.gameId, true);
        }
	}

	public virtual void Show() {
		this.Show (this.placementId);
	}

	public virtual void Show(string place) {
		if (Advertisement.IsReady() == false)
		{
			this.InitAds();
			return;	
		}
		ShowOptions options = new ShowOptions();
        options.resultCallback = this.HandleShowResult;
        Advertisement.Show(place, options);
		CGameSetting.ResetTimerToAd();
		if (this.OnShow != null) {
			this.OnShow.Invoke ();
		}
	}

	protected void HandleShowResult (ShowResult result)
    {
        if(result == ShowResult.Finished) {
        	Debug.Log("Video completed - Offer a reward to the player");
			if (this.OnFinish != null) {
				this.OnFinish.Invoke ();
			}
        }else if(result == ShowResult.Skipped) {
            Debug.LogWarning("Video was skipped - Do NOT reward the player");
			if (this.OnSkip != null) {
				this.OnSkip.Invoke ();
			}
        }else if(result == ShowResult.Failed) {
            Debug.LogError("Video failed to show");
			if (this.OnFail != null) {
				this.OnFail.Invoke ();
			}
        }
    }

}

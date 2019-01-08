using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;

using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class CStartScene : MonoBehaviour {

	protected Button m_PlayButton;
	protected Button m_RefeshButton;
	protected Button m_OpenLeaderboardButton;
	protected CBoard m_Board;

	public static bool IS_LEADERBOARD_INIT = false;

	protected virtual void Awake()
	{
		// ADMOB
		CAdmobManager.Init();
		CAdmobManager.InitBanner();
	}

	protected virtual void Start()
	{
		this.Init();
	}

	public virtual void Init()
	{
		// SOUND
		CSoundManager.Instance.MuteAll(CGameSetting.SETTING_SOUND_MUTE);
		// ADMOB
		CAdmobManager.ShowHideBanner(true);
		// BOARD
		this.m_Board = GameObject.FindObjectOfType<CBoard>();
		// PLAY
		this.m_PlayButton = this.transform.Find("GroupButtons/PlayButton").GetComponent<Button>();
		this.m_PlayButton.onClick.RemoveAllListeners();
		this.m_PlayButton.onClick.AddListener(() => {
			// PlayerPrefs.DeleteAll();
			this.gameObject.SetActive(false);
			this.m_Board.Init();
			// CLICK SOUND
			CSoundManager.Instance.Play("sfx_click");
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
			// CLICK SOUND
			CSoundManager.Instance.Play("sfx_click");
		});
		this.m_OpenLeaderboardButton = this.transform.Find("GroupButtons/OpenLeaderboardButton").GetComponent<Button>();
		this.m_OpenLeaderboardButton.onClick.RemoveAllListeners();
		this.m_OpenLeaderboardButton.onClick.AddListener(() => {
			if (IS_LEADERBOARD_INIT)
			{
				// show leaderboard UI
				PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard_score_leaderboard);
				// CLICK SOUND
				CSoundManager.Instance.Play("sfx_click");
			}
		});
		// INIT GOOGLE SERVICES
		this.InitGoogleServices();
		// SIGIN IN
		this.SignInGoogle();
	}

	public virtual void InitGoogleServices()
	{
		PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
        // enables saving game progress.
        // .EnableSavedGames()
        // // registers a callback to handle game invitations received while the game is not running.
        // .WithInvitationDelegate(<callback method>)
        // // registers a callback for turn based match notifications received while the
        // // game is not running.
        // .WithMatchDelegate(<callback method>)
        // // requests the email address of the player be available.
        // // Will bring up a prompt for consent.
        // .RequestEmail()
        // requests a server auth code be generated so it can be passed to an
        // associated back end server application and exchanged for an OAuth token.
        // .RequestServerAuthCode(false)
        // requests an ID token be generated.  This OAuth token can be used to
        // identify the player to other services such as Firebase.
        // .RequestIdToken()
		.AddOauthScope("email")
        .AddOauthScope("profile")
        .Build();

    	PlayGamesPlatform.InitializeInstance(config);
		// recommended for debugging:
		PlayGamesPlatform.DebugLogEnabled = false;
		// Activate the Google Play Games platform
		PlayGamesPlatform.Activate();
	}

	public virtual void SignInGoogle()
	{
		// authenticate user:
		Social.localUser.Authenticate((bool success) => {
			// handle success or failure
			IS_LEADERBOARD_INIT = success;
		});
	}

}

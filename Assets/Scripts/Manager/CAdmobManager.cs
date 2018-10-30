using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleSingleton;
using GoogleMobileAds.Api;

public class CAdmobManager {

#if UNITY_ANDROID
	public static string appId 	= "ca-app-pub-9073576735501135~2656325106";
	public static string adUnitId 	= "ca-app-pub-9073576735501135/6262443919";
#elif UNITY_IPHONE
	public static string appId 	= "ca-app-pub-9073576735501135~2656325106";
	public static string adUnitId 	= "ca-app-pub-9073576735501135/6262443919";
#else
	public static string appId 	= "ca-app-pub-9073576735501135~2656325106";
	public static string adUnitId 	= "ca-app-pub-9073576735501135/6262443919";
#endif

	protected static BannerView bannerView;

	public static void Init()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(appId);
    }

	public static void InitBanner()
    {
        // Create a 320x50 banner at the bottom of the screen.
        bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
		// Called when an ad request has successfully loaded.
        bannerView.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request failed to load.
        bannerView.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when an ad is clicked.
        bannerView.OnAdOpening += HandleOnAdOpened;
        // Called when the user returned from the app after an ad click.
        bannerView.OnAdClosed += HandleOnAdClosed;
        // Called when the ad click caused the user to leave the application.
        bannerView.OnAdLeavingApplication += HandleOnAdLeavingApplication;
    }

    public static void ShowBanner()
    {
		// Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the banner with the request.
        bannerView.LoadAd(request);
    }

	protected static void HandleOnAdLoaded(object sender, EventArgs args)
    {
        Debug.LogWarning("HandleAdLoaded event received");
    }

    protected static void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.LogWarning("HandleFailedToReceiveAd event received with message: " + args.Message);
    }

    protected static void HandleOnAdOpened(object sender, EventArgs args)
    {
        Debug.LogWarning("HandleAdOpened event received");
    }

    protected static void HandleOnAdClosed(object sender, EventArgs args)
    {
        Debug.LogWarning("HandleAdClosed event received");
    }

    protected static void HandleOnAdLeavingApplication(object sender, EventArgs args)
    {
         Debug.LogWarning("HandleAdLeavingApplication event received");
    }

}

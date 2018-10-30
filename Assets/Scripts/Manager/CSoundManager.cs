using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleSingleton;

public class CSoundManager : CMonoSingleton<CSoundManager> {

	protected List<AudioSource> m_AudioSources;

	protected Dictionary<string, AudioClip> m_AudioMaps;

	protected bool m_Inited = false;

	protected bool m_IsMuteAll = false;

	protected override void Awake()
	{
		base.Awake();
		DontDestroyOnLoad (this.gameObject);
	}

	public virtual void Init()
	{
		// AUDIOES
		this.m_AudioSources = new List<AudioSource>();
		// AUDIO MAPS
		this.m_AudioMaps = new Dictionary<string, AudioClip>();
		// INIT
		this.m_Inited = true;
	}

	public virtual AudioSource SetupAudioSource()
	{
		// GAME OBJECT
		var go = new GameObject("AudioSource " + (this.m_AudioSources.Count + 1));
		go.transform.SetParent(this.transform);
		// SOUND
		var audioSource = go.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.loop = false;
		audioSource.Stop();
		// ADD
		this.m_AudioSources.Add (audioSource);
		return audioSource;
	}

	public virtual void Play(string name, bool loop = false)
	{
		if (this.m_Inited == false)
			this.Init();
		if (this.m_IsMuteAll)
			return;
		// VALUE
		AudioClip audioClip = null;
		AudioSource audioSource = null;
		// AUDIO CLIP
		if (this.m_AudioMaps.ContainsKey(name))
		{
			audioClip = this.m_AudioMaps[name];
		}
		else
		{
			this.m_AudioMaps[name] = null;
		}
		if (audioClip == null)
		{
			audioClip = Resources.Load<AudioClip>(this.GetSoundPath(name));
			this.m_AudioMaps[name] = audioClip;
		}
		// AUDIO SOURCE
		for (int i = 0; i < this.m_AudioSources.Count; i++)
		{
			if (this.m_AudioSources[i].isPlaying == false || this.m_AudioSources[i].clip == null) // && this.m_AudioSources[i].loop == false
			{
				audioSource = this.m_AudioSources[i];
				break;
			}
		}	
		if (audioSource == null)
		{
			audioSource = this.SetupAudioSource();
		}
		audioSource.clip = audioClip;
		audioSource.loop = loop;
		audioSource.Play();
	}

	public virtual void MuteAll(bool value)
	{
		// INIT
		if (this.m_Inited == false)
			this.Init();
		// MUTE
		this.m_IsMuteAll = value;
		for (int i = 0; i < this.m_AudioSources.Count; i++)
		{
			var	audioSource = this.m_AudioSources[i];
			if (value)
			{
				if (audioSource.loop)
				{
					audioSource.Play();
				}
				else
				{
					audioSource.clip = null;
				}
			}
			else
			{
				audioSource.Stop();	
			}
		}	
	}

	public virtual string GetSoundPath(string name)
	{
		return string.Format("Sounds/{0}", name);
	}

}

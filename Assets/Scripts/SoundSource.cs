using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AudioSource))]

public class SoundSource : MonoBehaviour
{
	#region fields
	
		public bool isMusic = false;
		public AudioClip[] clips;
		new AudioSource audio;
	
		public bool isPlaying {
			get { return audio.isPlaying; }
		}
	
		public float volume {
			get { return audio.volume; }
			set { audio.volume = value; }
		}
	
		public float maxDistance {
			get { return audio.maxDistance; }
			set { audio.maxDistance = value; }
		}
	
	#endregion
	
	
	
	public bool IsPlaying()
	{
		return audio.isPlaying;
	}
	
	
	
	void OnDestroy ()
	{
		if ( Singletons.timeManager != null )
			Singletons.timeManager.OnTimeWarpChangedEvent -= OnTimeWarpChanged;
	}
	
	
	
	void OnTimeWarpChanged ( float timeWarp )
	{
		audio.pitch = timeWarp;
	}
	
	
	
	public void Play ()
	{
		if ( false == audio.enabled )
			return;
		
		if ( clips.Length > 0 )
			audio.clip = clips [ Random.Range (0, clips.Length) ];
		audio.pitch = Singletons.timeManager.timeWarp;
		audio.Play ();
	}
	
	
	
	void Start ()
	{
		audio = base.audio;
		Singletons.timeManager.OnTimeWarpChangedEvent += OnTimeWarpChanged;
	}
	
	
	
	public void Stop ()
	{
		audio.Stop ();
	}
	
}

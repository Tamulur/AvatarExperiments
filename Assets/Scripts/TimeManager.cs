using UnityEngine;
using System.Collections;

public class TimeManager : MonoBehaviour
{
	#region fields
	
		const float kWarpOutDuration = 0.5f;
		const float kWarpInDuration = 0.5f;
		const float kSlowTime = 0.2f;
	
		float normalTime = 1;
	
				float _timeWarp = 1;
		public float timeWarp {
			get { return _timeWarp; }
			set {
				Time.timeScale = value;
				Time.fixedDeltaTime = 0.02f * Time.timeScale;
				_timeWarp = value;
				if ( OnTimeWarpChangedEvent != null )
					OnTimeWarpChangedEvent ( _timeWarp );
//					OnTimeWarpChangedEvent ( (_timeWarp-kSlowTime)/(1-kSlowTime) );
			}
		}
		
		// 0: slowest time
		// 1: normal time
		public delegate void OnTimeWarpChangedCallback ( float newTimeWarp );
		public event OnTimeWarpChangedCallback OnTimeWarpChangedEvent;
	
		public delegate void OnTimeWarpedInCallback ( );
		public event OnTimeWarpedInCallback OnTimeWarpedInEvent;
		
		public delegate void OnTimeWarpedOutCallback ( );
		public event OnTimeWarpedOutCallback OnTimeWarpedOutEvent;
		
	#endregion
	
	
	void OnDestroy()
	{
		Singletons.timeManager = null;
	}
	
	
	
	void OnTimeWarpedIn ()
	{
		if ( OnTimeWarpedInEvent != null )
			OnTimeWarpedInEvent ();
	}
	
	
	
	void OnTimeWarpedOut ()
	{
		if ( OnTimeWarpedOutEvent != null )
			OnTimeWarpedOutEvent ();
	}
	
	
	
	public void SetNormalTime ( float normalTime )
	{
		this.normalTime = normalTime;
	}
	
	
	
	public void WarpTimeIn ()
	{
		Go.killAllTweensWithTarget( this );
		Go.to ( this, kWarpInDuration, new GoTweenConfig().floatProp ("timeWarp", kSlowTime).onComplete ( t => OnTimeWarpedIn () ) );
	}
	
	
	
	public void WarpTimeOut ()
	{
		Go.killAllTweensWithTarget( this );
		Go.to ( this, kWarpOutDuration, new GoTweenConfig().floatProp ("timeWarp", normalTime).onComplete ( t => OnTimeWarpedOut () ) );
	}
	
}

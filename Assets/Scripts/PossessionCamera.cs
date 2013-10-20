using UnityEngine;
using System.Collections;


[RequireComponent (typeof (Vignetting))]


public class PossessionCamera : MonoBehaviour
{
	#region fields
		const float maxVignetting = 7.0f;
		Vignetting vignetting;
	#endregion
	
	
	
	void Awake()
	{
		vignetting = GetComponent<Vignetting>();
		Singletons.timeManager.OnTimeWarpChangedEvent += OnTimeWarpChanged;
	}
	
	
	
	void OnDestroy ()
	{
		if ( Singletons.timeManager != null )
			Singletons.timeManager.OnTimeWarpChangedEvent -= OnTimeWarpChanged;
	}
	
	
	

	void OnTimeWarpChanged ( float timeWarp )
	{
		vignetting.intensity = maxVignetting * (1-timeWarp);
	}
	
	
}

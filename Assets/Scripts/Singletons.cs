using UnityEngine;
using System.Collections;

public class Singletons
{
	static TimeManager sTimeManager;
	public static TimeManager timeManager {
			get {
					if ( sTimeManager == null )
						sTimeManager = MiscUtils.GetComponentSafely<TimeManager>("TimeManager");
					return sTimeManager;
			}
			set { sTimeManager = value; }
	}
	
}

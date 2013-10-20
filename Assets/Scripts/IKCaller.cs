using UnityEngine;
using System;

public class IKCaller : MonoBehaviour
{
	public Action delegateOnAnimatorIK { get; set; }
	
	
	
	void OnAnimatorIK()
	{
		delegateOnAnimatorIK();
	}
	
}

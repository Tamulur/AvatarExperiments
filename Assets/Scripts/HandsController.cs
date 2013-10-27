using UnityEngine;
using System.Collections;

public class HandsController : MonoBehaviour
{
    #region fields
        
        Animator animator; 
		bool areHandsCalibrated;
        HydraHand leftHand;
        HydraHand rightHand;
	
    #endregion



    void Awake()
    {
        leftHand = new HydraHand(SixenseHands.LEFT);
        rightHand = new HydraHand(SixenseHands.RIGHT);
		
		areHandsCalibrated = false;
    }


	
	public void Deactivate()
	{
		Destroy( animator.gameObject.GetComponent<IKCaller>() );
		
		leftHand.Deactivate();
		rightHand.Deactivate();
		
		animator = null;
	}
	
	
	
	public void InitializeForNewCharacter()
	{
        animator = transform.parent.GetComponent<Animator>();
		animator.gameObject.AddComponent<IKCaller>().delegateOnAnimatorIK = OnAnimatorIK;
		
		CameraAnchor cameraAnchor = transform.parent.Find("CameraAnchor").GetComponent<CameraAnchor>();
		leftHand.InitializeForNewCharacter( animator, cameraAnchor );
		rightHand.InitializeForNewCharacter( animator, cameraAnchor );
	}
	
	
	
    void OnAnimatorIK()
    {
	    SixenseInput.Controller leftController = SixenseInput.GetController( SixenseHands.LEFT );
		bool useControllersAsTargets = false;
		if ( leftController != null )
		{
			if ( leftController.GetButtonUp(SixenseButtons.FOUR) )
			{
	            leftHand.ResetControllerPosition();
	            rightHand.ResetControllerPosition();
	
	            float controllerDistance = Vector3.Distance( leftHand.baseControllerPosition, rightHand.baseControllerPosition );
	            float avatarHandsDistance = Vector3.Distance( leftHand.initialGlobalPosition, rightHand.initialGlobalPosition );
	
	            leftHand.scale = rightHand.scale = avatarHandsDistance / (animator.transform.localScale.x * controllerDistance);
				
				useControllersAsTargets = true;
			}
			else if ( leftController.GetButtonDown(SixenseButtons.FOUR) )
			{
				leftHand.ResetHandPosition();
				rightHand.ResetHandPosition();
				
				areHandsCalibrated = true;
			}
			else
				useControllersAsTargets = false == leftController.GetButton(SixenseButtons.FOUR);
		}
		
		if ( areHandsCalibrated )
		{
			leftHand.UpdateIK( useControllersAsTargets );
		    rightHand.UpdateIK( useControllersAsTargets );
		}
	}

}

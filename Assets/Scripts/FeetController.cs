using UnityEngine;
using System.Collections;

public class FeetController : MonoBehaviour
{
	#region fields
	
		public float moveSpeed = 0.2f;
		public bool walkInStraightLine = false;
	
		Transform avatarTransform;
		CharacterController characterController;
	
		Vector3 lastLeftPos;
		Vector3 lastRightPos;
	
		// When switching legs while walking, there often is an overlap when
		// both triggers are pressed. Use a dead time to prevent misinterpretation
		// of that as turning.
		float turningDeadTime;
	
		enum State
		{
			Standing,
			Turning,
			LeftFootPushes,
			RightFootPushes
		}
		State state = State.Standing;
	
	    SixenseInput.Controller leftController;
	    SixenseInput.Controller rightController;
	
	#endregion
	
	
	
	void ChangeToState( State newState )
	{
		if ( state == newState )
			return;
		
		if ( newState == State.Turning && (state == State.LeftFootPushes || state == State.RightFootPushes ) )
			turningDeadTime = 0.1f;
		else
			turningDeadTime = 0;
		
		state = newState;
	}
	
	
	
	public void Deactivate()
	{
		ChangeToState( State.Standing );
	}
	
	
	
	public void Dispossess()
	{
		avatarTransform = null;
		characterController = null;
	}
	
	
	
	public void InitializeForNewCharacter( Transform avatarTransform )
	{
		this.avatarTransform = avatarTransform;
		characterController = avatarTransform.GetComponent<CharacterController>();
	}
	
	
	
	public void OnAnimatorIK()
	{
		turningDeadTime -= Time.deltaTime;
		
	    leftController = SixenseInput.GetController( SixenseHands.LEFT );
	    rightController = SixenseInput.GetController( SixenseHands.RIGHT );
		
		if ( leftController == null || rightController == null )
		{
			ChangeToState( State.Standing );
			return;
		}
		
		
		bool leftTriggerActive = leftController.GetButton( SixenseButtons.TRIGGER );
		bool rightTriggerActive = rightController.GetButton( SixenseButtons.TRIGGER );
		
		
		if ( leftTriggerActive && rightTriggerActive )
			ChangeToState ( State.Turning );
		else if ( leftTriggerActive )
			ChangeToState ( State.LeftFootPushes );
		else if ( rightTriggerActive )
			ChangeToState( State.RightFootPushes );
		else
			ChangeToState( State.Standing );
		
		
		switch ( state )
		{
			case State.Turning:
				if ( turningDeadTime <= 0 )
					Turn();
				break;
			
			case State.LeftFootPushes:
				PushFootLeft();
				break;
			
			case State.RightFootPushes:
				PushFootRight();
				break;
		}
		
		lastLeftPos = leftController.Position;
		lastRightPos = rightController.Position;
	}
	
	
	
	void PushFootLeft()
	{
		Vector3 leftPos = leftController.Position;
		Vector3 forwardMovement = walkInStraightLine	? Vector3.Project( lastLeftPos - leftPos, Vector3.forward )
														: lastLeftPos - leftPos;
		characterController.SimpleMove(	avatarTransform.rotation *( forwardMovement * moveSpeed ));
	}
	
	
	
	void PushFootRight()
	{
		Vector3 rightPos = rightController.Position;
		Vector3 forwardMovement = walkInStraightLine	? Vector3.Project( lastRightPos - rightPos, Vector3.forward )
														: lastRightPos - rightPos;
		characterController.SimpleMove(	avatarTransform.rotation * ( forwardMovement * moveSpeed ));
	}
	
	
	
	void Turn()
	{
		Vector3 leftPos = leftController.Position;
		Vector3 rightPos = rightController.Position;
		
		Vector3 lastLeftToRight = lastRightPos - lastLeftPos;
		Vector3 currentLeftToRight = rightPos - leftPos;
		
		Quaternion rotation = Quaternion.FromToRotation( lastLeftToRight, currentLeftToRight );
		avatarTransform.Rotate( new Vector3(0, -rotation.eulerAngles.y, 0), Space.Self );
		
	}
	
}

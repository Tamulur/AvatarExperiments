using UnityEngine;
using System.Collections;

public class MotionController : MonoBehaviour
{

	#region fields
	
		const float kForwardSpeedFactor = 1.5f;
		const float kMouseSensitivity = 1f;
	
		Animator animator;
		Transform avatarTransform;
		CharacterController characterController;
		bool footstepPlayedThisCycle;
		float lastFoostepParamValue;
		SoundSource lightFootstepSound;
		SoundSource heavyFootstepSound;
	
	#endregion
	
	
	
	void Awake()
	{
		lightFootstepSound = transform.Find("LightFootstepSound").GetComponent<SoundSource>();
		heavyFootstepSound = transform.Find("HeavyFootstepSound").GetComponent<SoundSource>();
	}
	
	
	
	public void Dispossess()
	{
		animator.SetFloat("speed", 0);
		animator = null;
		avatarTransform = null;
		characterController = null;
		
		enabled = false;
	}
	
	
	
	public void InitializeForNewCharacter( GameObject avatarGO )
	{
		animator = avatarGO.GetComponent<Animator>();
		characterController = avatarGO.GetComponent<CharacterController>();
		avatarTransform = avatarGO.transform;
		
		enabled = true;
	}
	
	

	void Update()
	{
	    SixenseInput.Controller leftController = SixenseInput.GetController(SixenseHands.LEFT);
	    SixenseInput.Controller rightController = SixenseInput.GetController(SixenseHands.RIGHT);
		
		float forwardSpeed = 0;
		float sideSpeed = 0;
		float direction = 0;
		bool isRunning  = false;
		
		//*** running?
		{
			if (rightController != null)
				isRunning = rightController.GetButton( SixenseButtons.BUMPER );
			isRunning = isRunning || Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift );
			animator.SetBool("run", isRunning);
		}
		
		//*** forward speed
		{
			if (leftController != null)
				forwardSpeed = leftController.JoystickY;
			
			if (Input.GetKey(KeyCode.W))
				forwardSpeed = 1;
			else if (Input.GetKey(KeyCode.S))
				forwardSpeed = -1;
			
			forwardSpeed *= kForwardSpeedFactor;
		}
		
		//*** side speed
		{
			if ( leftController != null )
				sideSpeed = leftController.JoystickX;
			
			if (Input.GetKey(KeyCode.A))
				sideSpeed = -1;
			else if (Input.GetKey(KeyCode.D))
				sideSpeed = 1;
		}
		
		//*** direction
		{
			if (rightController != null)
				direction = rightController.JoystickX;
			
			float mouseDiff = Input.GetAxis("Mouse X");
			if ( Mathf.Abs( mouseDiff ) > 0 )
				direction = mouseDiff * kMouseSensitivity;
			
			if (Input.GetKey(KeyCode.Q))
				direction = -1;
			else if (Input.GetKey(KeyCode.E))
				direction = 1;
		}
		
		//*** strafing
		{
			float directionSpeedsSum = Mathf.Abs( forwardSpeed ) + Mathf.Abs( sideSpeed );
			float strafeRatio = ( directionSpeedsSum > Mathf.Epsilon )	? sideSpeed / directionSpeedsSum
																						: 0;
			animator.SetFloat("strafeRatio", strafeRatio);
		}
		

		float totalSpeed = new Vector2( sideSpeed, forwardSpeed).magnitude;
		animator.SetFloat("speed", ((forwardSpeed >= 0) ? 1 : -1) * totalSpeed);
		
		//*** Play footsteps
		{
			if ( totalSpeed > 0.05f )
			{
				float footstepParamValue = animator.GetFloat("footsteps");
				if ( false == footstepPlayedThisCycle )
				{
					if ( lastFoostepParamValue < footstepParamValue && footstepParamValue > 0.6f )
					{
						if ( avatarTransform.localScale.y > 1.2f )
							heavyFootstepSound.Play ();
						else
							lightFootstepSound.Play ();
						footstepPlayedThisCycle = true;
					}
				}

				if ( footstepParamValue < lastFoostepParamValue)
					footstepPlayedThisCycle = false;

				lastFoostepParamValue = footstepParamValue;
			}
			else
			{
				lastFoostepParamValue = 0;
				footstepPlayedThisCycle = false;
			}
		}
		
		float runFactor = isRunning ? 2.5f : 1;
		
		avatarTransform.rotation = avatarTransform.rotation * Quaternion.Euler(0, 100 * direction * Time.deltaTime, 0);
		characterController.SimpleMove(	runFactor * forwardSpeed * avatarTransform.forward +
										runFactor * sideSpeed * avatarTransform.right);
	}
	
	
	
}

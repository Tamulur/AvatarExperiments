using UnityEngine;



class HydraHand
{
    #region fields
	
		public float scale { get; set; }
        public Vector3 baseControllerPosition { get; private set; }
        public Vector3 initialGlobalPosition { get; private set; }
	
		readonly bool isLeftHand;
        readonly AvatarIKGoal goal;
        readonly SixenseHands hand;
	
		CameraAnchor cameraAnchor;
        Transform avatarTransform;
        Animator animator;
		Vector3 initialLocalPosition;
        Quaternion initialLocalRotation;
        Quaternion baseControllerRotationInverse;
	
		float oldScale;

    #endregion


	
	public void Deactivate()
	{
		oldScale = cameraAnchor.avatarScale / avatarTransform.localScale.x;
	}
	
	
	
    public HydraHand (SixenseHands hand )
    {
        this.hand = hand;
		isLeftHand = (hand == SixenseHands.LEFT);
        goal = isLeftHand ? AvatarIKGoal.LeftHand : AvatarIKGoal.RightHand;
    }


	
	public void InitializeForNewCharacter( Animator animator, CameraAnchor cameraAnchor )
	{
		avatarTransform = animator.transform;
		this.animator = animator;
		this.cameraAnchor = cameraAnchor;
		
		if ( oldScale != 0 )
			initialLocalPosition *= cameraAnchor.avatarScale / (avatarTransform.localScale.x * oldScale);
		
 	}
	
	
	
    public void ResetControllerPosition()
    {
        SixenseInput.Controller controller = SixenseInput.GetController(hand);
 		if ( controller == null )
		{
			Debug.LogWarning("Couldn't find controller for " + hand);
			return;
		}
		
       	baseControllerPosition = controller.Position;
        baseControllerRotationInverse = Quaternion.Inverse( controller.Rotation );
    }


	
	public void ResetHandPosition()
	{
		Vector3 shoulderPos = avatarTransform.TransformPoint( isLeftHand	? cameraAnchor.initialLeftShoulderPos
																			: cameraAnchor.initialRightShoulderPos );
		Vector3 ellbowPos = avatarTransform.TransformPoint( isLeftHand	? cameraAnchor.initialLeftEllbowPos
																		: cameraAnchor.initialRightEllbowPos );
		Vector3 handPos = avatarTransform.TransformPoint( isLeftHand	? cameraAnchor.initialLeftHandPos
																		: cameraAnchor.initialRightHandPos );
		
		float upperArmLength = Vector3.Distance( shoulderPos, ellbowPos );
		float lowerArmLength = Vector3.Distance( ellbowPos, handPos );
		
		//*** Calibrate with angled arms
		{
			Vector3 right = avatarTransform.right;
			Vector3 forward = avatarTransform.forward;
			initialGlobalPosition = shoulderPos +
									( isLeftHand	? -right
													: right ) * upperArmLength
									+ forward * lowerArmLength;
			initialLocalRotation = ( isLeftHand 	? Quaternion.Euler( 0, 0, 90 )
													: Quaternion.Euler(0, 0, -90));
		}
		
		initialLocalPosition = avatarTransform.InverseTransformPoint( initialGlobalPosition );
	}
	
	
	
    public void UpdateIK( bool useController = true )
    {
		Vector3 targetPos = Vector3.zero;
		Quaternion targetRot = Quaternion.identity;
		
		if ( useController )
		{
	        SixenseInput.Controller controller = SixenseInput.GetController(hand);
			if ( controller == null )
			{
				Debug.LogWarning("Couldn't find controller for " + hand);
				return;
			}
	
	        Vector3 vDeltaControllerPos = scale * (controller.Position - baseControllerPosition);
	        targetPos = avatarTransform.TransformPoint((initialLocalPosition + vDeltaControllerPos) / avatarTransform.localScale.x);
	
	        Quaternion diffRot = controller.Rotation * baseControllerRotationInverse;
	        targetRot = avatarTransform.rotation * diffRot * initialLocalRotation;
		}
		else
		{
			targetPos = avatarTransform.TransformPoint(initialLocalPosition / avatarTransform.localScale.x);
			targetRot = avatarTransform.rotation * initialLocalRotation;
		}
		
        animator.SetIKPositionWeight(goal, 1);
        animator.SetIKPosition(goal, targetPos);
		
        animator.SetIKRotationWeight(goal, 1);
        animator.SetIKRotation(goal, targetRot);
  }
}

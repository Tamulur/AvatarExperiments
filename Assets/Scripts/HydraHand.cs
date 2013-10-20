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
	
        Transform avatarTransform;
        Animator animator;
		Vector3 initialLocalPosition;
        Quaternion initialLocalRotation;
        Quaternion baseControllerRotationInverse;
	
		float oldScale;

    #endregion


	
	public void Deactivate()
	{
		oldScale = animator.humanScale;
	}
	
	
	
    public HydraHand (SixenseHands hand )
    {
        this.hand = hand;
		isLeftHand = (hand == SixenseHands.LEFT);
        goal = isLeftHand ? AvatarIKGoal.LeftHand : AvatarIKGoal.RightHand;
    }


	
	public void InitializeForNewCharacter( Transform avatarTransform, Animator animator)
	{
		if ( oldScale != 0 )
			initialLocalPosition *= animator.humanScale / oldScale;
		
        this.animator = animator;
		this.avatarTransform = avatarTransform;
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
		Vector3 shoulderPos = animator.GetBoneTransform( isLeftHand	? HumanBodyBones.LeftUpperArm
																	: HumanBodyBones.RightUpperArm ).position;
		Vector3 ellbowPos = animator.GetBoneTransform( isLeftHand	? HumanBodyBones.LeftLowerArm
																	: HumanBodyBones.RightLowerArm ).position;
		Vector3 handPos = animator.GetBoneTransform( isLeftHand	? HumanBodyBones.LeftHand
																: HumanBodyBones.RightHand ).position;
		
		float upperArmLength = Vector3.Distance( shoulderPos, ellbowPos );
		float lowerArmLength = Vector3.Distance( ellbowPos, handPos );
		
		//*** Calibrate with angled arms
		{
			initialGlobalPosition = shoulderPos +
									( isLeftHand	? -avatarTransform.right
													: avatarTransform.right ) * upperArmLength
									+ avatarTransform.forward * lowerArmLength;
			initialLocalRotation = ( isLeftHand 	? Quaternion.Euler( 0, 0, 90 )
													: Quaternion.Euler(0, 0, -90));
		}
		
		//*** Calibrate with T-pose: arms stretched to the side
		// Seems to be worse than angled arms, maybe because the
		// controllers are too far away from the base.
		{
	//			initialGlobalPosition = shoulderPos +
	//									(hand == SixenseHands.LEFT	? -avatarTransform.right
	//																: avatarTransform.right ) * (lowerArmLength + upperArmLength);
	//			initialLocalRotation = (hand == SixenseHands.LEFT 	? Quaternion.Euler( 0, -90, 90 )
	//																: Quaternion.Euler(0, 90, -90));
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

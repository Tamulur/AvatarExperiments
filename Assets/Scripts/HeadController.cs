using UnityEngine;
using System.Collections;

public class HeadController : MonoBehaviour
{
    #region fields
    	
		public bool useNeckBone = true;
	
		Transform ovrControllerTransform;
		Transform leftCameraTransform;
		Transform rightCameraTransform;
		Transform anchorBoneTransform;
	
		Quaternion neckCorrectionRotation;
		OVRCameraController ovrCameraController;
	
    #endregion



    void Awake()
    {
		ovrCameraController = GetComponentInChildren<OVRCameraController>();
		ovrCameraController.FollowOrientation = transform;
		ovrControllerTransform = ovrCameraController.transform;
		leftCameraTransform = ovrControllerTransform.Find("CameraLeft");
		rightCameraTransform = ovrControllerTransform.Find("CameraRight");
	}

	
	
	public void Dispossess()
	{
		ovrControllerTransform.parent = transform;
		anchorBoneTransform = null;
		enabled = false;
	}
	
	
	
	public Vector3 GetViewDirection()
	{
		return (leftCameraTransform.forward + rightCameraTransform.forward).normalized;
	}
	
	
	
	public Vector3 GetViewOrigin()
	{
		return 0.5f * (leftCameraTransform.position + rightCameraTransform.position);
	}
	
	
	
	public void InitializeForNewCharacter( Transform avatarTransform )
	{
		CameraAnchor cameraAnchor = MiscUtils.FindInChildren( transform.parent.gameObject, "CameraAnchor" ).GetComponent<CameraAnchor>();
		Transform cameraAnchorTransform = cameraAnchor.transform;
		anchorBoneTransform = cameraAnchor.GetAnchorTransform();
		
		//*** Let the OVRCameraController pivot in a position such that there is no clipping when looking down
		{
			Vector3 shoulderPosLocal = cameraAnchor.GetAverageInitialShoulderPos();
			Vector3 headPosLocal = cameraAnchor.initialHeadPos;
			float pivotHeightLocal = Mathf.Lerp( shoulderPosLocal.y, headPosLocal.y, 0.2f );

			ovrControllerTransform.parent = avatarTransform;
			ovrControllerTransform.localPosition = Vector3.up * pivotHeightLocal;
			ovrControllerTransform.localRotation = Quaternion.identity;
			ovrCameraController.SetNeckPosition( new Vector3 ( 0, 0, 0 ) );
			float eyeCenterY = cameraAnchorTransform.position.y - ovrControllerTransform.position.y;
			float eyeCenterZ = cameraAnchorTransform.localPosition.z * avatarTransform.localScale.z;
			ovrCameraController.SetEyeCenterPosition( new Vector3 ( 0, eyeCenterY, eyeCenterZ ) );
		}
		
		//*** Set neckCorrectionRotation to compensate for the neck sometimes being rotated relative to the body
		{
			Vector3 neckRelativeToCharEuler = (Quaternion.Inverse( transform.rotation) * anchorBoneTransform.rotation).eulerAngles;
			float normX = MiscUtils.NormalizedDegAngle( neckRelativeToCharEuler.x );
			float normY = MiscUtils.NormalizedDegAngle( neckRelativeToCharEuler.y );
			float normZ = MiscUtils.NormalizedDegAngle( neckRelativeToCharEuler.z );
			float x=0, y=0, z=0;
			if (normX > 75)
				x = 90;
			else if (normX < -75)
				x = -90;
			
			if (normY > 75)
				y = 90;
			else if (normY < -75)
				y = -90;
			
			if (normZ > 75)
				z = 90;
			else if (normZ < -75)
				z = -90;
			
			neckCorrectionRotation = Quaternion.Euler( x, y, z );
		}
		
		enabled = true;
	}
	
	
	
	void LateUpdate()
	{
		Quaternion averageRotation = Quaternion.Slerp( leftCameraTransform.rotation, rightCameraTransform.rotation, 0.5f );
		anchorBoneTransform.rotation = averageRotation * neckCorrectionRotation;
	}
	
	
}

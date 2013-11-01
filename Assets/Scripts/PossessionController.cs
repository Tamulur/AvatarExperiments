using UnityEngine;
using System.Collections.Generic;

public class PossessionController : MonoBehaviour
{
	#region fields
		const float kChangeCharacterDuration = 0.2f;
	
		bool isGhostModeActive;
		bool isControllerGhostKeyActive;
		bool isMouseGhostKeyActive;
		bool isKeyboardGhostKeyActive;
	
		GameObject characterInGhostFocus;
	
		HandsController handsController;
		FeetController feetController;
		HeadController headController;
		MotionController motionController;
		OVRCameraController ovrCameraController;
		Transform ovrCameraControllerTransform;
		Transform avatarTransform;
		AudioSource swooshSound;
	
		List<Transform> characterCameraAnchorTransforms = new List<Transform>();
		TriggerSolidColor[] materialTriggers;
		HashSet<TriggerSolidColor> ownTriggerColors = new HashSet<TriggerSolidColor>();
	
		enum HydraMode
		{
			Hands,
			Feet
		}
		HydraMode hydraMode = HydraMode.Hands;
	
		enum State {
			InCharacter,
			ChangingCharacter
		}
		State state = State.InCharacter;
	#endregion
	
	
	
	void Awake()
	{
		ovrCameraController = GetComponentInChildren<OVRCameraController>();
		ovrCameraControllerTransform = ovrCameraController.transform;
		headController = GetComponent<HeadController>();
		handsController = GetComponent<HandsController>();
		feetController = GetComponent<FeetController>();
		motionController = GetComponent<MotionController>();
		swooshSound = ovrCameraControllerTransform.Find("SwooshSound").GetComponent<AudioSource>();
		
		materialTriggers = GameObject.FindObjectsOfType( typeof (TriggerSolidColor ) ) as TriggerSolidColor[];
		foreach ( CameraAnchor cameraAnchor in GameObject.FindObjectsOfType( typeof(CameraAnchor) ))
			characterCameraAnchorTransforms.Add( cameraAnchor.transform );
	}
	
	
	
	void CheckGhostModeButtons()
	{
		//*** Controller
		{
			SixenseInput.Controller rightController = SixenseInput.GetController( SixenseHands.RIGHT );
			if ( rightController != null )
			{
				if ( rightController.GetButtonDown( SixenseButtons.FOUR ) && false == isGhostModeActive )
				{
					isControllerGhostKeyActive = true;
					EnterGhostMode();
				}
				else if ( rightController.GetButtonUp( SixenseButtons.FOUR ) && isControllerGhostKeyActive )
					isControllerGhostKeyActive = false;
			}
		}
		
		//*** Mouse
		{
			if ( Input.GetMouseButtonDown(1) && false == isGhostModeActive )
			{
				isMouseGhostKeyActive = true;
				EnterGhostMode();
			}
			else if ( Input.GetMouseButtonUp(1) && isMouseGhostKeyActive )
				isMouseGhostKeyActive = false;
		}
		
		//*** Keyboard
		{
			if ( Input.GetKeyDown(KeyCode.R) && false == isGhostModeActive )
			{
				isKeyboardGhostKeyActive = true;
				EnterGhostMode();
			}
			else if ( Input.GetKeyUp(KeyCode.R) && isKeyboardGhostKeyActive )
				isKeyboardGhostKeyActive = false;
		}
		
		if ( isGhostModeActive && false == IsGhostModeKeyPressed() )
		{
			UpdateCharacterFocus();
			if ( characterInGhostFocus != null )
				SwooshToCharacterInFocus();
			else
				ExitGhostMode();
		}
	}
	
	
	
	void CheckHydraModeButtons()
	{
		SixenseInput.Controller leftController = SixenseInput.GetController( SixenseHands.LEFT );
		if ( leftController != null )
		{
			if (leftController.GetButtonDown( SixenseButtons.ONE ))
			{
				switch ( hydraMode )
				{
					case HydraMode.Hands:
						hydraMode = HydraMode.Feet;
						break;
					case HydraMode.Feet:
						feetController.Deactivate();
						hydraMode = HydraMode.Hands;
						break;
				}
			}
		}
	}
	
	
		
	void Dispossess()
	{
		avatarTransform.gameObject.layer = 8;
		transform.parent = null;
		ovrCameraController.FollowOrientation = ovrCameraControllerTransform;
		
		Destroy( avatarTransform.GetComponent<IKCaller>() );
		
		headController.Dispossess();
		handsController.Dispossess();
		feetController.Dispossess();
		motionController.Dispossess();
		
		ownTriggerColors.Clear();
	}
	
	
	
	void EnterGhostMode()
	{
		foreach ( TriggerSolidColor triggerMaterial in materialTriggers )
			if ( false == ownTriggerColors.Contains( triggerMaterial ) )
				triggerMaterial.MakeSolid();
		
		Singletons.timeManager.WarpTimeIn();
		
		characterInGhostFocus = null;
		isGhostModeActive = true;
	}
	
	
	
	void ExitGhostMode()
	{
		foreach ( TriggerSolidColor triggerMaterial in materialTriggers )
			triggerMaterial.RestoreOriginalTexture();
		
		Singletons.timeManager.WarpTimeOut();
		isGhostModeActive = false;
	}
	
	
	
	bool IsGhostModeKeyPressed()
	{
		return isControllerGhostKeyActive || isMouseGhostKeyActive || isKeyboardGhostKeyActive;
	}
	
	
	
	void OnAnimatorIK()
	{
		if ( hydraMode == HydraMode.Hands )
			handsController.OnAnimatorIK();
		else if ( hydraMode == HydraMode.Feet )
			feetController.OnAnimatorIK();
	}
	
	
	
	void Possess( GameObject avatarGO )
	{
		state = State.InCharacter;
		
		// Put own char on a different layer so we don't raycast
		// our own collider in UpdateCharacterFocus
		avatarGO.layer = 9;
		
		avatarTransform = avatarGO.transform;
		transform.parent = avatarTransform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		
		avatarTransform.gameObject.AddComponent<IKCaller>().delegateOnAnimatorIK = OnAnimatorIK;
		
		headController.InitializeForNewCharacter( avatarTransform );
		handsController.InitializeForNewCharacter();
		feetController.InitializeForNewCharacter( avatarTransform );
		motionController.InitializeForNewCharacter ( avatarGO );
		
		foreach ( TriggerSolidColor triggerSolidColor in avatarTransform.GetComponentsInChildren<TriggerSolidColor>() )
			ownTriggerColors.Add( triggerSolidColor );
	}
	
	
	
	void Start()
	{
		Possess( transform.parent.gameObject );
	}
	
	
	
	void SwooshToCharacterInFocus()
	{
		Dispossess();
		
		state = State.ChangingCharacter;
		
		swooshSound.Play();
		Transform targetCameraAnchor = MiscUtils.FindInChildren(characterInGhostFocus, "CameraAnchor").transform;
		Go.to(	ovrCameraControllerTransform,
				kChangeCharacterDuration,
				new GoTweenConfig()	.position( targetCameraAnchor.position )
									.rotation( targetCameraAnchor.rotation )
									.setEaseType( GoEaseType.SineInOut )
									.onComplete( t => { Possess( characterInGhostFocus );
														ExitGhostMode(); }));
	}
	
	
	
	void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Escape ) )
			Application.Quit();
		
		if ( state == State.ChangingCharacter )
			return;
		
		if ( isGhostModeActive )
			UpdateCharacterFocus();
		
		CheckHydraModeButtons();
		CheckGhostModeButtons();
	}
	
	
	
	void UpdateCharacterFocus()
	{
		// In ghost mode all characters are solid orange, except the character in
		// center of view, which we would warp into when exiting ghost mode.
		
		float minAngle = Mathf.Infinity;
		GameObject characterWithLeastAngle = null;
		Vector3 viewDirection = headController.GetViewDirection();
		Vector3 viewOrigin = headController.GetViewOrigin();
		
		foreach ( Transform cameraAnchorTransform in characterCameraAnchorTransforms )
		{
			GameObject characterGO = cameraAnchorTransform.parent.gameObject;
			
			if ( characterGO == avatarTransform.gameObject )
				continue;
			
			Vector3 directionToChar = cameraAnchorTransform.position - viewOrigin;
			float angle = Vector3.Angle( viewDirection, directionToChar );
			bool isCharacterInSight = false;
			
			RaycastHit hit;
			const float kMaxAngleForBeingInSight = 60;
			if ( angle < kMaxAngleForBeingInSight )
			{
				if ( Physics.Linecast( viewOrigin, cameraAnchorTransform.position, out hit, ~(1 << 9)) )
					isCharacterInSight = hit.collider == characterGO.collider;
				else
					isCharacterInSight = true;
			}
			
			if ( isCharacterInSight )
			{
				const float kMaxAngleToGetFocus = 25;
				if ( angle < kMaxAngleToGetFocus && angle < minAngle )
				{
					minAngle = angle;
					characterWithLeastAngle = characterGO;
				}					
			}
			else if ( characterGO == characterInGhostFocus )
			{
				characterInGhostFocus.GetComponent<TriggerSolidColor>().MakeSolid();
				characterInGhostFocus = null;
			}
		}
		
		if ( characterWithLeastAngle != null && characterWithLeastAngle != characterInGhostFocus )
		{
			if (characterInGhostFocus != null )
				characterInGhostFocus.GetComponent<TriggerSolidColor>().MakeSolid();
			characterInGhostFocus = characterWithLeastAngle;
			characterInGhostFocus.GetComponent<TriggerSolidColor>().RestoreOriginalTexture();
		}
	}
	
	
}

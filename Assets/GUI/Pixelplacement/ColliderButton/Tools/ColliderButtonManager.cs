using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ColliderButtonManager : MonoBehaviour {
	
	//-----------------------------------------------------------
	// Private Variables
	//-----------------------------------------------------------
	
	static ColliderButtonManager instance;
	List<ColliderButton> colliderButtons = new List<ColliderButton>();
	List<PressedButton> pressedButtons = new List<PressedButton>();
	
	class PressedButton{
		public ColliderButton colliderButton;
		public int touchedID;
		
		public PressedButton( ColliderButton colliderButton, int touchedID ){
			this.colliderButton = colliderButton;
			this.touchedID = touchedID;
		}
	}
	
	//-----------------------------------------------------------
	// Init
	//-----------------------------------------------------------
	
	void Awake(){
		StartCoroutine( "LookForInteraction" );	
	}
	
	//-----------------------------------------------------------
	// Coroutines
	//-----------------------------------------------------------
	
	IEnumerator LookForInteraction(){
		while (true) {
			List<Camera> filteredCameras = GetFilteredCameraList();
			List<PressedButton> releasedButtons = new List<PressedButton>();
			
			//touch checking:
			if ( Input.touchCount > 0 ) {
				foreach( Touch currentTouch in Input.touches ){
					
					//press:
					if( currentTouch.phase == TouchPhase.Began ){
						foreach ( Camera currentFilteredCamera in filteredCameras ){
							//if the rendering camera is disabled we won't check for any interactions:
							//if ( !currentFilteredCamera.enabled || !currentFilteredCamera.gameObject.active ) {
							if( !currentFilteredCamera.enabled || !currentFilteredCamera.gameObject.activeInHierarchy ) {
								break;
							}
							//look for interactions:
							Ray ray = currentFilteredCamera.ScreenPointToRay( currentTouch.position );
							RaycastHit hit;
							if( Physics.Raycast( ray, out hit, currentFilteredCamera.farClipPlane ) ){	
								foreach ( ColliderButton currentColliderButton in colliderButtons ){
									if ( currentColliderButton.useEvent && currentColliderButton.GetComponent<Collider>() == hit.collider ) {
										if ( currentColliderButton.debug ) {
											Debug.Log( currentColliderButton.name + " was PRESSED at " + Time.realtimeSinceStartup + "!" );
										}
										currentColliderButton.FirePressedEvent();
										
										//catalog this pressed collider button for use with release activities:
										PressedButton currentPressedColliderButton = new PressedButton( currentColliderButton, currentTouch.fingerId );
										pressedButtons.Add( currentPressedColliderButton );
										
										currentColliderButton.methodFire.Fire();
									}
								}
							}
						}
					}
					
					//release:
					if ( currentTouch.phase == TouchPhase.Ended || currentTouch.phase == TouchPhase.Canceled ) {
						
						//catalog buttons that are now released:
						foreach ( PressedButton item in pressedButtons ) {
							if ( item.touchedID == currentTouch.fingerId ) {
								releasedButtons.Add ( item );
							}
						}
						
						//final clean up to ensure we don't have any stuck pressed buttons from notoriously error-prone touch reporting due to loop frequencies and the real world:
						foreach ( PressedButton item in pressedButtons ) {
							if ( item.touchedID > Input.touchCount - 1 ) {
								releasedButtons.Add( item );
							}
						}
						
						//drop catalog of released buttons:
						foreach (PressedButton item in releasedButtons) {
							item.colliderButton.FireReleasedEvent();
							if ( item.colliderButton.debug ) {
								Debug.Log( item.colliderButton.name + " was RELEASED at " + Time.realtimeSinceStartup + "!" );
							}
							pressedButtons.Remove( item );
						}		
					}
				}
			}else{		
				//mouse checking:
				
				//press:
				if ( Input.GetMouseButtonDown( 0 )  ) {
					foreach ( Camera currentFilteredCamera in filteredCameras ){
						//if the rendering camera is disabled we won't check for any interactions:
						//if ( !currentFilteredCamera.enabled || !currentFilteredCamera.gameObject.active ) {
						if ( !currentFilteredCamera.enabled || !currentFilteredCamera.gameObject.activeInHierarchy ) {
							break;
						}
						//look for interactions:
						Ray ray = currentFilteredCamera.ScreenPointToRay( Input.mousePosition );
						RaycastHit hit;
						if( Physics.Raycast( ray, out hit, currentFilteredCamera.farClipPlane ) ){	
							foreach ( ColliderButton currentColliderButton in colliderButtons ){
								if ( currentColliderButton.useEvent && currentColliderButton.GetComponent<Collider>() == hit.collider  ) {
									if ( currentColliderButton.debug ) {
										Debug.Log( currentColliderButton.name + " was PRESSED at " + Time.realtimeSinceStartup + "!" );
									}
									currentColliderButton.FirePressedEvent();
									
									//catalog this pressed collider button for use with release activities:
									PressedButton currentPressedColliderButton = new PressedButton( currentColliderButton, 0 );
									pressedButtons.Add( currentPressedColliderButton );
									
									currentColliderButton.methodFire.Fire();
								}
							}
						}
					}
				}	
				
				//release:
				if ( Input.GetMouseButtonUp( 0 ) ) {
					//catalog buttons that are now released:
					foreach ( PressedButton item in pressedButtons ) {
						if ( item.touchedID == 0 ) {
							releasedButtons.Add ( item );
						}
					}
				}
				
				//drop catalog of released buttons:
				foreach (PressedButton item in releasedButtons) {
					item.colliderButton.FireReleasedEvent();
					if ( item.colliderButton.debug ) {
						Debug.Log( item.colliderButton.name + " was RELEASED at " + Time.realtimeSinceStartup + "!" );
					}
					pressedButtons.Remove( item );
				}	
			}
			
			yield return null;
		}
	}
	
	//-----------------------------------------------------------
	// Public Methods
	//-----------------------------------------------------------
	
	public static void Register( ColliderButton colliderButton ){
		if ( instance == null ) {
			GameObject go = new GameObject( "ColliderButtonManager" );
			instance = go.AddComponent<ColliderButtonManager>();
		}
		instance.colliderButtons.Add( colliderButton );
	}
	
	public static void UnRegister( ColliderButton colliderButton ){
		if ( instance != null ) {
			instance.colliderButtons.Remove( colliderButton );
		}
	}
	
	//-----------------------------------------------------------
	// Private Methods
	//-----------------------------------------------------------
	
	List<Camera> GetFilteredCameraList(){
		List<Camera> filteredCameras = new List<Camera>();
		foreach ( ColliderButton item in colliderButtons ) {
			if ( !filteredCameras.Contains( item.renderingCamera ) ) {
				filteredCameras.Add( item.renderingCamera );
			}
		}	
		return filteredCameras;
	}
}
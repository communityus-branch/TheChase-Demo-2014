using UnityEngine;

[AddComponentMenu("Pixelplacement/ColliderButton")]
[RequireComponent(typeof(MethodFire))]
public class ColliderButton : MonoBehaviour {
	
	//-----------------------------------------------------------
	// Events
	//-----------------------------------------------------------
	
	public event System.Action<ColliderButton> OnPress;
	public event System.Action<ColliderButton> OnRelease;
	
	//-----------------------------------------------------------
	// Public Variables
	//-----------------------------------------------------------
	
	public bool findRenderingCamera = true;
	public Camera renderingCamera;
	public bool useEvent = true;
	public bool debug;
	public bool drawConnections = true;
	public bool hasTouch;
	[HideInInspector]
	public MethodFire methodFire;
	
	//-----------------------------------------------------------
	// Init
	//-----------------------------------------------------------
	
	void Awake(){
		methodFire = GetComponent<MethodFire>();
		
		//a collider is mandatory:
		if ( GetComponent<Collider>() == null ) {
			gameObject.AddComponent<BoxCollider>();
		}
		
		//default to main camera if a rendering camera is not specified:
		if ( renderingCamera == null ) {
			foreach ( Camera item in Camera.allCameras ) {
				if ( ( ((LayerMask)item.cullingMask) & 1 << gameObject.layer ) != 0 ) {
					renderingCamera = item;
					break;
				}
			}
			
		}
		
		//if no camera still has been set or found lets just default to the main camera to ensure something is tracked:
		if ( renderingCamera == null ) {
			renderingCamera = Camera.main;
		}
	}	
	
	//-----------------------------------------------------------
	// Event Registration
	//-----------------------------------------------------------
	
	void OnEnable(){
		ColliderButtonManager.Register( this );
	}
	
	void OnDisable(){
		ColliderButtonManager.UnRegister( this );	
	}
	
	//-----------------------------------------------------------
	// Public Methods
	//-----------------------------------------------------------
	
	public void FirePressedEvent(){
		if ( OnPress != null ) OnPress(this);
	}
	
	public void FireReleasedEvent(){
		if ( OnRelease != null ) OnRelease(this);
	}
	
	//-----------------------------------------------------------
	// Gizmos
	//-----------------------------------------------------------
	
	void OnDrawGizmos(){
		//draw a line in the scene view to show sender and receiver connections:
		if ( !drawConnections || methodFire == null || methodFire.target == null ) {
			return;
		}
		Transform cachedTransform = transform;
		Transform cachedTargetTransform = methodFire.target.transform;
		Gizmos.color = Color.green;
		Gizmos.DrawLine( cachedTransform.position, Vector3.Lerp( cachedTransform.position, cachedTargetTransform.position, .25f ) );
		
		Gizmos.color = Color.white;
		Gizmos.DrawLine( Vector3.Lerp( cachedTransform.position, cachedTargetTransform.position, .25f ), Vector3.Lerp( cachedTargetTransform.position, cachedTransform.position, .25f ) );
		
		Gizmos.color = Color.red;
		Gizmos.DrawLine( cachedTargetTransform.position, Vector3.Lerp( cachedTargetTransform.position, cachedTransform.position, .25f ) );	

	}
}
class RenderCubemapWizard extends ScriptableWizard {
    var renderFromPosition : Transform;
    var cubemap : Cubemap;
    var go : Camera;
    
    function OnWizardUpdate () {
        helpString = "Select transform to render from and cubemap to render into";
        isValid = (renderFromPosition != null) && (cubemap != null);
    }
    
    function OnWizardCreate () {
        // create temporary camera for rendering
 
        
        // var go = new GameObject( "CubemapCamera", Camera );
        // go.camera.CopyFrom( Camera.main );
		        
        // place it on the object
        go.transform.position = renderFromPosition.position;
        go.transform.rotation = Quaternion.identity;

        // render into cubemap        
        go.GetComponent.<Camera>().RenderToCubemap( cubemap );
        
        // destroy temporary camera
        //DestroyImmediate( go );
        
        Debug.Log (cubemap);
    }
    
    @MenuItem("Custom/Render into Cubemap")
    static function RenderCubemap () {
        ScriptableWizard.DisplayWizard.<RenderCubemapWizard>(
            "Render cubemap", "Render!");
    }
}
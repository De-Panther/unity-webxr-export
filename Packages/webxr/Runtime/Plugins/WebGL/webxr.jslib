/* functions called from unity */
var LibraryWebXR = {
  SetWebXRSettings: function(strJson) {
    Module.WebXR.Settings = JSON.parse(UTF8ToString(strJson));
    console.log(Module.WebXR.Settings);
  },

  SetWebXREvents: function(onStartARPtr,
      onStartVRPtr, onVisibilityChangePtr, onEndXRPtr,
      onXRCapabilitiesPtr, onInputProfilesPtr) {
    Module.WebXR.onStartARPtr = onStartARPtr;
    Module.WebXR.onStartVRPtr = onStartVRPtr;
    Module.WebXR.onVisibilityChangePtr = onVisibilityChangePtr;
    Module.WebXR.onEndXRPtr = onEndXRPtr;
    Module.WebXR.onXRCapabilitiesPtr = onXRCapabilitiesPtr;
    Module.WebXR.onInputProfilesPtr = onInputProfilesPtr;
  },

  InitXRSharedArray: function(byteOffset) {
    Module.XRSharedArrayOffset = byteOffset / 4;
    Module.WebXR.onUnityLoaded({detail: {state: 'Ready', module: Module}});
  },

  InitControllersArray: function(byteOffset) {
    Module.ControllersArrayOffset = byteOffset / 4;
  },

  InitHandsArray: function(byteOffset) {
    Module.HandsArrayOffset = byteOffset / 4;
  },

  InitAnchorsArray: function(byteOffset) {
    Module.AnchorsArrayOffset = byteOffset / 4;
  },
  
  SetWebXRAnchorEvents: function(onAnchorCreatedPtr, onAnchorDeletedPtr) {
    Module.WebXR.onAnchorCreatedPtr = onAnchorCreatedPtr;
    Module.WebXR.onAnchorDeletedPtr = onAnchorDeletedPtr;
  },
  
  CreateAnchorFromViewerHitTest: function() {
    Module.WebXR.createAnchorFromViewerHitTest();
  },
  
  CreateAnchorFromPose: function(px, py, pz, qx, qy, qz, qw) {
    Module.WebXR.createAnchorFromPose(px, py, pz, qx, qy, qz, qw);
  },
  
  DeleteAnchor: function(anchorId) {
    Module.WebXR.deleteAnchor(anchorId);
  },
  
  DeleteAllAnchors: function() {
    Module.WebXR.deleteAllAnchors();
  },

  InitViewerHitTestPoseArray: function(byteOffset) {
    Module.ViewerHitTestPoseArrayOffset = byteOffset / 4;
  },

  WebXRGetViewsDataArray: function () {
    return Module.XRSharedArrayOffset * 4;
  },

  ToggleAR: function() {
    Module.WebXR.toggleAR();
  },

  ToggleVR: function() {
    Module.WebXR.toggleVR();
  },

  ToggleViewerHitTest: function() {
    Module.WebXR.toggleHitTest();
  },

  ControllerPulse: function(controller, intensity, duration) {
    Module.WebXR.callHapticPulse({detail: {'controller' : controller, 'intensity' : intensity, 'duration': duration}});
  },

  PreRenderSpectatorCamera: function() {
    Module.WebXR.startRenderSpectatorCamera();
  },
}

mergeInto(LibraryManager.library, LibraryWebXR);
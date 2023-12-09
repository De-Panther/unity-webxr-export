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

  WebXRInitDisplayRender: function() {
    console.log("WebXRInitDisplayRender");
    var xrFramebuffer = Module.WebXR.xrSession.renderState.baseLayer.framebuffer;
    var bufferId = GL.getNewId(GL.framebuffers);
    if (xrFramebuffer != null) {
      xrFramebuffer["name"] = bufferId;
      GL.framebuffers[bufferId] = xrFramebuffer;
    }
    return bufferId;
  },

  WebXRDestructDisplayRender: function(bufferId) {
    console.log("WebXRDestructDisplayRender");
    GL.framebuffers[bufferId] = null;
  },
}

mergeInto(LibraryManager.library, LibraryWebXR);
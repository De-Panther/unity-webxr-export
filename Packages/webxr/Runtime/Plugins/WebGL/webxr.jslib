/* functions called from unity */
mergeInto(LibraryManager.library, {
  SetWebXRSettings: function(strJson) {
    Module.WebXR.Settings = JSON.parse(Pointer_stringify(strJson));
    console.log(Module.WebXR.Settings);
  },

  InitXRSharedArray: function(byteOffset, length) {
    Module.XRSharedArrayOffset = byteOffset;
    Module.XRSharedArrayLength= length;
    Module.XRSharedArray = new Float32Array(buffer, byteOffset, length);
    Module.WebXR.onUnityLoaded({detail: {state: 'Ready', module: Module}});
  },

  InitControllersArray: function(byteOffset, length) {
    Module.ControllersArrayOffset = byteOffset;
    Module.ControllersArrayLength= length;
    Module.ControllersArray = new Float32Array(buffer, byteOffset, length);
  },

  InitHandsArray: function(byteOffset, length) {
    Module.HandsArrayOffset = byteOffset;
    Module.HandsArrayLength= length;
    Module.HandsArray = new Float32Array(buffer, byteOffset, length);
  },

  InitViewerHitTestPoseArray: function(byteOffset, length) {
    Module.ViewerHitTestPoseArrayOffset = byteOffset;
    Module.ViewerHitTestPoseArrayLength= length;
    Module.ViewerHitTestPoseArray = new Float32Array(buffer, byteOffset, length);
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
});
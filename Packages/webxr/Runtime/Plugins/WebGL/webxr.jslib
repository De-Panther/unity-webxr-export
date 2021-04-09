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
    document.dispatchEvent(new CustomEvent('UnityLoaded', {detail: {state: 'Ready', module: Module}}));
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
    document.dispatchEvent(new CustomEvent('toggleAR', {}));
  },

  ToggleVR: function() {
    document.dispatchEvent(new CustomEvent('toggleVR', {}));
  },

  ToggleViewerHitTest: function() {
    document.dispatchEvent(new CustomEvent('toggleHitTest', {}));
  },

  ControllerPulse: function(controller, intensity, duration) {
    document.dispatchEvent(new CustomEvent('callHapticPulse', {detail: {'controller' : controller, 'intensity' : intensity, 'duration': duration}}));
  },
});
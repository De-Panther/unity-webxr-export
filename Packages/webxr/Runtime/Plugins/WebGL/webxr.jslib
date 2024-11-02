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
    Module.dynCall_v = Module.dynCall_v || function (cb) {
      return getWasmTableEntry(cb)();
    };
    Module.dynCall_vi = Module.dynCall_vi || function (cb, arg1) {
      return getWasmTableEntry(cb)(arg1);
    };
    Module.dynCall_vii = Module.dynCall_vii || function (cb, arg1, arg2) {
      return getWasmTableEntry(cb)(arg1, arg2);
    };
    Module.dynCall_viffffffff = Module.dynCall_viffffffff || function (cb, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) {
      return getWasmTableEntry(cb)(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
    };
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
}

mergeInto(LibraryManager.library, LibraryWebXR);
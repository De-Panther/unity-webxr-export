/* functions called from unity */
mergeInto(LibraryManager.library, {
  InitXRSharedArray: function(byteOffset, length) {
    Module.XRSharedArrayOffset = byteOffset;
    Module.XRSharedArrayLength= length;
    Module.XRSharedArray = new Float32Array(buffer, byteOffset, length);
    document.dispatchEvent(new CustomEvent('UnityLoaded', {detail: 'Ready'}));
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

  ToggleViewerHitTest: function() {
    document.dispatchEvent(new CustomEvent('toggleHitTest', {}));
  },

  ControllerPulse: function(controller, intensity, duration) {
    document.dispatchEvent(new CustomEvent('callHapticPulse', {detail: {'controller' : controller, 'intensity' : intensity, 'duration': duration}}));
  },

  ListenWebXRData: function() {
    // Listen for headset updates from webxr.jspre and load data into shared Array which we pick up in Unity.
    document.addEventListener('XRData', function(evt) {
      var data = evt.detail;
      var index = 0;
      if (Module.XRSharedArray.byteLength == 0) {
        Module.XRSharedArray = new Float32Array(buffer, Module.XRSharedArrayOffset, Module.XRSharedArrayLength);
      }
      Object.keys(data).forEach(function (key, i) {
        var dataLength = data[key].length;
        if (dataLength) {
          for (var x = 0; x < dataLength; x++) {
            Module.XRSharedArray[index++] = data[key][x];
          }
        }
      });
    });
    document.addEventListener('XRControllersData', function(evt) {
      var data = evt.detail;
      var index = 0;
      if (Module.ControllersArray.byteLength == 0) {
        Module.ControllersArray = new Float32Array(buffer, Module.ControllersArrayOffset, Module.ControllersArrayLength);
      }
      Object.keys(data).forEach(function (key, i) {
        Module.ControllersArray[index++] = data[key].frame;
        Module.ControllersArray[index++] = data[key].enabled;
        Module.ControllersArray[index++] = data[key].hand;
        Module.ControllersArray[index++] = data[key].positionX;
        Module.ControllersArray[index++] = data[key].positionY;
        Module.ControllersArray[index++] = data[key].positionZ;
        Module.ControllersArray[index++] = data[key].rotationX;
        Module.ControllersArray[index++] = data[key].rotationY;
        Module.ControllersArray[index++] = data[key].rotationZ;
        Module.ControllersArray[index++] = data[key].rotationW;
        Module.ControllersArray[index++] = data[key].trigger;
        Module.ControllersArray[index++] = data[key].squeeze;
        Module.ControllersArray[index++] = data[key].thumbstick;
        Module.ControllersArray[index++] = data[key].thumbstickX;
        Module.ControllersArray[index++] = data[key].thumbstickY;
        Module.ControllersArray[index++] = data[key].touchpad;
        Module.ControllersArray[index++] = data[key].touchpadX;
        Module.ControllersArray[index++] = data[key].touchpadY;
        Module.ControllersArray[index++] = data[key].buttonA;
        Module.ControllersArray[index++] = data[key].buttonB;
      });
    });
    document.addEventListener('XRHandsData', function(evt) {
      var data = evt.detail;
      var index = 0;
      if (Module.HandsArray.byteLength == 0) {
        Module.HandsArray = new Float32Array(buffer, Module.HandsArrayOffset, Module.HandsArrayLength);
      }
      Object.keys(data).forEach(function (key, i) {
        Module.HandsArray[index++] = data[key].frame;
        Module.HandsArray[index++] = data[key].enabled;
        Module.HandsArray[index++] = data[key].hand;
        Module.HandsArray[index++] = data[key].trigger;
        Module.HandsArray[index++] = data[key].squeeze;
        for (var j = 0; j < 25; j++) {
          Module.HandsArray[index++] = data[key].joints[j].enabled;
          Module.HandsArray[index++] = data[key].joints[j].position[0];
          Module.HandsArray[index++] = data[key].joints[j].position[1];
          Module.HandsArray[index++] = data[key].joints[j].position[2];
          Module.HandsArray[index++] = data[key].joints[j].rotation[0];
          Module.HandsArray[index++] = data[key].joints[j].rotation[1];
          Module.HandsArray[index++] = data[key].joints[j].rotation[2];
          Module.HandsArray[index++] = data[key].joints[j].rotation[3];
          Module.HandsArray[index++] = data[key].joints[j].radius;
        }
      });
    });
    document.addEventListener('XRViewerHitTestPose', function(evt) {
      var data = evt.detail;
      var index = 0;
      if (Module.ViewerHitTestPoseArray.byteLength == 0) {
        Module.ViewerHitTestPoseArray = new Float32Array(buffer, Module.ViewerHitTestPoseArrayOffset, Module.ViewerHitTestPoseArrayLength);
      }
      Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.frame;
      Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.available;
      Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.position[0];
      Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.position[1];
      Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.position[2];
      Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.rotation[0];
      Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.rotation[1];
      Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.rotation[2];
      Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.rotation[3];
    });
  }
});
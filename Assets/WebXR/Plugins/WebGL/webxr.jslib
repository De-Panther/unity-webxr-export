/* functions called from unity */
mergeInto(LibraryManager.library, {
  InitXRSharedArray: function(byteOffset, length) {
    XRSharedArray = new Float32Array(buffer, byteOffset, length);
    document.dispatchEvent(new CustomEvent('UnityLoaded', {detail: 'Ready'}));
  },

  InitControllersArray: function(byteOffset, length) {
    ControllersArray = new Float32Array(buffer, byteOffset, length);
  },

  InitHandsArray: function(byteOffset, length) {
    HandsArray = new Float32Array(buffer, byteOffset, length);
  },

  InitViewerHitTestPoseArray: function(byteOffset, length) {
    ViewerHitTestPoseArray = new Float32Array(buffer, byteOffset, length);
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
      Object.keys(data).forEach(function (key, i) {
        var dataLength = data[key].length;
        if (dataLength) {
          for (var x = 0; x < dataLength; x++) {
            XRSharedArray[index++] = data[key][x];
          }
        }
      });
    });
    document.addEventListener('XRControllersData', function(evt) {
      var data = evt.detail;
      var index = 0;
      Object.keys(data).forEach(function (key, i) {
        ControllersArray[index++] = data[key].frame;
        ControllersArray[index++] = data[key].enabled;
        ControllersArray[index++] = data[key].hand;
        ControllersArray[index++] = data[key].positionX;
        ControllersArray[index++] = data[key].positionY;
        ControllersArray[index++] = data[key].positionZ;
        ControllersArray[index++] = data[key].rotationX;
        ControllersArray[index++] = data[key].rotationY;
        ControllersArray[index++] = data[key].rotationZ;
        ControllersArray[index++] = data[key].rotationW;
        ControllersArray[index++] = data[key].trigger;
        ControllersArray[index++] = data[key].squeeze;
        ControllersArray[index++] = data[key].thumbstick;
        ControllersArray[index++] = data[key].thumbstickX;
        ControllersArray[index++] = data[key].thumbstickY;
        ControllersArray[index++] = data[key].touchpad;
        ControllersArray[index++] = data[key].touchpadX;
        ControllersArray[index++] = data[key].touchpadY;
        ControllersArray[index++] = data[key].buttonA;
        ControllersArray[index++] = data[key].buttonB;
      });
    });
    document.addEventListener('XRHandsData', function(evt) {
      var data = evt.detail;
      var index = 0;
      Object.keys(data).forEach(function (key, i) {
        HandsArray[index++] = data[key].frame;
        HandsArray[index++] = data[key].enabled;
        HandsArray[index++] = data[key].hand;
        HandsArray[index++] = data[key].trigger;
        HandsArray[index++] = data[key].squeeze;
        for (var j = 0; j < 25; j++) {
          HandsArray[index++] = data[key].joints[j].enabled;
          HandsArray[index++] = data[key].joints[j].position[0];
          HandsArray[index++] = data[key].joints[j].position[1];
          HandsArray[index++] = data[key].joints[j].position[2];
          HandsArray[index++] = data[key].joints[j].rotation[0];
          HandsArray[index++] = data[key].joints[j].rotation[1];
          HandsArray[index++] = data[key].joints[j].rotation[2];
          HandsArray[index++] = data[key].joints[j].rotation[3];
          HandsArray[index++] = data[key].joints[j].radius;
        }
      });
    });
    document.addEventListener('XRViewerHitTestPose', function(evt) {
      var data = evt.detail;
      var index = 0;
      ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.frame;
      ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.available;
      ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.position[0];
      ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.position[1];
      ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.position[2];
      ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.rotation[0];
      ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.rotation[1];
      ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.rotation[2];
      ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.rotation[3];
    });
  }
});
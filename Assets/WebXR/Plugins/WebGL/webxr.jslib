/* functions called from unity */
mergeInto(LibraryManager.library, {
  InitSharedArray: function(byteOffset, length) {
    SharedArray = new Float32Array(buffer, byteOffset, length);
    document.dispatchEvent(new CustomEvent('UnityLoaded', {detail: 'Ready'}));
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
            SharedArray[index++] = data[key][x];
          }
        } else {
          SharedArray[index++] = data[key].enabled;
          SharedArray[index++] = data[key].hand;
          SharedArray[index++] = data[key].positionX;
          SharedArray[index++] = data[key].positionY;
          SharedArray[index++] = data[key].positionZ;
          SharedArray[index++] = data[key].rotationX;
          SharedArray[index++] = data[key].rotationY;
          SharedArray[index++] = data[key].rotationZ;
          SharedArray[index++] = data[key].rotationW;
          SharedArray[index++] = data[key].trigger;
          SharedArray[index++] = data[key].squeeze;
          SharedArray[index++] = data[key].thumbstick;
          SharedArray[index++] = data[key].thumbstickX;
          SharedArray[index++] = data[key].thumbstickY;
          SharedArray[index++] = data[key].touchpad;
          SharedArray[index++] = data[key].touchpadX;
          SharedArray[index++] = data[key].touchpadY;
          SharedArray[index++] = data[key].buttonA;
          SharedArray[index++] = data[key].buttonB;
        }
      });
    });
  }
});
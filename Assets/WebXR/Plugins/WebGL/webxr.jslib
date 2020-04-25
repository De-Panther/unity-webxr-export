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

      Object.keys(data).forEach(function (key, i) {
        var dataLength = data[key].length;
        for (var x = 0; x < dataLength; x++) {
          SharedArray[i * dataLength + x] = data[key][x];
        }
      });
    });
  }
});
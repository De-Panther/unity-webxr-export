/* functions called from unity */
mergeInto(LibraryManager.library, {
  FinishLoading: function() {
    document.dispatchEvent(new CustomEvent('Unity', {detail: 'Ready'}));
  },

  PostRender: function () {
    document.dispatchEvent(new CustomEvent('Unity', {detail: 'PostRender'}));
  },

  ConfigureToggleVRKeyName: function (keyName) {
    document.dispatchEvent(new CustomEvent('Unity', {detail: 'ConfigureToggleVRKeyName:' + Pointer_stringify(keyName)}));
  },

  displayElementId: function (id) {
    document.dispatchEvent(new CustomEvent('Unity', {detail: {type: 'displayElementId', id: Pointer_stringify(id)}}));
  },

  InitSharedArray: function(byteOffset, length) {
    SharedArray = new Float32Array(buffer, byteOffset, length);
  },

  ListenWebVRData: function() {
    // Listen for headset updates from webvr.js and load data into shared Array which we pick up in Unity.
    document.addEventListener('VRData', function(evt) {
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

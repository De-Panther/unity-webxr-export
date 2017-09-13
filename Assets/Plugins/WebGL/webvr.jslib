/* functions called from unity */
mergeInto(LibraryManager.library, {
  FinishLoading: function() {
    document.dispatchEvent(new CustomEvent('Unity', {detail: 'Ready'}));
  },

  TestTimeReturn: function (texture) {
    document.dispatchEvent(new CustomEvent('Unity', {detail: 'Timer'}));
  },

  PostRender: function () {
    document.dispatchEvent(new CustomEvent('Unity', {detail: 'PostRender'}));
  }
});

// mergeInto(LibraryManager.library, {
//   InitJavaScriptSharedArray: function(byteOffset, length) {
//     JavaScriptSharedArray = new Float32Array(buffer, byteOffset, length);
//   },

//   InitJavaScriptSharedArrayButtons: function() {
//     for(var i = 0; i < JavaScriptSharedArray.length; i++) {
//       var button = document.createElement('button');
//       button.index = i;
//       button.innerHTML = i;
//       button.onclick = function() { JavaScriptSharedArrayIncrement(this.index); }
//         document.body.appendChild(button);
//     }
//   }
// });
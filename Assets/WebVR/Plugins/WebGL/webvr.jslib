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
  },

  ConfigureToggleVRKeyName: function (keyName) {
    document.dispatchEvent(new CustomEvent('Unity', {detail: 'ConfigureToggleVRKeyName:' + Pointer_stringify(keyName)}));
  }
});

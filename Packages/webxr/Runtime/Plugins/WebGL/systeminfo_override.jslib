var LibrarySystemInfoWebGLOverride = {
  JS_SystemInfo_GetCanvasClientSize: function (domElementSelector, outWidth, outHeight) {
    var selector = UTF8ToString(domElementSelector);
    var canvas = (selector == '#canvas') ? Module['canvas'] : document.querySelector(selector);
    if (Module.WebXR && Module.WebXR.isInXR)
    {
      HEAPF64[outWidth >> 3] = canvas ? canvas.width : 0;
      HEAPF64[outHeight >> 3] = canvas ? canvas.height : 0;
      return;
    }
    HEAPF64[outWidth >> 3] = canvas ? canvas.clientWidth : 0;
    HEAPF64[outHeight >> 3] = canvas ? canvas.clientHeight : 0;
  },

  JS_SystemInfo_GetPreferredDevicePixelRatio: function () {
    if (Module.WebXR && Module.WebXR.isInXR)
    {
      return 1;
    }
    return Module.devicePixelRatio || window.devicePixelRatio || 1;
  }
};
mergeInto(LibraryManager.library, LibrarySystemInfoWebGLOverride);
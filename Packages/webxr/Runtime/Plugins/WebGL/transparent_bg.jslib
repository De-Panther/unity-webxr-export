// Modified version of https://forum.unity.com/threads/webgl-transparent-background.284699/#post-1880667
// More details at https://support.unity.com/hc/en-us/articles/208892946-How-can-I-make-the-canvas-transparent-on-WebGL-
var LibraryGLClear = {
  glClear: function (mask) {
    if (mask == 0x00004000 && GLctx.dontClearAlphaOnly) {
      var v = GLctx.getParameter(GLctx.COLOR_WRITEMASK);
      if (!v[0] && !v[1] && !v[2] && v[3])
        // We are trying to clear alpha only -- skip.
        return;
    }
    GLctx.clear(mask);
  }
};
mergeInto(LibraryManager.library, LibraryGLClear);

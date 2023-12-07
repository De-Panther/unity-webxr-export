// Modified version of https://forum.unity.com/threads/webgl-transparent-background.284699/#post-1880667
// More details at https://support.unity.com/hc/en-us/articles/208892946-How-can-I-make-the-canvas-transparent-on-WebGL-
var LibraryGLClear = {
  $colorMaskValue: [true, true, true, true],
  glClear1: function (mask) { // disable hack while WIP
    if (mask == 0x00004000 && GLctx.dontClearAlphaOnly) {
      if (!colorMaskValue[0] && !colorMaskValue[1] && !colorMaskValue[2] && colorMaskValue[3])
        // We are trying to clear alpha only -- skip.
        return;
    }
    GLctx.clear(mask);
  },
  glColorMask1: function (red, green, blue, alpha) { // disable hack while WIP
    colorMaskValue[0] = !!red;
    colorMaskValue[1] = !!green;
    colorMaskValue[2] = !!blue;
    colorMaskValue[3] = !!alpha;
    GLctx.colorMask(colorMaskValue[0], colorMaskValue[1], colorMaskValue[2], colorMaskValue[3]);
  }
};
autoAddDeps(LibraryGLClear, '$colorMaskValue');
mergeInto(LibraryManager.library, LibraryGLClear);

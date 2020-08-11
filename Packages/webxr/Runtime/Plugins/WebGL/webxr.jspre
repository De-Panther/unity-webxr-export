setTimeout(function () {
    if (GL && GL.createContext)
    {
        GL.createContextOld = GL.createContext;
        GL.createContext = function (canvas, webGLContextAttributes)
        {
            var contextAttributes = {
                xrCompatible: true
            };

            if (webGLContextAttributes) {
                for (var attribute in webGLContextAttributes) {
                    contextAttributes[attribute] = webGLContextAttributes[attribute];
                }
            }
            
            return GL.createContextOld(canvas, contextAttributes);
        }
    }
}, 0);

Module['WebXR'] = Module['WebXR'] || {};

Module['WebXR'].GetBrowserObject = function () {
  return Browser;
}

Module['WebXR'].GetJSEventsObject = function () {
  return JSEvents;
}

Module['WebXR'].OnStartAR = function (views_count, left_rect, right_rect) {
  this.OnStartARInternal = this.OnStartARInternal || Module.cwrap("on_start_ar", null, ["number",
                                                                  "number", "number", "number", "number",
                                                                  "number", "number", "number", "number"]);
  this.OnStartARInternal(views_count,
                          left_rect.x, left_rect.y, left_rect.w, left_rect.h,
                          right_rect.x, right_rect.y, right_rect.w, right_rect.h);
}

Module['WebXR'].OnStartVR = function (views_count) {
  this.OnStartVRInternal = this.OnStartVRInternal || Module.cwrap("on_start_vr", null, ["number"]);
  this.OnStartVRInternal(views_count);
}

Module['WebXR'].OnEndXR = function () {
  this.OnEndXRInternal = this.OnEndXRInternal || Module.cwrap("on_end_xr", null, []);
  this.OnEndXRInternal();
}

Module['WebXR'].OnXRCapabilities = function (display_capabilities) {
  this.OnXRCapabilitiesInternal = this.OnXRCapabilitiesInternal || Module.cwrap("on_xr_capabilities", null, ["string"]);
  this.OnXRCapabilitiesInternal(display_capabilities);
}

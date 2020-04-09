setTimeout(function () {
    Module['InternalBrowser'] = Browser || {};
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

Module['WebXR'].OnStartXR = function () {
  this.OnStartXRInternal = this.OnStartXRInternal || Module.cwrap("on_start_xr", null, []);
  this.OnStartXRInternal();
}

Module['WebXR'].OnEndXR = function () {
  this.OnEndXRInternal = this.OnEndXRInternal || Module.cwrap("on_end_xr", null, []);
  this.OnEndXRInternal();
}

Module['WebXR'].OnXRCapabilities = function (display_capabilities) {
  this.OnXRCapabilitiesInternal = this.OnXRCapabilitiesInternal || Module.cwrap("on_xr_capabilities", null, ["string"]);
  this.OnXRCapabilitiesInternal(display_capabilities);
}

Module['WebXR'].OnWebXRData = function (webxr_data) {
  this.OnWebXRDataInternal = this.OnWebXRDataInternal || Module.cwrap("on_webxr_data", null, ["string"]);
  this.OnWebXRDataInternal(webxr_data);
}

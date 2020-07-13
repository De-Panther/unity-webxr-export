(function () {
  'use strict';

  function XRData() {
    this.leftProjectionMatrix = mat4.create();
    this.rightProjectionMatrix = mat4.create();
    this.leftViewMatrix = mat4.create();
    this.rightViewMatrix = mat4.create();
    this.sitStandMatrix = mat4.create();
    this.gamepads = [];
    this.controllerA = new XRControllerData();
    this.controllerB = new XRControllerData();
    this.xrData = null;
  }
  
  function XRControllerData() {
    // TODO: set enabled 0 if controller was enable and then disable
    this.enabled = 0;
    this.hand = 0;
    this.positionX = 0;
    this.positionY = 0;
    this.positionZ = 0;
    this.rotationX = 0;
    this.rotationY = 0;
    this.rotationZ = 0;
    this.rotationW = 0;
    this.trigger = 0;
    this.squeeze = 0;
    this.thumbstick = 0;
    this.thumbstickX = 0;
    this.thumbstickY = 0;
    this.touchpad = 0;
    this.touchpadX = 0;
    this.touchpadY = 0;
    this.buttonA = 0;
    this.buttonB = 0;
  }

  function XRManager() {
    this.arSession = null;
    this.vrSession = null;
    this.inlineSession = null;
    this.xrData = new XRData();
    this.canvas = null;
    this.ctx = null;
    this.gameInstance = null;
    this.polyfill = null;
    this.didNotifyUnity = false;
    this.isARSupported = false;
    this.isVRSupported = false;
    this.rAFCB = null;
    this.onInputEvent = null;
    this.waitingHandheldARHack = false;
    this.init();
  }

  XRManager.prototype.init = async function () {
    if (window.WebXRPolyfill) {
      this.polyfill = new WebXRPolyfill();
    }
    
    this.attachEventListeners();

    navigator.xr.isSessionSupported('immersive-vr').then((supported) => {
      this.isVRSupported = supported;
      if (document.body.dataset.unityLoaded)
      {
        document.dispatchEvent(new CustomEvent('onVRSupportedCheck', { detail:{supported:this.isVRSupported} }));
        this.UpdateXRCapabilities();
      }
    });

    navigator.xr.isSessionSupported('immersive-ar').then((supported) => {
      this.isARSupported = supported;
      if (document.body.dataset.unityLoaded)
      {
        document.dispatchEvent(new CustomEvent('onARSupportedCheck', { detail:{supported:this.isARSupported} }));
        this.UpdateXRCapabilities();
      }
    });
  }


  XRManager.prototype.attachEventListeners = function () {
    var onToggleAr = this.toggleAr.bind(this);
    var onToggleVr = this.toggleVr.bind(this);
    var onUnityLoaded = this.unityLoaded.bind(this);

    // dispatched by index.html
    document.addEventListener('UnityLoaded', onUnityLoaded, false);

    document.addEventListener('toggleAR', onToggleAr, false);
    document.addEventListener('toggleVR', onToggleVr, false);
  }

  XRManager.prototype.onRequestARSession = function () {
    if (!this.isARSupported) return;
    // The window on Chrome for Android lose focus when asking permissions.
    // A popup is opened and the Canvas is painted with the last frame.
    // We want to make sure that the Canvas is transparent when entering Handheld AR Session.
    this.waitingHandheldARHack = true;
    var thisXRMananger = this;
    var tempRender = function () {
      thisXRMananger.ctx.clearColor(0, 0, 0, 0);
      thisXRMananger.ctx.clear(thisXRMananger.ctx.COLOR_BUFFER_BIT | thisXRMananger.ctx.DEPTH_BUFFER_BIT);
      if (thisXRMananger.waitingHandheldARHack)
      {
        window.requestAnimationFrame( tempRender );
      }
    }
    window.requestAnimationFrame( tempRender );
    navigator.xr.requestSession('immersive-ar', {
      requiredFeatures: ['local-floor'], // TODO: Get this value from Unity
      optionalFeatures: ['dom-overlay'],
      domOverlay: {root: this.canvas.parentElement}
    }).then(async (session) => {
      this.waitingHandheldARHack = false;
      session.isImmersive = true;
      session.isInSession = true;
      session.isAR = true;
      this.arSession = session;
      this.onSessionStarted(session);
    }).catch((error) => {
      thisXRMananger.waitingHandheldARHack = false;
    });
  }

  XRManager.prototype.onRequestVRSession = function () {
    if (!this.isVRSupported) return;
    navigator.xr.requestSession('immersive-vr', {
      requiredFeatures: ['local-floor'] // TODO: Get this value from Unity
    }).then(async (session) => {
      session.isImmersive = true;
      session.isInSession = true;
      this.vrSession = session;
      this.onSessionStarted(session);
    });
  }

  XRManager.prototype.exitARSession = function () {
    if (!this.arSession || !this.arSession.isInSession) {
      console.warn('No AR display to exit AR mode');
      return;
    }

    this.arSession.end();
  }

  XRManager.prototype.exitVRSession = function () {
    if (!this.vrSession || !this.vrSession.isInSession) {
      console.warn('No VR display to exit VR mode');
      return;
    }

    this.vrSession.end();
  }

  XRManager.prototype.onEndSession = function (xrSessionEvent) {
    if (xrSessionEvent.session) {
      xrSessionEvent.session.isInSession = false;
      xrSessionEvent.session.removeEventListener('select', this.onInputEvent);
      xrSessionEvent.session.removeEventListener('selectstart', this.onInputEvent);
      xrSessionEvent.session.removeEventListener('selectend', this.onInputEvent);
      xrSessionEvent.session.removeEventListener('squeeze', this.onInputEvent);
      xrSessionEvent.session.removeEventListener('squeezestart', this.onInputEvent);
      xrSessionEvent.session.removeEventListener('squeezeend', this.onInputEvent);
    }
    
    this.gameInstance.Module.WebXR.OnEndXR();
    this.didNotifyUnity = false;
    this.canvas.width = this.canvas.parentElement.clientWidth * window.devicePixelRatio;
    this.canvas.height = this.canvas.parentElement.clientHeight * window.devicePixelRatio;
  }
  
  XRManager.prototype.onInputSourceEvent = function (xrInputSourceEvent) {
    if (xrInputSourceEvent.type && xrInputSourceEvent.inputSource
        && xrInputSourceEvent.inputSource.handedness) {
      var hand = 0;
      var inputSource = xrInputSourceEvent.inputSource;
      var xrData = this.xrData;
      var controller = this.xrData.controllerA;
      if (inputSource.handedness == 'left') {
          hand = 1;
          controller = this.xrData.controllerB;
      } else if (inputSource.handedness == 'right') {
          hand = 2;
      }
      
      controller.enabled = 1;
      controller.hand = hand;
      
      switch (xrInputSourceEvent.type) {
        case "select":
          controller.trigger = 1;
          break;
        case "selectstart":
          controller.trigger = 1;
          break;
        case "selectend":
          controller.trigger = 0;
          break;
        case "squeeze":
          controller.squeeze = 1;
          break;
        case "squeezestart":
          controller.squeeze = 1;
          break;
        case "squeezeend":
          controller.squeeze = 0;
          break;
      }
      
      if (hand == 0 || hand == 2) {
        xrData.controllerA = controller;
      } else {
        xrData.controllerB = controller;
      }
    }
  }

  XRManager.prototype.toggleAr = function () {
    if (!this.gameInstance)
    {
      return;
    }
    if (this.isARSupported && this.arSession && this.arSession.isInSession) {
      this.exitARSession();
    } else {
      this.onRequestARSession();
    }
  }

  XRManager.prototype.toggleVr = function () {
    if (!this.gameInstance)
    {
      return;
    }
    if (this.isVRSupported && this.vrSession && this.vrSession.isInSession) {
      this.exitVRSession();
    } else {
      this.onRequestVRSession();
    }
  }

  XRManager.prototype.setGameInstance = function (gameInstance) {
    if (!this.gameInstance) {
      this.gameInstance = gameInstance;
      this.canvas = this.gameInstance.Module.canvas;
      this.ctx = this.gameInstance.Module.ctx;

      var thisXRMananger = this;
      this.gameInstance.Module.InternalBrowser.requestAnimationFrame = function (func) {
        if (!thisXRMananger.rAFCB)
        {
          thisXRMananger.rAFCB=func;
        }
        if (thisXRMananger.arSession && thisXRMananger.arSession.isInSession) {
          return thisXRMananger.arSession.requestAnimationFrame((time, xrFrame) =>
          {
            thisXRMananger.animate(xrFrame);
            func(time);
          });
        } else if (thisXRMananger.vrSession && thisXRMananger.vrSession.isInSession) {
          return thisXRMananger.vrSession.requestAnimationFrame((time, xrFrame) =>
          {
            thisXRMananger.animate(xrFrame);
            func(time);
          });
        } else if (thisXRMananger.inlineSession && thisXRMananger.inlineSession.isInSession) {
          return thisXRMananger.inlineSession.requestAnimationFrame((time, xrFrame) =>
          {
            thisXRMananger.animate(xrFrame);
            func(time);
          });
        } else {
          if (!thisXRMananger.waitingHandheldARHack) {
            window.requestAnimationFrame(func);
          }
        }
      };

      // bindFramebuffer frameBufferObject null in XRSession should use XRWebGLLayer FBO instead
      this.ctx.bindFramebuffer = (oldBindFramebuffer => function bindFramebuffer(target, fbo) {
        if (!fbo) {
          if (thisXRMananger.arSession && thisXRMananger.arSession.isInSession) {
            if (thisXRMananger.arSession.renderState.baseLayer) {
              fbo = thisXRMananger.arSession.renderState.baseLayer.framebuffer;
            }
          } else if (thisXRMananger.vrSession && thisXRMananger.vrSession.isInSession) {
            if (thisXRMananger.vrSession.renderState.baseLayer) {
              fbo = thisXRMananger.vrSession.renderState.baseLayer.framebuffer;
            }
          } else if (thisXRMananger.inlineSession && thisXRMananger.inlineSession.isInSession &&
                     thisXRMananger.inlineSession.renderState.baseLayer) {
            fbo = thisXRMananger.inlineSession.renderState.baseLayer.framebuffer;
          }
        }
        return oldBindFramebuffer.call(this, target, fbo);
      })(this.ctx.bindFramebuffer);
    }
  }

  XRManager.prototype.unityLoaded = function () {
    document.body.dataset.unityLoaded = 'true';

    this.setGameInstance(unityInstance);

    document.dispatchEvent(new CustomEvent('onARSupportedCheck', { detail:{supported:this.isARSupported} }));
    document.dispatchEvent(new CustomEvent('onVRSupportedCheck', { detail:{supported:this.isVRSupported} }));

    this.UpdateXRCapabilities();
    
    this.onInputEvent = this.onInputSourceEvent.bind(this);

    navigator.xr.isSessionSupported('inline').then((supported) => {
      if (supported) {
        navigator.xr.requestSession('inline').then((session) => {
          session.isInSession = true;
          this.inlineSession = session;
          this.onSessionStarted(session);
        });
      }
    });
  }

  XRManager.prototype.UpdateXRCapabilities = function() {
    // Send browser capabilities to Unity.
    this.gameInstance.Module.WebXR.OnXRCapabilities(
      JSON.stringify({
        canPresentAR: this.isARSupported,
        canPresentVR: this.isVRSupported,
        hasPosition: true, // TODO: check this
        hasExternalDisplay: false // TODO: check this
      })
    );
  }
  
  XRManager.prototype.getXRControllersData = function(frame, inputSources, refSpace, xrData) {
    if (!inputSources || !inputSources.length) {
      return;
    }
    for (var i = 0; i < inputSources.length; i++) {
      let inputSource = inputSources[i];
      // Show the input source if it has a grip space
      if (inputSource.gripSpace) {
        let inputPose = frame.getPose(inputSource.gripSpace, refSpace);
        if (inputPose) {
          var position = inputPose.transform.position;
          var orientation = inputPose.transform.orientation;
          var hand = 0;
          var controller = xrData.controllerA;
          if (inputSource.handedness == 'left') {
            hand = 1;
            controller = xrData.controllerB;
          } else if (inputSource.handedness == 'right') {
            hand = 2;
          }
          
          controller.enabled = 1;
          controller.hand = hand;
          
          controller.positionX = position.x;
          controller.positionY = position.y;
          controller.positionZ = -position.z;
          
          controller.rotationX = -orientation.x;
          controller.rotationY = -orientation.y;
          controller.rotationZ = orientation.z;
          controller.rotationW = orientation.w;
          
          // if there's gamepad, use the xr-standard mapping
          // TODO: check for profiles
          if (inputSource.gamepad) {
            for (var j = 0; j < inputSource.gamepad.buttons.length; j++) {
              switch (j) {
                case 0:
                  controller.trigger = inputSource.gamepad.buttons[j].value;
                  break;
                case 1:
                  controller.squeeze = inputSource.gamepad.buttons[j].value;
                  break;
                case 2:
                  controller.touchpad = inputSource.gamepad.buttons[j].value;
                  break;
                case 3:
                  controller.thumbstick = inputSource.gamepad.buttons[j].value;
                  break;
                case 4:
                  controller.buttonA = inputSource.gamepad.buttons[j].value;
                  break;
                case 5:
                  controller.buttonB = inputSource.gamepad.buttons[j].value;
                  break;
              }
            }
            for (var j = 0; j < inputSource.gamepad.axes.length; j++) {
              switch (j) {
                case 0:
                  controller.touchpadX = inputSource.gamepad.axes[j];
                  break;
                case 1:
                  controller.touchpadY = inputSource.gamepad.axes[j];
                  break;
                case 2:
                  controller.thumbstickX = inputSource.gamepad.axes[j];
                  break;
                case 3:
                  controller.thumbstickY = inputSource.gamepad.axes[j];
                  break;
              }
            }
          }
          
          if (hand == 0 || hand == 2) {
            xrData.controllerA = controller;
          } else {
            xrData.controllerB = controller;
          }
        }
      }
    }
  }

  // Convert WebGL to Unity compatible Vector3
  XRManager.prototype.GLVec3ToUnity = function(v) {
    v[2] *= -1;
    return v;
  }

  // Convert WebGL to Unity compatible Quaternion
  XRManager.prototype.GLQuaternionToUnity = function(q) {
    q[0] *= -1;
    q[1] *= -1;
    return q;
  }

  // Convert WebGL to Unity Projection Matrix4
  XRManager.prototype.GLProjectionToUnity = function(m) {
    var out = mat4.create();
    mat4.copy(out, m)
    mat4.transpose(out, out);
    return out;
  }

  // Convert WebGL to Unity View Matrix4
  XRManager.prototype.GLViewToUnity = function(m) {
    var out = mat4.create();
    mat4.copy(out, m);
    mat4.transpose(out, out);
    out[2] *= -1;
    out[6] *= -1;
    out[10] *= -1;
    out[14] *= -1;
    return out;
  }

  XRManager.prototype.onSessionStarted = function (session) {
    let glLayer = new XRWebGLLayer(session, this.ctx);
    session.updateRenderState({ baseLayer: glLayer });
    
    
    
    let refSpaceType = 'viewer';
    if (session.isImmersive) {
      refSpaceType = 'local-floor';

      var onSessionEnded = this.onEndSession.bind(this);
      session.addEventListener('end', onSessionEnded);

      this.canvas.width = glLayer.framebufferWidth;
      this.canvas.height = glLayer.framebufferHeight;
      
      session.addEventListener('select', this.onInputEvent);
      session.addEventListener('selectstart', this.onInputEvent);
      session.addEventListener('selectend', this.onInputEvent);
      session.addEventListener('squeeze', this.onInputEvent);
      session.addEventListener('squeezestart', this.onInputEvent);
      session.addEventListener('squeezeend', this.onInputEvent);
    }
    
    session.requestReferenceSpace(refSpaceType).then((refSpace) => {
      session.refSpace = refSpace;
      if (session.isImmersive)
      {
        // Inform the session that we're ready to begin drawing.
        this.gameInstance.Module.InternalBrowser.requestAnimationFrame(this.rAFCB);
      }
    });
  }

  XRManager.prototype.animate = function (frame) {
    let session = frame.session;
    if (!session) {
      return;
    }
    
    let glLayer = session.renderState.baseLayer;
    
    if (this.canvas.width != glLayer.framebufferWidth ||
        this.canvas.height != glLayer.framebufferHeight)
    {
      this.canvas.width = glLayer.framebufferWidth;
      this.canvas.height = glLayer.framebufferHeight;
    }
    
    this.ctx.bindFramebuffer(this.ctx.FRAMEBUFFER, glLayer.framebuffer);
    if (session.isAR) {
      this.ctx.dontClearOnFrameStart = true;
    } else {
      this.ctx.clear(this.ctx.COLOR_BUFFER_BIT | this.ctx.DEPTH_BUFFER_BIT);
    }
    
    let pose = frame.getViewerPose(session.refSpace);
    if (!pose) {
      return;
    }

    if (!session.isImmersive)
    {
      return;
    }

    var xrData = this.xrData;

    for (let view of pose.views) {
      if (view.eye === 'left') {
        xrData.leftProjectionMatrix = this.GLProjectionToUnity(view.projectionMatrix);
        xrData.leftViewMatrix = this.GLViewToUnity(view.transform.inverse.matrix);
      } else if (view.eye === 'right') {
        xrData.rightProjectionMatrix = this.GLProjectionToUnity(view.projectionMatrix);
        xrData.rightViewMatrix = this.GLViewToUnity(view.transform.inverse.matrix);
      }
    }

    this.getXRControllersData(frame, session.inputSources, session.refSpace, xrData);
    
    // Dispatch event with headset data to be handled in webxr.jslib
    document.dispatchEvent(new CustomEvent('XRData', { detail: {
      leftProjectionMatrix: xrData.leftProjectionMatrix,
      rightProjectionMatrix: xrData.rightProjectionMatrix,
      leftViewMatrix: xrData.leftViewMatrix,
      rightViewMatrix: xrData.rightViewMatrix,
      sitStandMatrix: xrData.sitStandMatrix,
      controllerA: xrData.controllerA,
      controllerB: xrData.controllerB
    }}));
    
    if (!this.didNotifyUnity)
    {
      if (session.isAR)
      {
        this.gameInstance.Module.WebXR.OnStartAR(pose.views.length);
      } else {
        this.gameInstance.Module.WebXR.OnStartVR(pose.views.length);
      }
      this.didNotifyUnity = true;
    }
  }

  function initWebXRManager () {
    var xrManager = window.xrManager = new XRManager();
    return xrManager;
  }

  function init() {
    if (typeof(navigator.xr) == 'undefined') {
      var script = document.createElement('script');
      script.src = 'https://cdn.jsdelivr.net/npm/webxr-polyfill@latest/build/webxr-polyfill.js';
      document.getElementsByTagName('head')[0].appendChild(script);

      script.addEventListener('load', function () {
        initWebXRManager();
      });

      script.addEventListener('error', function (err) {
        console.warn('Could not load the WebXR Polyfill script:', err);
      });
    }
    else
    {
      initWebXRManager();
    }
  }
  
  // A workaround to make it work under Firefox Reality that does not implement isContextLost()
  // Thanks Rufus31415
  // https://github.com/Rufus31415/Simple-WebXR-Unity/blob/28331fc890e316e13401618a8e0da1e84bad7a39/Assets/SimpleWebXR.jspre#L6
  if(!WebGLRenderingContext.prototype.isContextLost) {
    WebGLRenderingContext.prototype.isContextLost = function() {
      return false;
    }
  }

  init();
})();
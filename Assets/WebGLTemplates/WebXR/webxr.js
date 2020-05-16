(function () {
  'use strict';

  function XRData() {
    this.leftProjectionMatrix = mat4.create();
    this.rightProjectionMatrix = mat4.create();
    this.leftViewMatrix = mat4.create();
    this.rightViewMatrix = mat4.create();
    this.sitStandMatrix = mat4.create();
    this.gamepads = [];
    this.xrData = null;
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
    navigator.xr.requestSession('immersive-ar', {
      requiredFeatures: ['local-floor'] // TODO: Get this value from Unity
    }).then(async (session) => {
      session.isImmersive = true;
      session.isInSession = true;
      session.isAR = true;
      this.arSession = session;
      this.onSessionStarted(session);
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
    }
    
    this.gameInstance.Module.WebXR.OnEndXR();
    this.didNotifyUnity = false;
    this.canvas.width = this.canvas.parentElement.clientWidth * window.devicePixelRatio;
    this.canvas.height = this.canvas.parentElement.clientHeight * window.devicePixelRatio;
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
          window.requestAnimationFrame(func);
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

  XRManager.prototype.getGamepadAxes = function(gamepad) {
    var axes = [];
    for (var i = 0; i < gamepad.axes.length; i++) {
      axes.push(gamepad.axes[i]);
    }
    return axes;
  }

  XRManager.prototype.getGamepadButtons = function(gamepad) {
    var buttons = [];
    for (var i = 0; i < gamepad.buttons.length; i++) {
      buttons.push({
        pressed: gamepad.buttons[i].pressed,
        touched: gamepad.buttons[i].touched,
        value: gamepad.buttons[i].value
      });
    }
    return buttons;
  }

  XRManager.prototype.getXRGamepads = function(frame, inputSources, refSpace) {
    var vrGamepads = []
    for (let inputSource of inputSources) {
      // Show the input source if it has a grip space
      if (inputSource.gripSpace && inputSource.gamepad) {
        let inputPose = frame.getPose(inputSource.gripSpace, refSpace);
        
        var position = inputPose.transform.position;
        var orientation = inputPose.transform.orientation;

        vrGamepads.push({
          id: inputSource.gamepad.id,
          index: inputSource.gamepad.index,
          hand: inputSource.handedness,
          buttons: this.getGamepadButtons(inputSource.gamepad),
          axes: this.getGamepadAxes(inputSource.gamepad),
          hasOrientation: true,
          hasPosition: true,
          orientation: this.GLQuaternionToUnity([orientation.x, orientation.y, orientation.z, orientation.w]),
          position: this.GLVec3ToUnity([position.x, position.y, position.z]),
          linearAcceleration: [0, 0, 0],
          linearVelocity: [0, 0, 0]
        });
      }
    }
    return vrGamepads;
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

    // Gamepads
    xrData.gamepads = this.getXRGamepads(frame, session.inputSources, session.refSpace);

    // Dispatch event with headset data to be handled in webxr.jslib
    document.dispatchEvent(new CustomEvent('XRData', { detail: {
      leftProjectionMatrix: xrData.leftProjectionMatrix,
      rightProjectionMatrix: xrData.rightProjectionMatrix,
      leftViewMatrix: xrData.leftViewMatrix,
      rightViewMatrix: xrData.rightViewMatrix,
      sitStandMatrix: xrData.sitStandMatrix
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

    this.gameInstance.Module.WebXR.OnWebXRData(
      JSON.stringify({
      controllers: xrData.gamepads
    }));
  }

  // Show instruction dialogue for non-VR enabled browsers.
  XRManager.prototype.displayElement = function (el) {
    if (el.dataset.enabled) {
      return;
    }
    var confirmButton = el.querySelector('button');
    el.dataset.enabled = true;

    function onConfirm () {
      el.dataset.enabled = false;
      confirmButton.removeEventListener('click', onConfirm);
    }
    confirmButton.addEventListener('click', onConfirm);
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

  init();
})();
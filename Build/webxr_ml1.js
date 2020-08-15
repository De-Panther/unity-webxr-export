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
    this.handLeft = new XRHandData();
    this.handRight = new XRHandData();
    this.viewerHitTestPose = new XRHitPoseData();
    this.frameNumber = 0;
    this.handHeldMove = false;
    this.xrData = null;
  }
  
  function XRControllerData() {
    this.frame = 0;
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
    this.gamepad = null;
  }

  function XRHandData() {
    this.frame = 0;
    // TODO: set enabled 0 if hand was enable and then disable
    this.enabled = 0;
    this.hand = 0;
    this.trigger = 0;
    this.squeeze = 0;
    this.joints = [];
    for (let i = 0; i < 25; i++) {
      this.joints.push(new XRJointData());
    }
  }

  function XRJointData() {
    this.enabled = 0;
    this.position = [0, 0, 0];
    this.rotation = [0, 0, 0, 1];
    this.radius = 0;
  }

  function XRHitPoseData() {
    this.frame = 0;
    this.available = 0;
    this.position = [0, 0, 0];
    this.rotation = [0, 0, 0, 1];
  }

  function lerp(start, end, percentage)
  {
    return start + (end - start) * percentage;
  }

  function XRMouseEvent(eventName, pageElement, xPercentage, yPercentage, buttonNumber) {
    let rect = pageElement.getBoundingClientRect();
    this.type = eventName;
    this.clientX = lerp(rect.left, rect.left + pageElement.width / window.devicePixelRatio, xPercentage);
    this.clientY = lerp(rect.top, rect.top + pageElement.height / window.devicePixelRatio, yPercentage);
    this.layerX = this.clientX;
    this.layerY = this.clientY;
    this.offsetX = this.clientX;
    this.offsetY = this.clientY;
    this.pageX = this.clientX;
    this.pageY = this.clientY;
    this.x = this.clientX;
    this.y = this.clientY;
    this.screenX = this.clientX;
    this.screenY = this.clientY;
    this.movementX = 0; // diff between movements
    this.movementY = 0; // diff between movements
    this.button = 0; // 0 none or main, 1 middle, 2 secondary
    this.buttons = 0; // 0 none, 1 main, 4 middle, 2 secondary
    switch (buttonNumber)
    {
      case -1:
        this.button = 0;
        this.buttons = 0;
        break;
      case 0:
        this.button = 0;
        this.buttons = 1;
        break;
      case 1:
        this.button = 1;
        this.buttons = 4;
        break;
      case 2:
        this.button = 2;
        this.buttons = 2;
        break;
    }
    this.ctrlKey = false;
    this.altKey = false;
    this.metaKey = false;
    this.shiftKey = false;
    this.detail = 0;
  }

  function XRManager() {
    this.xrSession = null;
    this.inlineSession = null;
    this.viewerSpace = null;
    this.viewerHitTestSource = null;
    this.xrData = new XRData();
    this.canvas = null;
    this.ctx = null;
    this.gameInstance = null;
    this.polyfill = null;
    this.didNotifyUnity = false;
    this.isARSupported = false;
    this.isVRSupported = false;
    this.onInputEvent = null;
    this.waitingHandheldARHack = false;
    this.BrowserObject = null;
    this.JSEventsObject = null;
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
    var onToggleHitTest = this.toggleHitTest.bind(this);
    var onCallHapticPulse = this.hapticPulse.bind(this);

    // dispatched by index.html
    document.addEventListener('UnityLoaded', onUnityLoaded, false);

    document.addEventListener('toggleAR', onToggleAr, false);
    document.addEventListener('toggleVR', onToggleVr, false);

    document.addEventListener('toggleHitTest', onToggleHitTest, false);
    document.addEventListener('callHapticPulse', onCallHapticPulse, false);
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
      optionalFeatures: ['hand-tracking', 'hit-test']
    }).then(async (session) => {
      this.waitingHandheldARHack = false;
      session.isImmersive = true;
      session.isInSession = true;
      session.isAR = true;
      this.xrSession = session;
      this.onSessionStarted(session);
    }).catch((error) => {
      thisXRMananger.waitingHandheldARHack = false;
    });
  }

  XRManager.prototype.onRequestVRSession = function () {
    if (!this.isVRSupported) return;
    navigator.xr.requestSession('immersive-vr', {
      requiredFeatures: ['local-floor'], // TODO: Get this value from Unity
      optionalFeatures: ['hand-tracking']
    }).then(async (session) => {
      session.isImmersive = true;
      session.isInSession = true;
      session.isAR = false;
      this.xrSession = session;
      this.onSessionStarted(session);
    });
  }

  XRManager.prototype.exitXRSession = function () {
    if (!this.xrSession || !this.xrSession.isInSession) {
      console.warn('No XR display to exit XR mode');
      return;
    }

    this.xrSession.end();
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

    if (this.viewerHitTestSource) {
      this.viewerHitTestSource.cancel();
      this.viewerHitTestSource = null;
    }
    
    this.gameInstance.Module.WebXR.OnEndXR();
    this.didNotifyUnity = false;
    this.canvas.width = this.canvas.parentElement.clientWidth * window.devicePixelRatio;
    this.canvas.height = this.canvas.parentElement.clientHeight * window.devicePixelRatio;
  }
  
  XRManager.prototype.onInputSourceEvent = function (xrInputSourceEvent) {
    if (xrInputSourceEvent.type && xrInputSourceEvent.inputSource
        && xrInputSourceEvent.inputSource.handedness != 'none') {
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
        xrData.handRight.trigger = controller.trigger;
        xrData.handRight.squeeze = controller.squeeze;
      } else {
        xrData.controllerB = controller;
        xrData.handLeft.trigger = controller.trigger;
        xrData.handLeft.squeeze = controller.squeeze;
      }
    } else {
      let xPercentage = 0.5;
      let yPercentage = 0.5;
      if (xrInputSourceEvent.inputSource &&
          xrInputSourceEvent.inputSource.gamepad &&
          xrInputSourceEvent.inputSource.gamepad.axes) {
        xPercentage = (xrInputSourceEvent.inputSource.gamepad.axes[0] + 1.0) * 0.5;
        yPercentage = (xrInputSourceEvent.inputSource.gamepad.axes[1] + 1.0) * 0.5;
      }
      switch (xrInputSourceEvent.type) {
        case "select": // mousemove 5
          this.JSEventsObject.eventHandlers[5].eventListenerFunc(
            new XRMouseEvent("mousemove", this.canvas, xPercentage, yPercentage, 0));
          break;
        case "selectstart": // mousedown 4
          this.xrData.handHeldMove = true;
          this.JSEventsObject.eventHandlers[5].eventListenerFunc(
            new XRMouseEvent("mousemove", this.canvas, xPercentage, yPercentage, 0));
            this.JSEventsObject.eventHandlers[4].eventListenerFunc(
            new XRMouseEvent("mousedown", this.canvas, xPercentage, yPercentage, 0));
          break;
        case "selectend": // mouseup 3
          this.xrData.handHeldMove = false;
          this.JSEventsObject.eventHandlers[3].eventListenerFunc(
            new XRMouseEvent("mouseup", this.canvas, xPercentage, yPercentage, 0));
          break;
      }
    }
  }

  XRManager.prototype.toggleAr = function () {
    if (!this.gameInstance)
    {
      return;
    }
    if (this.xrSession && this.xrSession.isInSession) {
      this.exitXRSession();
    } else {
      this.onRequestARSession();
    }
  }

  XRManager.prototype.toggleVr = function () {
    if (!this.gameInstance)
    {
      return;
    }
    if (this.xrSession && this.xrSession.isInSession) {
      this.exitXRSession();
    } else {
      this.onRequestVRSession();
    }
  }

  XRManager.prototype.toggleHitTest = function () {
    if (!this.gameInstance)
    {
      return;
    }
    if (this.xrSession && this.xrSession.isInSession && this.xrSession.isAR) {
      if (this.viewerHitTestSource) {
        this.viewerHitTestSource.cancel();
        this.viewerHitTestSource = null;
      } else {
        this.xrSession.requestReferenceSpace('viewer').then((refSpace) => {
          this.viewerSpace = refSpace;
          this.xrSession.requestHitTestSource({space: this.viewerSpace}).then((hitTestSource) => {
            this.viewerHitTestSource = hitTestSource;
          });
        });
      }
    }
  }
  
  XRManager.prototype.hapticPulse = function (hapticPulseAction) {
    let controller = null;
    switch(hapticPulseAction.detail.controller)
    {
      case 0:
      case 2:
        controller = this.xrData.controllerA;
        break;
      case 1:
        controller = this.xrData.controllerB;
        break;
    }
    if (controller && controller.enabled == 1 && controller.gamepad && controller.gamepad.hapticActuators && controller.gamepad.hapticActuators.length > 0)
    {
      controller.gamepad.hapticActuators[0].pulse(hapticPulseAction.detail.intensity, hapticPulseAction.detail.duration);
    }
  }

  XRManager.prototype.setGameInstance = function (gameInstance) {
    if (!this.gameInstance) {
      this.gameInstance = gameInstance;
      this.canvas = this.gameInstance.Module.canvas;
      this.ctx = this.gameInstance.Module.ctx;

      var thisXRMananger = this;
      this.JSEventsObject = this.gameInstance.Module.WebXR.GetJSEventsObject();
      this.BrowserObject = this.gameInstance.Module.WebXR.GetBrowserObject();
      this.BrowserObject.requestAnimationFrame = function (func) {
        if (thisXRMananger.xrSession && thisXRMananger.xrSession.isInSession) {
          return thisXRMananger.xrSession.requestAnimationFrame((time, xrFrame) =>
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
          if (thisXRMananger.xrSession && thisXRMananger.xrSession.isInSession) {
            if (thisXRMananger.xrSession.renderState.baseLayer) {
              fbo = thisXRMananger.xrSession.renderState.baseLayer.framebuffer;
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
    xrData.handLeft.enabled = 0;
    xrData.handRight.enabled = 0;
    xrData.controllerA.enabled = 0;
    xrData.controllerB.enabled = 0;
    xrData.handLeft.frame = xrData.frameNumber;
    xrData.handRight.frame = xrData.frameNumber;
    xrData.controllerA.frame = xrData.frameNumber;
    xrData.controllerB.frame = xrData.frameNumber;
    if (!inputSources || !inputSources.length) {
      return;
    }
    for (var i = 0; i < inputSources.length; i++) {
      let inputSource = inputSources[i];
      // Show the input source if it has a grip space
      if (inputSource.hand) {
        var hand = 1;
        var xrHand = xrData.handLeft;
        if (inputSource.handedness == 'right') {
          hand = 2;
          xrHand = xrData.handRight;
        }
        xrHand.enabled = 1;
        xrHand.hand = hand;
        for (let j = 0; j < 25; j++) {
          let joint = null;
          if (inputSource.hand[j] !== null) {
            joint = frame.getJointPose(inputSource.hand[j], refSpace);
          }
          if (joint !== null) {
            xrHand.joints[j].enabled = 1;
            xrHand.joints[j].position[0] = joint.transform.position.x;
            xrHand.joints[j].position[1] = joint.transform.position.y;
            xrHand.joints[j].position[2] = -joint.transform.position.z;
            xrHand.joints[j].rotation[0] = -joint.transform.orientation.x;
            xrHand.joints[j].rotation[1] = -joint.transform.orientation.y;
            xrHand.joints[j].rotation[2] = joint.transform.orientation.z;
            xrHand.joints[j].rotation[3] = joint.transform.orientation.w;
            if (joint.radius !== null) {
              xrHand.joints[j].radius = joint.radius;
            }
          }
        }
      } else if (inputSource.gripSpace) {
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
          controller.gamepad = inputSource.gamepad;
          
          if (hand == 0 || hand == 2) {
            xrData.controllerA = controller;
          } else {
            xrData.controllerB = controller;
          }
        }
      } else if (xrData.handHeldMove && inputSource.gamepad && inputSource.gamepad.axes) {
            if (xrData.handHeldMove)
            {
              this.JSEventsObject.eventHandlers[5].eventListenerFunc(
                new XRMouseEvent("mousemove", this.canvas,
                                  (inputSource.gamepad.axes[0] + 1.0) * 0.5,
                                  (inputSource.gamepad.axes[1] + 1.0) * 0.5, 0));
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
        this.BrowserObject.requestAnimationFrame(this.BrowserObject.mainLoop.runner);
      }
    });
  }

  XRManager.prototype.animate = function (frame) {
    let session = frame.session;
    if (!session) {
      return;
    }
    
    let glLayer = session.renderState.baseLayer;
    
    // test remove canvas size update for ML1
    /*if (this.canvas.width != glLayer.framebufferWidth ||
        this.canvas.height != glLayer.framebufferHeight)
    {
      this.canvas.width = glLayer.framebufferWidth;
      this.canvas.height = glLayer.framebufferHeight;
    }*/
    
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
    xrData.frameNumber++;

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

    if (session.isAR && this.viewerHitTestSource) {
      xrData.viewerHitTestPose.frame = xrData.frameNumber;
      let viewerHitTestResults = frame.getHitTestResults(this.viewerHitTestSource);
      if (viewerHitTestResults.length > 0) {
        let hitTestPose = viewerHitTestResults[0].getPose(session.refSpace);
        xrData.viewerHitTestPose.available = 1;
        xrData.viewerHitTestPose.position[0] = hitTestPose.transform.position.x;
        xrData.viewerHitTestPose.position[1] = hitTestPose.transform.position.y;
        xrData.viewerHitTestPose.position[2] = -hitTestPose.transform.position.z;
        xrData.viewerHitTestPose.rotation[0] = -hitTestPose.transform.orientation.x;
        xrData.viewerHitTestPose.rotation[1] = -hitTestPose.transform.orientation.y;
        xrData.viewerHitTestPose.rotation[2] = hitTestPose.transform.orientation.z;
        xrData.viewerHitTestPose.rotation[3] = hitTestPose.transform.orientation.w;
      } else {
        xrData.viewerHitTestPose.available = 0;
      }
      document.dispatchEvent(new CustomEvent('XRViewerHitTestPose', { detail: {
        viewerHitTestPose: xrData.viewerHitTestPose
      }}));
    }
    
    // Dispatch event with headset data to be handled in webxr.jslib
    document.dispatchEvent(new CustomEvent('XRData', { detail: {
      leftProjectionMatrix: xrData.leftProjectionMatrix,
      rightProjectionMatrix: xrData.rightProjectionMatrix,
      leftViewMatrix: xrData.leftViewMatrix,
      rightViewMatrix: xrData.rightViewMatrix,
      sitStandMatrix: xrData.sitStandMatrix
    }}));

    document.dispatchEvent(new CustomEvent('XRControllersData', { detail: {
      controllerA: xrData.controllerA,
      controllerB: xrData.controllerB
    }}));

    document.dispatchEvent(new CustomEvent('XRHandsData', { detail: {
      handLeft: xrData.handLeft,
      handRight: xrData.handRight
    }}));
    
    if (!this.didNotifyUnity)
    {
      if (session.isAR)
      {
        let eyeCount = 1;
        let leftRect = {
          x:0,
          y:0,
          w:1,
          h:1
        }
        let rightRect = {
          x:0.5,
          y:0,
          w:0.5,
          h:1
        }
        for (let view of pose.views) {
          let viewport = session.renderState.baseLayer.getViewport(view);
          if (view.eye === 'left') {
            if (viewport) {
              leftRect.x = viewport.x / glLayer.framebufferWidth;
              leftRect.y = viewport.y / glLayer.framebufferHeight;
              leftRect.w = viewport.width / glLayer.framebufferWidth;
              leftRect.h = viewport.height / glLayer.framebufferHeight;
            }
          } else if (view.eye === 'right') {
            eyeCount = 2;
            if (viewport) {
              rightRect.x = viewport.x / glLayer.framebufferWidth;
              rightRect.y = viewport.y / glLayer.framebufferHeight;
              rightRect.w = viewport.width / glLayer.framebufferWidth;
              rightRect.h = viewport.height / glLayer.framebufferHeight;
            }
          }
        }
        this.gameInstance.Module.WebXR.OnStartAR(eyeCount, leftRect, rightRect);
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
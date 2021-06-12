Module['WebXR'] = Module['WebXR'] || {};

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


    (function () {
      'use strict';
    
      function XRData() {
        this.leftProjectionMatrix =  [1, 0, 0, 0,
                                      0, 1, 0, 0,
                                      0, 0, 1, 0,
                                      0, 0, 0, 1];
        this.rightProjectionMatrix =  [1, 0, 0, 0,
                                      0, 1, 0, 0,
                                      0, 0, 1, 0,
                                      0, 0, 0, 1];
        this.leftViewRotation =  [0, 0, 0, 1];
        this.rightViewRotation = [0, 0, 0, 1];
        this.leftViewPosition =  [0, 0, 0];
        this.rightViewPosition = [0, 0, 0];
        this.gamepads = [];
        this.controllerA = new XRControllerData();
        this.controllerB = new XRControllerData();
        this.handLeft = new XRHandData();
        this.handRight = new XRHandData();
        this.viewerHitTestPose = new XRHitPoseData();
        this.frameNumber = 0;
        this.touchIDs = [];
        this.touches = [];
        this.CreateTouch = function (pageElement, xPercentage, yPercentage) {
          var touchID = 0;
          while (this.touchIDs.includes(touchID))
          {
            touchID++;
          }
          var touch = new XRTouch(touchID, pageElement, xPercentage, yPercentage);
          this.touchIDs.push(touchID);
          this.touches.push(touch);
          return touch;
        }
        this.RemoveTouch = function (touch) {
          touch.ended = true;
          this.touchIDs = this.touchIDs.filter(function(item) {
            return item !== touch.identifier
          });
          this.touches = this.touches.filter(function(item) {
            return item !== touch
          });
        }
        this.SendTouchEvent = function(JSEventsObject, eventID, eventName, target, changedTouches) {
          var touchEvent = new XRTouchEvent(eventName, target, this.touches, this.touches, changedTouches);
          JSEventsObject.eventHandlers[eventID].eventListenerFunc(touchEvent);
        }
      }
      
      function XRControllerData() {
        this.bufferIndex = 0;
        this.gamepad = null;
        this.profiles = [];
        this.updatedProfiles = 0;
      }
    
      function XRHandData() {
        this.bufferIndex = 0;
        this.poses = new Float32Array(16 * 25);
        this.radii = new Float32Array(25);
        this.jointQuaternion = new Float32Array(4);
        this.jointIndex = 0;
        this.unityJointIndex = 0;
        this.handValuesType = 0;
        this.hasRadii = false;
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
    
      function XRTouch(touchID, pageElement, xPercentage, yPercentage) {
        this.identifier = touchID;
        this.ended = false;
        var rect = pageElement.getBoundingClientRect();
        // It was pageElement.size / window.devicePixelRatio, but now we treat devicePixelRatio in XR session as 1
        this.clientX = lerp(rect.left, rect.left + pageElement.width / 1, xPercentage);
        this.clientY = lerp(rect.top, rect.top + pageElement.height / 1, yPercentage);
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
        this.UpdateTouch = function (pageElement, xPercentage, yPercentage) {
          var rect = pageElement.getBoundingClientRect();
          var newClientX = lerp(rect.left, rect.left + pageElement.width / 1, xPercentage);
          var newClientY = lerp(rect.top, rect.top + pageElement.height / 1, yPercentage);
          this.movementX = newClientX-this.clientX;
          this.movementY = newClientY-this.clientY;
          this.clientX = newClientX;
          this.clientY = newClientY;
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
        }
        this.HasMovement = function () {
          return (this.movementX != 0 || this.movementY != 0);
        }
        this.ResetMovement = function () {
          this.movementX = 0;
          this.movementY = 0;
        }
      }
      
      function XRTouchEvent(eventName, target, touches, targetTouchs, changedTouches) {
        this.type = eventName;
        this.target = target;
        this.touches = touches;
        this.targetTouches = targetTouchs;
        this.changedTouches = changedTouches;
        this.ctrlKey = false;
        this.altKey = false;
        this.metaKey = false;
        this.shiftKey = false;
        this.preventDefault = function () {};
      }
    
      function XRManager() {
        this.xrSession = null;
        this.viewerSpace = null;
        this.viewerHitTestSource = null;
        this.xrData = new XRData();
        this.canvas = null;
        this.ctx = null;
        this.gameModule = null;
        this.polyfill = null;
        this.didNotifyUnity = false;
        this.isARSupported = false;
        this.isVRSupported = false;
        this.onInputEvent = null;
        this.onSessionVisibilityEvent = null;
        this.BrowserObject = null;
        this.JSEventsObject = null;
        this.init();
      }
    
      XRManager.prototype.init = function () {
        if (window.WebXRPolyfill) {
          this.polyfill = new WebXRPolyfill();
        }
        
        this.attachEventListeners();
        var thisXRMananger = this;
        navigator.xr.isSessionSupported('immersive-vr').then(function (supported) {
          thisXRMananger.isVRSupported = supported;
          if (Module.WebXR.unityLoaded)
          {
            document.dispatchEvent(new CustomEvent('onVRSupportedCheck', { detail:{supported:thisXRMananger.isVRSupported} }));
            thisXRMananger.UpdateXRCapabilities();
          }
        });
    
        navigator.xr.isSessionSupported('immersive-ar').then(function (supported) {
          thisXRMananger.isARSupported = supported;
          if (Module.WebXR.unityLoaded)
          {
            document.dispatchEvent(new CustomEvent('onARSupportedCheck', { detail:{supported:thisXRMananger.isARSupported} }));
            thisXRMananger.UpdateXRCapabilities();
          }
        });
      }
    
    
      XRManager.prototype.attachEventListeners = function () {
        var onToggleAr = this.toggleAr.bind(this);
        var onToggleVr = this.toggleVr.bind(this);
        var onUnityLoaded = this.unityLoaded.bind(this);
        var onToggleHitTest = this.toggleHitTest.bind(this);
        var onCallHapticPulse = this.hapticPulse.bind(this);

        Module.WebXR.onUnityLoaded = onUnityLoaded;
        Module.WebXR.toggleAR = onToggleAr;
        Module.WebXR.toggleVR = onToggleVr;
        Module.WebXR.toggleHitTest = onToggleHitTest;
        Module.WebXR.callHapticPulse = onCallHapticPulse;
      }
    
      XRManager.prototype.onRequestARSession = function () {
        if (!this.isARSupported) return;
        this.BrowserObject.pauseAsyncCallbacks();
        this.BrowserObject.mainLoop.pause();
        var thisXRMananger = this;
        var tempRender = function () {
          thisXRMananger.ctx.clearColor(0, 0, 0, 0);
          thisXRMananger.ctx.clear(thisXRMananger.ctx.COLOR_BUFFER_BIT | thisXRMananger.ctx.DEPTH_BUFFER_BIT);
        }
        window.requestAnimationFrame( tempRender );
        navigator.xr.requestSession('immersive-ar', {
          requiredFeatures: thisXRMananger.gameModule.WebXR.Settings.ARRequiredReferenceSpace,
          optionalFeatures: thisXRMananger.gameModule.WebXR.Settings.AROptionalFeatures
        }).then(function (session) {
          session.isImmersive = true;
          session.isInSession = true;
          session.isAR = true;
          thisXRMananger.xrSession = session;
          thisXRMananger.onSessionStarted(session);
        }).catch(function (error) {
          thisXRMananger.BrowserObject.resumeAsyncCallbacks();
          thisXRMananger.BrowserObject.mainLoop.resume();
        });
      }
    
      XRManager.prototype.onRequestVRSession = function () {
        if (!this.isVRSupported) return;
        this.BrowserObject.pauseAsyncCallbacks();
        this.BrowserObject.mainLoop.pause();
        var thisXRMananger = this;
        var tempRender = function () {
          thisXRMananger.ctx.clearColor(0, 0, 0, 0);
          thisXRMananger.ctx.clear(thisXRMananger.ctx.COLOR_BUFFER_BIT | thisXRMananger.ctx.DEPTH_BUFFER_BIT);
        }
        window.requestAnimationFrame( tempRender );
        navigator.xr.requestSession('immersive-vr', {
          requiredFeatures: thisXRMananger.gameModule.WebXR.Settings.VRRequiredReferenceSpace,
          optionalFeatures: thisXRMananger.gameModule.WebXR.Settings.VROptionalFeatures
        }).then(function (session) {
          session.isImmersive = true;
          session.isInSession = true;
          session.isAR = false;
          thisXRMananger.xrSession = session;
          thisXRMananger.onSessionStarted(session);
        }).catch(function (error) {
          thisXRMananger.BrowserObject.resumeAsyncCallbacks();
          thisXRMananger.BrowserObject.mainLoop.resume();
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
          xrSessionEvent.session.removeEventListener('visibilitychange', this.onSessionVisibilityEvent);
        }
    
        if (this.viewerHitTestSource) {
          this.viewerHitTestSource.cancel();
          this.viewerHitTestSource = null;
        }
        
        this.removeRemainingTouches();

        Module.HEAPF32[this.xrData.controllerA.bufferIndex] = -1; // XRControllerData.frame
        Module.HEAPF32[this.xrData.controllerB.bufferIndex] = -1; // XRControllerData.frame
        Module.HEAPF32[this.xrData.controllerA.bufferIndex + 1] = 0; // XRControllerData.enabled
        Module.HEAPF32[this.xrData.controllerB.bufferIndex + 1] = 0; // XRControllerData.enabled

        Module.HEAPF32[this.xrData.handLeft.bufferIndex] = -1; // XRHandData.frame
        Module.HEAPF32[this.xrData.handRight.bufferIndex] = -1; // XRHandData.frame
        Module.HEAPF32[this.xrData.handLeft.bufferIndex + 1] = 0; // XRHandData.enabled
        Module.HEAPF32[this.xrData.handRight.bufferIndex + 1] = 0; // XRHandData.enabled

        this.gameModule.WebXR.OnEndXR();
        this.didNotifyUnity = false;
        this.canvas.width = this.canvas.parentElement.clientWidth * this.gameModule.asmLibraryArg._JS_SystemInfo_GetPreferredDevicePixelRatio();
        this.canvas.height = this.canvas.parentElement.clientHeight * this.gameModule.asmLibraryArg._JS_SystemInfo_GetPreferredDevicePixelRatio();
        
        this.BrowserObject.pauseAsyncCallbacks();
        this.BrowserObject.mainLoop.pause();
        this.ctx.bindFramebuffer(this.ctx.FRAMEBUFFER);
        var thisXRMananger = this;
        window.setTimeout(function () {
          thisXRMananger.BrowserObject.resumeAsyncCallbacks();
          thisXRMananger.BrowserObject.mainLoop.resume();
        });
      }
      
      XRManager.prototype.removeRemainingTouches = function () {
        while (this.xrData.touches.length > 0)
        {
          var touch = this.xrData.touches[0];
          this.xrData.RemoveTouch(touch);
          this.xrData.SendTouchEvent(this.JSEventsObject, 8, "touchend", this.canvas, [touch]);
        }
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
          
          Module.HEAPF32[controller.bufferIndex + 1] = 1; // XRControllerData.enabled
          Module.HEAPF32[controller.bufferIndex + 2] = hand; // XRControllerData.hand
          
          switch (xrInputSourceEvent.type) {
            case "select":
              Module.HEAPF32[controller.bufferIndex + 10] = 1; // XRControllerData.trigger
              break;
            case "selectstart":
              Module.HEAPF32[controller.bufferIndex + 10] = 1; // XRControllerData.trigger
              break;
            case "selectend":
              Module.HEAPF32[controller.bufferIndex + 10] = 0; // XRControllerData.trigger
              break;
            case "squeeze":
              Module.HEAPF32[controller.bufferIndex + 11] = 1; // XRControllerData.squeeze
              break;
            case "squeezestart":
              Module.HEAPF32[controller.bufferIndex + 11] = 1; // XRControllerData.squeeze
              break;
            case "squeezeend":
              Module.HEAPF32[controller.bufferIndex + 11] = 0; // XRControllerData.squeeze
              break;
          }
          
          if (hand == 0 || hand == 2) {
            Module.HEAPF32[xrData.handRight.bufferIndex + 3] = Module.HEAPF32[controller.bufferIndex + 10]; // XRHandData.trigger
            Module.HEAPF32[xrData.handRight.bufferIndex + 4] = Module.HEAPF32[controller.bufferIndex + 11]; // XRHandData.squeeze
          } else {
            Module.HEAPF32[xrData.handLeft.bufferIndex + 3] = Module.HEAPF32[controller.bufferIndex + 10]; // XRHandData.trigger
            Module.HEAPF32[xrData.handLeft.bufferIndex + 4] = Module.HEAPF32[controller.bufferIndex + 11]; // XRHandData.squeeze
          }
        } else {
          var xPercentage = 0.5;
          var yPercentage = 0.5;
          var inputSource = xrInputSourceEvent.inputSource;
          if (inputSource) {
            if (inputSource.gamepad &&
                inputSource.gamepad.axes) {
              xPercentage = (inputSource.gamepad.axes[0] + 1.0) * 0.5;
              yPercentage = (inputSource.gamepad.axes[1] + 1.0) * 0.5;
            }
            switch (xrInputSourceEvent.type) {
              case "select": // 9 touchmove
                // no need to call touchmove here
                break;
              case "selectstart": // 7 touchstart
                inputSource.xrTouchObject = this.xrData.CreateTouch(this.canvas, xPercentage, yPercentage);
                this.xrData.SendTouchEvent(this.JSEventsObject, 7, "touchstart", this.canvas, [inputSource.xrTouchObject])
                break;
              case "selectend": // 8 touchend
                this.xrData.RemoveTouch(inputSource.xrTouchObject);
                this.xrData.SendTouchEvent(this.JSEventsObject, 8, "touchend", this.canvas, [inputSource.xrTouchObject]);
                inputSource.xrTouchObject = null;
                break;
            }
          }
        }
      }

      XRManager.prototype.onVisibilityChange = function (event) {
        this.gameModule.WebXR.OnVisibilityChange(this.xrSession.visibilityState);
      }

      XRManager.prototype.toggleAr = function () {
        if (!this.gameModule)
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
        if (!this.gameModule)
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
        if (!this.gameModule)
        {
          return;
        }
        if (this.xrSession && this.xrSession.isInSession && this.xrSession.isAR) {
          if (this.viewerHitTestSource) {
            this.viewerHitTestSource.cancel();
            this.viewerHitTestSource = null;
          } else {
            var thisXRMananger = this;
            this.xrSession.requestReferenceSpace('local').then(function (refSpace) {
              thisXRMananger.xrSession.localRefSpace = refSpace;
            });
            this.xrSession.requestReferenceSpace('viewer').then(function (refSpace) {
              thisXRMananger.viewerSpace = refSpace;
              thisXRMananger.xrSession.requestHitTestSource({space: thisXRMananger.viewerSpace}).then(function (hitTestSource) {
                thisXRMananger.viewerHitTestSource = hitTestSource;
              });
            });
          }
        }
      }
      
      XRManager.prototype.hapticPulse = function (hapticPulseAction) {
        var controller = null;
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
    
      XRManager.prototype.setGameModule = function (gameModule) {
        if (gameModule && !this.gameModule) {
          this.gameModule = gameModule;
          this.canvas = this.gameModule.canvas;
          this.ctx = this.gameModule.ctx;
    
          var thisXRMananger = this;
          this.JSEventsObject = this.gameModule.WebXR.GetJSEventsObject();
          this.BrowserObject = this.gameModule.WebXR.GetBrowserObject();
          this.BrowserObject.requestAnimationFrame = function (func) {
            if (thisXRMananger.xrSession && thisXRMananger.xrSession.isInSession) {
              return thisXRMananger.xrSession.requestAnimationFrame(function (time, xrFrame) {
                thisXRMananger.animate(xrFrame);
                func(time);
              });
            } else {
              window.requestAnimationFrame(func);
            }
          };
    
          // bindFramebuffer frameBufferObject null in XRSession should use XRWebGLLayer FBO instead
          thisXRMananger.ctx.oldBindFramebuffer = thisXRMananger.ctx.bindFramebuffer;
          thisXRMananger.ctx.bindFramebuffer = function (target, fbo) {
            if (!fbo) {
              if (thisXRMananger.xrSession && thisXRMananger.xrSession.isInSession) {
                if (thisXRMananger.xrSession.renderState.baseLayer) {
                  fbo = thisXRMananger.xrSession.renderState.baseLayer.framebuffer
                }
              }
            }
            return thisXRMananger.ctx.oldBindFramebuffer(target, fbo)
          };
        }
      }
    
      XRManager.prototype.unityLoaded = function (event) {
        Module.WebXR.unityLoaded = 'true';
    
        this.setGameModule(event.detail.module);
    
        document.dispatchEvent(new CustomEvent('onARSupportedCheck', { detail:{supported:this.isARSupported} }));
        document.dispatchEvent(new CustomEvent('onVRSupportedCheck', { detail:{supported:this.isVRSupported} }));
    
        this.UpdateXRCapabilities();
        
        this.onInputEvent = this.onInputSourceEvent.bind(this);
        this.onSessionVisibilityEvent = this.onVisibilityChange.bind(this);
      }
    
      XRManager.prototype.UpdateXRCapabilities = function() {
        // Send browser capabilities to Unity.
        this.gameModule.WebXR.OnXRCapabilities(
          JSON.stringify({
            canPresentAR: this.isARSupported,
            canPresentVR: this.isVRSupported
          })
        );
      }
      
      // http://answers.unity.com/answers/11372/view.html
      XRManager.prototype.quaternionFromMatrix = function(offset, matrix, quaternion) {
        quaternion[3] = Math.sqrt( Math.max( 0, 1 + matrix[offset+0] + matrix[offset+5] + matrix[offset+10] ) ) / 2; 
        quaternion[0] = Math.sqrt( Math.max( 0, 1 + matrix[offset+0] - matrix[offset+5] - matrix[offset+10] ) ) / 2; 
        quaternion[1] = Math.sqrt( Math.max( 0, 1 - matrix[offset+0] + matrix[offset+5] - matrix[offset+10] ) ) / 2; 
        quaternion[2] = Math.sqrt( Math.max( 0, 1 - matrix[offset+0] - matrix[offset+5] + matrix[offset+10] ) ) / 2; 
        quaternion[0] *= Math.sign( quaternion[0] * ( matrix[offset+6] - matrix[offset+9] ) );
        quaternion[1] *= Math.sign( quaternion[1] * ( matrix[offset+8] - matrix[offset+2] ) );
        quaternion[2] *= Math.sign( quaternion[2] * ( matrix[offset+1] - matrix[offset+4] ) );
      }
      
      XRManager.prototype.getXRControllersData = function(frame, inputSources, refSpace, xrData) {
        Module.HEAPF32[xrData.handLeft.bufferIndex] = xrData.frameNumber; // XRHandData.frame
        Module.HEAPF32[xrData.handRight.bufferIndex] = xrData.frameNumber; // XRHandData.frame
        Module.HEAPF32[xrData.handLeft.bufferIndex + 1] = 0; // XRHandData.enabled
        Module.HEAPF32[xrData.handRight.bufferIndex + 1] = 0; // XRHandData.enabled
        Module.HEAPF32[xrData.controllerA.bufferIndex] = xrData.frameNumber; // XRControllerData.frame
        Module.HEAPF32[xrData.controllerB.bufferIndex] = xrData.frameNumber; // XRControllerData.frame
        Module.HEAPF32[xrData.controllerA.bufferIndex + 1] = 0; // XRControllerData.enabled
        Module.HEAPF32[xrData.controllerB.bufferIndex + 1] = 0; // XRControllerData.enabled
        if (!inputSources || !inputSources.length || inputSources.length == 0) {
          this.removeRemainingTouches();
          return;
        }
        var touchesToSend = [];
        for (var i = 0; i < inputSources.length; i++) {
          var inputSource = inputSources[i];
          // Show the input source if it has a grip space
          if (inputSource.hand) {
            var xrHand = xrData.handLeft;
            Module.HEAPF32[xrHand.bufferIndex + 2] = 1; // XRHandData.hand
            if (inputSource.handedness == 'right') {
              xrHand = xrData.handRight;
              Module.HEAPF32[xrHand.bufferIndex + 2] = 2; // XRHandData.hand
            }
            Module.HEAPF32[xrHand.bufferIndex + 1] = 1; // XRHandData.enabled

            if (xrHand.handValuesType == 0) {
              if (inputSource.hand.values) {
                xrHand.handValuesType = 1
              } else {
                xrHand.handValuesType = 2
              }
            }
            if (!frame.fillPoses(
                xrHand.handValuesType == 1 ? inputSource.hand.values() : inputSource.hand,
                refSpace,
                xrHand.poses)) {
              Module.HEAPF32[xrHand.bufferIndex + 1] = 0; // XRHandData.enabled
              continue;
            }
            if (!xrHand.hasRadii)
            {
              xrHand.hasRadii = frame.fillJointRadii(
                xrHand.handValuesType == 1 ? inputSource.hand.values() : inputSource.hand,
                xrHand.radii);
            }
            for (var j = 0; j < 25; j++) {
              xrHand.jointIndex = j*16;
              xrHand.unityJointIndex = xrHand.bufferIndex + 5 + (j*8);
              if (!isNaN(xrHand.poses[xrHand.jointIndex])) {
                Module.HEAPF32[xrHand.unityJointIndex] = xrHand.poses[xrHand.jointIndex+12]; // XRJointData.position.x
                Module.HEAPF32[xrHand.unityJointIndex + 1] = xrHand.poses[xrHand.jointIndex+13]; // XRJointData.position.y
                Module.HEAPF32[xrHand.unityJointIndex + 2] = -xrHand.poses[xrHand.jointIndex+14]; // XRJointData.position.z
                this.quaternionFromMatrix(xrHand.jointIndex, xrHand.poses, xrHand.jointQuaternion);
                Module.HEAPF32[xrHand.unityJointIndex + 3] = -xrHand.jointQuaternion[0]; // XRJointData.rotation.x
                Module.HEAPF32[xrHand.unityJointIndex + 4] = -xrHand.jointQuaternion[1]; // XRJointData.rotation.y
                Module.HEAPF32[xrHand.unityJointIndex + 5] = xrHand.jointQuaternion[2]; // XRJointData.rotation.z
                Module.HEAPF32[xrHand.unityJointIndex + 6] = xrHand.jointQuaternion[3]; // XRJointData.rotation.w
                if (!isNaN(xrHand.radii[j])) {
                  Module.HEAPF32[xrHand.unityJointIndex + 7] = xrHand.radii[j]; // XRJointData.radius
                }
              }
            }
          } else if (inputSource.gripSpace) {
            var inputRayPose = frame.getPose(inputSource.targetRaySpace, refSpace);
            if (inputRayPose) {
              var position = inputRayPose.transform.position;
              var orientation = inputRayPose.transform.orientation;
              var hand = 0;
              var controller = xrData.controllerA;
              if (inputSource.handedness == 'left') {
                hand = 1;
                controller = xrData.controllerB;
              } else if (inputSource.handedness == 'right') {
                hand = 2;
              }
              
              Module.HEAPF32[controller.bufferIndex + 1] = 1; // XRControllerData.enabled
              Module.HEAPF32[controller.bufferIndex + 2] = hand; // XRControllerData.hand

              if (controller.updatedProfiles == 0) {
                controller.profiles = inputSource.profiles;
                controller.updatedProfiles = 1;
              }
              
              Module.HEAPF32[controller.bufferIndex + 3] = position.x; // XRControllerData.positionX
              Module.HEAPF32[controller.bufferIndex + 4] = position.y; // XRControllerData.positionY
              Module.HEAPF32[controller.bufferIndex + 5] = -position.z; // XRControllerData.positionZ
              
              Module.HEAPF32[controller.bufferIndex + 6] = -orientation.x; // XRControllerData.rotationX
              Module.HEAPF32[controller.bufferIndex + 7] = -orientation.y; // XRControllerData.rotationY
              Module.HEAPF32[controller.bufferIndex + 8] = orientation.z; // XRControllerData.rotationZ
              Module.HEAPF32[controller.bufferIndex + 9] = orientation.w; // XRControllerData.rotationW

              if (Module.HEAPF32[controller.bufferIndex + 20] == 0 && inputSource.gripSpace) { // XRControllerData.updatedGrip
                var inputPose = frame.getPose(inputSource.gripSpace, refSpace);
                if (inputPose) {
                  var gripPosition = inputPose.transform.position;
                  var gripOrientation = inputPose.transform.orientation;

                  Module.HEAPF32[controller.bufferIndex + 21] = gripPosition.x; // XRControllerData.gripPositionX
                  Module.HEAPF32[controller.bufferIndex + 22] = gripPosition.y; // XRControllerData.gripPositionY
                  Module.HEAPF32[controller.bufferIndex + 23] = -gripPosition.z; // XRControllerData.gripPositionZ

                  Module.HEAPF32[controller.bufferIndex + 24] = -gripOrientation.x; // XRControllerData.gripRotationX
                  Module.HEAPF32[controller.bufferIndex + 25] = -gripOrientation.y; // XRControllerData.gripRotationY
                  Module.HEAPF32[controller.bufferIndex + 26] = gripOrientation.z; // XRControllerData.gripRotationZ
                  Module.HEAPF32[controller.bufferIndex + 27] = gripOrientation.w; // XRControllerData.gripRotationW

                  Module.HEAPF32[controller.bufferIndex + 20] = 1; // XRControllerData.updatedGrip
                }
              }
              
              var triggerIndex = controller.bufferIndex + 10;
              var squeezeIndex = controller.bufferIndex + 11;
              // if there's gamepad, use the xr-standard mapping
              if (inputSource.gamepad) {
                for (var j = 0; j < inputSource.gamepad.buttons.length; j++) {
                  switch (j) {
                    case 0:
                      Module.HEAPF32[triggerIndex] = inputSource.gamepad.buttons[j].value; // XRControllerData.trigger
                      break;
                    case 1:
                      Module.HEAPF32[squeezeIndex] = inputSource.gamepad.buttons[j].value; // XRControllerData.squeeze
                      break;
                    case 2:
                      Module.HEAPF32[controller.bufferIndex + 15] = inputSource.gamepad.buttons[j].value; // XRControllerData.touchpad
                      break;
                    case 3:
                      Module.HEAPF32[controller.bufferIndex + 12] = inputSource.gamepad.buttons[j].value; // XRControllerData.thumbstick
                      break;
                    case 4:
                      Module.HEAPF32[controller.bufferIndex + 18] = inputSource.gamepad.buttons[j].value; // XRControllerData.buttonA
                      break;
                    case 5:
                      Module.HEAPF32[controller.bufferIndex + 19] = inputSource.gamepad.buttons[j].value; // XRControllerData.buttonB
                      break;
                  }
                }
                
                if (Module.HEAPF32[triggerIndex] <= 0.02) {
                  Module.HEAPF32[triggerIndex] = 0;
                } else if (Module.HEAPF32[triggerIndex] >= 0.98) {
                  Module.HEAPF32[triggerIndex] = 1;
                }
                
                if (Module.HEAPF32[squeezeIndex] <= 0.02) {
                  Module.HEAPF32[squeezeIndex] = 0;
                } else if (Module.HEAPF32[squeezeIndex] >= 0.98) {
                  Module.HEAPF32[squeezeIndex] = 1;
                }
                
                for (var j = 0; j < inputSource.gamepad.axes.length; j++) {
                  switch (j) {
                    case 0:
                      Module.HEAPF32[controller.bufferIndex + 16] = inputSource.gamepad.axes[j]; // XRControllerData.touchpadX
                      break;
                    case 1:
                      Module.HEAPF32[controller.bufferIndex + 17] = -inputSource.gamepad.axes[j]; // XRControllerData.touchpadY
                      break;
                    case 2:
                      Module.HEAPF32[controller.bufferIndex + 13] = inputSource.gamepad.axes[j]; // XRControllerData.thumbstickX
                      break;
                    case 3:
                      Module.HEAPF32[controller.bufferIndex + 14] = -inputSource.gamepad.axes[j]; // XRControllerData.thumbstickY
                      break;
                  }
                }
              }
              controller.gamepad = inputSource.gamepad;
            }
          } else if (inputSource.xrTouchObject && !inputSource.xrTouchObject.ended && inputSource.gamepad && inputSource.gamepad.axes) {
            inputSource.xrTouchObject.UpdateTouch( this.canvas,
                                                   (inputSource.gamepad.axes[0] + 1.0) * 0.5,
                                                   (inputSource.gamepad.axes[1] + 1.0) * 0.5);
            if (inputSource.xrTouchObject.HasMovement()) {
              touchesToSend.push(inputSource.xrTouchObject);
            }
          }
        }
        if (touchesToSend.length > 0) {
          this.xrData.SendTouchEvent(this.JSEventsObject, 9, "touchmove", this.canvas, touchesToSend);
          for (var i = 0; i < touchesToSend.length; i++) {
            touchesToSend[i].ResetMovement();
          }
        }
      }
    
      XRManager.prototype.onSessionStarted = function (session) {
        var glLayer = new XRWebGLLayer(session, this.ctx);
        session.updateRenderState({ baseLayer: glLayer });
        
        var refSpaceType = 'viewer';
        if (session.isImmersive) {
          refSpaceType = this.gameModule.WebXR.Settings.VRRequiredReferenceSpace[0];
          if (session.isAR) {
            refSpaceType = this.gameModule.WebXR.Settings.ARRequiredReferenceSpace[0];
          }
    
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
          session.addEventListener('visibilitychange', this.onSessionVisibilityEvent);
    
          this.xrData.controllerA.bufferIndex = Module.ControllersArrayOffset;
          this.xrData.controllerB.bufferIndex = Module.ControllersArrayOffset + 28;
          this.xrData.handLeft.bufferIndex = Module.HandsArrayOffset;
          this.xrData.handRight.bufferIndex = Module.HandsArrayOffset + 205;
          this.xrData.controllerA.updatedProfiles = 0;
          this.xrData.controllerB.updatedProfiles = 0;
          this.xrData.controllerA.profiles = [];
          this.xrData.controllerB.profiles = [];
          Module.HEAPF32[this.xrData.controllerA.bufferIndex + 20] = 0; // XRControllerData.updatedGrip
          Module.HEAPF32[this.xrData.controllerB.bufferIndex + 20] = 0; // XRControllerData.updatedGrip
        }
        var thisXRMananger = this;
        session.requestReferenceSpace(refSpaceType).then(function (refSpace) {
          session.refSpace = refSpace;
          var tempRaf = function (time, xrFrame) {
            if (thisXRMananger.animate(xrFrame))
            {
              thisXRMananger.BrowserObject.resumeAsyncCallbacks();
              thisXRMananger.BrowserObject.mainLoop.resume();
            } else {
              // No XR session yet
              session.requestAnimationFrame(tempRaf);
            }
          }
          session.requestAnimationFrame(tempRaf);
        });
      }
    
      XRManager.prototype.animate = function (frame) {
        var session = frame.session;
        if (!session) {
          return this.didNotifyUnity;
        }
        
        var glLayer = session.renderState.baseLayer;
        
        if (this.canvas.width != glLayer.framebufferWidth ||
            this.canvas.height != glLayer.framebufferHeight)
        {
          this.canvas.width = glLayer.framebufferWidth;
          this.canvas.height = glLayer.framebufferHeight;
        }
        
        this.ctx.bindFramebuffer(this.ctx.FRAMEBUFFER, glLayer.framebuffer);
        if (session.isAR) {
          this.ctx.dontClearOnFrameStart = true;
          // Workaround for Chromium depth bug https://bugs.chromium.org/p/chromium/issues/detail?id=1167450#c21
          this.ctx.depthMask(false);
          this.ctx.clear(this.ctx.DEPTH_BUFFER_BIT);
          this.ctx.depthMask(true);
        } else {
          this.ctx.clear(this.ctx.COLOR_BUFFER_BIT | this.ctx.DEPTH_BUFFER_BIT);
        }
        
        var pose = frame.getViewerPose(session.refSpace);
        if (!pose) {
          return this.didNotifyUnity;
        }
    
        if (!session.isImmersive)
        {
          return this.didNotifyUnity;
        }
    
        var xrData = this.xrData;
        xrData.frameNumber++;
    
        for (var i = 0; i < pose.views.length; i++) {
          var view = pose.views[i];
          var transformMatrix = view.transform.matrix;
          if (view.eye === "left" || view.eye === "none") {
            xrData.leftProjectionMatrix = view.projectionMatrix;
            this.quaternionFromMatrix(0, transformMatrix, xrData.leftViewRotation);
            xrData.leftViewRotation[0] = -xrData.leftViewRotation[0];
            xrData.leftViewRotation[1] = -xrData.leftViewRotation[1];
            xrData.leftViewPosition[0] = transformMatrix[12];
            xrData.leftViewPosition[1] = transformMatrix[13];
            xrData.leftViewPosition[2] = -transformMatrix[14];
          } else if (view.eye === 'right') {
            xrData.rightProjectionMatrix = view.projectionMatrix;
            this.quaternionFromMatrix(0, transformMatrix, xrData.rightViewRotation);
            xrData.rightViewRotation[0] = -xrData.rightViewRotation[0];
            xrData.rightViewRotation[1] = -xrData.rightViewRotation[1];
            xrData.rightViewPosition[0] = transformMatrix[12];
            xrData.rightViewPosition[1] = transformMatrix[13];
            xrData.rightViewPosition[2] = -transformMatrix[14];
          }
        }
    
        this.getXRControllersData(frame, session.inputSources, session.refSpace, xrData);
    
        if (session.isAR && this.viewerHitTestSource) {
          xrData.viewerHitTestPose.frame = xrData.frameNumber;
          var viewerHitTestResults = frame.getHitTestResults(this.viewerHitTestSource);
          if (viewerHitTestResults.length > 0) {
            var hitTestPose = viewerHitTestResults[0].getPose(session.localRefSpace);
            xrData.viewerHitTestPose.available = 1;
            xrData.viewerHitTestPose.position[0] = hitTestPose.transform.position.x;
            var hitTestPoseBase = viewerHitTestResults[0].getPose(session.refSpace); // Ugly hack for y position on Samsung Internet
            xrData.viewerHitTestPose.position[1] = hitTestPose.transform.position.y + Math.abs(hitTestPose.transform.position.y - hitTestPoseBase.transform.position.y);
            xrData.viewerHitTestPose.position[2] = -hitTestPose.transform.position.z;
            xrData.viewerHitTestPose.rotation[0] = -hitTestPose.transform.orientation.x;
            xrData.viewerHitTestPose.rotation[1] = -hitTestPose.transform.orientation.y;
            xrData.viewerHitTestPose.rotation[2] = hitTestPose.transform.orientation.z;
            xrData.viewerHitTestPose.rotation[3] = hitTestPose.transform.orientation.w;
          } else {
            xrData.viewerHitTestPose.available = 0;
          }
          this.updateUnityXRViewerHitTestPose({
            viewerHitTestPose: xrData.viewerHitTestPose
          });
        }
    
        if (xrData.controllerA.updatedProfiles == 1 || xrData.controllerB.updatedProfiles == 1)
        {
          var inputProfiles = {};
          inputProfiles.conrtoller1 = xrData.controllerA.profiles;
          inputProfiles.conrtoller2 = xrData.controllerB.profiles;
          if (xrData.controllerA.updatedProfiles == 1)
          {
            xrData.controllerA.updatedProfiles = 2;
          }
          if (xrData.controllerB.updatedProfiles == 1)
          {
            xrData.controllerB.updatedProfiles = 2;
          }
          this.gameModule.WebXR.OnInputProfiles(JSON.stringify(inputProfiles));
        }
        
        // Dispatch event with headset data to be handled in webxr.jslib
        this.updateUnityXRData({
          leftProjectionMatrix: xrData.leftProjectionMatrix,
          rightProjectionMatrix: xrData.rightProjectionMatrix,
          leftViewRotation: xrData.leftViewRotation,
          rightViewRotation: xrData.rightViewRotation,
          leftViewPosition: xrData.leftViewPosition,
          rightViewPosition: xrData.rightViewPosition
        });
        
        if (!this.didNotifyUnity)
        {
          var eyeCount = 1;
          var leftRect = {
            x:0,
            y:0,
            w:1,
            h:1
          }
          var rightRect = {
            x:0.5,
            y:0,
            w:0.5,
            h:1
          }
          for (var i = 0; i < pose.views.length; i++) {
            var view = pose.views[i];
            var viewport = session.renderState.baseLayer.getViewport(view);
            if (view.eye === 'left') {
              if (viewport) {
                leftRect.x = (viewport.x / glLayer.framebufferWidth) * (glLayer.framebufferWidth / this.canvas.width);
                leftRect.y = (viewport.y / glLayer.framebufferHeight) * (glLayer.framebufferHeight / this.canvas.height);
                leftRect.w = (viewport.width / glLayer.framebufferWidth) * (glLayer.framebufferWidth / this.canvas.width);
                leftRect.h = (viewport.height / glLayer.framebufferHeight) * (glLayer.framebufferHeight / this.canvas.height);
              }
            } else if (view.eye === 'right' && viewport.width != 0 && viewport.height != 0) {
              eyeCount = 2;
              if (viewport) {
                rightRect.x = (viewport.x / glLayer.framebufferWidth) * (glLayer.framebufferWidth / this.canvas.width);
                rightRect.y = (viewport.y / glLayer.framebufferHeight) * (glLayer.framebufferHeight / this.canvas.height);
                rightRect.w = (viewport.width / glLayer.framebufferWidth) * (glLayer.framebufferWidth / this.canvas.width);
                rightRect.h = (viewport.height / glLayer.framebufferHeight) * (glLayer.framebufferHeight / this.canvas.height);
              }
            }
          }
          if (session.isAR)
          {
            this.gameModule.WebXR.OnStartAR(eyeCount, leftRect, rightRect);
          } else {
            this.gameModule.WebXR.OnStartVR(eyeCount, leftRect, rightRect);
          }
          this.gameModule.WebXR.OnVisibilityChange(session.visibilityState);
          this.didNotifyUnity = true;
        }
        return this.didNotifyUnity;
      }

      XRManager.prototype.updateUnityXRData = function (data) {
        var index = 0;
        if (Module.XRSharedArray.byteLength == 0) {
          Module.XRSharedArray = new Float32Array(buffer, Module.XRSharedArrayOffset, Module.XRSharedArrayLength);
        }
        Object.keys(data).forEach(function (key, i) {
          var dataLength = data[key].length;
          if (dataLength) {
            for (var x = 0; x < dataLength; x++) {
              Module.XRSharedArray[index++] = data[key][x];
            }
          }
        });
      }

      XRManager.prototype.updateUnityXRViewerHitTestPose = function (data) {
        var index = 0;
        if (Module.ViewerHitTestPoseArray.byteLength == 0) {
          Module.ViewerHitTestPoseArray = new Float32Array(buffer, Module.ViewerHitTestPoseArrayOffset, Module.ViewerHitTestPoseArrayLength);
        }
        Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.frame;
        Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.available;
        Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.position[0];
        Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.position[1];
        Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.position[2];
        Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.rotation[0];
        Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.rotation[1];
        Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.rotation[2];
        Module.ViewerHitTestPoseArray[index++] = data.viewerHitTestPose.rotation[3];
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

}, 0);

Module['WebXR'].GetBrowserObject = function () {
  return Browser;
}

Module['WebXR'].GetJSEventsObject = function () {
  return JSEvents;
}

Module['WebXR'].OnStartAR = function (views_count, left_rect, right_rect) {
  Module.WebXR.isInXR = true;
  this.OnStartARInternal = this.OnStartARInternal || Module.cwrap("on_start_ar", null, ["number",
                                                                  "number", "number", "number", "number",
                                                                  "number", "number", "number", "number"]);
  this.OnStartARInternal(views_count,
                          left_rect.x, left_rect.y, left_rect.w, left_rect.h,
                          right_rect.x, right_rect.y, right_rect.w, right_rect.h);
}

Module['WebXR'].OnStartVR = function (views_count, left_rect, right_rect) {
  Module.WebXR.isInXR = true;
  this.OnStartVRInternal = this.OnStartVRInternal || Module.cwrap("on_start_vr", null, ["number",
                                                                  "number", "number", "number", "number",
                                                                  "number", "number", "number", "number"]);
  this.OnStartVRInternal(views_count,
                          left_rect.x, left_rect.y, left_rect.w, left_rect.h,
                          right_rect.x, right_rect.y, right_rect.w, right_rect.h);
}

Module['WebXR'].OnVisibilityChange = function (visibility_state) {
  this.OnVisibilityChangeInternal = this.OnVisibilityChangeInternal || Module.cwrap("on_visibility_change", null, ["number"]);
  var visibility_state_int = 0;
  if (visibility_state == "visible-blurred") {
    visibility_state_int = 1;
  } else if (visibility_state == "hidden") {
    visibility_state_int = 2;
  }
  this.OnVisibilityChangeInternal(visibility_state_int);
}

Module['WebXR'].OnEndXR = function () {
  Module.WebXR.isInXR = false;
  this.OnEndXRInternal = this.OnEndXRInternal || Module.cwrap("on_end_xr", null, []);
  this.OnEndXRInternal();
}

Module['WebXR'].OnXRCapabilities = function (display_capabilities) {
  this.OnXRCapabilitiesInternal = this.OnXRCapabilitiesInternal || Module.cwrap("on_xr_capabilities", null, ["string"]);
  this.OnXRCapabilitiesInternal(display_capabilities);
}

Module['WebXR'].OnInputProfiles = function (input_profiles) {
  this.OnInputProfilesInternal = this.OnInputProfilesInternal || Module.cwrap("on_input_profiles", null, ["string"]);
  this.OnInputProfilesInternal(input_profiles);
}

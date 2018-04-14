(function () {
  'use strict';

  var defaultHeight = 1.5;

  var enterVRButton = document.getElementById('entervr');
  var gameContainer = document.getElementById('game');
  var vrHardwareStatus = document.getElementById('status');
  var statusIcons = document.getElementById('icons');
  var controllerIconTemplate = document.getElementById('motion-controller');
  var noVRInstructions = document.getElementById('novr');

  var windowRaf = window.requestAnimationFrame;
  var vrDisplay = null;
  var canvas = null;
  var frameData = null;
  var submitNextFrame = false;
  var testTimeStart = null;
  var leftProjectionMatrix = mat4.create();
  var rightProjectionMatrix = mat4.create();
  var leftViewMatrix = mat4.create();
  var rightViewMatrix = mat4.create();
  var sitStand = mat4.create();
  var gamepads = [];
  var vrGamepads = [];
  var toggleVRKeyName = '';
  var vrPolyfill = new WebVRPolyfill();

  if ('serviceWorker' in navigator && 'isSecureContext' in window && !window.isSecureContext) {
    console.warn('The site is insecure; Service Workers will not work and the site will not be recognized as a PWA');
  } else if ('serviceWorker' in navigator) {
    if (navigator.serviceWorker.controller) {
      console.log('Running active Service Worker (controller: %s)', navigator.serviceWorker.controller.scriptURL);
    } else {
      navigator.serviceWorker.register('./sw.js').then(function (registration) {
        console.log('Successfully registered Service Worker (scope: %s)', registration.scope);
      }, function (err) {
        console.warn('Failed to register Service Worker:\n', err);
      });
    }
  }

  function onUnityLoaded () {
    MozillaResearch.telemetry.performance.measure('LoadingTime', 'LoadingStart');
    canvas = document.getElementById('#canvas');
    document.body.dataset.unityLoaded = 'true';
    onResize();
    getVRDisplay();
  }

  function onUnity (msg) {
    // This way of passing messages is deprecated. Use rich objects instead.
    if (typeof msg.detail === 'string') {
      // Measure Round-Trip Time from Unity.
      if (msg.detail === 'Timer') {
        var delta = window.performance.now() - testTimeStart;
        console.log('return time (ms): ',delta);
        testTimeStart = null;
        return;
      }

      // Wait for Unity to render the frame; then submit the frame to the VR display.
      if (msg.detail === 'PostRender') {
        submitNextFrame = vrDisplay && vrDisplay.isPresenting;
        if (submitNextFrame) {
          vrDisplay.requestAnimationFrame(onAnimate);
        }
      }

      // Handle quick VR/normal toggling.
      if (msg.detail.indexOf('ConfigureToggleVRKeyName') === 0) {
        toggleVRKeyName = msg.detail.split(':')[1];
      }
    }

    // Handle an UI command
    if (msg.detail.type === 'ShowPanel') {
      var panelId = document.getElementById(msg.detail.panelId);
      showInstruction(panelId);
    }
  }

  function onToggleVR() {
    if (vrDisplay && vrDisplay.isPresenting) {
      console.log('Toggled to exit VR mode');
      onExitPresent();
    } else {
      console.log('Toggled to enter VR mode');
      onRequestPresent();
    }
  }

  function onRequestPresent() {
    if (!vrDisplay) {
      throw new Error('No VR display available to enter VR mode');
    }
    if (!vrDisplay.capabilities || !vrDisplay.capabilities.canPresent) {
      throw new Error('VR display is not capable of presenting');
    }
    return vrDisplay.requestPresent([{source: canvas}]).then(function () {
      // Start stereo rendering in Unity.
      console.log('Entered VR mode');
      gameInstance.SendMessage('WebVRCameraSet', 'OnStartVR');
    }).catch(function (err) {
      console.error('Unable to enter VR mode:', err);
    });
  }

  function onExitPresent () {
    if (!vrDisplay && !vrDisplay.isPresenting) {
      console.warn('No VR display to exit VR mode');
      return;
    }
    function done () {
      // End stereo rendering in Unity.
      gameInstance.SendMessage('WebVRCameraSet', 'OnEndVR');
      onResize();
    }
    return vrDisplay.exitPresent().then(function () {
      console.log('Exited VR mode');
      done();
    }).catch(function (err) {
      console.error('Unable to exit VR mode:', err);
      done();
    });
  }

  function onAnimate () {
    if (!vrDisplay || !vrDisplay.isPresenting) {
      windowRaf(onAnimate);
    }

    if (vrDisplay) {
      // When this is called for the first time, it will be using the standard
      // `window.requestAnimationFrame` API, which will throw a Console warning when we call
      // `vrDisplay.submitFrame(…)`. So for the first frame that this is called, we will
      // abort early and request a new frame from the VR display instead.
      if (vrDisplay.isPresenting && !submitNextFrame) {
        submitNextFrame = true;
        return vrDisplay.requestAnimationFrame(onAnimate);
      }

      // Check for polyfill so that we can utilize its mouse-look controls.
      if (vrDisplay.isPresenting || isPolyfilled(vrDisplay)) {
        vrDisplay.getFrameData(frameData);

        // convert view and projection matrices for use in Unity.
        mat4.copy(leftProjectionMatrix, frameData.leftProjectionMatrix);
        mat4.transpose(leftProjectionMatrix, leftProjectionMatrix);

        mat4.copy(rightProjectionMatrix, frameData.rightProjectionMatrix);
        mat4.transpose(rightProjectionMatrix, rightProjectionMatrix);

        mat4.copy(leftViewMatrix, frameData.leftViewMatrix);
        mat4.transpose(leftViewMatrix, leftViewMatrix);
        leftViewMatrix[2] *= -1;
        leftViewMatrix[6] *= -1;
        leftViewMatrix[10] *= -1;
        leftViewMatrix[14] *= -1;

        mat4.copy(rightViewMatrix, frameData.rightViewMatrix);
        mat4.transpose(rightViewMatrix, rightViewMatrix);
        rightViewMatrix[2] *= -1;
        rightViewMatrix[6] *= -1;
        rightViewMatrix[10] *= -1;
        rightViewMatrix[14] *= -1;

        // Sit Stand transform
        if (vrDisplay.stageParameters) {
          mat4.copy(sitStand, vrDisplay.stageParameters.sittingToStandingTransform);
        } else {
          mat4.identity(sitStand);
          mat4.translate(sitStand, sitStand, [0, defaultHeight, 0]);
        }
        mat4.transpose(sitStand, sitStand);

        // gamepads
        gamepads = navigator.getGamepads();
        vrGamepads = [];
        for (var i = 0; i < gamepads.length; ++i) {
          var gamepad = gamepads[i];
          if (gamepad && (gamepad.pose || gamepad.displayId)) {
            if (gamepad.pose.position && gamepad.pose.orientation) {
              // flips gamepad axis to work with Unity.
              var position = gamepad.pose.position;
              position[2] *= -1;
              var orientation = gamepad.pose.orientation;
              orientation[0] *= -1;
              orientation[1] *= -1;

              var buttons = [];
              for (var j = 0; j < gamepad.buttons.length; j++) {
                buttons.push({
                  pressed: gamepad.buttons[j].pressed,
                  touched: gamepad.buttons[j].touched,
                  value: gamepad.buttons[j].value
                });
              }

              vrGamepads.push({
                index: gamepad.index,
                hand: gamepad.hand,
                buttons: buttons,
                orientation: Array.from(orientation),
                position: Array.from(position)
              });
            }
          }
        }

        var vrData = {
          leftProjectionMatrix: Array.from(leftProjectionMatrix),
          rightProjectionMatrix: Array.from(rightProjectionMatrix),
          leftViewMatrix: Array.from(leftViewMatrix),
          rightViewMatrix: Array.from(rightViewMatrix),
          sitStand: Array.from(sitStand),
          controllers: vrGamepads
        };

        gameInstance.SendMessage('WebVRCameraSet', 'OnWebVRData', JSON.stringify(vrData));
      }

      if (!vrDisplay.isPresenting || isPolyfilled(vrDisplay)) {
        submitNextFrame = false;
      }
      if (submitNextFrame) {
        vrDisplay.submitFrame();
      }

      updateStatus();
    }
  }

  function onResize() {
    if (!canvas) return;

    if (vrDisplay && vrDisplay.isPresenting) {
      var leftEye = vrDisplay.getEyeParameters('left');
      var rightEye = vrDisplay.getEyeParameters('right');
      var renderWidth = Math.max(leftEye.renderWidth, rightEye.renderWidth) * 2;
      var renderHeight = Math.max(leftEye.renderHeight, rightEye.renderHeight);
      canvas.width = renderWidth;
      canvas.height = renderHeight;

      // scale game container so we get a proper sized mirror of VR content to desktop.
      var scaleX = window.innerWidth / renderWidth;
      var scaleY = window.innerHeight / renderHeight;
      gameContainer.setAttribute('style', `transform: scale(${scaleX}, ${scaleY}); transform-origin: top left;`);
    } else {
      canvas.width = window.innerWidth;
      canvas.height = window.innerHeight;
      gameContainer.style.transform = '';
    }
  }

  function togglePerf() {
    gameInstance.SendMessage('WebVRCameraSet', 'TogglePerf');
  }

  function testRoundtripTime() {
    console.log('Testing roundtrip time...');
    testTimeStart = window.performance.now();
    gameInstance.SendMessage('WebVRCameraSet', 'TestTime');
  }

  function showInstruction (el) {
    if (el.dataset.enabled) { return; }
    var confirmButton = el.querySelector('button');
    el.dataset.enabled = true;
    confirmButton.addEventListener('click', onConfirm);
    function onConfirm () {
      el.dataset.enabled = false;
      confirmButton.removeEventListener('click', onConfirm);
    }
  }

  function updateStatus () {
    if (parseInt(vrHardwareStatus.dataset.gamepads) !== vrGamepads.length) {
      var controllerClassName = 'controller-icon';
      var controlIcons = statusIcons.getElementsByClassName(controllerClassName);
      while (controlIcons.length > 0) {
        controlIcons[0].parentNode.removeChild(controlIcons[0]);
      }

      vrGamepads.forEach(function (gamepad) {
        var controllerIcon = document.importNode(controllerIconTemplate.content, true);
        controllerIcon.querySelector('img').className = controllerClassName;
        statusIcons.appendChild(controllerIcon);
      });
      vrHardwareStatus.dataset.gamepads = vrGamepads.length;
    }
  }

  // Unity drives its rendering from the window `rAF`. We reassign to use `VRDisplay`'s `rAF` when presenting
  // such that Unity renders at the VR display's proper framerate.
  function onRequestAnimationFrame(cb) {
    if (vrDisplay && vrDisplay.isPresenting) {
      submitNextFrame = true;
      return vrDisplay.requestAnimationFrame(cb);
    } else {
      return windowRaf(cb);
    }
  }

  function getVRDisplay () {
    if (!navigator.getVRDisplays) {
      console.warn('Your browser does not support WebVR');
      return;
    }
    frameData = new VRFrameData();

    return navigator.getVRDisplays().then(function(displays) {
      var canPresent = false;
      var hasPosition = false;
      var hasOrientation = false;
      var hasExternalDisplay = false;

      if (displays.length) {
        vrDisplay = displays[displays.length - 1];
        canPresent = vrDisplay.capabilities.canPresent;
        hasPosition = vrDisplay.capabilities.hasPosition;
        hasOrientation = vrDisplay.capabilities.hasOrientation;
        hasExternalDisplay = vrDisplay.capabilities.hasExternalDisplay;
      }

      if (canPresent) {
        vrHardwareStatus.dataset.enabled = true;
      }

      enterVRButton.dataset.enabled = canPresent;

      gameInstance.SendMessage(
        'WebVRCameraSet', 'OnVRCapabilities',
        JSON.stringify({
          canPresent: canPresent,
          hasPosition: hasPosition,
          hasOrientation: hasOrientation,
          hasExternalDisplay: hasExternalDisplay
        })
      );

      return vrDisplay;
    }).catch(function (err) {
      console.error('Error occurred getting VR display:', err);
    });
  }

  // Check to see if we are using polyfill.
  function isPolyfilled(display) {
    return display.isPolyfilled;
  }

  function onKeyUp(evt) {
    if (toggleVRKeyName && toggleVRKeyName === evt.key) {
      onToggleVR();
    }
  }

  // Monkeypatch `rAF` so that we can render at the VR display's framerate.
  window.requestAnimationFrame = onRequestAnimationFrame;

  window.addEventListener('resize', onResize, true);
  window.addEventListener('vrdisplaypresentchange', onResize, false);
  window.addEventListener('vrdisplayactivate', onRequestPresent, false);
  window.addEventListener('vrdisplaydeactivate', onExitPresent, false);
  window.addEventListener('keyup', onKeyUp, false);
  document.addEventListener('UnityLoaded', onUnityLoaded, false);
  document.addEventListener('Unity', onUnity);
  enterVRButton.addEventListener('click', onToggleVR, false);

  onResize();

  window.requestAnimationFrame(onAnimate);
})();

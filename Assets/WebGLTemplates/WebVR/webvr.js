(function () {
  'use strict';

  var defaultHeight = 1.5;
  var entervrButton = document.querySelector('#entervr');
  var container = document.querySelector('#game');
  var status = document.querySelector('#status');
  var icons = document.querySelector('#icons');
  var controller = document.querySelector('#motion-controller');
  var windowRaf = window.requestAnimationFrame;
  var vrDisplay = null;
  var canvas = null;
  var frameData = null;
  var testTimeStart = null;
  var leftProjectionMatrix = mat4.create();
  var rightProjectionMatrix = mat4.create();
  var leftViewMatrix = mat4.create();
  var rightViewMatrix = mat4.create();
  var sitStand = mat4.create();
  var gamepads = [];
  var vrGamepads = [];

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

  function onUnityLoaded() {
    canvas = document.getElementById('#canvas');
    document.body.dataset.unityLoaded = 'true';
    onResize();
  }

  function onUnity(msg) {
    if (msg.detail === "Ready") {
      // Get and hide Unity's canvas instance
      getVRDisplays();
    }

    // measures round-trip time from Unity.
    if (msg.detail === "Timer") {
      var delta = performance.now() - testTimeStart;
      console.log('return time (ms): ',delta);
      testTimeStart = null;
    }

    // Wait for Unity to render frame, then submit to vrDisplay.
    if (msg.detail === "PostRender") {
      if (vrDisplay && vrDisplay.isPresenting) {
        vrDisplay.submitFrame();
      }
    }
  }

  function onToggleVR() {
    if (!vrDisplay.isPresenting) {
      console.log('toggle present')
      onRequestPresent();
    } else {
      console.log('toggle exit present')
      onExitPresent();
    }
  }

  function onRequestPresent() {
    vrDisplay.requestPresent([{ source: canvas }]).then(function() {
      // starts stereo rendering in Unity.
      console.log('vr request present success!');
      gameInstance.SendMessage('WebVRCameraSet', 'Begin');
    });
  }

  function onExitPresent() {
    vrDisplay.exitPresent().then(function () {
      console.log('vr exit present success!')
    });
    // ends stereo rendering in Unity.
    gameInstance.SendMessage('WebVRCameraSet', 'End');
    onResize();
  }

  function onAnimate() {
    // RAF has been shimmed, See onRequestAnimationFrame function
    window.requestAnimationFrame(onAnimate);

    if (vrDisplay) {
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

            vrGamepads.push({
              index: gamepad.index,
              hand: gamepad.hand,
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

      gameInstance.SendMessage('WebVRCameraSet', 'WebVRData', JSON.stringify(vrData));

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
      container.setAttribute('style', `transform: scale(${scaleX}, ${scaleY}); transform-origin: top left;`);
    } else {
      canvas.width = window.innerWidth;
      canvas.height = window.innerHeight;
      container.style.transform = '';
    }
  }

  function togglePerf() {
    gameInstance.SendMessage('WebVRCameraSet', 'TogglePerf');
  }

  function testRoundtripTime() {
    console.log("Testing roundtrip time...");
    testTimeStart = performance.now();
    gameInstance.SendMessage('WebVRCameraSet', 'TestTime');
  }

  function showInstruction(el) {
    var confirmButton = el.querySelector('button');
    el.dataset.enabled = true;
    confirmButton.addEventListener('click', onConfirm);
    function onConfirm() {
      el.dataset.enabled = false;
      confirmButton.removeEventListener('click', onConfirm);
    }
  };

  function updateStatus() {
    if (parseInt(status.dataset.gamepads) !== vrGamepads.length) {
      var controllerClassName = 'controller-icon';
      var controlIcons = icons.getElementsByClassName(controllerClassName);
      while(controlIcons.length > 0) {
        controlIcons[0].parentNode.removeChild(controlIcons[0]);
      };

      vrGamepads.forEach(function (gamepad, i) {
        var controllerIcon = document.importNode(controller.content, true);
        controllerIcon.querySelector('img').className = controllerClassName;
        icons.appendChild(controllerIcon);
      })
      status.dataset.gamepads = vrGamepads.length;
    }
  }

  // Unity drives its rendering from the window RAF, we'll reassign to use
  // VRDisplay RAF when presenting so that Unity renders at the appropriate
  // VR framerate.
  function onRequestAnimationFrame(cb) {
    if (vrDisplay && vrDisplay.isPresenting) {
      return vrDisplay.requestAnimationFrame(cb);
    } else {
      return windowRaf(cb);
    }
  }

  function getVRDisplays() {
    if (navigator.getVRDisplays) {
      frameData = new VRFrameData();

      navigator.getVRDisplays().then(function(displays) {
        if (displays.length > 0) {
          vrDisplay = displays[displays.length - 1];

          // Check to see if we are polyfilled.
          var isPolyfilled = (vrDisplay.deviceId || '').indexOf('polyfill') > 0 ||
            (vrDisplay.displayName || '').indexOf('polyfill') > 0 ||
            (vrDisplay.deviceName || '').indexOf('polyfill') > 0 ||
            vrDisplay.hardwareUnitId;

          if (isPolyfilled) {
            showInstruction(document.querySelector('#novr'));
          } else {
            status.dataset.enabled = 'true';
          }

          // enables enter VR button
          if (vrDisplay.capabilities.canPresent) {
            entervrButton.dataset.enabled = 'true';
          }
        }
      });
    } else {
      console.log('Your browser does not support WebVR!');
    }
  }

  // shim raf so that we can drive framerate using VR display.
  window.requestAnimationFrame = onRequestAnimationFrame;

  window.addEventListener('resize', onResize, true);
  window.addEventListener('vrdisplaypresentchange', onResize, false);
  window.addEventListener('vrdisplayactivate', onRequestPresent, false);
  window.addEventListener('vrdisplaydeactivate', onExitPresent, false);
  document.addEventListener('UnityLoaded', onUnityLoaded, false);
  document.addEventListener('Unity', onUnity);
  entervrButton.addEventListener('click', onToggleVR, false);
  onResize();
  window.requestAnimationFrame(onAnimate);
})();

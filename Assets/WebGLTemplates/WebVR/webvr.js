(function () {
  'use strict';

  var defaultHeight = 1.5;
  var entervrButton = document.querySelector('#entervr');
  var container = document.querySelector('#game');
  var loader = document.querySelector('#loader');
  var status = document.querySelector('#status');
  var icons = document.querySelector('#icons');
  var controller = document.querySelector('#motion-controller');
  var windowRaf = window.requestAnimationFrame;
  var vrDisplay = null;
  var canvas = null;
  var frameData = null;
  var inVR = false;
  var isPresenting = false;
  var testTimeStart = null;
  var leftProjectionMatrix = mat4.create();
  var rightProjectionMatrix = mat4.create();
  var leftViewMatrix = mat4.create();
  var rightViewMatrix = mat4.create();
  var sitStand = mat4.create();
  var gamepads = [];
  var vrGamepads = [];

  function getVRDisplays() {
    if (navigator.getVRDisplays) {
      navigator.getVRDisplays().then(function(displays) {
        if (displays.length > 0) {
          vrDisplay = displays[displays.length - 1];

          // check to see if we are polyfilled
          if (vrDisplay.displayName.indexOf('polyfill') > 0) {
            showInstruction(document.querySelector('#novr'));
          } else {
            status.dataset.enabled = true;
          }
          onResize();
          onAnimate();
        }

        if (vrDisplay.capabilities.canPresent) {
          entervrButton.dataset.enabled = 'true';
        }
      });
    } else {
      console.log('Your browser does not support WebVR!');
    }
  }

  function onUnity(msg) {
    if (msg.detail === "Ready") {
      // Get and hide Unity's canvas instance
      canvas = document.getElementById('#canvas');
      loader.dataset.complete = 'true';
      getVRDisplays();
    }

    // measures round-trip time from Unity.
    if (msg.detail === "Timer") {
      var delta = performance.now() - testTimeStart;
      console.log('return time (ms): ',delta);
      testTimeStart = null;
    }

    if (msg.detail === "PostRender" && isPresenting) {
      // WebVR: Indicate that we are ready to present the rendered frame to the VR display
      vrDisplay.submitFrame();
    }
  }

  function onEnterVR() {
    if (!inVR) {
      inVR = true;
      if (vrDisplay.capabilities.canPresent) {
        vrDisplay.requestPresent([{ source: canvas }]).then(function() {
          var leftEye = vrDisplay.getEyeParameters('left');
          var rightEye = vrDisplay.getEyeParameters('right');
          var renderWidth = Math.max(leftEye.renderWidth, rightEye.renderWidth) * 2;
          var renderHeight = Math.max(leftEye.renderHeight, rightEye.renderHeight);
          canvas.width = Math.max(leftEye.renderWidth, rightEye.renderWidth) * 2;
          canvas.height = Math.max(leftEye.renderHeight, rightEye.renderHeight);
          onResize();
          isPresenting = true;
        });
      }

      // starts stereo rendering in Unity.
      gameInstance.SendMessage('WebVRCameraSet', 'Begin');
    } else {
      inVR = false;
      if (vrDisplay.isPresenting) {
        vrDisplay.exitPresent();

        isPresenting = false;
      }
      // starts stereo rendering in Unity.
      gameInstance.SendMessage('WebVRCameraSet', 'End');

      onResize();
    }
  }

  function onAnimate() {
    window.requestAnimationFrame(onAnimate);

    // headset framedata
    frameData = new VRFrameData();
    vrDisplay.getFrameData(frameData);

    if (frameData) {
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
    }

    // Sit Stand transform
    if (vrDisplay.stageParameters) {
      mat4.copy(sitStand, vrDisplay.stageParameters.sittingToStandingTransform);
    } else {
      mat4.identity(sitStand);
      mat4.translate(sitStand, sitStand, [0, defaultHeight, 0]);
    }
    mat4.transpose(sitStand, sitStand);
    sitStand = Array.from(sitStand);

    // gamepads
    gamepads = navigator.getGamepads();
    vrGamepads = [];
    for (var i = 0; i < gamepads.length; ++i) {
      var gamepad = gamepads[i];
      if (gamepad) {
        if (gamepad.pose || gamepad.displayId) {
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
    }

    var vrData = {
      leftProjectionMatrix: Array.from(leftProjectionMatrix),
      rightProjectionMatrix: Array.from(rightProjectionMatrix),
      leftViewMatrix: Array.from(leftViewMatrix),
      rightViewMatrix: Array.from(rightViewMatrix),
      sitStand : Array.from(sitStand),
      controllers: vrGamepads
    };

    gameInstance.SendMessage('WebVRCameraSet', 'WebVRData', JSON.stringify(vrData));

    updateStatus();
  }

  function onResize() {
    if (!canvas) return;

    if (inVR) {
      // scale game container so we get a proper sized mirror of VR content to desktop.
      var renderWidth = canvas.width;
      var renderHeight = canvas.height;
      var scaleX = window.innerWidth / renderWidth;
      var scaleY = window.innerHeight / renderHeight;
      container.setAttribute('style', `transform: scale(${scaleX}, ${scaleY}); transform-origin: top left;`);
    } else {
      canvas.width = window.innerWidth;
      canvas.height = window.innerHeight;
      container.style.transform = '';
    }
  }

  function onKeyDown (e) {
    if (e.keyCode === 80) { // p, toggles perf counter
      gameInstance.SendMessage('WebVRCameraSet', 'TogglePerf');
    }
    if(e.keyCode === 86) //v, tesets round-trip time between browser and Unity game instance.
    {
      console.log("pressed v, roundtrip time");
      testTimeStart = performance.now();
      gameInstance.SendMessage('WebVRCameraSet', 'TestTime');
    }
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

  function onRequestAnimationFrame(cb) {
    if (inVR && vrDisplay && vrDisplay.capabilities.canPresent) {
      return vrDisplay.requestAnimationFrame(cb);
    } else {
      return windowRaf(cb);
    }
  }

  function init() {
    // shim raf so that we can drive framerate using VR display.
    window.requestAnimationFrame = onRequestAnimationFrame;
    // messages from Unity.
    document.addEventListener('Unity', onUnity);
    window.addEventListener('resize', onResize, true);
    document.addEventListener('keydown', onKeyDown);
    entervrButton.addEventListener('click', onEnterVR);
  }

  init();
})();

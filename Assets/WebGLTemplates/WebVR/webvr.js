(function () {
  'use strict';

  var vrDisplay = null;
  var canvas = null;
  var frameData = null;
  var inVR = false;
  var isPresenting = false;
  var testTimeStart = null;
  var leftProjectionMatrix = [];
  var rightProjectionMatrix = [];
  var leftViewMatrix = [];
  var rightViewMatrix = [];
  var sitStand = [];
  var entervrButton = document.querySelector('#entervr');
  var container = document.querySelector('#game');
  var loading = document.getElementById('loader');
  var windowRaf = window.requestAnimationFrame;

  function getVRDisplays() {
    if (navigator.getVRDisplays) {
      navigator.getVRDisplays().then(function(displays) {
        vrDisplay = displays[displays.length - 1];
        onResize();
        onAnimate();
      });
    } else {
      console.log('Your browser does not support WebVR!');
    }
  }

  function onUnity(msg) {
    if (msg.detail === "Ready") {
      // Get and hide Unity's canvas instance
      canvas = document.getElementById('#canvas');
      loading.style.display = 'none';
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
      leftProjectionMatrix = transformMatrixToUnity(frameData.leftProjectionMatrix, false);
      rightProjectionMatrix = transformMatrixToUnity(frameData.rightProjectionMatrix, false);
      leftViewMatrix = transformMatrixToUnity(frameData.leftViewMatrix, true);
      rightViewMatrix = transformMatrixToUnity(frameData.rightViewMatrix, true);
    }

    // sitstand transform
    if (vrDisplay.stageParameters) {
      sitStand = transformMatrixToUnity(vrDisplay.stageParameters.sittingToStandingTransform, false);
    }

    // gamepads
    var gamepads = navigator.getGamepads();
    var vrGamepads = [];
    for (var i = 0; i < gamepads.length; ++i) {
      var gamepad = gamepads[i];
      if (gamepad) {
        if (gamepad.pose || gamepad.displayId) {
          if (gamepad.pose.position && gamepad.pose.orientation) {
            // flips gamepad axis to work with Unity.
            var position = gamepad.pose.position;
            position[2] *= -1;
            var orientation = gamepad.pose.orientation;
            orientation[1] *= -1;
            orientation[2] *= -1;

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
      leftProjectionMatrix: leftProjectionMatrix,
      rightProjectionMatrix: rightProjectionMatrix,
      leftViewMatrix: leftViewMatrix,
      rightViewMatrix: rightViewMatrix,
      sitStand : sitStand,
      controllers: vrGamepads
    };

    gameInstance.SendMessage('WebVRCameraSet', 'WebVRData', JSON.stringify(vrData));
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

  // transforms webGL matrix for use in Unity.
  function transformMatrixToUnity(array, flipZ) {
    if (flipZ) {
      // flip z to work with Unity coordinates.
      array[8] *= -1;
      array[9] *= -1;
      array[10] *= -1;
      array[11] *= -1;
    }

    // transpose to Unity column major order.
    var unityArray = new Array(16);
    unityArray[0] = array[0];
    unityArray[1] = array[4];
    unityArray[2] = array[8];
    unityArray[3] = array[12];
    unityArray[4] = array[1];
    unityArray[5] = array[5];
    unityArray[6] = array[9];
    unityArray[7] = array[13];
    unityArray[8] = array[2];
    unityArray[9] = array[6];
    unityArray[10] = array[10];
    unityArray[11] = array[14];
    unityArray[12] = array[3];
    unityArray[13] = array[7];
    unityArray[14] = array[11];
    unityArray[15] = array[15];

    return unityArray;
  }

  function onRequestAnimationFrame(cb) {
    if (inVR && vrDisplay && vrDisplay.capabilities.canPresent) {
      return vrDisplay.requestAnimationFrame(cb);
    } else {
      return windowRaf(cb);
    }
  }

  // shim raf so that we can drive framerate using VR display.
  window.requestAnimationFrame = onRequestAnimationFrame;

  // listen for any messages from Unity.
  document.addEventListener('Unity', onUnity);

  window.addEventListener('resize', onResize, true);

  document.addEventListener('keydown', onKeyDown);

  entervrButton.addEventListener('click', onEnterVR);
})();

(function () {
  'use strict';

  var display = null;
  var canvas = null;
  var frameData = null;
  var leftHand = null;
  var rightHand = null;
  var loader = null;
  var inVR = false;
  var vrSceneFrame, normalSceneFrame;
  var entervrButton = document.querySelector('#entervr');

  var raf = window.requestAnimationFrame;

  // shim raf so that we can
  window.requestAnimationFrame = function(cb) {
    if (inVR && display) {
      if (!display.capabilities.canPresent) {
        return window.setTimeout(cb, 1000 / 10);
      } else {
        return display.requestAnimationFrame(cb);
      }
    } else {
      return raf(cb);
    }
  }

  function initVR(displays) {
    if (displays.length > 0) {
      display = displays[0];

      console.log('vrDisplay', display);

      if (display.stageParameters) {
        var sitStand = transformMatrixToUnity(display.stageParameters.sittingToStandingTransform, false);
        gameInstance.SendMessage('WebVRCameraSet', 'HMDSittingToStandingTransform', sitStand.join());
      }

      window.addEventListener('resize', handleResize, true);
      handleResize();

      if (display.capabilities.canPresent) {
        entervrButton.style.display = 'block';
      }

      vrAnimate();
    }
  }

  // waits for messages back from unity.
  function handleUnity(msg) {
    if (msg.detail === "Ready") {
      canvas = document.getElementById('canvas');
      loader = document.getElementById('loader');

      // starts stereo rendering in Unity.
      gameInstance.SendMessage('WebVRCameraSet', 'Begin');

      loader.style.display = 'none';
      if (navigator.getVRDisplays) {
        navigator.getVRDisplays().then(initVR);
      } else {
        console.log('Your browser does not support WebVR!');
      }
    }

    if (msg.detail === "Timer") {
      var delta = performance.now() - testTimeStart;
      console.log('return time (ms): ',delta);
      testTimeStart = null;
    }

    if (msg.detail === "PostRender" && display) {
      // WebVR: Indicate that we are ready to present the rendered frame to the VR display
      display.submitFrame();
    }
  }

  // listen for any messages from Unity.
  document.addEventListener('Unity', handleUnity);

  function handleResize() {
    if (!canvas) {
      canvas = document.getElementsByTagName('canvas')[0];
    }
    // should resize to eye rects for VR.
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
  }

  entervrButton.addEventListener('click', function () {
    inVR = inVR === false ? true : false;
    console.log('Enter VR');
    if (inVR && display.capabilities.canPresent) {
      display.requestPresent([{ source: canvas }]).then(function() {
        console.log('Presenting to WebVR display');

        var leftEye = display.getEyeParameters('left');
        var rightEye = display.getEyeParameters('right');

        canvas.width = Math.max(leftEye.renderWidth, rightEye.renderWidth) * 2;
        canvas.height = Math.max(leftEye.renderHeight, rightEye.renderHeight);
      });
    } else {
      if (display.isPresenting) {
        display.exitPresent();
      }
      console.log('Stopped presenting to WebVR display');

      handleResize();
    }
  })

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

  function vrAnimate() {
    vrSceneFrame = window.requestAnimationFrame(vrAnimate);
    frameData = new VRFrameData();
    display.getFrameData(frameData);
    var curFramePose = frameData.pose;
    var curPos = curFramePose.position;
    var curOrient = curFramePose.orientation;

    var leftProjectionMatrix = transformMatrixToUnity(frameData.leftProjectionMatrix, false);
    var rightProjectionMatrix = transformMatrixToUnity(frameData.rightProjectionMatrix, false);
    var leftViewMatrix = transformMatrixToUnity(frameData.leftViewMatrix, true);
    var rightViewMatrix = transformMatrixToUnity(frameData.rightViewMatrix, true);

    var leftMatrix = leftProjectionMatrix.concat(leftViewMatrix);
    var rightMatrix = rightProjectionMatrix.concat(rightViewMatrix);
    var hmdMatrix = leftMatrix.concat(rightMatrix);

    gameInstance.SendMessage('WebVRCameraSet', 'HMDViewProjection', hmdMatrix.join());

    var gamepads = navigator.getGamepads();
    var vrGamepads = [];
    for (var i = 0; i < gamepads.length; ++i) {
      var gamepad = gamepads[i];
      if (gamepad) {
        if (gamepad.pose || gamepad.displayId) {
          if (gamepad.pose.position && gamepad.pose.orientation) {
            var position = gamepad.pose.position;
            position[2] *= -1;
            var orientation = gamepad.pose.orientation;
            orientation[0] *= -1;
            orientation[1] *= -1;

            vrGamepads.push({
              index: gamepad.index,
              hand: gamepad.hand,
              orientation: orientation.join(','),
              position: position.join(',')
            });
          }
        }
      }
    }

    var controllerJson = JSON.stringify({
      controllers: vrGamepads
    });

    gameInstance.SendMessage('WebVRCameraSet', 'VRGamepads', controllerJson);

    // if (display) display.submitFrame();
  }

  document.onkeydown = getKey;
  var testTimeStart = null;
  function getKey(e)
  {
    console.log("the keycode is "+e.keyCode);
    if(e.keyCode == "86") //v
    {
      console.log("pressed v, roundtrip time");
      testTimeStart = performance.now();
      gameInstance.SendMessage('WebVRCameraSet', 'TestTime');
    }
  }

  // window.addEventListener("gamepadconnected", function(e) {
  // var gpArr = navigator.getGamepads();
  // for(var i = 0; i < gpArr.length; i++)
  //   {
  //     //if we find a VR gamepad
  //     if(gpArr[i].id == "OpenVR Gamepad")
  //     {
  //       //determine which hand it is (gamepad API gets them backwards)
  //       if(gpArr[i].hand == "left")
  //       {
  //         rightHand = gpArr[i];
  //         //console.log("got right hand, position = "+rightHand.pose.position);
  //       }
  //       else if(gpArr[i].hand == "right")
  //       {
  //         leftHand = gpArr[i];
  //         //console.log("got left hand, position = "+leftHand.pose.position);
  //       }
  //     }
  //   }
  // });

})();

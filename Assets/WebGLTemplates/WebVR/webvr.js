(function () {
  'use strict';

  function vrAnimate() {
	  
	//request the next frame of the animation
	vrSceneFrame = display.requestAnimationFrame(vrAnimate);
	
	if(display.isPresenting)
		  console.log("thinks it's showing something in the HMD");
	  else
		  console.log("not showing anything in the HMD");
	
	// You can get the position, orientation, etc. of the display from the current frame's pose
	// curFramePose is a VRPose object
	frameData = new VRFrameData();
	display.getFrameData(frameData);
	var curFramePose = frameData.pose;
	var curPos = curFramePose.position;
	var curOrient = curFramePose.orientation;
	
	
	//send HMD position and rotation to Unity
	if(curOrient != null)
	{
    gameInstance.SendMessage('WebVRCameraSet', 'HMDTiltX', -curOrient[0]);
    gameInstance.SendMessage('WebVRCameraSet', 'HMDTiltY', -curOrient[1]);
    gameInstance.SendMessage('WebVRCameraSet', 'HMDTiltZ', curOrient[2]);
    gameInstance.SendMessage('WebVRCameraSet', 'HMDTiltW', curOrient[3]);
	}
	if(curPos != null)
	{
		gameInstance.SendMessage('WebVRCameraSet', 'HMDPosX', curPos[0]);
		gameInstance.SendMessage('WebVRCameraSet', 'HMDPosY', curPos[1]);
		gameInstance.SendMessage('WebVRCameraSet', 'HMDPosZ', -curPos[2]);
	}
	
	if(leftHand != null)
		if(leftHand.pose.hasPosition)//send left hand position to Unity
		{
			console.log("left hand rotation = "+leftHand.pose.orientation);
			var leftPose = leftHand.pose;
			var leftPos = leftPose.position;
			var leftOrient = leftPose.orientation;
			gameInstance.SendMessage('WebVRCameraSet', 'LHTiltX', -leftOrient[0]);
			gameInstance.SendMessage('WebVRCameraSet', 'LHTiltY', -leftOrient[1]);
			gameInstance.SendMessage('WebVRCameraSet', 'LHTiltZ', leftOrient[2]);
			gameInstance.SendMessage('WebVRCameraSet', 'LHTiltW', leftOrient[3]);
			gameInstance.SendMessage('WebVRCameraSet', 'LHPosX', leftPos[0]);
			gameInstance.SendMessage('WebVRCameraSet', 'LHPosY', leftPos[1]);
			gameInstance.SendMessage('WebVRCameraSet', 'LHPosZ', -leftPos[2]);
		}
	if(rightHand != null)
		if(rightHand.pose.hasPosition)//send right hand position to Unity
		{
			//console.log("right hand position = "+rightHand.pose.position);
			var rightPose = rightHand.pose;
			var rightPos = rightPose.position;
			var rightOrient = rightPose.orientation;
			gameInstance.SendMessage('WebVRCameraSet', 'RHTiltX', -rightOrient[0]);
			gameInstance.SendMessage('WebVRCameraSet', 'RHTiltY', -rightOrient[1]);
			gameInstance.SendMessage('WebVRCameraSet', 'RHTiltZ', rightOrient[2]);
			gameInstance.SendMessage('WebVRCameraSet', 'RHTiltW', rightOrient[3]);
			gameInstance.SendMessage('WebVRCameraSet', 'RHPosX', rightPos[0]);
			gameInstance.SendMessage('WebVRCameraSet', 'RHPosY', rightPos[1]);
			gameInstance.SendMessage('WebVRCameraSet', 'RHPosZ', -rightPos[2]);
		}

	// WebVR: Indicate that we are ready to present the rendered frame to the VR display
	display.submitFrame();
  }

  function initVR(displays) {
    if (displays.length > 0) {
      display = displays[0];
	  
	  window.addEventListener('resize', handleResize, true);
	  handleResize();
	  
	  //vrAnimate();
    }
  }

  function handleResize() {
    if (!canvas) {
      canvas = document.getElementsByTagName('canvas')[0];
    }
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
  }

  function handleUnity(msg) {
    if (msg.detail === "Ready") {
      canvas = document.getElementById('canvas');
      loader = document.getElementById('loader');
	  
	  //initWebGL(canvas);

      gameInstance.SendMessage('WebVRCameraSet', 'Begin');

      loader.style.display = 'none';

      navigator.getVRDisplays().then(initVR);
    }
  }

  var display = null,
    canvas = null,
	frameData = null,
	leftHand = null,
	rightHand = null,
    loader = null;
  var inVR = false;	
  var vrSceneFrame, normalSceneFrame;
  document.addEventListener('Unity', handleUnity);
  
  window.addEventListener("gamepadconnected", function(e) {
  var gpArr = navigator.getGamepads();
  for(var i = 0; i < gpArr.length; i++)
  {
	  //if we find a VR gamepad
	  if(gpArr[i].id == "OpenVR Gamepad")
	  {
		  //determine which hand it is (gamepad API gets them backwards)
		  if(gpArr[i].hand == "left")
		  {
			  rightHand = gpArr[i];
			  //console.log("got right hand, position = "+rightHand.pose.position);
		  }
		  else if(gpArr[i].hand == "right")
		  {
			  leftHand = gpArr[i];
			  //console.log("got left hand, position = "+leftHand.pose.position);
		  }
	  }
  }
  });
  
  document.onkeydown = getKey;
  function getKey(e)
  {
	  console.log("the keycode is "+e.keyCode);
	  if(e.keyCode == "86")
	  {
		  console.log("pressed v, entering VR");
			if(!inVR) {
			  inVR = true;
			  display.requestPresent([{ source: canvas }]).then(function() {
				console.log('Presenting to WebVR display');

				// Set the canvas size to the size of the vrDisplay viewport

				var leftEye = display.getEyeParameters('left');
				var rightEye = display.getEyeParameters('right');

				canvas.width = Math.max(leftEye.renderWidth, rightEye.renderWidth) * 2;
				canvas.height = Math.max(leftEye.renderHeight, rightEye.renderHeight);

				// stop the normal presentation, and start the vr presentation
				//window.cancelAnimationFrame(normalSceneFrame);
				//drawVRScene();
				vrAnimate();
				//btn.textContent = 'Exit VR display';
			  });
			} 
			else {
				display.exitPresent();
				console.log('Stopped presenting to WebVR display');
				inVR = false;
				//btn.textContent = 'Start VR display';

				handleResize();

				// Stop the VR presentation, and start the normal presentation
				display.cancelAnimationFrame(vrSceneFrame);
			}
	  }
  }

})();

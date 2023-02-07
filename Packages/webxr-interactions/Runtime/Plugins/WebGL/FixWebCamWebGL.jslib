var LibraryFixWebCamWebGL = {
  $webcamBufferToTextureTable: {},
  $webcamLatestTextureId: 0,
  $disableNextSubImage: false,
  
  glTexSubImage2D: function(target, level, xoffset, yoffset, width, height, format, type, pixels) {
    if (disableNextSubImage) {
      disableNextSubImage = false;
      return;
    }
    if (GL.currentContext.supportsWebGL2EntryPoints) {
      if (GLctx.currentPixelUnpackBufferBinding) {
        GLctx.texSubImage2D(target, level, xoffset, yoffset, width, height, format, type, pixels)
      } else if (pixels != 0) {
        GLctx.texSubImage2D(target, level, xoffset, yoffset, width, height, format, type, emscriptenWebGLGetHeapForType(type), pixels >> emscriptenWebGLGetShiftForType(type))
      } else {
        GLctx.texSubImage2D(target, level, xoffset, yoffset, width, height, format, type, null)
      }
      return
    }
    var pixelData = null;
    if (pixels)
      pixelData = emscriptenWebGLGetTexPixelData(type, format, width, height, pixels, 0);
    GLctx.texSubImage2D(target, level, xoffset, yoffset, width, height, format, type, pixelData)
  },

  JS_WebCamVideo_SetLatestTextureId: function(textureId) {
    webcamLatestTextureId = textureId;
    // Webcam texture is created with texStorage2D so we have to recreate it
    GLctx.deleteTexture(GL.textures[textureId]);
    GL.textures[textureId] = GLctx.createTexture();
    GL.textures[textureId].name = textureId;
    GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[textureId]);
    GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_S, GLctx.CLAMP_TO_EDGE);
    GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_T, GLctx.CLAMP_TO_EDGE);
    GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);
  },

  JS_WebCamVideo_RemoveWhereTextureId: function(textureId) {
    Object.entries(webcamBufferToTextureTable).forEach(function (pair) {
      if (pair[1] == textureId) {
        delete webcamBufferToTextureTable[pair[0]];
      }
    });
  },
  
  JS_WebCamVideo_GrabFrame: function(deviceId, buffer, destWidth, destHeight) {
    var videoElement;
    if (typeof activeWebCams !== "undefined") {
      var webcamDevice = activeWebCams[deviceId];
      if (!webcamDevice)
        return;
      var timeNow = performance.now();
      if (timeNow < webcamDevice.nextFrameAvailableTime) {
        return;
      }
      webcamDevice.nextFrameAvailableTime += webcamDevice.frameLengthInMsecs;
      if (webcamDevice.nextFrameAvailableTime < timeNow) {
        webcamDevice.nextFrameAvailableTime = timeNow + webcamDevice.frameLengthInMsecs
      }
      videoElement = webcamDevice.video;
    } else if (!typeof MediaDevices !== "undefined") {
      if (!MediaDevices[deviceId].video) {
        console.error("WebCam not initialized.");
        return;
      }
      videoElement = MediaDevices[deviceId].video;
    }
    if (!webcamBufferToTextureTable[buffer]) {
      if (!webcamLatestTextureId) {
        return;
      }
      webcamBufferToTextureTable[buffer] = webcamLatestTextureId;
      webcamLatestTextureId = 0
    }
    GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[webcamBufferToTextureTable[buffer]]);
    GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, true);
    GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, videoElement);
    GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, false);
    disableNextSubImage = true;
    return 1;
  }
};

autoAddDeps(LibraryFixWebCamWebGL, '$webcamBufferToTextureTable');
autoAddDeps(LibraryFixWebCamWebGL, '$webcamLatestTextureId');
autoAddDeps(LibraryFixWebCamWebGL, '$disableNextSubImage');
mergeInto(LibraryManager.library, LibraryFixWebCamWebGL);

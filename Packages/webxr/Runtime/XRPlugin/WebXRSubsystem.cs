using System;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace WebXR
{
    // TODO: we need an XRInputSubsystem implementation - this can only be done via native code
    
    public class WebXRSubsystemDescriptor : SubsystemDescriptor<WebXRSubsystem>
    {
    }

    public class WebXRSubsystem : Subsystem<WebXRSubsystemDescriptor>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterDescriptor()
        {
            var res = SubsystemRegistration.CreateDescriptor(new WebXRSubsystemDescriptor()
            {
                id = typeof(WebXRSubsystem).FullName,
                subsystemImplementationType = typeof(WebXRSubsystem)
            });
            if (res)
                Debug.Log("Registered " + nameof(WebXRSubsystemDescriptor));
            else Debug.Log("Failed registering " + nameof(WebXRSubsystemDescriptor));
        }

        public override void Start()
        {
            if (running) return;
            Debug.Log("Start " + nameof(WebXRSubsystem));
            _running = true;
            Instance = this;
            InternalStart();
        }

        public override void Stop()
        {
            if (!_running) return;
            Debug.Log("Stop " + nameof(WebXRSubsystem));
            _running = false;
            Instance = null;
        }

        protected override void OnDestroy()
        {
            if (!running) return;
            Debug.Log("Destroy " + nameof(WebXRSubsystem));
            _running = false;
            Instance = null;
        }

        internal void OnUpdate()
        {
            bool hasHandsData = false;
            if (OnHandUpdate != null && this.xrState != WebXRState.NORMAL)
            {
                if (GetHandFromHandsArray(0, ref leftHand))
                {
                    OnHandUpdate?.Invoke(leftHand);
                }

                if (GetHandFromHandsArray(1, ref rightHand))
                {
                    OnHandUpdate?.Invoke(rightHand);
                }

                hasHandsData = leftHand.enabled || rightHand.enabled;
            }

            if (!hasHandsData && OnControllerUpdate != null && this.xrState != WebXRState.NORMAL)
            {
                if (GetGamepadFromControllersArray(0, ref controller1))
                {
                    OnControllerUpdate?.Invoke(controller1);
                }

                if (GetGamepadFromControllersArray(1, ref controller2))
                {
                    OnControllerUpdate?.Invoke(controller2);
                }
            }

            if (OnViewerHitTestUpdate != null && this.xrState == WebXRState.AR)
            {
                if (GetHitTestPoseFromViewerHitTestPoseArray(ref viewerHitTestPose))
                {
                    OnViewerHitTestUpdate?.Invoke(viewerHitTestPose);
                }
            }
        }

        internal void OnLateUpdate()
        {
            if (OnHeadsetUpdate != null && this.xrState != WebXRState.NORMAL)
            {
                Matrix4x4 leftProjectionMatrix = WebXRMatrixUtil.NumbersToMatrix(GetMatrixFromSharedArray(0));
                Matrix4x4 rightProjectionMatrix = WebXRMatrixUtil.NumbersToMatrix(GetMatrixFromSharedArray(1));
                Matrix4x4 leftViewMatrix = WebXRMatrixUtil.NumbersToMatrix(GetMatrixFromSharedArray(2));
                Matrix4x4 rightViewMatrix = WebXRMatrixUtil.NumbersToMatrix(GetMatrixFromSharedArray(3));
                Matrix4x4 sitStandMatrix = WebXRMatrixUtil.NumbersToMatrix(GetMatrixFromSharedArray(4));
                if (!this.capabilities.hasPosition)
                {
                    const float defaultHeight = 1;
                    sitStandMatrix = Matrix4x4.Translate(new Vector3(0, defaultHeight, 0));
                }

                OnHeadsetUpdate?.Invoke(
                    leftProjectionMatrix,
                    rightProjectionMatrix,
                    leftViewMatrix,
                    rightViewMatrix,
                    sitStandMatrix);
            }
        }

        private bool _running;
        public override bool running => _running;

        private static WebXRSubsystem Instance;

        private void InternalStart()
        {
            Native.set_webxr_events(OnStartAR, OnStartVR, OnEndXR, OnXRCapabilities);
            Native.InitControllersArray(controllersArray, controllersArray.Length);
            Native.InitHandsArray(handsArray, handsArray.Length);
            Native.InitViewerHitTestPoseArray(viewerHitTestPoseArray, viewerHitTestPoseArray.Length);
            Native.InitXRSharedArray(sharedArray, sharedArray.Length);
            Native.ListenWebXRData();
        }

        private static class Native
        {
            [DllImport("__Internal")]
            public static extern void InitXRSharedArray(float[] array, int length);

            [DllImport("__Internal")]
            public static extern void InitControllersArray(float[] array, int length);

            [DllImport("__Internal")]
            public static extern void InitHandsArray(float[] array, int length);

            [DllImport("__Internal")]
            public static extern void InitViewerHitTestPoseArray(float[] array, int length);

            [DllImport("__Internal")]
            public static extern void ToggleViewerHitTest();

            [DllImport("__Internal")]
            public static extern void ControllerPulse(int controller, float intensity, float duration);

            [DllImport("__Internal")]
            public static extern void ListenWebXRData();

            [DllImport("__Internal")]
            public static extern void set_webxr_events(Action<int, float, float, float, float, float, float, float, float> on_start_ar,
                Action<int, float, float, float, float, float, float, float, float> on_start_vr,
                Action on_end_xr,
                Action<string> on_xr_capabilities);
        }

        internal WebXRState xrState = WebXRState.NORMAL;

        public delegate void XRCapabilitiesUpdate(WebXRDisplayCapabilities capabilities);

        public static event XRCapabilitiesUpdate OnXRCapabilitiesUpdate;

        public delegate void XRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect);

        public static event XRChange OnXRChange;

        public delegate void HeadsetUpdate(
            Matrix4x4 leftProjectionMatrix,
            Matrix4x4 leftViewMatrix,
            Matrix4x4 rightProjectionMatrix,
            Matrix4x4 rightViewMatrix,
            Matrix4x4 sitStandMatrix);

        public static event HeadsetUpdate OnHeadsetUpdate;

        public delegate void ControllerUpdate(WebXRControllerData controllerData);

        public static event ControllerUpdate OnControllerUpdate;

        public delegate void HandUpdate(WebXRHandData handData);

        public static event HandUpdate OnHandUpdate;

        public delegate void HitTestUpdate(WebXRHitPoseData hitPoseData);

        public static event HitTestUpdate OnViewerHitTestUpdate;

        // Shared array which we will load headset data in from webxr.jslib
        // Array stores  5 matrices, each 16 values, stored linearly.
        float[] sharedArray = new float[5 * 16];

        // Shared array for controllers data
        float[] controllersArray = new float[2 * 20];

        // Shared array for hands data
        float[] handsArray = new float[2 * (25 * 9 + 5)];

        // Shared array for hit-test pose data
        float[] viewerHitTestPoseArray = new float[9];

        bool viewerHitTestOn = false;

        private WebXRHandData leftHand = new WebXRHandData();
        private WebXRHandData rightHand = new WebXRHandData();

        private WebXRControllerData controller1 = new WebXRControllerData();
        private WebXRControllerData controller2 = new WebXRControllerData();

        private WebXRHitPoseData viewerHitTestPose = new WebXRHitPoseData();

        private WebXRDisplayCapabilities capabilities = new WebXRDisplayCapabilities();

        // Handles WebXR capabilities from browser
        [MonoPInvokeCallback(typeof(Action<string>))]
        public static void OnXRCapabilities(string json)
        {
            WebXRDisplayCapabilities capabilities = JsonUtility.FromJson<WebXRDisplayCapabilities>(json);
            Instance.OnXRCapabilities(capabilities);
        }

        public void OnXRCapabilities(WebXRDisplayCapabilities cap)
        {
            this.capabilities = cap;
            OnXRCapabilitiesUpdate?.Invoke(cap);
        }

        public void setXrState(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            this.xrState = state;
            viewerHitTestOn = false;
            OnXRChange?.Invoke(state, viewsCount, leftRect, rightRect);
        }

        // received start AR from WebXR browser
        [MonoPInvokeCallback(typeof(Action<int, float, float, float, float, float, float, float, float>))]
        public static void OnStartAR(int viewsCount,
            float left_x, float left_y, float left_w, float left_h,
            float right_x, float right_y, float right_w, float right_h)
        {
            Instance.setXrState(WebXRState.AR, viewsCount,
                new Rect(left_x, left_y, left_w, left_h),
                new Rect(right_x, right_y, right_w, right_h));
        }

        // received start VR from WebXR browser
        [MonoPInvokeCallback(typeof(Action<int, float, float, float, float, float, float, float, float>))]
        public static void OnStartVR(int viewsCount,
            float left_x, float left_y, float left_w, float left_h,
            float right_x, float right_y, float right_w, float right_h)
        {
            Instance.setXrState(WebXRState.VR, viewsCount,
                new Rect(left_x, left_y, left_w, left_h),
                new Rect(right_x, right_y, right_w, right_h));
        }

        // receive end VR from WebVR browser
        [MonoPInvokeCallback(typeof(Action))]
        public static void OnEndXR()
        {
            Instance.setXrState(WebXRState.NORMAL, 1, new Rect(), new Rect());
        }

        public void StartViewerHitTest()
        {
            if (xrState == WebXRState.AR && !viewerHitTestOn)
            {
                viewerHitTestOn = true;
                Native.ToggleViewerHitTest();
            }
        }

        public void StopViewerHitTest()
        {
            if (xrState == WebXRState.AR && viewerHitTestOn)
            {
                viewerHitTestOn = false;
                Native.ToggleViewerHitTest();
            }
        }

        public void HapticPulse(WebXRControllerHand hand, float intensity, float duration)
        {
            Native.ControllerPulse((int) hand, intensity, duration);
        }

        float[] GetMatrixFromSharedArray(int index)
        {
            float[] newArray = new float[16];
            for (int i = 0; i < newArray.Length; i++)
            {
                newArray[i] = sharedArray[index * 16 + i];
            }

            return newArray;
        }

        bool GetGamepadFromControllersArray(int controllerIndex, ref WebXRControllerData newControllerData)
        {
            int arrayPosition = controllerIndex * 20;
            int frameNumber = (int) controllersArray[arrayPosition++];
            if (newControllerData.frame == frameNumber)
            {
                return false;
            }

            newControllerData.frame = frameNumber;
            newControllerData.enabled = controllersArray[arrayPosition++] != 0;
            newControllerData.hand = (int) controllersArray[arrayPosition++];
            if (!newControllerData.enabled)
            {
                return true;
            }

            newControllerData.position = new Vector3(controllersArray[arrayPosition++], controllersArray[arrayPosition++], controllersArray[arrayPosition++]);
            newControllerData.rotation = new Quaternion(controllersArray[arrayPosition++], controllersArray[arrayPosition++], controllersArray[arrayPosition++],
                controllersArray[arrayPosition++]);
            newControllerData.trigger = controllersArray[arrayPosition++];
            newControllerData.squeeze = controllersArray[arrayPosition++];
            newControllerData.thumbstick = controllersArray[arrayPosition++];
            newControllerData.thumbstickX = controllersArray[arrayPosition++];
            newControllerData.thumbstickY = controllersArray[arrayPosition++];
            newControllerData.touchpad = controllersArray[arrayPosition++];
            newControllerData.touchpadX = controllersArray[arrayPosition++];
            newControllerData.touchpadY = controllersArray[arrayPosition++];
            newControllerData.buttonA = controllersArray[arrayPosition++];
            newControllerData.buttonB = controllersArray[arrayPosition];
            return true;
        }

        bool GetHandFromHandsArray(int handIndex, ref WebXRHandData handObject)
        {
            int arrayPosition = handIndex * 230;
            int frameNumber = (int) handsArray[arrayPosition++];
            if (handObject.frame == frameNumber)
            {
                return false;
            }

            handObject.frame = frameNumber;
            handObject.enabled = handsArray[arrayPosition++] != 0;
            handObject.hand = (int) handsArray[arrayPosition++];
            handObject.trigger = handsArray[arrayPosition++];
            handObject.squeeze = handsArray[arrayPosition++];
            if (!handObject.enabled)
            {
                return true;
            }

            for (int i = 0; i <= WebXRHandData.LITTLE_PHALANX_TIP; i++)
            {
                handObject.joints[i].enabled = handsArray[arrayPosition++] != 0;
                handObject.joints[i].position = new Vector3(handsArray[arrayPosition++], handsArray[arrayPosition++], handsArray[arrayPosition++]);
                handObject.joints[i].rotation = new Quaternion(handsArray[arrayPosition++], handsArray[arrayPosition++], handsArray[arrayPosition++],
                    handsArray[arrayPosition++]);
                handObject.joints[i].radius = handsArray[arrayPosition++];
            }

            return true;
        }

        bool GetHitTestPoseFromViewerHitTestPoseArray(ref WebXRHitPoseData hitPoseData)
        {
            int arrayPosition = 0;
            int frameNumber = (int) viewerHitTestPoseArray[arrayPosition++];
            if (hitPoseData.frame == frameNumber)
            {
                return false;
            }

            hitPoseData.frame = frameNumber;
            hitPoseData.available = viewerHitTestPoseArray[arrayPosition++] != 0;
            if (!hitPoseData.available)
            {
                return true;
            }

            hitPoseData.position = new Vector3(viewerHitTestPoseArray[arrayPosition++], viewerHitTestPoseArray[arrayPosition++],
                viewerHitTestPoseArray[arrayPosition++]);
            hitPoseData.rotation = new Quaternion(viewerHitTestPoseArray[arrayPosition++], viewerHitTestPoseArray[arrayPosition++],
                viewerHitTestPoseArray[arrayPosition++], viewerHitTestPoseArray[arrayPosition++]);
            return true;
        }
    }
}
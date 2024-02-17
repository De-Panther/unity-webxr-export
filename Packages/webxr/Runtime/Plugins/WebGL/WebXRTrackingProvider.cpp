#include "UnityHeaders/IUnityXRInput.h"
#include "UnityHeaders/IUnityXRTrace.h"

#include "WebXRProviderContext.h"

#include <cmath>
#include <stdio.h>

class WebXRTrackingProvider : public ProviderImpl
{
public:
  WebXRTrackingProvider(WebXRProviderContext &ctx, UnitySubsystemHandle handle)
      : ProviderImpl(ctx, handle)
  {
  }
  virtual ~WebXRTrackingProvider() {}

  UnitySubsystemErrorCode Initialize() override;
  UnitySubsystemErrorCode Start() override;

  UnitySubsystemErrorCode Tick(UnityXRInputUpdateType updateType);
  UnitySubsystemErrorCode FillDeviceDefinition(UnityXRInternalInputDeviceId deviceId, UnityXRInputDeviceDefinition *definition);
  UnitySubsystemErrorCode UpdateDeviceState(UnityXRInternalInputDeviceId deviceId, UnityXRInputUpdateType updateType, UnityXRInputDeviceState *state);
  UnitySubsystemErrorCode HandleEvent(unsigned int eventType, UnityXRInternalInputDeviceId deviceId, void *buffer, unsigned int size);
  UnitySubsystemErrorCode TryGetDeviceStateAtTime(UnityXRTimeStamp time, UnityXRInternalInputDeviceId deviceId, UnityXRInputDeviceState *state);

  void Stop() override;
  void Shutdown() override;

private:
  static const int kInputDeviceHMD = 72;
  float *m_ViewsDataArray;
  bool hasMultipleViews = true;
};

UnitySubsystemErrorCode WebXRTrackingProvider::Initialize()
{
  return kUnitySubsystemErrorCodeSuccess;
}

UnitySubsystemErrorCode WebXRTrackingProvider::Start()
{
  m_ViewsDataArray = WebXRGetViewsDataArray();
  hasMultipleViews = *(m_ViewsDataArray + 54) > 1;
  m_Ctx.input->InputSubsystem_DeviceConnected(m_Handle, kInputDeviceHMD);
  return kUnitySubsystemErrorCodeSuccess;
}

UnitySubsystemErrorCode WebXRTrackingProvider::Tick(UnityXRInputUpdateType updateType)
{
  return kUnitySubsystemErrorCodeSuccess;
}

UnitySubsystemErrorCode WebXRTrackingProvider::FillDeviceDefinition(UnityXRInternalInputDeviceId deviceId, UnityXRInputDeviceDefinition *definition)
{
  // Fill in your connected device information here when requested.  Used to create customized device states.
  auto &input = *m_Ctx.input;
  input.DeviceDefinition_SetName(definition, "WebXR Tracked Display");
  input.DeviceDefinition_SetCharacteristics(definition, (UnityXRInputDeviceCharacteristics)(kUnityXRInputDeviceCharacteristicsHeadMounted | kUnityXRInputDeviceCharacteristicsTrackedDevice));
  input.DeviceDefinition_SetManufacturer(definition, "WebXR");

  input.DeviceDefinition_AddFeatureWithUsage(definition, "is tracked", kUnityXRInputFeatureTypeBinary, kUnityXRInputFeatureUsageIsTracked);
  input.DeviceDefinition_AddFeatureWithUsage(definition, "tracking state", kUnityXRInputFeatureTypeDiscreteStates, kUnityXRInputFeatureUsageTrackingState);

  input.DeviceDefinition_AddFeatureWithUsage(definition, "device position", kUnityXRInputFeatureTypeAxis3D, kUnityXRInputFeatureUsageDevicePosition);
  input.DeviceDefinition_AddFeatureWithUsage(definition, "device rotation", kUnityXRInputFeatureTypeRotation, kUnityXRInputFeatureUsageDeviceRotation);
  input.DeviceDefinition_AddFeatureWithUsage(definition, "center eye position", kUnityXRInputFeatureTypeAxis3D, kUnityXRInputFeatureUsageCenterEyePosition);
  input.DeviceDefinition_AddFeatureWithUsage(definition, "center eye rotation", kUnityXRInputFeatureTypeRotation, kUnityXRInputFeatureUsageCenterEyeRotation);
  if (hasMultipleViews)
  {
    input.DeviceDefinition_AddFeatureWithUsage(definition, "left eye position", kUnityXRInputFeatureTypeAxis3D, kUnityXRInputFeatureUsageLeftEyePosition);
    input.DeviceDefinition_AddFeatureWithUsage(definition, "left eye rotation", kUnityXRInputFeatureTypeRotation, kUnityXRInputFeatureUsageLeftEyeRotation);
    input.DeviceDefinition_AddFeatureWithUsage(definition, "right eye position", kUnityXRInputFeatureTypeAxis3D, kUnityXRInputFeatureUsageRightEyePosition);
    input.DeviceDefinition_AddFeatureWithUsage(definition, "right eye rotation", kUnityXRInputFeatureTypeRotation, kUnityXRInputFeatureUsageRightEyeRotation);
  }

  return kUnitySubsystemErrorCodeSuccess;
}

UnitySubsystemErrorCode WebXRTrackingProvider::UpdateDeviceState(UnityXRInternalInputDeviceId deviceId, UnityXRInputUpdateType updateType, UnityXRInputDeviceState *state)
{
  if (deviceId != kInputDeviceHMD)
  {
    return kUnitySubsystemErrorCodeSuccess;
  }
  /// Called by Unity when it needs a current device snapshot
  UnityXRVector3 position;
  UnityXRVector4 rotation;
  auto &input = *m_Ctx.input;
  int start = 32;
  rotation.x = *(m_ViewsDataArray + start);
  rotation.y = *(m_ViewsDataArray + start + 1);
  rotation.z = *(m_ViewsDataArray + start + 2);
  rotation.w = *(m_ViewsDataArray + start + 3);
  start = 40;
  // Get left position first
  position.x = *(m_ViewsDataArray + start);
  position.y = *(m_ViewsDataArray + start + 1);
  position.z = *(m_ViewsDataArray + start + 2);

  if (hasMultipleViews)
  {
    // Left pose
    input.DeviceState_SetAxis3DValue(state, 6, position);
    input.DeviceState_SetRotationValue(state, 7, rotation);

    UnityXRVector3 rightPosition;
    rightPosition.x = *(m_ViewsDataArray + start + 3);
    rightPosition.y = *(m_ViewsDataArray + start + 4);
    rightPosition.z = *(m_ViewsDataArray + start + 5);
    // Right pose
    input.DeviceState_SetAxis3DValue(state, 8, position);
    input.DeviceState_SetRotationValue(state, 9, rotation);

    // Update center pose
    position.x = 0.5f * (position.x + rightPosition.x);
    position.y = 0.5f * (position.y + rightPosition.y);
    position.z = 0.5f * (position.z + rightPosition.z);
  }
  // Center pose
  input.DeviceState_SetAxis3DValue(state, 2, position);
  input.DeviceState_SetRotationValue(state, 3, rotation);
  // Device pose
  input.DeviceState_SetAxis3DValue(state, 4, position);
  input.DeviceState_SetRotationValue(state, 5, rotation);

  // Tracking
  input.DeviceState_SetBinaryValue(state, 0, true);
  input.DeviceState_SetDiscreteStateValue(state, 1, 3);

  return kUnitySubsystemErrorCodeSuccess;
}

UnitySubsystemErrorCode WebXRTrackingProvider::HandleEvent(unsigned int eventType, UnityXRInternalInputDeviceId deviceId, void *buffer, unsigned int size)
{
  /// Simple, generic method callback to inform the plugin or individual devices of events occurring within unity
  return kUnitySubsystemErrorCodeFailure;
}

UnitySubsystemErrorCode WebXRTrackingProvider::TryGetDeviceStateAtTime(UnityXRTimeStamp time, UnityXRInternalInputDeviceId deviceId, UnityXRInputDeviceState *state)
{
  /// Unity calls this when requesting a state at a specific time in the past
  return kUnitySubsystemErrorCodeSuccess;
}

void WebXRTrackingProvider::Stop()
{
  m_Ctx.input->InputSubsystem_DeviceDisconnected(m_Handle, kInputDeviceHMD);
}

void WebXRTrackingProvider::Shutdown()
{
}

// Binding to C-API below here

static UnitySubsystemErrorCode UNITY_INTERFACE_API Input_Initialize(UnitySubsystemHandle handle, void *userData)
{
  auto &ctx = GetWebXRProviderContext(userData);

  ctx.trackingProvider = new WebXRTrackingProvider(ctx, handle);

  UnityXRInputProvider inputProvider{};
  inputProvider.userData = &ctx;

  inputProvider.Tick = [](UnitySubsystemHandle handle, void *userData, UnityXRInputUpdateType updateType) -> UnitySubsystemErrorCode
  {
    auto &ctx = GetWebXRProviderContext(userData);
    return ctx.trackingProvider->Tick(updateType);
  };

  inputProvider.FillDeviceDefinition = [](UnitySubsystemHandle handle, void *userData, UnityXRInternalInputDeviceId deviceId, UnityXRInputDeviceDefinition *definition) -> UnitySubsystemErrorCode
  {
    auto &ctx = GetWebXRProviderContext(userData);
    return ctx.trackingProvider->FillDeviceDefinition(deviceId, definition);
  };

  inputProvider.UpdateDeviceState = [](UnitySubsystemHandle handle, void *userData, UnityXRInternalInputDeviceId deviceId, UnityXRInputUpdateType updateType, UnityXRInputDeviceState *state) -> UnitySubsystemErrorCode
  {
    auto &ctx = GetWebXRProviderContext(userData);
    return ctx.trackingProvider->UpdateDeviceState(deviceId, updateType, state);
  };

  inputProvider.HandleEvent = [](UnitySubsystemHandle handle, void *userData, unsigned int eventType, UnityXRInternalInputDeviceId deviceId, void *buffer, unsigned int size) -> UnitySubsystemErrorCode
  {
    auto &ctx = GetWebXRProviderContext(userData);
    return ctx.trackingProvider->HandleEvent(eventType, deviceId, buffer, size);
  };

  inputProvider.TryGetDeviceStateAtTime = [](UnitySubsystemHandle handle, void *userData, UnityXRTimeStamp time, UnityXRInternalInputDeviceId deviceId, UnityXRInputDeviceState *state) -> UnitySubsystemErrorCode
  {
    auto &ctx = GetWebXRProviderContext(userData);
    return ctx.trackingProvider->TryGetDeviceStateAtTime(time, deviceId, state);
  };

  ctx.input->RegisterInputProvider(handle, &inputProvider);

  return ctx.trackingProvider->Initialize();
}

UnitySubsystemErrorCode Load_Input(WebXRProviderContext &ctx)
{
  ctx.input = ctx.interfaces->Get<IUnityXRInputInterface>();
  if (ctx.input == nullptr)
    return kUnitySubsystemErrorCodeFailure;

  UnityLifecycleProvider inputLifecycleHandler{};
  inputLifecycleHandler.userData = &ctx;

  inputLifecycleHandler.Initialize = &Input_Initialize;

  inputLifecycleHandler.Start = [](UnitySubsystemHandle handle, void *userData) -> UnitySubsystemErrorCode
  {
    auto &ctx = GetWebXRProviderContext(userData);
    return ctx.trackingProvider->Start();
  };

  inputLifecycleHandler.Stop = [](UnitySubsystemHandle handle, void *userData) -> void
  {
    auto &ctx = GetWebXRProviderContext(userData);
    ctx.trackingProvider->Stop();
  };

  inputLifecycleHandler.Shutdown = [](UnitySubsystemHandle handle, void *userData) -> void
  {
    auto &ctx = GetWebXRProviderContext(userData);
    ctx.trackingProvider->Shutdown();

    delete ctx.trackingProvider;
  };

  return ctx.input->RegisterLifecycleProvider("WebXR Export", "WebXR Tracked Display", &inputLifecycleHandler);
}
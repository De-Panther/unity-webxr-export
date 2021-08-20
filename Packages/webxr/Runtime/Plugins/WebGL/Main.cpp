#include "../../../../../../../UnityHeaders/Headers/IUnityInterface.h"
#include "../../../../../../../UnityHeaders/Headers/XR/IUnityXRTrace.h"
#include "../../../../../../../UnityHeaders/Headers/XR/UnitySubsystemTypes.h"

#include <stdio.h>

#include "ProviderContext.h"

static ProviderContext* s_Context{};

UnitySubsystemErrorCode Load_Display(ProviderContext&);
UnitySubsystemErrorCode Load_Input(ProviderContext&);

static bool ReportError(const char* name, UnitySubsystemErrorCode err)
{
    if (err != kUnitySubsystemErrorCodeSuccess)
    {
        XR_TRACE_ERROR(s_Context->trace, "Error loading subsystem: %s (%d)\n", name, err);
        return true;
    }
    return false;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    auto* ctx = s_Context = new ProviderContext;

    ctx->interfaces = unityInterfaces;
    ctx->trace = unityInterfaces->Get<IUnityXRTrace>();

    if (ReportError("Display", Load_Display(*ctx)))
        return;

    if (ReportError("Input", Load_Input(*ctx)))
        return;
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API
UnityPluginUnload()
{
    delete s_Context;
}

typedef void    (UNITY_INTERFACE_API * PluginLoadFunc)(IUnityInterfaces* unityInterfaces);
typedef void    (UNITY_INTERFACE_API * PluginUnloadFunc)();
extern "C" void UnityRegisterRenderingPlugin(PluginLoadFunc loadPlugin, PluginUnloadFunc unloadPlugin);

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API RegisterWebXRPlugin()
{
    printf("RegisterWebXRPlugin\n");
    UnityRegisterRenderingPlugin(UnityPluginLoad, UnityPluginUnload);
}
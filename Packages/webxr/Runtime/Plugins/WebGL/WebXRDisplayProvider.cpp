#include "UnityHeaders/IUnityXRDisplay.h"
#include "UnityHeaders/IUnityXRTrace.h"

#include "WebXRProviderContext.h"
#include <cmath>
#include <vector>

#define SIDE_BY_SIDE 1
#define NUM_RENDER_PASSES 2
static float s_PoseXPositionPerPass[] = {-1.0f, 1.0f};

// BEGIN WORKAROUND: skip first frame since we get invalid data.  Fix coming to trunk.
static bool s_SkipFrame = true;
#define WORKAROUND_SKIP_FIRST_FRAME()           \
    if (s_SkipFrame)                            \
    {                                           \
        s_SkipFrame = false;                    \
        return kUnitySubsystemErrorCodeSuccess; \
    }
#define WORKAROUND_RESET_SKIP_FIRST_FRAME() s_SkipFrame = true;
// END WORKAROUND

class WebXRDisplayProvider : ProviderImpl
{
public:
    WebXRDisplayProvider(WebXRProviderContext& ctx, UnitySubsystemHandle handle)
        : ProviderImpl(ctx, handle)
    {
    }

    UnitySubsystemErrorCode Initialize() override;
    UnitySubsystemErrorCode Start() override;

    UnitySubsystemErrorCode GfxThread_Start(UnityXRRenderingCapabilities& renderingCaps);

    UnitySubsystemErrorCode GfxThread_SubmitCurrentFrame();
    UnitySubsystemErrorCode GfxThread_PopulateNextFrameDesc(const UnityXRFrameSetupHints& frameHints, UnityXRNextFrameDesc& nextFrame);

    UnitySubsystemErrorCode GfxThread_Stop();
    UnitySubsystemErrorCode GfxThread_FinalBlitToGameViewBackBuffer(const UnityXRMirrorViewBlitInfo* mirrorBlitInfo, WebXRProviderContext& ctx);

    UnitySubsystemErrorCode UpdateDisplayState(UnityXRDisplayState* state);

    void Stop() override;
    void Shutdown() override;

private:
    void CreateTextures(int numTextures, int textureArrayLength, float requestedTextureScale);
    void DestroyTextures();

    UnityXRPose GetPose(int pass);
    UnityXRProjection GetProjection(int pass);

private:
    std::vector<UnityXRRenderTextureId> m_UnityTextures;
    float *m_ViewsDataArray;
    float viewWidth;
    float viewHeight;
    float frameBufferWidth;
    float frameBufferHeight;
    bool hasMultipleViews = true;
    bool transparentBackground = false;
};

UnitySubsystemErrorCode WebXRDisplayProvider::Initialize()
{
    return kUnitySubsystemErrorCodeSuccess;
}

UnitySubsystemErrorCode WebXRDisplayProvider::Start()
{
    m_ViewsDataArray = WebXRGetViewsDataArray();
    viewWidth = *(m_ViewsDataArray + 46);
    viewHeight = *(m_ViewsDataArray + 47);
    frameBufferWidth = *(m_ViewsDataArray + 56);
    frameBufferHeight = *(m_ViewsDataArray + 57);
    hasMultipleViews = *(m_ViewsDataArray + 54) > 1;
    if (hasMultipleViews)
    {
      float viewsHalfDistance = 0.5f * sqrt(
        pow((*(m_ViewsDataArray + 40) - *(m_ViewsDataArray + 43)), 2)
        + pow((*(m_ViewsDataArray + 41) - *(m_ViewsDataArray + 44)), 2)
        + pow((*(m_ViewsDataArray + 42) - *(m_ViewsDataArray + 45)), 2));
      s_PoseXPositionPerPass[0] = -viewsHalfDistance;
      s_PoseXPositionPerPass[1] = viewsHalfDistance;
    }
    else
    {
      s_PoseXPositionPerPass[0] = 0;
      s_PoseXPositionPerPass[1] = 0;
    }
    transparentBackground = *(m_ViewsDataArray + 55) > 0;
    return kUnitySubsystemErrorCodeSuccess;
}

UnitySubsystemErrorCode WebXRDisplayProvider::GfxThread_Start(UnityXRRenderingCapabilities& renderingCaps)
{
    renderingCaps.noSinglePassRenderingSupport = true;
    renderingCaps.invalidateRenderStateAfterEachCallback = false;
    renderingCaps.skipPresentToMainScreen = false;
    return kUnitySubsystemErrorCodeSuccess;
}

UnitySubsystemErrorCode WebXRDisplayProvider::GfxThread_SubmitCurrentFrame()
{
    return kUnitySubsystemErrorCodeSuccess;
}

UnitySubsystemErrorCode WebXRDisplayProvider::GfxThread_PopulateNextFrameDesc(const UnityXRFrameSetupHints& frameHints, UnityXRNextFrameDesc& nextFrame)
{
    WORKAROUND_SKIP_FIRST_FRAME();

    // BlockUntilUnityShouldStartSubmittingRenderingCommands();

    bool reallocateTextures = (m_UnityTextures.size() == 0);
    if ((kUnityXRFrameSetupHintsChangedSinglePassRendering & frameHints.changedFlags) != 0)
    {
        reallocateTextures = true;
    }
    if ((kUnityXRFrameSetupHintsChangedRenderViewport & frameHints.changedFlags) != 0)
    {
        // Change sampling UVs for compositor, pass through new viewport on `nextFrame`
    }
    if ((kUnityXRFrameSetupHintsChangedTextureResolutionScale & frameHints.changedFlags) != 0)
    {
        reallocateTextures = true;
    }
    if ((kUnityXRFrameSetuphintsChangedContentProtectionState & frameHints.changedFlags) != 0)
    {
        // App wants different content protection mode.
    }
    if ((kUnityXRFrameSetuphintsChangedReprojectionMode & frameHints.changedFlags) != 0)
    {
        // App wants different reprojection mode, configure compositor if possible.
    }
    if ((kUnityXRFrameSetuphintsChangedFocusPlane & frameHints.changedFlags) != 0)
    {
        // App changed focus plane, configure compositor if possible.
    }

    if (reallocateTextures)
    {
        DestroyTextures();

#if SIDE_BY_SIDE
        int numTextures = 1;
        int textureArrayLength = 0;
#else
        int numTextures = frameHints.appSetup.singlePassRendering ? NUM_RENDER_PASSES - 1 : NUM_RENDER_PASSES;
        int textureArrayLength = frameHints.appSetup.singlePassRendering ? 2 : 0;
#endif
        CreateTextures(numTextures, textureArrayLength, frameHints.appSetup.textureResolutionScale);
    }

    // Frame hints tells us if we should setup our renderpasses with a single pass
    if (!frameHints.appSetup.singlePassRendering)
    {
        // Use multi-pass rendering to render

        // Can increase render pass count to do wide FOV or to have a separate view into scene.
        nextFrame.renderPassesCount = hasMultipleViews ? NUM_RENDER_PASSES : 1;

        for (int pass = 0; pass < nextFrame.renderPassesCount; ++pass)
        {
            auto& renderPass = nextFrame.renderPasses[pass];

            // Texture that unity will render to next frame.  We created it above.
            // You might want to change this dynamically to double / triple buffer.
#if !SIDE_BY_SIDE
            renderPass.textureId = m_UnityTextures[pass];
#else
            renderPass.textureId = m_UnityTextures[0];
#endif

            // One set of render params per pass.
            renderPass.renderParamsCount = 1;

            // Note that you can share culling between multiple passes by setting this to the same index.
            renderPass.cullingPassIndex = pass;

            // Fill out render params. View, projection, viewport for pass.
            auto& cullingPass = nextFrame.cullingPasses[pass];
            cullingPass.separation = fabs(s_PoseXPositionPerPass[1]) + fabs(s_PoseXPositionPerPass[0]);

            auto& renderParams = renderPass.renderParams[0];
            renderParams.deviceAnchorToEyePose = cullingPass.deviceAnchorToCullingPose = GetPose(pass);
            renderParams.projection = cullingPass.projection = GetProjection(pass);

#if !SIDE_BY_SIDE
            // App has hinted that it would like to render to a smaller viewport.  Tell unity to render to that viewport.
            renderParams.viewportRect = frameHints.appSetup.renderViewport;

            // Tell the compositor what pixels were rendered to for display.
            // Compositor_SetRenderSubRect(pass, renderParams.viewportRect);
#else
            // TODO: frameHints.appSetup.renderViewport
            renderParams.viewportRect = {
                pass == 0 ? 0.0f : 0.5f,        // x
                0.0f,                           // y
                hasMultipleViews ? 0.5f : 0.0f, // width
                1.0f                            // height
            };
#endif
        }
    }
    else
    {
        // Example of using single-pass stereo to combine the first two render passes.
        nextFrame.renderPassesCount = NUM_RENDER_PASSES - 1;

        UnityXRNextFrameDesc::UnityXRRenderPass& renderPass = nextFrame.renderPasses[0];

        // Texture that unity will render to next frame.  We created it above.
        // You might want to change this dynamically to double / triple buffer.
        renderPass.textureId = m_UnityTextures[0];

        // Two sets of render params for first pass, view / projection for each eye.  Fill them out next.
        renderPass.renderParamsCount = 2;

        for (int eye = 0; eye < 2; ++eye)
        {
            UnityXRNextFrameDesc::UnityXRRenderPass::UnityXRRenderParams& renderParams = renderPass.renderParams[eye];
            renderParams.deviceAnchorToEyePose = GetPose(eye);
            renderParams.projection = GetProjection(eye);

#if SIDE_BY_SIDE
            // TODO: frameHints.appSetup.renderViewport
            renderParams.viewportRect = {
                eye == 0 ? 0.0f : 0.5f, // x
                0.0f,                   // y
                0.5f,                   // width
                1.0f                    // height
            };
#else
            // Each eye goes to different texture array slices.
            renderParams.textureArraySlice = eye;
#endif
        }

        renderPass.cullingPassIndex = 0;

        // TODO: set up culling pass to use a combine frustum
        auto& cullingPass = nextFrame.cullingPasses[0];
        cullingPass.deviceAnchorToCullingPose = GetPose(0);
        cullingPass.projection = GetProjection(0);
        cullingPass.separation = 0.625f;
    }

    return kUnitySubsystemErrorCodeSuccess;
}

UnitySubsystemErrorCode WebXRDisplayProvider::GfxThread_Stop()
{
    WORKAROUND_RESET_SKIP_FIRST_FRAME();
    return kUnitySubsystemErrorCodeSuccess;
}

UnitySubsystemErrorCode WebXRDisplayProvider::GfxThread_FinalBlitToGameViewBackBuffer(const UnityXRMirrorViewBlitInfo* mirrorBlitInfo, WebXRProviderContext& ctx)
{
    return UnitySubsystemErrorCode::kUnitySubsystemErrorCodeSuccess;
}

void WebXRDisplayProvider::Stop()
{
}

void WebXRDisplayProvider::Shutdown()
{
}

void WebXRDisplayProvider::CreateTextures(int numTextures, int textureArrayLength, float requestedTextureScale)
{
    const int texWidth = (int)(SIDE_BY_SIDE ? frameBufferWidth : viewWidth);
    const int texHeight = (int)(SIDE_BY_SIDE ? frameBufferHeight : viewHeight);

    m_UnityTextures.resize(numTextures);

    // Tell unity about the native textures, getting back UnityXRRenderTextureIds.
    for (int i = 0; i < numTextures; ++i)
    {
        UnityXRRenderTextureDesc uDesc{};
        // Example of telling Unity to create the texture.  You can later obtain the native texture resource with
        // QueryTextureDesc
        uDesc.color.nativePtr = (void*)kUnityXRRenderTextureIdDontCare;
        uDesc.width = texWidth;
        uDesc.height = texHeight;
        uDesc.textureArrayLength = textureArrayLength;

        // Create an UnityXRRenderTextureId for the native texture so we can tell unity to render to it later.
        UnityXRRenderTextureId uTexId;
        m_Ctx.display->CreateTexture(m_Handle, &uDesc, &uTexId);
        UnityXRVector2 size;
        size.x = texWidth;
        size.x = texHeight;
        m_UnityTextures[i] = uTexId;
    }
}

void WebXRDisplayProvider::DestroyTextures()
{
    for (int i = 0; i < m_UnityTextures.size(); ++i)
    {
        if (m_UnityTextures[i] != 0)
        {
            m_Ctx.display->DestroyTexture(m_Handle, m_UnityTextures[i]);
        }
    }

    m_UnityTextures.clear();
}

UnityXRPose WebXRDisplayProvider::GetPose(int pass)
{
    UnityXRPose pose{};
    if (pass < (sizeof(s_PoseXPositionPerPass) / sizeof(s_PoseXPositionPerPass[0])))
        pose.position.x = s_PoseXPositionPerPass[pass];
    pose.position.y = 0.0f;
    pose.position.z = 0.0f;
    pose.rotation.x = 0.0f;
    pose.rotation.y = 0.0f;
    pose.rotation.z = 0.0f;
    pose.rotation.w = 1.0f;
    return pose;
}

UnityXRProjection WebXRDisplayProvider::GetProjection(int pass)
{
    UnityXRProjection ret;
    ret.type = kUnityXRProjectionTypeMatrix;
    int start = pass * 16;
    ret.data.matrix.columns[0].x = *(m_ViewsDataArray + start);
    ret.data.matrix.columns[0].y = *(m_ViewsDataArray + start + 1);
    ret.data.matrix.columns[0].z = *(m_ViewsDataArray + start + 2);
    ret.data.matrix.columns[0].w = *(m_ViewsDataArray + start + 3);
    ret.data.matrix.columns[1].x = *(m_ViewsDataArray + start + 4);
    ret.data.matrix.columns[1].y = *(m_ViewsDataArray + start + 5);
    ret.data.matrix.columns[1].z = *(m_ViewsDataArray + start + 6);
    ret.data.matrix.columns[1].w = *(m_ViewsDataArray + start + 7);
    ret.data.matrix.columns[2].x = *(m_ViewsDataArray + start + 8);
    ret.data.matrix.columns[2].y = *(m_ViewsDataArray + start + 9);
    ret.data.matrix.columns[2].z = *(m_ViewsDataArray + start + 10);
    ret.data.matrix.columns[2].w = *(m_ViewsDataArray + start + 11);
    ret.data.matrix.columns[3].x = *(m_ViewsDataArray + start + 12);
    ret.data.matrix.columns[3].y = *(m_ViewsDataArray + start + 13);
    ret.data.matrix.columns[3].z = *(m_ViewsDataArray + start + 14);
    ret.data.matrix.columns[3].w = *(m_ViewsDataArray + start + 15);
    return ret;
}

UnitySubsystemErrorCode WebXRDisplayProvider::UpdateDisplayState(UnityXRDisplayState * state)
{
    state->displayIsTransparent = transparentBackground;
    return kUnitySubsystemErrorCodeSuccess;
}

// Binding to C-API below here

static UnitySubsystemErrorCode UNITY_INTERFACE_API Display_Initialize(UnitySubsystemHandle handle, void* userData)
{
    auto& ctx = GetWebXRProviderContext(userData);

    ctx.displayProvider = new WebXRDisplayProvider(ctx, handle);

    // Register for callbacks on the graphics thread.
    UnityXRDisplayGraphicsThreadProvider gfxThreadProvider{};
    gfxThreadProvider.userData = &ctx;

    gfxThreadProvider.Start = [](UnitySubsystemHandle handle, void* userData, UnityXRRenderingCapabilities* renderingCaps) -> UnitySubsystemErrorCode {
        auto& ctx = GetWebXRProviderContext(userData);
        return ctx.displayProvider->GfxThread_Start(*renderingCaps);
    };

    gfxThreadProvider.SubmitCurrentFrame = [](UnitySubsystemHandle handle, void* userData) -> UnitySubsystemErrorCode {
        auto& ctx = GetWebXRProviderContext(userData);
        return ctx.displayProvider->GfxThread_SubmitCurrentFrame();
    };

    gfxThreadProvider.PopulateNextFrameDesc = [](UnitySubsystemHandle handle, void* userData, const UnityXRFrameSetupHints* frameHints, UnityXRNextFrameDesc* nextFrame) -> UnitySubsystemErrorCode {
        auto& ctx = GetWebXRProviderContext(userData);
        return ctx.displayProvider->GfxThread_PopulateNextFrameDesc(*frameHints, *nextFrame);
    };

    gfxThreadProvider.Stop = [](UnitySubsystemHandle handle, void* userData) -> UnitySubsystemErrorCode {
        auto& ctx = GetWebXRProviderContext(userData);
        return ctx.displayProvider->GfxThread_Stop();
    };

    gfxThreadProvider.BlitToMirrorViewRenderTarget = [](UnitySubsystemHandle handle, void* userData, const UnityXRMirrorViewBlitInfo mirrorBlitInfo) -> UnitySubsystemErrorCode {
        auto& ctx = GetWebXRProviderContext(userData);
        return ctx.displayProvider->GfxThread_FinalBlitToGameViewBackBuffer(&mirrorBlitInfo, ctx);
    };

    ctx.display->RegisterProviderForGraphicsThread(handle, &gfxThreadProvider);

    UnityXRDisplayProvider provider{&ctx, NULL, NULL};

    provider.UpdateDisplayState = [](UnitySubsystemHandle handle, void* userData, UnityXRDisplayState* state) -> UnitySubsystemErrorCode {
        auto& ctx = GetWebXRProviderContext(userData);
        return ctx.displayProvider->UpdateDisplayState(state);
    };

    ctx.display->RegisterProvider(handle, &provider);

    return ctx.displayProvider->Initialize();
}

UnitySubsystemErrorCode Load_Display(WebXRProviderContext& ctx)
{
    ctx.display = ctx.interfaces->Get<IUnityXRDisplayInterface>();
    if (ctx.display == NULL)
        return kUnitySubsystemErrorCodeFailure;

    UnityLifecycleProvider displayLifecycleHandler{};
    displayLifecycleHandler.userData = &ctx;
    displayLifecycleHandler.Initialize = &Display_Initialize;

    displayLifecycleHandler.Start = [](UnitySubsystemHandle handle, void* userData) -> UnitySubsystemErrorCode {
        auto& ctx = GetWebXRProviderContext(userData);
        return ctx.displayProvider->Start();
    };

    displayLifecycleHandler.Stop = [](UnitySubsystemHandle handle, void* userData) -> void {
        auto& ctx = GetWebXRProviderContext(userData);
        ctx.displayProvider->Stop();
    };

    displayLifecycleHandler.Shutdown = [](UnitySubsystemHandle handle, void* userData) -> void {
        auto& ctx = GetWebXRProviderContext(userData);
        ctx.displayProvider->Shutdown();
        delete ctx.displayProvider;
    };

    return ctx.display->RegisterLifecycleProvider("WebXR Export", "WebXR Display", &displayLifecycleHandler);
}

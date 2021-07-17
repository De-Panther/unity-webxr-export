#include "../../../../../../UnityHeaders/Headers/XR/IUnityXRDisplay.h"
#include "../../../../../../UnityHeaders/Headers/XR/IUnityXRTrace.h"

#include "ProviderContext.h"
#include <cmath>
#include <stdio.h>
#include <vector>

// We'll use DX11 to allocate textures if we're on windows.
#if defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(_WIN64) || defined(WINAPI_FAMILY)
#include "D3D11.h"
#include "IUnityGraphicsD3D11.h"
#define XR_DX11 1
#else
#define XR_DX11 0
#endif

#define SIDE_BY_SIDE 0
#define NUM_RENDER_PASSES 2
static const float s_PoseXPositionPerPass[] = {-1.0f, 1.0f};

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
    WebXRDisplayProvider(ProviderContext& ctx, UnitySubsystemHandle handle)
        : ProviderImpl(ctx, handle)
    {
    }

    UnitySubsystemErrorCode Initialize() override;
    UnitySubsystemErrorCode Start() override;

    UnitySubsystemErrorCode GfxThread_Start(UnityXRRenderingCapabilities& renderingCaps);

    UnitySubsystemErrorCode GfxThread_SubmitCurrentFrame();
    UnitySubsystemErrorCode GfxThread_PopulateNextFrameDesc(const UnityXRFrameSetupHints& frameHints, UnityXRNextFrameDesc& nextFrame);

    UnitySubsystemErrorCode GfxThread_Stop();
    UnitySubsystemErrorCode GfxThread_FinalBlitToGameViewBackBuffer(const UnityXRMirrorViewBlitInfo* mirrorBlitInfo, ProviderContext& ctx);

    UnitySubsystemErrorCode QueryMirrorViewBlitDesc(const UnityXRMirrorViewBlitInfo* mirrorRtDesc, UnityXRMirrorViewBlitDesc* blitDescriptor, ProviderContext& ctx);

    void Stop() override;
    void Shutdown() override;

private:
    void CreateTextures(int numTextures, int textureArrayLength, float requestedTextureScale);
    void DestroyTextures();

    UnityXRPose GetPose(int pass);
    UnityXRProjection GetProjection(int pass);

private:
#if XR_DX11
    std::vector<ID3D11Texture2D*> m_NativeTextures;
#else
    std::vector<void*> m_NativeTextures;
#endif
    std::vector<UnityXRRenderTextureId> m_UnityTextures;
};

UnitySubsystemErrorCode WebXRDisplayProvider::Initialize()
{
    return kUnitySubsystemErrorCodeSuccess;
}

UnitySubsystemErrorCode WebXRDisplayProvider::Start()
{
    return kUnitySubsystemErrorCodeSuccess;
}

UnitySubsystemErrorCode WebXRDisplayProvider::GfxThread_Start(UnityXRRenderingCapabilities& renderingCaps)
{
    renderingCaps.noSinglePassRenderingSupport = true;
    return kUnitySubsystemErrorCodeSuccess;
}

UnitySubsystemErrorCode WebXRDisplayProvider::GfxThread_SubmitCurrentFrame()
{
    // SubmitFrame();

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
        printf("reallocateTextures\n");
        DestroyTextures();

#if SIDE_BY_SIDE
        printf("SIDE_BY_SIDE\n");
        int numTextures = 1;
        int textureArrayLength = 0;
#else
        printf("!SIDE_BY_SIDE\n");
        int numTextures = frameHints.appSetup.singlePassRendering ? NUM_RENDER_PASSES - 1 : NUM_RENDER_PASSES;
        int textureArrayLength = frameHints.appSetup.singlePassRendering ? 2 : 0;
#endif
        CreateTextures(numTextures, textureArrayLength, frameHints.appSetup.textureResolutionScale);
    }

    // Frame hints tells us if we should setup our renderpasses with a single pass
    if (!frameHints.appSetup.singlePassRendering)
    {
        printf("Use multi-pass rendering to render\n");
        // Use multi-pass rendering to render

        // Can increase render pass count to do wide FOV or to have a separate view into scene.
        nextFrame.renderPassesCount = NUM_RENDER_PASSES;

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
                pass == 0 ? 0.0f : 0.5f, // x
                0.0f,                    // y
                0.5f,                    // width
                1.0f                     // height
            };
#endif
        }
    }
    else
    {
        printf("Use single-pass stereo rendering to render\n");
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

UnitySubsystemErrorCode WebXRDisplayProvider::GfxThread_FinalBlitToGameViewBackBuffer(const UnityXRMirrorViewBlitInfo* mirrorBlitInfo, ProviderContext& ctx)
{
#if XR_DX11
    ID3D11Device* dxDevice = ctx.interfaces->Get<IUnityGraphicsD3D11>()->GetDevice();
    ID3D11RenderTargetView* rtv = ctx.interfaces->Get<IUnityGraphicsD3D11>()->RTVFromRenderBuffer(mirrorBlitInfo->mirrorRtDesc->rtNative);
    ID3D11DeviceContext* immContext;
    dxDevice->GetImmediateContext(&immContext);

    immContext->OMSetRenderTargets(1, &rtv, NULL);
    // clear to blue
    const FLOAT clrColor[4] = {0, 0, 1, 1};
    immContext->ClearRenderTargetView(rtv, clrColor);
#endif

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
    printf("CreateTextures\n");
    const int texWidth = (int)(700.0f * requestedTextureScale * (SIDE_BY_SIDE ? 2.0f : 1.0f));
    const int texHeight = (int)(400.0f * requestedTextureScale);

    m_NativeTextures.resize(numTextures);
    m_UnityTextures.resize(numTextures);

    // Tell unity about the native textures, getting back UnityXRRenderTextureIds.
    for (int i = 0; i < numTextures; ++i)
    {
        UnityXRRenderTextureDesc uDesc{};
#if XR_DX11
        // Example of your compositor creating textures for unity to render in to.
        ID3D11Device* dxDevice = m_Ctx.interfaces->Get<IUnityGraphicsD3D11>()->GetDevice();
        D3D11_TEXTURE2D_DESC dxDesc = CD3D11_TEXTURE2D_DESC(DXGI_FORMAT_R8G8B8A8_UNORM, texWidth, texHeight);
        dxDesc.BindFlags = D3D11_BIND_RENDER_TARGET | D3D11_BIND_SHADER_RESOURCE;
        dxDesc.ArraySize = (textureArrayLength == 0 ? 1 : textureArrayLength);
        ID3D11Texture2D* nativeTex;
        dxDevice->CreateTexture2D(&dxDesc, NULL, &nativeTex);
        m_NativeTextures[i] = nativeTex;

        uDesc.color.nativePtr = nativeTex;
#else
        // Example of telling Unity to create the texture.  You can later obtain the native texture resource with
        // QueryTextureDesc
        uDesc.color.nativePtr = (void*)kUnityXRRenderTextureIdDontCare;
#endif
        uDesc.width = texWidth;
        uDesc.height = texHeight;
        uDesc.textureArrayLength = textureArrayLength;

        // Create an UnityXRRenderTextureId for the native texture so we can tell unity to render to it later.
        UnityXRRenderTextureId uTexId;
        m_Ctx.display->CreateTexture(m_Handle, &uDesc, &uTexId);
        m_UnityTextures[i] = uTexId;
    }
}

void WebXRDisplayProvider::DestroyTextures()
{
    printf("DestroyTextures\n");
#if XR_DX11
    assert(m_NativeTextures.size() == m_UnityTextures.size());
#endif

    for (int i = 0; i < m_UnityTextures.size(); ++i)
    {
        if (m_UnityTextures[i] != 0)
        {
            m_Ctx.display->DestroyTexture(m_Handle, m_UnityTextures[i]);
#if XR_DX11
            m_NativeTextures[i]->Release();
#endif
        }
    }

    m_UnityTextures.clear();
    m_NativeTextures.clear();
}

UnityXRPose WebXRDisplayProvider::GetPose(int pass)
{
    UnityXRPose pose{};
    if (pass < (sizeof(s_PoseXPositionPerPass) / sizeof(s_PoseXPositionPerPass[0])))
        pose.position.x = s_PoseXPositionPerPass[pass];
    pose.position.z = -10.0f;
    pose.rotation.w = 1.0f;
    return pose;
}

UnityXRProjection WebXRDisplayProvider::GetProjection(int pass)
{
    UnityXRProjection ret;
    ret.type = kUnityXRProjectionTypeHalfAngles;
    ret.data.halfAngles.left = -1.0;
    ret.data.halfAngles.right = 1.0;
    ret.data.halfAngles.top = 0.625;
    ret.data.halfAngles.bottom = -0.625;
    return ret;
}

UnitySubsystemErrorCode WebXRDisplayProvider::QueryMirrorViewBlitDesc(const UnityXRMirrorViewBlitInfo* mirrorBlitInfo, UnityXRMirrorViewBlitDesc* blitDescriptor, ProviderContext& ctx)
{
    if (ctx.displayProvider->m_UnityTextures.size() == 0)
    {
        // Eye texture is not available yet, return failure
        return UnitySubsystemErrorCode::kUnitySubsystemErrorCodeFailure;
    }

    for (int eyeidx = 0; eyeidx < 2; eyeidx++)
    {
        int srcTexId = ctx.displayProvider->m_UnityTextures[eyeidx];

        const UnityXRVector2 sourceTextureSize = {static_cast<float>(700), static_cast<float>(400)};
        const UnityXRRectf sourceUVRect = {0.0f, 0.0f, 1.0f, 1.0f};
        const UnityXRVector2 destTextureSize = {static_cast<float>(mirrorBlitInfo->mirrorRtDesc->rtScaledWidth), static_cast<float>(mirrorBlitInfo->mirrorRtDesc->rtScaledHeight)};
        UnityXRRectf destUVRect = {0.0f, 0.0f, 0.5f, 1.0f};
        if (eyeidx == 1)
            destUVRect = {0.5f, 0.0f, 0.5f, 1.0f};

        // By default, The source rect will be adjust so that it matches the dest rect aspect ratio.
        // This has the visual effect of expanding the source image, resulting in cropping
        // along the non-fitting axis. In this mode, the destination rect will be completely
        // filled, but not all the source image may be visible.
        UnityXRVector2 sourceUV0, sourceUV1, destUV0, destUV1;

        float sourceAspect = (sourceTextureSize.x * sourceUVRect.width) / (sourceTextureSize.y * sourceUVRect.height);
        float destAspect = (destTextureSize.x * destUVRect.width) / (destTextureSize.y * destUVRect.height);
        float ratio = sourceAspect / destAspect;
        UnityXRVector2 sourceUVCenter = {sourceUVRect.x + sourceUVRect.width * 0.5f, sourceUVRect.y + sourceUVRect.height * 0.5f};
        UnityXRVector2 sourceUVSize = {sourceUVRect.width, sourceUVRect.height};
        UnityXRVector2 destUVCenter = {destUVRect.x + destUVRect.width * 0.5f, destUVRect.y + destUVRect.height * 0.5f};
        UnityXRVector2 destUVSize = {destUVRect.width, destUVRect.height};

        if (ratio > 1.0f)
        {
            sourceUVSize.x /= ratio;
        }
        else
        {
            sourceUVSize.y *= ratio;
        }

        sourceUV0 = {sourceUVCenter.x - (sourceUVSize.x * 0.5f), sourceUVCenter.y - (sourceUVSize.y * 0.5f)};
        sourceUV1 = {sourceUV0.x + sourceUVSize.x, sourceUV0.y + sourceUVSize.y};
        destUV0 = {destUVCenter.x - destUVSize.x * 0.5f, destUVCenter.y - destUVSize.y * 0.5f};
        destUV1 = {destUV0.x + destUVSize.x, destUV0.y + destUVSize.y};

        (*blitDescriptor).blitParamsCount = 2;
        (*blitDescriptor).blitParams[eyeidx].srcTexId = srcTexId;
        (*blitDescriptor).blitParams[eyeidx].srcTexArraySlice = 0;
        (*blitDescriptor).blitParams[eyeidx].srcRect = {sourceUV0.x, sourceUV0.y, sourceUV1.x - sourceUV0.x, sourceUV1.y - sourceUV0.y};
        (*blitDescriptor).blitParams[eyeidx].destRect = {destUV0.x, destUV0.y, destUV1.x - destUV0.x, destUV1.y - destUV0.y};
    }
    return kUnitySubsystemErrorCodeSuccess;
}

// Binding to C-API below here

static UnitySubsystemErrorCode UNITY_INTERFACE_API Display_Initialize(UnitySubsystemHandle handle, void* userData)
{
    auto& ctx = GetProviderContext(userData);

    ctx.displayProvider = new WebXRDisplayProvider(ctx, handle);

    // Register for callbacks on the graphics thread.
    UnityXRDisplayGraphicsThreadProvider gfxThreadProvider{};
    gfxThreadProvider.userData = &ctx;

    gfxThreadProvider.Start = [](UnitySubsystemHandle handle, void* userData, UnityXRRenderingCapabilities* renderingCaps) -> UnitySubsystemErrorCode {
        auto& ctx = GetProviderContext(userData);
        return ctx.displayProvider->GfxThread_Start(*renderingCaps);
    };

    gfxThreadProvider.SubmitCurrentFrame = [](UnitySubsystemHandle handle, void* userData) -> UnitySubsystemErrorCode {
        auto& ctx = GetProviderContext(userData);
        return ctx.displayProvider->GfxThread_SubmitCurrentFrame();
    };

    gfxThreadProvider.PopulateNextFrameDesc = [](UnitySubsystemHandle handle, void* userData, const UnityXRFrameSetupHints* frameHints, UnityXRNextFrameDesc* nextFrame) -> UnitySubsystemErrorCode {
        auto& ctx = GetProviderContext(userData);
        return ctx.displayProvider->GfxThread_PopulateNextFrameDesc(*frameHints, *nextFrame);
    };

    gfxThreadProvider.Stop = [](UnitySubsystemHandle handle, void* userData) -> UnitySubsystemErrorCode {
        auto& ctx = GetProviderContext(userData);
        return ctx.displayProvider->GfxThread_Stop();
    };

    gfxThreadProvider.BlitToMirrorViewRenderTarget = [](UnitySubsystemHandle handle, void* userData, const UnityXRMirrorViewBlitInfo mirrorBlitInfo) -> UnitySubsystemErrorCode {
        auto& ctx = GetProviderContext(userData);
        return ctx.displayProvider->GfxThread_FinalBlitToGameViewBackBuffer(&mirrorBlitInfo, ctx);
    };

    ctx.display->RegisterProviderForGraphicsThread(handle, &gfxThreadProvider);

    UnityXRDisplayProvider provider{&ctx, NULL, NULL};
    provider.QueryMirrorViewBlitDesc = [](UnitySubsystemHandle handle, void* userData, const UnityXRMirrorViewBlitInfo mirrorBlitInfo, UnityXRMirrorViewBlitDesc* blitDescriptor) -> UnitySubsystemErrorCode {
        auto& ctx = GetProviderContext(userData);
        return ctx.displayProvider->QueryMirrorViewBlitDesc(&mirrorBlitInfo, blitDescriptor, ctx);
    };

    ctx.display->RegisterProvider(handle, &provider);

    return ctx.displayProvider->Initialize();
}

UnitySubsystemErrorCode Load_Display(ProviderContext& ctx)
{
    ctx.display = ctx.interfaces->Get<IUnityXRDisplayInterface>();
    if (ctx.display == NULL)
    {
        printf("ctx.display == NULL\n");
        return kUnitySubsystemErrorCodeFailure;
    }
    printf("Got ctx.display\n");

    UnityLifecycleProvider displayLifecycleHandler{};
    displayLifecycleHandler.userData = &ctx;
    displayLifecycleHandler.Initialize = &Display_Initialize;

    displayLifecycleHandler.Start = [](UnitySubsystemHandle handle, void* userData) -> UnitySubsystemErrorCode {
        auto& ctx = GetProviderContext(userData);
        return ctx.displayProvider->Start();
    };

    displayLifecycleHandler.Stop = [](UnitySubsystemHandle handle, void* userData) -> void {
        auto& ctx = GetProviderContext(userData);
        ctx.displayProvider->Stop();
    };

    displayLifecycleHandler.Shutdown = [](UnitySubsystemHandle handle, void* userData) -> void {
        auto& ctx = GetProviderContext(userData);
        ctx.displayProvider->Shutdown();
        delete ctx.displayProvider;
    };

    return ctx.display->RegisterLifecycleProvider("WebXR Export", "WebXR VR Display", &displayLifecycleHandler);
}
#include <stdint.h>
#include "emscripten.h"

typedef void (*webxr_void)();
typedef void (*webxr_void_int)(int32_t);
typedef void (*webxr_void_int_float4_float4)(int32_t, float, float, float, float, float, float, float, float);
typedef void (*webxr_void_string)(const char *ptr);

webxr_void_int_float4_float4 on_start_ar_ref;
webxr_void_int on_start_vr_ref;
webxr_void on_end_xr_ref;
webxr_void_string on_xr_capabilities_ref;

void set_webxr_events(
  webxr_void_int_float4_float4 _on_start_ar,
  webxr_void_int _on_start_vr,
  webxr_void _on_end_xr,
  webxr_void_string _on_xr_capabilities
) {
  on_start_ar_ref = _on_start_ar;
  on_start_vr_ref = _on_start_vr;
  on_end_xr_ref = _on_end_xr;
  on_xr_capabilities_ref = _on_xr_capabilities;
}

void EMSCRIPTEN_KEEPALIVE on_start_ar(int views_count,
      float left_x, float left_y, float left_w, float left_h,
      float right_x, float right_y, float right_w, float right_h)
{
  on_start_ar_ref(views_count,
                  left_x, left_y, left_w, left_h,
                  right_x, right_y, right_w, right_h);
}

void EMSCRIPTEN_KEEPALIVE on_start_vr(int views_count)
{
  on_start_vr_ref(views_count);
}

void EMSCRIPTEN_KEEPALIVE on_end_xr()
{
  on_end_xr_ref();
}

// TODO: 3 bools instead of one string
void EMSCRIPTEN_KEEPALIVE on_xr_capabilities(const char *display_capabilities)
{
  on_xr_capabilities_ref(display_capabilities);
}

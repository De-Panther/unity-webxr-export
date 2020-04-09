#include <stdint.h>
#include "emscripten.h"

typedef void (*webxr_void)();
typedef void (*webxr_void_string)();

webxr_void on_start_xr_ref;
webxr_void on_end_xr_ref;
webxr_void_string on_xr_capabilities_ref;
webxr_void_string on_webxr_data_ref;

void set_webxr_events(
  webxr_void _on_start_xr,
  webxr_void _on_end_xr,
  webxr_void_string _on_xr_capabilities,
  webxr_void_string _on_webxr_data
) {
  on_start_xr_ref = _on_start_xr;
  on_end_xr_ref = _on_end_xr;
  on_xr_capabilities_ref = _on_xr_capabilities;
  on_webxr_data_ref = _on_webxr_data;
}

void EMSCRIPTEN_KEEPALIVE on_start_xr()
{
  on_start_xr_ref();
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

// TODO: find a better way to transfer array of controllers and buttons
void EMSCRIPTEN_KEEPALIVE on_webxr_data(const char *webxr_data)
{
  on_webxr_data_ref(webxr_data);
}

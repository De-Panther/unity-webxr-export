# Disabling telemetry

The code enabling telemetry data collection is in the VR template that comes with the package. Ensure you've already added [the package from the store](https://u3d.as/1476) to your Unity project and look for the following snippet inside the [`index.html` file](../../Assets/WebGLTemplates/WebVR/index.html) of the template:

```js
MozillaResearch.telemetry.start({
  analytics: true,
  errorLogging: true,
  performance: true
});
```

By default, collecting number of visits, error logs and some performance measurements is enabled by setting the proper options (`analytics`, `errorLogging` and `performance` respectively) to `true`.

To prevent the template from sending such information, set the corresponding options to `false`:

```js
MozillaResearch.telemetry.start({
  analytics: false,
  errorLogging: true,
  performance: false
});
```

Or remove/comment the line completely:

```js
/*MozillaResearch.telemetry.start({
  analytics: true,
  errorLogging: true,
  performance: true
});*/
```
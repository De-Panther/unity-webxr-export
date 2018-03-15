# Disabling Telemetry

The code that enables Mozilla-Research Telemetry collection is in the VR template that comes with the package. Ensure you've already added [the package from the store](https://u3d.as/1476) to your Unity project and look for the following snippet inside the [`index.html` file](../../Assets/WebGLTemplates/WebVR/index.html) of the template:

```js
MozillaResearch.telemetry.start({
  analytics: true,
  errorLogging: true,
  performance: true
});
```

By default, each, number of visits, error logs and some performance measurements are enabled by setting the proper option (`analytics`, `errorLogging` and `performance` respectively) to `true`.

To prevent the template from sending such information, set the appropiate options to `false`:

```js
MozillaResearch.telemetry.start({
  analytics: false,
  errorLogging: true,
  performance: false
});
```

Or by commenting out these lines, or removing the code completely from the `index.html` file:

```js
/*MozillaResearch.telemetry.start({
  analytics: true,
  errorLogging: true,
  performance: true
});*/
```
/* global localStorage, location, Raven */
(function () {
// This checks for "production"-looking origins (e.g., `https://example.com`).
if (window.isSecureContext === false ||
    (location.hostname === 'localhost' ||
     location.hostname === '127.0.0.1' ||
     location.hostname === '0.0.0.0' ||
     location.hostname.indexOf('ngrok.io') > -1 ||
     location.hostname.indexOf('localtunnel.me') > -1)) {
  return;
}

injectScript('https://cdn.ravenjs.com/3.22.3/console/raven.min.js', function (err) {
  if (err) {
    console.warn('Could not load Raven.js script:', err);
    return;
  }
  if (!('Raven' in window)) {
    console.warn('Could not find `window.Raven` global');
    return;
  }
  ravenLoaded();
});

function ravenLoaded () {
  console.log('Raven.js script loaded');
  Raven.config('https://e359be9fb9324addb0dc97b664cf5ee6@sentry.io/294878')
       .install();
}

function injectScript (src, callback) {
  var script = document.createElement('script');
  script.src = src;
  script.crossorigin = 'anonymous';
  script.addEventListener('load', function () {
    if (callback) {
      callback(null, true);
    }
  });
  script.addEventListener('error', function (err) {
    if (callback) {
      callback(err);
    }
  });
  document.head.appendChild(script);
  return script;
}
})();

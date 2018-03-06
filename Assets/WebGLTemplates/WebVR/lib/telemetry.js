/* global localStorage, location, Raven */
(function (window) {
'use strict';

if (!('MozillaResearch' in window)) {
  window.MozillaResearch = {};
}

if (!('telemetry' in window.MozillaResearch)) {
  window.MozillaResearch.telemetry = {} ;
}

var navigator = window.navigator;
var telemetry = window.MozillaResearch.telemetry;

telemetry.ga = {
  create: function (trackingId, cookieDomain, name, fieldsObject) {
    window.ga('create', trackingId, cookieDomain, name, fieldsObject);
    return function (command) {
      if (navigator.doNotTrack === '1') { return; }
      var args = Array.prototype.slice.call(arguments);
      if (name && command !== 'provide') {
        command = name + '.' + command;
        args[0] = command;
      }
      window.ga.apply(undefined, args);
    };
  }
};

telemetry.start = function (config) {
  if (navigator.doNotTrack === '1') {
    return;
  }
  if (config.researchErrorLogging) {
    startErrorLogging();
  }
  if (config.researchAnalytics) {
    startAnalytics();
  }
};

setupAnalytics();

function setupAnalytics() {
  window.ga=window.ga||function(){(ga.q=ga.q||[]).push(arguments)};ga.l=+new Date;
  if (navigator.doNotTrack === '1') {
    return;
  }
  injectScript('https://www.google-analytics.com/analytics.js', function (err) {
    if (err) {
      console.warn('Could not load Analytics.js script:', err);
      return;
    }
  });  
}

function startErrorLogging() {
  injectScript('https://cdn.ravenjs.com/3.22.3/console/raven.min.js', function (err) {
    if (err) {
      console.warn('Could not load Raven.js script:', err);
      return;
    }
    if (!('Raven' in window)) {
      console.warn('Could not find `window.Raven` global');
      return;
    }
    configureRaven();
  });

  function configureRaven () {
    console.log('Raven.js script loaded');
    Raven.config('https://e359be9fb9324addb0dc97b664cf5ee6@sentry.io/294878')
         .install();
  }
};

function startAnalytics() {
  var CURRENT_VERSION = '1.0.1';
  var ga = telemetry.ga.create('UA-77033033-6', 'auto', 'mozillaResearch');
  ga('set', 'dimension1', CURRENT_VERSION);
  ga('send', 'pageview');
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

})(window);

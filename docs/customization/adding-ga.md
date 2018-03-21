# How to add Google Analytics to your game

Google Analytics is a popular web analytics service that tracks and reports website traffic. It offers insight about your user behaviour and allows you to make informed decisions regarding the evolution of your site.

The [VR template](../../Assets/WebGLTemplates/WebVR/index.html) that comes with the [Unity package](https://u3d.as/1476) already includes Analytics as a part of the telemetry library used for [collecting usage data](../data-collection.md) but you'll need to set up a Google Analytics account and get a [Tracking ID](https://support.google.com/analytics/answer/7372977) first, before using it.

## Quick set up

Start by visiting [Google Analytics](https://analytics.google.com/analytics/web) and sign up for a new account:

![Summary of the three steps to setup Google Analytics](./images/setup-ga.png)

Fill in the details of your organization and website. Remember to select `https://` in the drop-down menu of the `Website URL` field.

![Account and website names, website URL, industry classifications and reporting timezone](./images/filled-1.png)

When you finish, click on `Get Tracking ID` at the bottom of the page:

![Buttons Get Tracking ID and Cancel at the bottom of the page](./images/filled-2.png)

This will redirect you to the configuration page where you find the HTML code you'd use in the template:

![The configuration page includes the instructions for setting up your site](./images/setup-done.png)

Copy and paste this code as the first item into the `<head>` tag.

Read [Google Analytics' "Sending Data" documentation](https://developers.google.com/analytics/devguides/collection/gtagjs/sending-data?hl=es) to learn how to use the `gtag` API.

## Using Google Analytics

The amount of data in Google Analytics can be overwhelming to navigate and dissect. Fortunately, the folks at Google have developed a series of courses to explain you the main concepts of traffic analysis. We recommend you start at [Google Analytics Academy](https://analytics.google.com/analytics/academy/).

## Open-source alternatives

There are a few [open-source alternatives](https://en.wikipedia.org/wiki/List_of_web_analytics_software#Free_/_Open_source_(FLOSS)) out there. Like Google Analytics, they are free although you need to host them yourself. We recommend [Matomo](https://matomo.org/) and [Open Web Analytics](http://www.openwebanalytics.com) (OWA). Both offer geolocation, click-heat maps, traffic by referrer, event tracking, etc. Which specifically depends on your needs but you can compare both side by side visiting the [demo pages for Matomo](https://demo.matomo.org) and [OWA](http://demo.openwebanalytics.com/owa/).
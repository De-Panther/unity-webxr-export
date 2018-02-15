# How to add Google Analytics to your game

Google Analytics is a popular web analytics service by Google that tracks and reports website traffic. It offers insight about your user behaviour and allow you to make informed decisions.

You will need to modify the VR template that comes with the package so be sure you've already added [the package from the store](http://u3d.as/1476) to your Unity project.

## Set up

Start by visiting [Google Analytics](https://analytics.google.com/analytics/web) and sign up for a new account:

![Summary of the three steps to setup Google Analytics](./images/setup-ga.png)

Fill in the details of your organization and website. Remember to select `https` in the drop menu of the `Website URL` field.

![Account and website names, website URL, industry classifications and reporting timezone](./images/filled-1.png)

Whe you finish, click on `Get Tracking ID` at the bottom of the page:

![Buttons Get Tracking ID and Cancel at the bottom of the page](./images/filled-2.png)

This will redirect you to the configuration page where you find the HTML code you need to use in the template:

![The configuraiton page includes the instructions for setting up your site](./images/setup-done.png)

Now, in your project, with your favourite code editor, open the file at `Assets/WebGLTemplates/WebVR/index.html` and paste the code provided by Google Analytics just after the `<head>` tag:

![The code provided by GA should be added right after the head tag](./images/add-to-index.png)

## Getting started

The amount of data in Google Analytics is huge and overwhelming. Fortunately, the folks in Google have developed a series of courses to explain you the main concepts of traffic analysicis. We recommend you start at [Google Analytics Academy](https://analytics.google.com/analytics/academy/). 

## Free/Open source alternatives

There are a few [free/open source alternatives](https://en.wikipedia.org/wiki/List_of_web_analytics_software#Free_/_Open_source_(FLOSS)) out there. We recommend [Matomo](https://matomo.org/) and [Open Web Analytics](http://www.openwebanalytics.com) (OWA). Both offer geolocation, click heat maps, traffic by referrer, event tracking... Which specifically depends on your needs but you can compare both side by side visiting the [demo pages for Motomo](https://demo.matomo.org) and [OWA](http://demo.openwebanalytics.com/owa/). 
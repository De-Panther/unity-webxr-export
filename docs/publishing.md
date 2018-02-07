# Publishing

Publishing on the Internet is not different from publishing a blog or the landing page of your game. Actually, with a web-based game you could embed the game in your site, putting it just one-click away from your players! No stores, no approvals, no installations... [on compatible browsers](../README.md#compatibility), they just work.

## Choosing a static web server

Unless you use your own machines, it is likely you want to use a free hosting service. The main limit you'll find with free-hosting is restrictions over file-size, type and bandwith. WebVR exports propietary formats for packaged asset and those can be heavy; from few megabytes to several hundreds.

This document assumes you have already exported your game with Unity, targeting WebGL and using the WebVR template that comes with the assets. To know how to export your project for WebVR, please refer to [Setting up a Unity project for WebVR](./project-setup.md).

### Deploying with `surge`

The most simple way to publish your game under a custom domain is using `surge`. Install it [using `npm`](https://nodejs.org/en/download/):

```
$ npm install --global surge
```

And inside your build directory, simply run:

```
$ surge
```

It's that easy. **Seriously**.

![Using surge](https://surge.sh/images/help/getting-started-with-surge.gif)

Refer to [`surge` documentation](https://surge.sh/help/getting-started-with-surge) to learn how to futher customize the deploy.

Remember `surge` will serve your files under both `http` and `https` connections but the free plan won't redirect you from `http` to `https` automatically. Remember to share the `https` version of your site!  

### Deploying on GitHub

Another good choice is using GitHub Pages. If your build folder [meets the constraints](https://help.github.com/articles/what-is-github-pages/#usage-limits), you can convert it into a `git` repository and publish it. Refer to [Create a repo](https://help.github.com/articles/create-a-repo/) and [Configuring a publishing source for GitHub Pages](https://help.github.com/articles/configuring-a-publishing-source-for-github-pages/#enabling-github-pages-to-publish-your-site-from-master-or-gh-pages) to find out how to create and publish your game.

### Reaching bandwith restrictions

Even if your files are not that big, we hope you'll be successful and attract tons of users to your game. That means potentially thousands of gigabytes being downloaded per month. We are working on improving the template to [save as much bandwith as possible](https://github.com/mozilla/unity-webvr-export/issues/98) but eventually, you'll reach bandwith limits. If that's the case, consider using a CDN or get a paid hosting plan. There are [very good and inexpensive plans out there](http://www.turiyaware.com/finding-the-right-hosting-for-indie-game-developers/).

#### Integrating with a CDN

Refer to the section `Moving build output files` from Unity's [WebGL building documentation](https://docs.unity3d.com/Manual/webgl-building.html) to learn how to configure the template to work with an external CDN. 
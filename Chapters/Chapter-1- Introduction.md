# Chapter 1 Introduction

One of the biggest challenges you face as a programmer on a new project is your design framework.  Using a framework will save you an enormous amount of time in the long run, but costs you up front.  It's often hard to justify that up-front time.

This book is about my Blazor Framework.  It's been developed over several projects, both commercial and altruisic.  In publishing this book I'm not saying this is it, the bee's knees, I know better than you.  Form your own opinion, use as little or as much of it as you like.  Butcher it, enhance it, plagerise.  It's free.

There are some guidelines I try not to break (too often):

1. Don't re-invent the wheel.  I break this one with my `Component`! (I have my reasons as I'll explain in the relevant chapter).
2. We're writing an **application**, not a website.  It's hosted on a website, but that's as far as we need to constraint ourselves.  Too many designs are trapped by the web page paradigm, and fail to take advantage of what lies outside.
3. We live in a asynchronous world, data access both read and write is asynchronous.  So programme that way - **Async all the Way** from the database layer to the UI.
4. Build boilerplate code - get everything that's common into generic base classes and interfaces.  We'll see this in action in both the database access layers and UI.
5. Componentise the UI.  HTML markup belongs in components, not in you View. 

Frameworks need projects to demonstrate them.  I'm going to keep what we use here simple.  The Blazor template produces a site with Weather Forecasts.  We're going to develop that theme, building the data access layers and UI to display, view and edit Weather Forecasts.  We'll also add Weather Stations and Weather Reports to show how the implement out the framework.

There are some key design decisions I've made which you may or may not agree with.

#### Routing

Most SPA frameworks implement routing and most designs rely on it.  For those not fully familiar with routing, routing is what the application does with a url like *https://mysite/products/6*.  There's no real directory at */products* or a page at that url.  The application applies a matching process to it's routing table to find the correct component that serves that URL and then puts the component into the page.  The URL above will in all probability use a component `productcomponent` and set it's `id` parameter to 6.  It will then grab the relevant data though the data access layers, format out the page and display it.


  
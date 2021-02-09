<p align="center"><a href="https://pushradar.com" target="_blank"><img src="https://pushradar.com/images/logo/pushradar-logo-dark.svg" width="300"></a></p>

<p align="center">
    <a href="https://www.nuget.org/packages/PushRadar"><img src="https://img.shields.io/nuget/v/pushradar?cacheSeconds=60&color=5b86e5"></a>
    <a href="https://www.nuget.org/packages/PushRadar"><img src="https://img.shields.io/nuget/dt/pushradar?cacheSeconds=60&color=5b86e5"></a>
    <a href="https://www.nuget.org/packages/PushRadar"><img alt="GitHub" src="https://img.shields.io/github/license/pushradar/pushradar-server-dotnet?cacheSeconds=60&color=5b86e5"></a>
</p>
<br />

## PushRadar .NET Server Library

[PushRadar](https://pushradar.com) is a realtime API service for the web. The service uses a simple publish-subscribe model, allowing you to broadcast "messages" on "channels" that are subscribed to by one or more clients. Messages are pushed in realtime to those clients.

This is PushRadar's official .NET server library.

## Prerequisites

In order to use this library, please ensure that you have the following:

- .NET Standard 2+
- A PushRadar account - you can sign up at [pushradar.com](https://pushradar.com)

## Installation

The easiest way to get up and running is to install the library from [NuGet](http://nuget.org). Run the following command in the Package Manager console:

```bash
$ Install-Package PushRadar
```

## Broadcasting Messages

```sharp
var radar = new PushRadar.PushRadar("your-secret-key");
radar.Broadcast("channel-1", new Dictionary<string, object>() {
    { "message", "Hello world!" }
});
```

## Receiving Messages

```html
<script src="https://pushradar.com/js/v3/pushradar.min.js"></script>
<script>
    var radar = new PushRadar('your-public-key');
    radar.subscribe.to('channel-1', function (data) {
        console.log(data.message);
    });
</script>
```

## Private Channels

Private channels require authentication and start with the prefix **private-**. We recommend that you use private channels by default to prevent unauthorised access to channels.

You will need to set up an authentication endpoint that returns a token using the `Auth(...)` method if the user is allowed to subscribe to the channel. For example:

```csharp
var radar = new PushRadar.PushRadar("your-secret-key");
var channelName = HttpContext.Current.Request.QueryString['channelName'];
if (/* is user allowed to access channel? */ true) {
    var kvp = new KeyValuePair<string, object>("token", radar.Auth(channelName));
    return Json(kvp);
}
```

Then register your authentication endpoint by calling the `auth(...)` method client-side:

```javascript
radar.auth('/auth');
```

## Complete Documentation

Complete documentation for PushRadar's .NET server library can be found at: <https://pushradar.com/docs/3.x?lang=dotnet>

## License

Copyright 2021, PushRadar. PushRadar's .NET server library is licensed under the MIT license:
http://www.opensource.org/licenses/mit-license.php
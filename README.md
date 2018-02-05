AzureIntegration
===

Features that integrate ASP.NET Core with Azure.

This project is part of ASP.NET Core. You can find samples, documentation and getting started instructions for ASP.NET Core at the [Home](https://github.com/aspnet/home) repo.


SiteExtensions
===

To install ASP.NET Core runtime site extension:

1. Set `SCM_SITEEXTENSIONS_FEED_URL` application setting to `https://dotnet.myget.org/F/aspnetcore-release/`
2. Go to `Advanced Tools` -> `Site extensions` -> `Gallery`
3. Enter `AspNetCoreRuntime` into `Search` box and click `Search`
4. Click `+` to install site extension, click `Restart site` on the right side of the page when installation finishes.
5. Restart site in `Overview` tab of `App service`


To update ASP.NET Core runtime site extension:
1. Go to `Advanced Tools` -> `Process Explorer`
2. Kill `dotnet.exe` process
3. Go to `Site extensions`
4. Click update on site extension
# Escc.Schools.SchoolClosures

Displays a list of all current emergency school closures in East Sussex.

## How school closures work

The data is pulled from the school closures data source which is updated regularly by setting up a scheduled task which runs at regular intervals (recommended to be every 5 minutes):

	Escc.ServiceClosures.AdministrationTool.exe /pulldata school

The tool run by the scheduled task is maintained in a separate repository.

You can view school closures for a specific date in the future using a query string:

	https://hostname/?date=2018-06-27

## Caching

There are two caches on the page which displays the full list of school closures - the data from the data source is cached in the ASP.NET cache for 5 minutes, then displayed by a partial view which is output cached for 5 minutes. Although these are separate caches they will usually align as the initial update is triggered by loading the page in both cases.

## How the RSS feed works

The RSS feed for school closures relies on a file called `ClosuresRss.xslt` being present. This comes from the `Escc.ServiceClosures.Rss` NuGet package.
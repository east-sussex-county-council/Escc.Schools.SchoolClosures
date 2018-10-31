# Escc.Schools.SchoolClosures

Displays a list of all current emergency school closures in East Sussex.

## How school closures work

The data is pulled from the school closures data source and cached locally as XML by setting up a scheduled task which runs at regular intervals (recommended to be every 5 minutes):

	Escc.ServiceClosures.AdministrationTool.exe /pulldata school

The tool run by the scheduled task is maintained in a separate repository.

You can view school closures for a specific date in the future using a query string:

	https://hostname/?date=2018-06-27

## How the RSS feed works

The RSS feed for school closures relies on a file called `ClosuresRss.xslt` being present. This comes from the `Escc.ServiceClosures.Rss` NuGet package.
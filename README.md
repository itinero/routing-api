#  Itinero API

This project is an implementation of a basic routing API based on Itinero. It loads RouterDb files from a configured folder and exposes one routing instance per file.

### Goal

This project serves both as a demo project for Itinero and an easy way of setting up a routing instance.

### Setup

Basically you need build the project and place a RouterDb file into the configured data folder.

1. Install [.NET core](https://www.microsoft.com/net/core) for your platform.
1. Run the build script, build.sh or build.bat on windows.
1. Then [download a RouterDb](http://files.itinero.tech/data/itinero/routerdbs/planet/europe/) file or create one using the [Itinero data processing tool](https://github.com/itinero/idp).
1. Place the RouterDb file into the configured data folder, by default this is ./src/Itinero.API/data. Configuration file is [here](https://github.com/itinero/routing-api/blob/develop/src/Itinero.API/appsettings.json).
1. Run the run script, run.sh or run.bat on windows.

You should new see the service reporting messages on loading the RouterDb's you configured:

```
[Bootstrapper] information - Loading all routerdb's from path: \path\to\src\Itinero.API\data
[Bootstrapper] information - Loaded instance luxembourg from: \path\to\src\Itinero.API\data\luxembourg.c.cf.routerdb
Hosting environment: Production
Content root path: \path\to\src\Itinero.API
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
[Bootstrapper] information - Loaded instance belgium from: \path\to\src\Itinero.API\data\belgium.c.cf.routerdb
[Bootstrapper] information - Loaded instance netherlands from: \path\to\rc\Itinero.API\data\netherlands.c.cf.routerd
```

### API

When the service is setup one instance per RouterDb will be available. The name of the instance is identical to the name of the file until the first '.'. For example, 'belgium.c.cf.routerdb' will be 'belgium'.

By default when opening your browser at http://localhost:5000/ there will be a list with loaded instances. When you click one of the instances a map will open centered on the loaded area, clicking on the map still set a startpoint and clicking again an instance.

The following is available:

- http://localhost:5000/ : A list of loaded instances.
- http://localhost:5000/{instance} : The map centered on the loaded area in the instance with routing enable for cars.

The API:

http://localhost:5000/{instance}/routing : Accepts requests using the following parameters:
- profile: One of the profiles loaded in the RouterDb, think _car_, _bicycle_ or _pedestrian_.
- loc: Add this parameter at least twice, first is startpoint, last is the destination.

Make sure to ask the API for JSON by adding _Content-Type: application/json_ header. The route is returned as GeoJson.

Example: http://localhost:5000/belgium/routing?profile=car&loc=51.055207,3.722992&loc=50.906497,4.194031

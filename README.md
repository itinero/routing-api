OsmSharp.Service.Routing
========================

A routing webservice API built around OsmSharp. Accepts and handles routing requests over HTTP.

Use as a standalone service
---------------------------

This project can be used as a library in an existing webservice or can be used standalone using the [Selfhost-project](https://github.com/OsmSharp/OsmSharp.Service.Routing/tree/master/OsmSharp.Service.Routing.SelfHost). The project uses a simple configuration file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <section name="ApiConfiguration" type="OsmSharp.Service.Routing.Configurations.ApiConfiguration,
             OsmSharp.Service.Routing"/>
  </configSections>
  <ApiConfiguration>
    <instances>
      <add name="instance-name" graph="/path/to/raw/osm-file.osm.pbf" type="raw" format="osm-pbf" />
    </instances>
  </ApiConfiguration>
</configuration>
```

API
---

{instance}/routing: A routing query endpoint accepting the following parameters:
* vehicle=car|bicycle|pedestrian
* loc=lat,lon

Default responses are always in GeoJSON.

##### A routing-query would look like this:

http://{hostingurl}/{instance}/routing?vehicle={vehicle}&loc={lat,lon}&loc={lat,lon}

##### An example with the default GeoJSON response and the default settings:

[http://localhost:1234/kempen-big/routing?vehicle=car&loc=51.261028637438386,4.780511856079102&loc=51.26795006429507,4.801969528198242](http://localhost:1234/kempen-big/routing?vehicle=car&loc=51.261028637438386,4.780511856079102&loc=51.26795006429507,4.801969528198242)

[http://geojson.io/#id=gist:anonymous/77311b1ddc209b41bd90&map=15/51.2653/4.7906](http://geojson.io/#id=gist:anonymous/77311b1ddc209b41bd90&map=15/51.2653/4.7906)

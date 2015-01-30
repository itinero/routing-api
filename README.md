OsmSharp.Service.Routing
========================

A routing webservice API built around OsmSharp. Serves routing requests over HTTP.

API
---

{instance}/routing: A routing query endpoint.

{instance}/routing/network: A network query endpoint.

Use a standalone service
------------------------

This project can be used as a library in an existing webservice or can be used standalone using the [Selfhost-project](https://github.com/OsmSharp/OsmSharp.Service.Routing/tree/master/OsmSharp.Service.Routing.SelfHost). The project uses a simple configuration file:

´´´
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
´´´

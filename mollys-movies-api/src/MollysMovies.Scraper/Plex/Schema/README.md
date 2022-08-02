# Plex XML

The Plex client uses XML, here's how the schema files and generated object models were setup.

1. Take an example document from the API.
2. Generate XML schema from instance
   document [e.g. with Rider](https://www.jetbrains.com/help/rider/Generating_XML_Schema_From_Instance_Document.html).
3. Generate C# classes with [LinqToXsdCore](https://github.com/mamift/LinqToXsdCore) (requires .NET Core 3.1) e.g.

```
dotnet tool install LinqToXsdCore -g
linqtoxsd config -e metadata.xsd
linqtoxsd gen metadata.xsd -c metadata.xsd.config
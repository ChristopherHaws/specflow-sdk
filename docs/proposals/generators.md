## Todo:
Some ideas...

## Default Generator
Instead of using an app.config file to pick a test adapter, lets set it in the csproj file.
```xml
<PropertyGroup>
    <DefaultSpecFlowTestAdapter>xUnit</DefaultSpecFlowTestAdapter>
</PropertyGroup>
```

## Generator Per Feature
To override the test adapter for a given feature file, add the `TestAdapter` metadata to your item.
```xml
<ItemGroup>
    <SpecFlowFeature Include="Feature.feature" TestAdapter="xUnit" />
</ItemGroup>
```

## Authoring Generators
To make a generator available for use, create a props file in your generators nuget package with the following:
```xml
<ItemGroup>
    <SpecFlowGeneratorAssembly Include="$(MSBuildThisFileDirectory)../tools/SpecFlow.Generators.Xunit.dll" TestAdapter="xUnit" />
</ItemGroup>
```

Possibly this could be made simpler by making a property that instructs the build to create a props file:
```xml
<PropertyGroup>
    <TargetFramework>net46</TargetFramework>
    <SpecFlowGeneratorName>xUnit</SpecFlowGeneratorName>
</PropertyGroup>
```

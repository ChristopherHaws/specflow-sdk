# SpecFlow.Sdk Prototype 
This is a prototype nuget package that provides SpecFlow integration with SDK based projects.

[![Build status](https://ci.appveyor.com/api/projects/status/2c7k3ucgh751e6do/branch/master?svg=true)](https://ci.appveyor.com/project/ChristopherHaws/specflow-sdk/branch/master)

## Features
- [X] Works with SDK based projects
- [X] Real-time Unit Test Discovery for adapters that support it
- [X] Incremental builds work so features are only generated when they get modified
- [X] SpecRun Shim to make SpecRun works with SDK based projects

### Known Limitations
- This project doesn't work well with the Visual Studio plugin. The only known limitation so far is that when you add a feature file it adds the SpecFlowSingleFileGenerator to the newly added feature file. You will either need to disable to SpecFlow Visual Studio Extension or manually remove the generator from the csproj file everytime you add a feature.
- Since SpecFlow is compiled against full .NET Framework, this project does not work with .NET Core

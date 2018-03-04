# Todo
- Raname `SpecFlowFeatureFile` to `SpecFlowFeature`
- Create a `PropertyPageSchema` for the `SpecFlowFeature` item group. This will allow the feature files to be removed from the `None` item group while still showing up in the solution explorer
- Modify the generator task to output files to the intermediate directory, and change the file extension to `.feature.g.cs` (configurable)
- Modify the generator task to take an array of from files and an array of to files
- Instead of probing for generators, take an array of them from msbuild and then execute the correct one using the info in the `SpecFlowFeature` metadata.
- Clean up logging (add a log message that always logs the number of generated files and how long it took to generate them)
- Make the generation happen in parallel
- Figure out any additional properties that can be configured in app.config and move them to the sdk
    - StepAssemblies (possibly custom metadata on ProjectReference and PackageReference item group)
    - Runtime Plugins
    - Generator Plugins
- Figure out if it's possible to get codelense to work with feature files
    - Possibly using a fake line of code in the generated method `var a = nameof(CalculatorSteps.GivenIHaveEntered)`. Problem with this is it wont work without runtime being known, which is should be since it is all part of msbuild now. Then this file gets compiled in `CoreCompileDependsOn` so that it gets added at design compile time.
- Figure out if I should add `<Generator>MSBuild:GenerateSpecFlowFeatureFile</Generator>` to each `SpecFlowFeature`
- Figure out if I can make `AddSpecFlowGeneratedFeatureFiles` use the `SpecFlowGeneratedFile` items instead of the `SpecFlowFeature` items
- Create unit tests
- Create integration tests

## Research
- `ImportProjectExtensionProps` and `ImportProjectExtensionTargets`
- `ProjectSchemaDefinitions`
- `PropertyPageSchema`
- `ItemDefinitionGroup`
- `ProjectCapability`

## References
- https://github.com/Microsoft/msbuild
- https://github.com/dotnet/sdk
- https://github.com/dotnet/project-system
- https://github.com/Microsoft/VSProjectSystem

## Other References
- https://daveaglick.com/posts/running-a-design-time-build-with-msbuild-apis
- https://github.com/AArnott/CodeGeneration.Roslyn/blob/master/src/CodeGeneration.Roslyn.Tasks/CodeGeneration.Roslyn.Tasks.csproj
- https://mhut.ch/journal/2015/06/30/build_time_code_generation_msbuild
- https://www.safaribooksonline.com/library/view/inside-the-microsoft/9780735659827/ch06.html

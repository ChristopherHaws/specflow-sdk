using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using TechTalk.SpecFlow.Configuration;
using TechTalk.SpecFlow.Generator;
using TechTalk.SpecFlow.Generator.Interfaces;
using TechTalk.SpecFlow.Generator.Project;
using TechTalk.SpecFlow.Plugins;
using TechTalk.SpecFlow.Tracing;
using TechTalk.SpecFlow.Utils;

namespace SpecFlow.Build.Tasks
{
	public class SpecFlowCompile : SpecFlowTask
	{
		private readonly List<ITaskItem> generatedFiles = new List<ITaskItem>();

		public Boolean VerboseOutput { get; set; }

		[Required]
		public String ProjectPath { get; set; }

		/// <summary>
		/// Gets or sets the collection of feature files being transpiled.
		/// </summary>
		[Required]
		public ITaskItem[] FeatureFiles { get; set; }

		/// <summary>
		/// Gets the generated feature files that have had been transpiled.
		/// </summary>
		[Output]
		public ITaskItem[] GeneratedFiles => this.generatedFiles.ToArray();

		protected override void DoExecute()
		{
			//System.Diagnostics.Debugger.Launch();

			var traceListener = this.VerboseOutput
				? (TechTalk.SpecFlow.Tracing.ITraceListener)new TechTalk.SpecFlow.Tracing.TextWriterTraceListener(GetMessageWriter(MessageImportance.High), "SpecFlow: ")
				: new NullListener();

			var specFlowProject = MsBuildProjectReader.LoadSpecFlowProjectFromMsBuild(this.ProjectPath);
			specFlowProject.ProjectSettings.ConfigurationHolder = new SpecFlowConfigurationHolder(
				specFlowProject.ProjectSettings.ConfigurationHolder.ConfigSource,
				Environment.ExpandEnvironmentVariables(specFlowProject.ProjectSettings.ConfigurationHolder.Content)
			);

			FixPluginPaths(specFlowProject.Configuration.SpecFlowConfiguration);

			traceListener.WriteToolOutput("Processing project: " + specFlowProject.ProjectSettings.ProjectName);
			var generationSettings = new GenerationSettings
			{
				CheckUpToDate = false,
				WriteResultToFile = true
			};

			using (var container = GeneratorContainerBuilder.CreateContainer(specFlowProject.ProjectSettings.ConfigurationHolder, specFlowProject.ProjectSettings))
			using (var generator = container.Resolve<ITestGenerator>())
			{
				traceListener.WriteToolOutput("Using Generator: {0}", generator.GetType().FullName);

				var featureFiles = this.FeatureFiles
					.Select(x => new FeatureFileInput(FileSystemHelper.GetRelativePath(x.ItemSpec, specFlowProject.ProjectSettings.ProjectFolder)))
					.ToList();

				foreach (var featureFile in featureFiles)
				{
					var outputFilePath = generator.GetTestFullPath(featureFile);
					featureFile.GeneratedTestProjectRelativePath = FileSystemHelper.GetRelativePath(outputFilePath, specFlowProject.ProjectSettings.ProjectFolder);

					var generationResult = generator.GenerateTestFile(featureFile, generationSettings);
					if (!generationResult.Success)
					{
						traceListener.WriteToolOutput("{0} -> test generation failed", featureFile.ProjectRelativePath);

						foreach (var testGenerationError in generationResult.Errors)
						{
							this.RecordError(testGenerationError.Message, featureFile.GetFullPath(specFlowProject.ProjectSettings), testGenerationError.Line, testGenerationError.LinePosition);
						}
					}
					else if (generationResult.IsUpToDate)
					{
						traceListener.WriteToolOutput("{0} -> test up-to-date", featureFile.ProjectRelativePath);
					}
					else
					{
						traceListener.WriteToolOutput("{0} -> test updated", featureFile.ProjectRelativePath);
					}

					if (generationResult.Success)
					{
						this.generatedFiles.Add(new TaskItem(featureFile.GetGeneratedTestFullPath(specFlowProject.ProjectSettings)));
					}
				}
			}
		}

		private static void FixPluginPaths(SpecFlowConfiguration configuration)
		{
			var plugins = new List<PluginDescriptor>();

			foreach (var plugin in configuration.Plugins)
			{
				var path = plugin.Path;
				if (path != null)
				{
					path = Environment.ExpandEnvironmentVariables(path);
				}

				plugins.Add(new PluginDescriptor(plugin.Name, path, plugin.Type, plugin.Parameters));
			}

			configuration.Plugins = plugins;
		}
	}
}

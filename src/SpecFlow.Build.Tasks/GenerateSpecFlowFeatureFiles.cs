using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;
using TechTalk.SpecFlow.Configuration;
using TechTalk.SpecFlow.Generator;
using TechTalk.SpecFlow.Generator.Interfaces;
using TechTalk.SpecFlow.Generator.Project;
using TechTalk.SpecFlow.Plugins;
using TechTalk.SpecFlow.Utils;

namespace SpecFlow.Build.Tasks
{
	public class GenerateSpecFlowFeatureFiles : SpecFlowTask
	{
		private readonly List<ITaskItem> generatedFiles = new List<ITaskItem>();

		public Boolean LaunchDebugger { get; set; }

		[Required]
		public String ProjectPath { get; set; }

		/// <summary>
		/// Gets or sets the collection of feature files being transpiled.
		/// </summary>
		[Required]
		public ITaskItem[] FeatureFiles { get; set; }

		[Required]
		public ITaskItem[] FeatureGeneratedFiles { get; set; }

		/// <summary>
		/// Gets the generated feature files that have had been transpiled.
		/// </summary>
		[Output]
		public ITaskItem[] GeneratedFiles => this.generatedFiles.ToArray();

		protected override void DoExecute()
		{
			if (this.LaunchDebugger)
			{
				Debugger.Launch();
			}

			var specFlowProject = MsBuildProjectReader.LoadSpecFlowProjectFromMsBuild(this.ProjectPath);
			specFlowProject.ProjectSettings.ConfigurationHolder = new SpecFlowConfigurationHolder(
				specFlowProject.ProjectSettings.ConfigurationHolder.ConfigSource,
				Environment.ExpandEnvironmentVariables(specFlowProject.ProjectSettings.ConfigurationHolder.Content)
			);

			FixPluginPaths(specFlowProject.Configuration.SpecFlowConfiguration);

			this.Log.LogMessage(MessageImportance.Normal, $"SpecFlow: Processing project: {specFlowProject.ProjectSettings.ProjectName}");
			var generationSettings = new GenerationSettings
			{
				CheckUpToDate = false,
				WriteResultToFile = false
			};

			using (var container = GeneratorContainerBuilder.CreateContainer(specFlowProject.ProjectSettings.ConfigurationHolder, specFlowProject.ProjectSettings))
			using (var generator = container.Resolve<ITestGenerator>())
			{
				var sw = Stopwatch.StartNew();

				this.Log.LogMessage(MessageImportance.Normal, $"SpecFlow: Using Generator {generator.GetType().FullName}");

				for (var i = 0; i < this.FeatureFiles.Length; i++)
				{
					var inputItem = this.FeatureFiles[i];
					var outputItem = this.FeatureGeneratedFiles[i];

					var featureRelativePath = FileSystemHelper.GetRelativePath(inputItem.ItemSpec, specFlowProject.ProjectSettings.ProjectFolder);
					var generatedFeatureRelativePath = FileSystemHelper.GetRelativePath(inputItem.ItemSpec, Path.GetDirectoryName(outputItem.ItemSpec));

					var featureFile = new FeatureFileInput(featureRelativePath);

					//var outputFilePath = generator.GetTestFullPath(featureFile);
					//featureFile.GeneratedTestProjectRelativePath = FileSystemHelper.GetRelativePath(outputFilePath, specFlowProject.ProjectSettings.ProjectFolder);

					var generationResult = generator.GenerateTestFile(featureFile, generationSettings);

					if (generationResult.Success)
					{
						var outputDirectory = Path.GetDirectoryName(outputItem.ItemSpec);
						if (!Directory.Exists(outputDirectory))
						{
							Directory.CreateDirectory(outputDirectory);
						}

						//HACK
						var code = generationResult.GeneratedTestCode.Replace($@"""{featureRelativePath}""", $@"""{generatedFeatureRelativePath}""");

						File.WriteAllText(outputItem.ItemSpec, code, Encoding.UTF8);
						this.generatedFiles.Add(outputItem);
					}

					if (!generationResult.Success)
					{
						this.Log.LogMessage(MessageImportance.High, $"SpecFlow: {featureFile.ProjectRelativePath} -> test generation failed");

						foreach (var testGenerationError in generationResult.Errors)
						{
							this.RecordError(testGenerationError.Message, featureFile.GetFullPath(specFlowProject.ProjectSettings), testGenerationError.Line, testGenerationError.LinePosition);
						}
					}
					else if (generationResult.IsUpToDate)
					{
						this.Log.LogMessage(MessageImportance.Normal, $"SpecFlow: {featureFile.ProjectRelativePath} -> test up-to-date");
					}
					else
					{
						this.Log.LogMessage(MessageImportance.Normal, $"SpecFlow: {featureFile.ProjectRelativePath} -> test updated");
					}
				}

				sw.Stop();

				this.Log.LogMessage(MessageImportance.High, $"SpecFlow: Generated {this.generatedFiles.Count} feature files in {sw.ElapsedMilliseconds}ms.");
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

using System.Text;
using Cocona;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using NxJestMerge;

var builder = CoconaApp.CreateBuilder();
builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Trace);
var app = builder.Build();

const string searchPattern = "**/*.spec.ts";

app.AddCommand("merge", async (
	[Argument(Description = "Root path of your nx workspace")]
	string root,
	[Option("exclude", ['e'],
		Description =
			"Exclude patterns to use while searching for projects. Default: [node_modules, dist]")]
	string[]? exclude,
	ILogger<Program> logger) =>
{
	logger.LogDebug("Root path: {root}", root);

	var matcher = new Matcher(StringComparison.Ordinal);
	matcher.AddInclude(searchPattern);

	var excludePattern = exclude ?? ["node_modules", "dist"];
	var searcher = new ProjectSearcher(root, excludePattern, logger);
	var projects = searcher.FindProjects().ToArray();

	foreach (var project in projects)
	{
		logger.LogDebug("Project: {name}, Source root: {sourceRoot}", project.Name,
			project.SourceRoot);

		var targetFileName = Path.Combine(project.SourceRoot, "test-files-bundle.spec.ts");
		if (File.Exists(targetFileName))
			File.Delete(targetFileName);

		var files = matcher.GetResultsInFullPath(project.SourceRoot);

		var code = new StringBuilder();
		var imports = new List<Import>();

		foreach (var file in files)
		{
			var content = await File.ReadAllTextAsync(file);

			var data = ImportParser.SplitContent(content, file);

			code.AppendLine("{");
			code.AppendLine(data.Code);
			code.AppendLine("}");
			imports.AddRange(data.Imports);
		}

		await using var targetWriter = new StreamWriter(targetFileName);

		var mergedImports = ImportMerger.Merge(imports);

		foreach (var import in mergedImports)
		{
			await targetWriter.WriteLineAsync(import.ToString());
		}

		await targetWriter.WriteAsync(code);
	}
});

app.AddCommand("clear", ([Argument(Description = "Root path of your nx workspace")] string root,
	ILogger<Program> logger) =>
{
	var searcher = new ProjectSearcher(root, [], logger);
	var matcher = new Matcher(StringComparison.Ordinal);
	matcher.AddInclude(searchPattern);

	var projects = searcher.FindProjects().ToArray();


	foreach (var project in projects)
	{
		logger.LogDebug("Project: {name}, Source root: {sourceRoot}", project.Name,
			project.SourceRoot);

		var targetFileName = Path.Combine(project.SourceRoot, "test-files-bundle.spec.ts");
		if (File.Exists(targetFileName))
		{
			logger.LogDebug("Deleting {file}", targetFileName);
			File.Delete(targetFileName);
		}
	}
});

app.Run();
using System.Text.Json;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;

namespace NxJestMerge;

internal sealed class ProjectSearcher
{
	public ProjectSearcher(string root, string[] excludes, ILogger logger)
	{
		_excludes = excludes;
		_logger = logger;
		_root = Path.GetFullPath(root);
	}

	public IEnumerable<NxProject> FindProjects()
	{
		var matcher = new Matcher(StringComparison.Ordinal);
		matcher.AddInclude("**/project.json");
		matcher.AddExcludePatterns(_excludes);

		var projectFiles = matcher.GetResultsInFullPath(_root);

		foreach (var file in projectFiles)
		{
			NxProject? project = null;

			try
			{
				var content = File.ReadAllText(file);

				project = JsonSerializer.Deserialize<NxProject>(content) ??
				          throw new InvalidOperationException("Failed to deserialize project.json");

				project = project with { SourceRoot = Path.GetFullPath(project.SourceRoot, _root) };
			}
			catch (Exception ex)
			{
				_logger.LogError("Failed to read project file {file}: {message}", file, ex.Message);
			}

			if (project is not null)
				yield return project;
		}
	}

	private readonly string[] _excludes;
	private readonly ILogger _logger;
	private readonly string _root;
}
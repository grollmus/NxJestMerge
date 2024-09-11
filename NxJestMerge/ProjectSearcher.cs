using System.Text.Json;

namespace NxJestMerge;

internal sealed class ProjectSearcher
{
	public ProjectSearcher(string root)
	{
		_root = Path.GetFullPath(root);
	}

	public IEnumerable<NxProject> FindProjects()
	{
		var projectFiles = Directory.GetFiles(_root, "project.json", SearchOption.AllDirectories);

		foreach (var file in projectFiles)
		{
			var content = File.ReadAllText(file);
			var project = JsonSerializer.Deserialize<NxProject>(content) ??
			              throw new InvalidOperationException("Failed to deserialize project.json");

			project = project with { SourceRoot = Path.GetFullPath(project.SourceRoot, _root) };

			yield return project;
		}
	}

	private readonly string _root;
}
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace NxJestMerge;

public sealed class ProjectSearcherTests : IDisposable
{
	public ProjectSearcherTests()
	{
		_sut = new ProjectSearcher(_tempFolder, [], Substitute.For<ILogger>());
	}

	public void Dispose()
	{
		_tempFolder.Dispose();
	}

	private readonly TempFolder _tempFolder = new();
	private readonly ProjectSearcher _sut;

	private string CreateProjectFile(string name)
	{
		var path = Path.Combine(_tempFolder, name, "project.json");
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);

		var json = JsonSerializer.Serialize(new NxProject
		{
			Name = name,
			SourceRoot = name
		});
		File.WriteAllText(path, json);

		return path;
	}

	[Fact]
	public void DoesNotThrow_WhenDeserializationFails()
	{
		// Arrange
		var projectFile = CreateProjectFile("test");
		File.WriteAllText(projectFile, "no json");

		// Act
		var projects = _sut.FindProjects().ToList();

		// Assert
		projects.Should().BeEmpty();
	}

	[Fact]
	public void IsEmpty_WhenNoProjectsAreFound()
	{
		// Act
		var projects = _sut.FindProjects();

		// Assert
		projects.Should().BeEmpty();
	}

	[Fact]
	public void ParsesFoundProjects()
	{
		// Arrange
		CreateProjectFile("test");
		CreateProjectFile("test1");

		// Act
		var projects = _sut.FindProjects().ToList();

		// Assert
		projects.Should().HaveCount(2);
	}
}
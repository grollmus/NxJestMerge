namespace NxJestMerge;

public sealed class ImportMergerTests
{
	[Fact]
	public void Merges_MultipleModules()
	{
		// Arrange
		Import[] imports =
		[
			new("a", "module/sub", ImportType.Named),
			new("a", "module", ImportType.Named),
			new("b", "module", ImportType.Named)
		];

		// Act
		var merged = ImportMerger.Merge(imports);

		// Assert
		merged.Should().HaveCount(1)
			.And.ContainSingle(x => x.Type == "a, b" && x.Module == "module");
	}

	[Fact]
	public void Merges_WithStars()
	{
		// Arrange
		Import[] imports =
		[
			new("* as a", "b/sub", ImportType.Named),
			new("a, c", "b", ImportType.Default)
		];

		// Act
		var merged = ImportMerger.Merge(imports);

		// Assert
		merged.Should().HaveCount(1)
			.And.ContainSingle(x => x.Type == "a, c" && x.Module == "b");
	}
	
	[Fact]
	public void Merges_WithOneStarFromSameModule()
	{
		// Arrange
		Import[] imports =
		[
			new("* as a", "b", ImportType.Named),
			new("c", "b", ImportType.Default)
		];

		// Act
		var merged = ImportMerger.Merge(imports);

		// Assert
		merged.Should().HaveCount(2).And
			.ContainSingle(x => x.Type == "* as a" && x.Module == "b").
			And.ContainSingle(x => x.Type == "c" && x.Module == "b");
	}

	[Fact]
	public void Merges_WithPrefixedSubmodules()
	{
		// Arrange
		Import[] imports =
		[
			new("a", "@module/sub", ImportType.Named),
			new("b", "@module/other", ImportType.Named)
		];

		// Act
		var merged = ImportMerger.Merge(imports);

		// Assert
		merged.Should().HaveCount(2)
			.And.ContainSingle(x => x.Type == "a" && x.Module == "@module/sub")
			.And.ContainSingle(x => x.Type == "b" && x.Module == "@module/other");
	}

	[Fact]
	public void Merges_WithSubmodules()
	{
		// Arrange
		Import[] imports =
		[
			new("a", "module", ImportType.Named),
			new("b", "module/sub", ImportType.Named)
		];

		// Act
		var merged = ImportMerger.Merge(imports);

		// Assert
		merged.Should().HaveCount(1)
			.And.ContainSingle(x => x.Type == "a, b" && x.Module == "module");
	}

	[Fact]
	public void Merges_WithTwoImportsFromDifferentModule()
	{
		// Arrange
		Import[] imports =
		[
			new("a", "b", ImportType.Named),
			new("c", "d", ImportType.Named)
		];

		// Act
		var merged = ImportMerger.Merge(imports);

		// Assert
		merged.Should().HaveCount(2)
			.And.ContainSingle(x => x.Type == "a" && x.Module == "b")
			.And.ContainSingle(x => x.Type == "c" && x.Module == "d");
	}

	[Fact]
	public void Merges_WithTwoImportsFromDifferentSubmodule()
	{
		// Arrange
		Import[] imports =
		[
			new("a", "@b/one", ImportType.Named),
			new("c", "@b/two", ImportType.Named)
		];

		// Act
		var merged = ImportMerger.Merge(imports);

		// Assert
		merged.Should().HaveCount(2)
			.And.ContainSingle(x => x.Type == "a" && x.Module == "@b/one")
			.And.ContainSingle(x => x.Type == "c" && x.Module == "@b/two");
	}

	[Fact]
	public void Merges_WithTwoTypesFromSameModule()
	{
		// Arrange
		Import[] imports =
		[
			new("a", "b", ImportType.Named),
			new("c", "b", ImportType.Named)
		];

		// Act
		var merged = ImportMerger.Merge(imports);

		// Assert
		merged.Should().HaveCount(1).And.ContainSingle(x => x.Type == "a, c" && x.Module == "b");
	}
}
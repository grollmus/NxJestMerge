using FluentAssertions.Execution;

namespace NxJestMerge;

public sealed class ImportParserTests
{
	[Theory]
	[InlineData("import './module';", "/path/to/module")]
	[InlineData("import './../module';", "/path/module")]
	[InlineData("import './../other/module';", "/path/other/module")]
	public void ParsesImport_WithRelativePaths(string line, string expectedModule)
	{
		// Arrange
		const string filePath = "/path/to/file";

		// Act
		var imports = ImportParser.ParseImports(line, filePath);

		// Assert
		using var _ = new AssertionScope();
		var import = imports.SingleOrDefault();
		import.Should().NotBeNull();
		import!.Module.Should().Be(Path.GetFullPath(expectedModule));
	}

	[Theory]
	[InlineData("import a from \n'b';", "a", "b")]
	[InlineData("import \na from \n'b';", "a", "b")]
	[InlineData("import \na\n from \n'b';", "a", "b")]
	[InlineData("import \na\n from \n'b';\n", "a", "b")]
	public void ParsesImport_WithWhitespaces(string line, string expectedImport,
		string expectedModule)
	{
		// Arrange
		const string filePath = "/path/to/file";

		// Act
		var imports = ImportParser.ParseImports(line, filePath);

		// Assert
		using var _ = new AssertionScope();
		var import = imports.SingleOrDefault();
		import.Should().NotBeNull();
		import!.ImportType.Should().Be(ImportType.Default);
		import.Module.Should().Be(expectedModule);
		import.Type.Should().Be(expectedImport);
	}

	[Theory]
	[InlineData("import 'file.name';", "file.name")]
	public void ParsesImports_WithDefaultImport(string line, string expectedModule)
	{
		// Arrange
		const string filePath = "/path/to/file";

		// Act
		var imports = ImportParser.ParseImports(line, filePath);

		// Assert
		using var _ = new AssertionScope();
		var import = imports.SingleOrDefault();
		import.Should().NotBeNull();
		import!.ImportType.Should().Be(ImportType.Empty);
		import.Module.Should().Be(expectedModule);
	}

	[Theory]
	[InlineData("import { a, b } from 'c';", new[] { "a", "b" }, "c")]
	[InlineData("import { a, b } from '@c/d';", new[] { "a", "b" }, "@c/d")]
	public void ParsesImports_WithMultipleFrom(string line, string[] expectedImports,
		string expectedModule)
	{
		// Arrange
		const string filePath = "/path/to/file";

		// Act
		var imports = ImportParser.ParseImports(line, filePath);

		// Assert
		using var _ = new AssertionScope();
		imports.Should().HaveCount(expectedImports.Length);
		for (var i = 0; i < expectedImports.Length; i++)
		{
			var import = imports[i];
			import.ImportType.Should().Be(ImportType.Named);
			import.Module.Should().Be(expectedModule);
			import.Type.Should().Be(expectedImports[i]);
		}
	}

	[Theory]
	[InlineData("import { a } from 'b';", "a", "b")]
	[InlineData("import {a} from 'c';", "a", "c")]
	[InlineData("import {a} from 'c'", "a", "c")]
	[InlineData("import {a} from '@c/d'", "a", "@c/d")]
	public void ParsesImports_WithSingleFrom(string line, string expectedImport,
		string expectedModule)
	{
		// Arrange
		const string filePath = "/path/to/file";

		// Act
		var imports = ImportParser.ParseImports(line, filePath);

		// Assert
		using var _ = new AssertionScope();
		var import = imports.SingleOrDefault();
		import.Should().NotBeNull();
		import!.ImportType.Should().Be(ImportType.Named);
		import.Module.Should().Be(expectedModule);
		import.Type.Should().Be(expectedImport);
	}

	[Theory]
	[InlineData("import * as a from 'b';", "* as a", "b")]
	public void ParsesImports_WithStar(string line, string expectedImport, string expectedModule)
	{
		// Arrange
		const string filePath = "/path/to/file";

		// Act
		var imports = ImportParser.ParseImports(line, filePath);

		// Assert
		using var _ = new AssertionScope();
		var import = imports.SingleOrDefault();
		import.Should().NotBeNull();
		import!.ImportType.Should().Be(ImportType.Default);
		import.Module.Should().Be(expectedModule);
		import.Type.Should().Be(expectedImport);
	}

	[Theory]
	[InlineData("import a from 'b';", "a", "b")]
	[InlineData("import a from 'b'", "a", "b")]
	[InlineData("import a from 'c';", "a", "c")]
	[InlineData("import a from '@c/d';", "a", "@c/d")]
	public void ParsesImports_WithSingleFromAndDefault(string line, string expectedImport,
		string expectedModule)
	{
		// Arrange
		const string filePath = "/path/to/file";

		// Act
		var imports = ImportParser.ParseImports(line, filePath);

		// Assert
		using var _ = new AssertionScope();
		var import = imports.SingleOrDefault();
		import.Should().NotBeNull();
		import!.ImportType.Should().Be(ImportType.Default);
		import.Module.Should().Be(expectedModule);
		import.Type.Should().Be(expectedImport);
	}

	[Fact]
	public void SplitsContent_WithCode()
	{
		// Arrange
		const string content = "import a from 'b';\nimport { c } from 'd';\nconst e = 1;\n";
		const string filePath = "/path/to/file";

		// Act
		var fileContent = ImportParser.SplitContent(content, filePath);

		// Assert
		using var _ = new AssertionScope();
		fileContent.Imports.Should().HaveCount(2);
		fileContent.Code.Should().Be("const e = 1;" + Environment.NewLine);
	}

	[Fact]
	public void SplitsContent_WithOnlyImports()
	{
		// Arrange
		const string content = "import a from 'b';\nimport { c } from 'd';\n";
		const string filePath = "/path/to/file";

		// Act
		var fileContent = ImportParser.SplitContent(content, filePath);

		// Assert
		using var _ = new AssertionScope();
		fileContent.Imports.Should().HaveCount(2);
		fileContent.Code.Should().BeEmpty();
	}

	[Fact]
	public void SplitsContent_WithWhitespaces()
	{
		// Arrange
		const string content = "import a \nfrom 'b';\nimport { c } from 'd';\nconst e \n= 1;\n";
		const string filePath = "/path/to/file";

		// Act
		var fileContent = ImportParser.SplitContent(content, filePath);

		// Assert
		using var _ = new AssertionScope();
		fileContent.Imports.Should().HaveCount(2);
		fileContent.Code.Should()
			.Be("const e " + Environment.NewLine + "= 1;" + Environment.NewLine);
	}
}
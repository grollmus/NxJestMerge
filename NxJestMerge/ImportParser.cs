using System.Text;

namespace NxJestMerge;

internal class ImportParser
{
	public static Import[] ParseImports(string line, string filePath)
	{
		if (!line.Contains("from"))
		{
			line = Trim(line.Replace("import", string.Empty));
			line = ToAbsolutePath(line, filePath);
			return [new Import(string.Empty, line, ImportType.Empty)];
		}

		var parts = line.Split("from");
		var module = Trim(parts[1]);
		module = ToAbsolutePath(module, filePath);
		var import = parts[0].Replace("import", string.Empty).Trim();
		var type = ImportType.Default;

		if (import.Contains("{"))
		{
			type = ImportType.Named;
			import = import.Trim('{', '}');

			var types = import.Split(',', StringSplitOptions.RemoveEmptyEntries);
			if (types.Length > 1)
				return types.Select(t => new Import(Trim(t), module, type)).ToArray();
		}

		return [new Import(Trim(import), module, type)];
	}

	public static FileContent SplitContent(string content, string filePath)
	{
		var lines = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

		var imports = new List<Import>();
		var code = new StringBuilder();

		var lineBuffer = new StringBuilder();

		foreach (var line in lines)
		{
			if (line.StartsWith("import"))
			{
				lineBuffer.Append(line);

				if (line.Contains(';'))
				{
					imports.AddRange(ParseImports(lineBuffer.ToString(), filePath));
					lineBuffer.Clear();
				}
			}
			else if (lineBuffer.Length > 0)
			{
				lineBuffer.Append(line);
				if (line.Contains(';'))
				{
					imports.AddRange(ParseImports(lineBuffer.ToString(), filePath));
					lineBuffer.Clear();
				}
			}
			else
				code.AppendLine(line);
		}

		var (mocks, codeContent) = ExtractMocks(code.ToString());

		return new FileContent
		{
			Code = codeContent,
			Imports = imports.ToArray(),
			Mocks = mocks
		};
	}

	private static (string Mocks, string Code) ExtractMocks(string content)
	{
		if (!content.Contains("jest.mock", StringComparison.Ordinal))
			return ("", content);

		var mocks = new StringBuilder();
		var code = new StringBuilder();

		var startIndex = 0;
		int index;
		while ((index = content.IndexOf("jest.mock", startIndex, StringComparison.Ordinal)) != -1)
		{
			code.AppendLine(content.Substring(startIndex, index - startIndex));

			var endIndex = FindEndOfStatement(content, index);
			var mock = content.Substring(index, endIndex - index + 1);
			mocks.AppendLine(mock);

			startIndex = endIndex;
		}

		if (startIndex < content.Length)
			code.AppendLine(content[(startIndex+1)..]);

		return (mocks.ToString(), code.ToString());
	}

	private static int FindEndOfStatement(string content, int index)
	{
		var bracketCount = 0;

		for (; index < content.Length; ++index)
		{
			if (content[index] == '(')
				bracketCount++;
			else if (content[index] == ')')
				bracketCount--;

			if (content[index] == ';' && bracketCount == 0)
				return index;
		}

		return -1;
	}

	private static string ToAbsolutePath(string module, string filePath)
	{
		if (module.StartsWith("."))
		{
			var path = Path.GetDirectoryName(filePath);
			return Path.GetFullPath(Path.Combine(path!, module));
		}

		return module;
	}

	private static string Trim(string module) =>
		module.Trim().Trim('\'', '\"', ';', '{', '}').Trim();
}
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

		return new FileContent(code.ToString(), imports.ToArray());
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
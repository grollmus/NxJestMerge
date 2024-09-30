namespace NxJestMerge;

internal sealed record Import
{
	public Import(string type, string module, ImportType importType)
	{
		Type = type;
		Module = module;
		ImportType = importType;
	}

	public ImportType ImportType { get; init; }
	public string Module { get; init; }

	public string[] SplitTypes => Type.Split(',', StringSplitOptions.RemoveEmptyEntries)
		.Select(x => x.Trim())
		.ToArray();

	public string Type { get; init; }

	public override string ToString()
	{
		var modulePath = Module.Replace("\\", "/");

		switch (ImportType)
		{
			case ImportType.Empty:
				return $"import '{modulePath}';";

			case ImportType.Default:
				return $"import {Type} from '{modulePath}';";

			case ImportType.Named when Type.Contains('*'):
				return $"import {Type} from '{modulePath}';";

			case ImportType.Named:
				return $"import {{{Type}}} from '{modulePath}';";
		}

		throw new InvalidOperationException();
	}
}
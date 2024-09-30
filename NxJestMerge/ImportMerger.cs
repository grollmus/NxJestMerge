namespace NxJestMerge;

internal static class ImportMerger
{
	public static Import[] Merge(IEnumerable<Import> imports)
	{
		var groupedByModule = GroupByModule(imports);
		var mergedModules = MergeModules(groupedByModule);
		var merged = MergeTypes(mergedModules);
		return merged.ToArray();
	}

	private static List<KeyValuePair<string, List<Import>>> GroupByModule(
		IEnumerable<Import> imports)
	{
		var groupedByModule = imports.GroupBy(i => i.Module);
		return groupedByModule
			.OrderByDescending(x => x.Key.Length)
			.Select(g => new KeyValuePair<string, List<Import>>(g.Key, g.ToList()))
			.OrderBy(x => x.Key.Length)
			.ToList();
	}

	private static Dictionary<string, List<Import>> MergeModules(
		List<KeyValuePair<string, List<Import>>> groupedByModule)
	{
		var mergedModules = new Dictionary<string, List<Import>>();

		foreach (var group in groupedByModule)
		{
			var module = group.Key;
			var moduleImports = group.Value;

			var import = moduleImports.First();
			var type = moduleImports.First().ImportType;

			var types = moduleImports.Select(i => i.Type).Distinct().ToList();

			if (types.Count > 1)
				import = new Import(string.Join(", ", types), import.Module, type);

			var hasExistingImports = !module.StartsWith('@') &&
			                         mergedModules.Any(mm => module.StartsWith(mm.Key));
			if (hasExistingImports)
			{
				var existing = mergedModules.First(mm => module.StartsWith(mm.Key));
				existing.Value.Add(import);
			}
			else
				mergedModules.Add(module, [import]);
		}

		return mergedModules;
	}

	private static List<Import> MergeTypes(Dictionary<string, List<Import>> mergedModules)
	{
		var merged = new List<Import>();
		foreach (var (module, moduleImports) in mergedModules)
		{
			var allTypes = moduleImports
				.SelectMany(i => i.SplitTypes)
				.ToList();

			var starTypes = allTypes.Where(x => x.Contains('*')).ToList();
			var otherTypes = allTypes.Except(starTypes);

			var types = otherTypes
				.Select(x => x.Trim())
				.Distinct().ToList();

			var import = new Import(string.Join(", ", types), module, ImportType.Named);

			merged.Add(import);

			foreach (var starType in starTypes)
			{
				var alias = starType.Split("as").Last().Trim().Trim('*').Trim();
				if (types.Contains(alias))
					continue;

				var starImport = new Import(starType, import.Module, ImportType.Named);
				merged.Add(starImport);
			}
		}

		return merged;
	}
}

internal sealed class ImportComparer : IEqualityComparer<string>
{
	public bool Equals(string? x, string? y)
	{
		if (x is null || y is null)
			throw new ArgumentException();

		return x == y || x.StartsWith(y) || y.StartsWith(x);
	}

	public int GetHashCode(string obj) => obj.GetHashCode();
	public static readonly ImportComparer Instance = new();
}
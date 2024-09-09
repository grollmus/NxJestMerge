namespace NxJestMerge;

internal static class ImportMerger
{
	public static Import[] Merge(IEnumerable<Import> imports)
	{
		var groupedByModule = imports.GroupBy(i => i.Module).OrderByDescending(x => x.Key.Length);

		var merged = new List<Import>();
		var mergedModules = new List<string>();

		foreach (var group in groupedByModule)
		{
			var module = group.Key;

			if (!module.StartsWith('@') && mergedModules.Any(m => !m.EndsWith("specmocks") && m.StartsWith(module)))
				continue;

			var import = group.First();
			var type = group.First().ImportType;

			if (group.Count() > 1)
			{
				var names = group.Select(i => i.Type).Distinct();
				import = new Import(string.Join(", ", names), import.Module, type);
			}

			merged.Add(import);
			mergedModules.Add(module);
		}

		return merged.ToArray();
	}
}
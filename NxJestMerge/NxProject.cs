using System.Text.Json.Serialization;

namespace NxJestMerge;

internal sealed record NxProject
{
	[JsonPropertyName("name")] public required string Name { get; init; }

	[JsonPropertyName("sourceRoot")] public required string SourceRoot { get; init; }
}
namespace NxJestMerge;

internal sealed record FileContent
{
	public required string Code { get; init; }
	public Import[] Imports { get; init; }
	public string Mocks { get; init; }
}
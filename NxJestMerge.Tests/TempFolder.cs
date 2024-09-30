namespace NxJestMerge;

internal sealed class TempFolder : IDisposable
{
	public TempFolder()
	{
		_path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(_path);
	}

	public void Dispose()
	{
		Directory.Delete(_path, true);
	}

	public static implicit operator string(TempFolder folder) => folder._path;
	private readonly string _path;
}
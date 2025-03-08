namespace Clawssets.Builder;

public class FileData
{
	public bool NeedUpdate = true;
	public string OutputPath = string.Empty;
	public readonly string FullPath, SubDirectory;
	public readonly DateTime LastModified;

	public FileData(string path, string workingDirectory)
	{
		FullPath = path;
		SubDirectory = Directory.GetParent(path).FullName.Replace(workingDirectory, string.Empty);
		LastModified = File.GetLastWriteTimeUtc(path);

		if (SubDirectory.Length > 0 && SubDirectory[0] == '/') SubDirectory = SubDirectory.Substring(1);
	}
	public FileData(string path, string outputPath, string workingDirectory, DateTime lastModified)
	{
		FullPath = path;
		OutputPath = outputPath;
		SubDirectory = Directory.GetParent(path).FullName.Replace(workingDirectory, string.Empty);
		LastModified = lastModified;
	}

	public static bool operator ==(FileData a, FileData b) => a.FullPath == b.FullPath && a.LastModified == b.LastModified;
	public static bool operator !=(FileData a, FileData b) => a.FullPath != b.FullPath || a.LastModified != b.LastModified;
}

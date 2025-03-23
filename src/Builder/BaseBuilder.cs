namespace Clawssets.Builder;

public abstract class BaseBuilder
{
	protected readonly Dictionary<string, FileGroup> Groups = new();

	public bool TryToAdd(AssetBuilder builder, FileData file)
	{
		if (IsValid(file))
		{
			FileGroup files;

			if (!Groups.TryGetValue(file.SubDirectory, out files))
			{
				Groups.Add(file.SubDirectory, files = new(Path.Combine(builder.TargetDirectory, file.SubDirectory)));

				if (!Directory.Exists(files.OutputPath)) Directory.CreateDirectory(files.OutputPath);
			}

			files.NeedUpdate = file.NeedUpdate || files.NeedUpdate;
			file.OutputPath = Path.Combine(files.OutputPath, GetOutputPath(file));

			files.Add(file);

			return true;
		}

		return false;
	}

	protected abstract bool IsValid(FileData file);
	protected abstract string GetOutputPath(FileData file);
	public abstract void Build();

	protected class FileGroup : List<FileData>
	{
		public bool NeedUpdate = false;
		public readonly string OutputPath;

		public FileGroup(string outputPath) => OutputPath = outputPath;
	}
}

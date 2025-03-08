namespace Clawssets.Builder;

public abstract class BaseBuilder
{
	protected readonly Dictionary<string, FileGroup> Groups = new();

	public bool TryToAdd(FileData file)
	{
		if (IsValid(file))
		{
			FileGroup files;

			if (!Groups.TryGetValue(file.SubDirectory, out files)) Groups.Add(file.SubDirectory, files = new());

			files.NeedUpdate = file.NeedUpdate || files.NeedUpdate;

			files.Add(file);
		}

		return false;
	}

	protected abstract bool IsValid(FileData file);
	public abstract void Build(AssetBuilder builder);

	protected class FileGroup : List<FileData>
	{
		public bool NeedUpdate = false;
	}
}

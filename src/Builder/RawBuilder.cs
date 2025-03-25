namespace Clawssets.Builder;

public sealed class RawBuilder : BaseBuilder
{
	protected override bool IsValid(FileData file)
	{
		return Path.GetExtension(file.FullPath).ToLower() == AssetBuilder.AssetExtension;
	}
    protected override string GetOutputPath(FileData file) => Path.GetFileName(file.FullPath);

	public override void Build()
	{
		foreach (KeyValuePair<string, FileGroup> group in Groups)
		{
			if (!group.Value.NeedUpdate) continue;

			for (int i = 0; i < group.Value.Count; i++) File.Copy(group.Value[i].FullPath, group.Value[i].OutputPath, true);
		}
	}
}

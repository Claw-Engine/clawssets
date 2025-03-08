namespace Clawssets.Builder;

public class RawBuilder : BaseBuilder
{
	protected override bool IsValid(FileData file)
	{
		return Path.GetExtension(file.FullPath).ToLower() == AssetBuilder.AssetExtension;
	}

	public override void Build(AssetBuilder builder)
	{
		string outputPath;

		foreach (KeyValuePair<string, FileGroup> group in Groups)
		{
			if (!group.Value.NeedUpdate) continue;

			outputPath = Path.Combine(builder.TargetDirectory, group.Key);

			if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

			for (int i = 0; i < group.Value.Count; i++)
			{
				outputPath = Path.Combine(outputPath, Path.GetFileName(group.Value[i].FullPath));

				File.Copy(group.Value[i].FullPath, outputPath);

				group.Value[i].OutputPath = outputPath;
			}
		}
	}
}

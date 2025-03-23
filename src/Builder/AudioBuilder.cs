using Clawssets.Builder.Data;

namespace Clawssets.Builder;

public sealed class AudioBuilder : BaseBuilder
{
	protected override bool IsValid(FileData file)
	{
		switch (Path.GetExtension(file.FullPath).ToLower())
		{
			case ".wav": case ".wave":
			case ".aiff": case ".aif": case ".aifc":
			case ".mp3":
				return true;
			default: return false;
		}
	}
    protected override string GetOutputPath(FileData file) => Path.GetFileNameWithoutExtension(file.FullPath) + AssetBuilder.AssetExtension;

	public override void Build()
	{
		foreach (KeyValuePair<string, FileGroup> group in Groups)
		{
			if (!group.Value.NeedUpdate) continue;

			for (int i = 0; i < group.Value.Count; i++) Build(group.Value[i]);
		}
	}
	private void Build(FileData file)
	{
		Console.WriteLine("${0} > ${1}...", file.FullPath, file.OutputPath);

		StreamWriter stream = new(file.OutputPath);
		BinaryWriter writer = new(stream.BaseStream);
		AudioDescription track = new(file.FullPath);

		track.Resample();
		writer.Write("audio");
		writer.Write(track.Channels);
		writer.Write(track.Samples.LongLength);

		for (long i = 0; i < track.Samples.LongLength; i++) writer.Write(track.Samples[i]);

		stream.Close();
	}
}

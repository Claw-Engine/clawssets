using System.Xml;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;

namespace Clawssets.Builder;

public sealed class FontBuilder : BaseBuilder
{
	private Config config = new();

	protected override bool IsValid(FileData file)
	{
		switch (Path.GetExtension(file.FullPath).ToLower())
		{
			case ".xml":return true;
			default: return false;
		}
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
				group.Value[i].OutputPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(group.Value[i].FullPath) + AssetBuilder.AssetExtension);

				Build(group.Value[i]);
			}
		}
	}
	private void Build(FileData file)
	{
		ParseConfig(file);

		FontCollection collection = new();
		FontFamily family = collection.Add(config.path);
		Font font = family.CreateFont(config.size, config.style);
	}
	private void ParseConfig(FileData file)
	{
		XmlTextReader reader = new(file.FullPath);
		bool insideRegions = false;

		config.Clear();

		while (reader.Read())
		{
			switch (reader.NodeType)
			{
				case XmlNodeType.Element:
					switch (reader.Name)
					{
						case "Path":
							reader.Read();

							config.path = Path.GetFullPath(Path.Combine(file.FullPath, reader.Value));
							break;
						case "Size":
							reader.Read();
							
							config.size = int.Parse(reader.Value);
							break;
						case "Spacing":
							config.spacing.X = int.Parse(reader.GetAttribute("X"));
							config.spacing.Y = int.Parse(reader.GetAttribute("Y"));
							break;
						case "UseKerning":
							reader.Read();

							switch (reader.Value.ToLower())
							{
								case "true": case "1": config.useKerning = true; break;
								default: config.useKerning = false; break;
							}
							break;
						case "Style":
							reader.Read();
							
							config.style = Enum.Parse<FontStyle>(reader.Value, ignoreCase: true);
							break;
						case "CharacterRegions": insideRegions = true; break;
						case "Region":
							if (insideRegions) config.charRegions.Add(new Point(int.Parse(reader.GetAttribute("Start")), int.Parse(reader.GetAttribute("End"))));
							break;
					}
					break;
				case XmlNodeType.EndElement:
					if (reader.Name == "CharacterRegions") insideRegions = false;
				break;
			}
		}

		reader.Close();
	}

	private class Config
	{
		public int size;
		public bool useKerning;
		public string path;
		public FontStyle style;
		public Point spacing;
		public List<Point> charRegions = new();

		public void Clear()
		{
			charRegions.Clear();

			size = 32;
			useKerning = true;
			style = FontStyle.Regular;
			spacing = Point.Empty;
			path = string.Empty;
		}
	}
}

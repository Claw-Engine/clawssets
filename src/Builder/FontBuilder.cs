using System.Xml;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
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
		Console.WriteLine("${0} > ${1}...", file.FullPath, file.OutputPath);
		ParseConfig(file);

		FontCollection collection = new();
		FontFamily family = collection.Add(config.path);
		Font font = family.CreateFont(config.size, config.style);
		TextOptions options = new(font);

		List<Claw.Graphics.Glyph> glyphs = new();
		Image<Rgba32> atlas = new(TextureBuilder.AtlasSize, TextureBuilder.AtlasSize);
		PointF location = new(TextureBuilder.TextureGap, TextureBuilder.TextureGap);
		SizeF size = new();
		float addToY = 0;
		FontRectangle measure;
		string stringChar;

		StreamWriter stream = new(file.OutputPath);
		BinaryWriter writer = new(stream.BaseStream);

		writer.Write("font");

		for (int i = 0; i < config.charRegions.Count; i++)
		{
			for (int @char = config.charRegions[i].X; @char <= config.charRegions[i].Y; @char++)
			{
				stringChar = ((char)@char).ToString();
				measure = TextMeasurer.MeasureSize(stringChar, options);

				if (measure.Width == 0) continue;

				if (location.X + measure.Width + TextureBuilder.TextureGap > TextureBuilder.AtlasSize)
				{
					location.X = TextureBuilder.TextureGap;
					location.Y += addToY + TextureBuilder.TextureGap;
					addToY = 0;
				}

				if (location.Y + measure.Height + TextureBuilder.TextureGap > TextureBuilder.AtlasSize)
				{
					Console.WriteLine("ERRO: A fonte ultrapassou a barreira de {0}x{1}!", TextureBuilder.AtlasSize, TextureBuilder.AtlasSize);

					return;
				}

				atlas.Mutate((x) => x.DrawText(stringChar, font, Color.White, location));

				glyphs.Add(new(new Claw.Rectangle(location.X, location.Y, measure.Width, measure.Height), config.useKerning ? new Dictionary<char, float>() : null));

				addToY = Math.Max(measure.Height, addToY);
				size.Width = Math.Max(location.X + measure.Width + TextureBuilder.TextureGap, size.Width);
				size.Height = Math.Max(location.Y + addToY + TextureBuilder.TextureGap, size.Height);
				location.X += measure.Width + TextureBuilder.TextureGap;
			}
		}

		if (config.useKerning) GetKerningPairs(glyphs);

		TextureBuilder.WriteImage(writer, atlas, new Size((int)size.Width, (int)size.Height));
		writer.Write(glyphs.Count);
		
		for (int i = 0; i < glyphs.Count; i++)
		{
			writer.Write(glyphs[i].Area.X);
			writer.Write(glyphs[i].Area.Y);
			writer.Write(glyphs[i].Area.Width);
			writer.Write(glyphs[i].Area.Height);

			if (config.useKerning)
			{
			}
			else writer.Write(0);
		}

		stream.Close();
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
	private void GetKerningPairs(List<Claw.Graphics.Glyph> glyphs)
	{
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

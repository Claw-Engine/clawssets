using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Clawssets.Builder;

public sealed class TextureBuilder : BaseBuilder
{
	public const int AtlasSize = 2000, TextureGap = 1;
	private const string AtlasName = "atlas" + AssetBuilder.AssetExtension;

	protected override bool IsValid(FileData file)
	{
		switch (Path.GetExtension(file.FullPath).ToLower())
		{
			case ".bmp": case ".jpeg": case ".jpg": case ".png": return true;
			default: return false;
		}
	}
    protected override string GetOutputPath(FileData file)
	{
		if (file.SubDirectory.Length == 0) return Path.GetFileNameWithoutExtension(file.FullPath) + AssetBuilder.AssetExtension;

		return AtlasName;
	}

	public override void Build()
	{
		List<Texture> textures = new();

		foreach (KeyValuePair<string, FileGroup> group in Groups)
		{
			if (!group.Value.NeedUpdate) continue;

			if (group.Key.Length == 0)// Único
			{
				for (int i = 0; i < group.Value.Count; i++) BuildSingle(group.Value[i]);
			}
			else// Atlas
			{
				for (int i = 0; i < group.Value.Count; i++) textures.Add(new Texture(Path.GetFileNameWithoutExtension(group.Value[i].FullPath), Image.Load(group.Value[i].FullPath)));

				textures.OrderByDescending((texture) => texture.image.Height);
				BuildMultiple(textures, group.Value[0].OutputPath);
				textures.Clear();
			}
		}
	}
	private void BuildSingle(FileData file)
	{
		Console.WriteLine("${0} > ${1}...", file.FullPath, file.OutputPath);

		StreamWriter stream = new(file.OutputPath);
		BinaryWriter writer = new(stream.BaseStream);
		Image<Rgba32> image = Image.Load<Rgba32>(file.FullPath);

		writer.Write("texture");
		WriteImage(writer, image, image.Size);
		stream.Close();
	}
	private void BuildMultiple(List<Texture> textures, string output)
	{
		Console.WriteLine("Compilando ${0}...", output);

		Image<Rgba32> atlas = new(AtlasSize, AtlasSize);
		Point location = new(TextureGap);
		Size size = new();
		int addToY = 0;
		StreamWriter stream = new(output);
		BinaryWriter writer = new(stream.BaseStream);

		writer.Write("atlas");

		for (int i = 0; i < textures.Count; i++)
		{
			if (location.X + textures[i].image.Width + TextureGap > AtlasSize)
			{
				location.X = TextureGap;
				location.Y += addToY + TextureGap;
				addToY = 0;
			}

			if (location.Y + textures[i].image.Height + TextureGap > AtlasSize)
			{
				Console.WriteLine("ERRO: O Texture Atlas ultrapassou a barreira de {0}x{1}!", AtlasSize, AtlasSize);

				return;
			}

			atlas.Mutate((x) => x.DrawImage(textures[i].image, location, 1));

			textures[i].location = location;
			addToY = Math.Max(textures[i].image.Height, addToY);
			size.Width = Math.Max(location.X + textures[i].image.Width + TextureGap, size.Width);
			size.Height = Math.Max(location.Y + addToY + TextureGap, size.Height);
			location.X += textures[i].image.Width + TextureGap;
		}

		WriteImage(writer, atlas, size);
		writer.Write(textures.Count);

		for (int i = 0; i < textures.Count; i++)
		{
			writer.Write(textures[i].name);
			writer.Write(textures[i].location.X);
			writer.Write(textures[i].location.Y);
			writer.Write(textures[i].image.Width);
			writer.Write(textures[i].image.Height);
		}

		stream.Close();
	}
	public static unsafe void WriteImage(BinaryWriter writer, Image<Rgba32> image, Size size)
	{
		writer.Write(size.Width);
		writer.Write(size.Height);
		image.ProcessPixelRows((pixelAccessor) =>
		{
			for (int y = 0; y < size.Height; y++)
			{
				Span<Rgba32> row = pixelAccessor.GetRowSpan(y);

				for (int x = 0; x < size.Width; x++) writer.Write(row[x].PackedValue);
			}
		});
	}

	private class Texture
	{
		public string name;
		public Image image;
		public Point location;

		public Texture(string name, Image image)
		{
			this.name = name;
			this.image = image;
		}
	}
}

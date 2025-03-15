namespace Clawssets.Builder;

public class AssetBuilder
{
	public const string AssetExtension = ".ca", ResultDirectory = ".build", CacheName = ".cache";
	public string WorkingDirectory { get; private set; }
	public string TargetDirectory { get; private set; }
	public string OutputsPath { get; private set; }
	private List<FileData> oldFileData, newFileData;
	private string cachePath;
	private readonly BaseBuilder[] builders;

	public AssetBuilder(string workingDirectory)
	{
		WorkingDirectory = workingDirectory;
		TargetDirectory = Path.Combine(WorkingDirectory, ResultDirectory);
		cachePath = Path.Combine(TargetDirectory, CacheName);
		OutputsPath = Path.Combine(WorkingDirectory, "outputs");
		builders = new BaseBuilder[]
		{
			new RawBuilder(),
			new AudioBuilder(),
			new TextureBuilder(),
			new FontBuilder()
		};
	}

	public void Setup()
	{
		newFileData = new();

		LoopFiles(WorkingDirectory);
		ReadCache();

		for (int i = oldFileData.Count - 1; i >= 0; i--)
		{
			for (int j = 0; j < newFileData.Count; j++)
			{
				if (oldFileData[i].FullPath == newFileData[j].FullPath)
				{
					if (oldFileData[i].LastModified == newFileData[j].LastModified) newFileData[j].NeedUpdate = false;
					
					oldFileData.RemoveAt(i);

					break;
				}
			}
		}

		for (int i = 0; i < newFileData.Count; i++)
		{
			for (int j = 0; j < builders.Length; j++)
			{
				if (builders[j].TryToAdd(newFileData[i])) break;
			}
		}
	}
	private void LoopFiles(string path)
	{
		Console.WriteLine("Listando arquivos de \"{0}\"...", path);

		string[] files = Directory.GetFiles(path), directories = Directory.GetDirectories(path);

		for (int i = 0; i < files.Length; i++)
		{
			if (files[i] != OutputsPath) newFileData.Add(new(files[i], WorkingDirectory));
		}

		for (int i = 0; i < directories.Length; i++)
		{
			if (directories[i] != TargetDirectory) LoopFiles(directories[i]);
		}
	}
	private void ReadCache()
	{
		oldFileData = new();

		if (!File.Exists(cachePath)) return;

		Console.WriteLine("Lendo cache...");

		StreamReader stream = new(cachePath);
		BinaryReader reader = new(stream.BaseStream);
		int count = reader.ReadInt32();

		for (int i = 0; i < count; i++) oldFileData.Add(new(reader.ReadString(), reader.ReadString(), WorkingDirectory, new DateTime(reader.ReadInt64())));

		stream.Close();
	}
	private void WriteCache()
	{
		Console.WriteLine("Salvando cache...");

		StreamWriter stream = new(cachePath);
		BinaryWriter writer = new(stream.BaseStream);

		writer.Write(newFileData.Count);

		for (int i = 0; i < newFileData.Count; i++)
		{
			writer.Write(newFileData[i].FullPath);
			writer.Write(newFileData[i].OutputPath);
			writer.Write(newFileData[i].LastModified.Ticks);
		}

		stream.Close();
	}

	public void Build()
	{
		if (!Directory.Exists(TargetDirectory)) Directory.CreateDirectory(TargetDirectory);

		for (int i = 0; i < builders.Length; i++) builders[i].Build(this);
	}

	public void Finish()
	{
		if (oldFileData.Count > 0)
		{
			Console.WriteLine("Apagando arquivos antigos...");

			bool canDelete;

			for (int i = 0; i < oldFileData.Count; i++)
			{
				if (oldFileData[i].OutputPath.Length > 0)
				{
					canDelete = true;

					for (int j = 0; j < newFileData.Count; j++)
					{
						if (newFileData[j].OutputPath == oldFileData[i].OutputPath)
						{
							canDelete = false;

							break;
						}
					}

					if (canDelete) File.Delete(oldFileData[i].OutputPath);
				}
			}
		}

		WriteCache();
	}

	public void CopyTo(List<string> directories)
	{
		if (directories.Count == 0) return;

		for (int i = 0; i < directories.Count; i++)
		{
			if (Directory.Exists(directories[i])) Directory.Delete(directories[i], true);

			Directory.CreateDirectory(directories[i]);
			Console.WriteLine("Copiando arquivos para \"{0}\"...", directories[i]);
			CopyTo(TargetDirectory, directories[i]);
		}
	}
	private void CopyTo(string path, string target)
	{
		string[] files = Directory.GetFiles(path), directories = Directory.GetDirectories(path);
		string helper;

		for (int i = 0; i < files.Length; i++)
		{
			helper = Path.GetFileName(files[i]);

			if (helper == CacheName) continue;

			helper = Path.Combine(target, helper);

			File.Copy(files[i], helper);
		}

		for (int i = 0; i < directories.Length; i++)
		{
			helper = Path.Combine(target, Path.GetFileName(directories[i]));

			Directory.CreateDirectory(helper);
			CopyTo(directories[i], helper);
		}
	}
}

using Clawssets.Builder;

namespace Clawssets;

class Program
{
	static void Main(string[] args)
	{
		string workingDir = Directory.GetCurrentDirectory();

#if DEBUG
		workingDir = Path.Combine(Directory.GetParent(workingDir).Parent.FullName, "test", "Assets");
#endif

		List<string> outputDirs = new();
		bool readingOutput = false;

		for (int i = 0; i < args.Length; i++)
		{
			if (!readingOutput)
			{
				if (args[i] == "-o" || args[i] == "--output") readingOutput = true;
				else if (Directory.Exists(args[i])) workingDir = args[i];
				else Console.WriteLine("ERRO: \"{0}\" não é um diretório válido!", args[i]);
			}
			else if (Directory.Exists(args[i])) outputDirs.Add(args[i]);
			else Console.WriteLine("ERRO: \"{0}\" não é um diretório válido!", args[i]);
		}

		AssetBuilder builder = new(workingDir);

		if (File.Exists(builder.OutputsPath))
		{
			string[] paths = File.ReadAllLines(builder.OutputsPath);

			for (int i = 0; i < paths.Length; i++) outputDirs.Add(Path.GetFullPath(Path.Combine(builder.WorkingDirectory, paths[i])));
		}
		
		if (outputDirs.Count == 0) Console.WriteLine("Nenhum destino selecionado. Os resultados estarão presentes em \"{0}\"...", Path.Combine(workingDir, AssetBuilder.ResultDirectory));

		builder.Setup();
		builder.Build();
		builder.Finish();
		builder.CopyTo(outputDirs);
	}
}

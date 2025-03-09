using Claw.Audio;
using NAudio.Wave;

namespace Clawssets.Builder.Data;

public sealed class AudioDescription
{
	public int SampleRate;
	public byte Channels;
	public float[] Samples;

	public AudioDescription(string path)
	{
		AudioFileReader reader = new(path);

		SampleRate = reader.WaveFormat.SampleRate;
		Channels = (byte)reader.WaveFormat.Channels;
		int bytesPerSample = reader.WaveFormat.BitsPerSample / 8 * Channels;
		Samples = new float[reader.Length / (bytesPerSample / Channels)];

		reader.Read(Samples, 0, Samples.Length);
	}

	public void Resample()
	{
		if (AudioManager.SampleRate > SampleRate)
		{
			float factor = (float)AudioManager.SampleRate / SampleRate;
			float[] resampled = new float[(long)(Samples.LongLength * factor)];
			
			for (long i = 0; i < Samples.LongLength; i += Channels)
			{
				long index = (long)(i * factor);

				if (index != resampled.LongLength - 1)
				{
					for (int j = 1; j < factor; j++)
					{
						resampled[index + j] = Samples[i];

						if (Channels == 2) resampled[index + j + 1] = Samples[i + 1];
					}
				}

				resampled[index] = Samples[i];

				if (Channels == 2) resampled[index + 1] = Samples[i + 1];
			}

			SampleRate = AudioManager.SampleRate;
			Samples = resampled;
		}
	}
}

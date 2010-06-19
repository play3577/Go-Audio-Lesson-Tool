﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using csvorbis;
using System.IO;

namespace EasyOggVorbis
{
	public class OggVorbisDecoder
	{
		private readonly VorbisFile VorbisFile;
		private readonly Stream Stream;

		public int SamplesPerSecond { get; private set; }
		public int Channels { get; private set; }

		public TimeSpan SampleToTime(long sample)
		{
			return TimeSpan.FromSeconds(((double)SampleDuration) / SamplesPerSecond);
		}

		public long TimeToSample(TimeSpan time)
		{
			return (long)Math.Round(time.TotalSeconds * SamplesPerSecond);
		}

		public long SamplePosition { get { VorbisFile.pcm_tell(); } }
		public long SampleDuration { get { return VorbisFile.pcm_total(-1); } }

		public TimeSpan Duration { get { return SampleToTime(SampleDuration); } }
		public TimeSpan Position { get { return SampleToTime(SamplePosition); } }

		public OggVorbisDecoder(Stream stream)
		{
			Stream = stream;
			if (!stream.CanSeek)
				throw new ArgumentException("Stream is not seekable");
			VorbisFile = new VorbisFile(Stream, null, 0);
			if (VorbisFile.streams == 0)
				throw new InvalidDataException("File contains no logical bitstreams");
			Info firstInfo = VorbisFile.getInfo(0);
			Channels = firstInfo.channels;
			SamplesPerSecond = firstInfo.rate;
			for (int i = 0; i < VorbisFile.streams; i++)
			{
				Info info = VorbisFile.getInfo(i);
				if (info.channels != firstInfo.channels)
					throw new NotSupportedException("Bitstreams have different number of channels");
				if (info.rate != firstInfo.rate)
					throw new NotSupportedException("Bitstreams have different sample-rate");
			}
		}

		public long Seek(long sample)
		{
			VorbisFile.pcm_seek(sample);
			return SamplePosition;
		}

		public TimeSpan Seek(TimeSpan time)
		{
			VorbisFile.pcm_seek(TimeToSample(time));
			return Position;
		}

		public int Read(float[,] data, int offset, int count)
		{
			return VorbisFile.read(data, offset, count);
		}
	}
}

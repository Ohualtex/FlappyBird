using System.Runtime.InteropServices;

namespace FlappyBird;

internal sealed class WaveEffect(string name) : IDisposable
{
    private const uint WaveMapper = uint.MaxValue;
    private const uint BufferInQueue = 0x10;
    private static readonly uint HeaderSize = (uint)Marshal.SizeOf<WaveHeader>();

    private nint device;
    private nint samples;
    private nint header;
    private bool openAttempted;

    public bool IsPlaying => device != 0
        && (Marshal.PtrToStructure<WaveHeader>(header).Flags & BufferInQueue) != 0;

    public void Play()
    {
        if (!openAttempted)
        {
            openAttempted = true;
            Open();
        }

        if (device == 0)
        {
            return;
        }

        if (!IsPlaying)
        {
            waveOutWrite(device, header, HeaderSize);
        }
    }

    public void Stop()
    {
        if (device != 0)
        {
            waveOutReset(device);
        }
    }

    private void Open()
    {
        using Stream? stream = typeof(WaveEffect).Assembly.GetManifestResourceStream($"FlappyBird.Assets.{name}.wav");
        if (stream is null)
        {
            return;
        }

        using var reader = new BinaryReader(stream);
        if (!TryReadWave(reader, out WaveFormat format, out byte[] pcm))
        {
            return;
        }

        if (waveOutOpen(out nint output, WaveMapper, ref format, 0, 0, 0) != 0)
        {
            return;
        }

        samples = Marshal.AllocHGlobal(pcm.Length);
        Marshal.Copy(pcm, 0, samples, pcm.Length);
        header = Marshal.AllocHGlobal((int)HeaderSize);
        Marshal.StructureToPtr(new WaveHeader { Data = samples, Length = (uint)pcm.Length }, header, false);
        if (waveOutPrepareHeader(output, header, HeaderSize) != 0)
        {
            waveOutClose(output);
            FreeBuffer();
            return;
        }

        device = output;
    }

    private static bool TryReadWave(BinaryReader reader, out WaveFormat format, out byte[] pcm)
    {
        const uint riff = 0x46464952;
        const uint wave = 0x45564157;
        const uint fmt = 0x20746D66;
        const uint data = 0x61746164;
        format = default;
        pcm = [];
        Stream stream = reader.BaseStream;
        if (stream.Length < 12 || reader.ReadUInt32() != riff)
        {
            return false;
        }

        reader.ReadUInt32();
        if (reader.ReadUInt32() != wave)
        {
            return false;
        }

        while (stream.Position + 8 <= stream.Length)
        {
            uint chunk = reader.ReadUInt32();
            uint length = reader.ReadUInt32();
            long end = stream.Position + length;
            if (end > stream.Length)
            {
                return false;
            }

            if (chunk == fmt && length >= 16)
            {
                format.Tag = reader.ReadUInt16();
                format.Channels = reader.ReadUInt16();
                format.SampleRate = reader.ReadUInt32();
                format.BytesPerSecond = reader.ReadUInt32();
                format.BlockAlign = reader.ReadUInt16();
                format.BitsPerSample = reader.ReadUInt16();
            }
            else if (chunk == data)
            {
                pcm = reader.ReadBytes((int)length);
            }

            stream.Position = end + (length & 1);
        }

        return format.Tag == 1 && format.BitsPerSample == 16
            && format.BlockAlign > 0 && pcm.Length > 0 && pcm.Length % format.BlockAlign == 0;
    }

    public void Dispose()
    {
        if (device == 0)
        {
            return;
        }

        Stop();
        // A driver can still own the buffer if reset or unprepare fails.
        if (waveOutUnprepareHeader(device, header, HeaderSize) != 0)
        {
            return;
        }

        waveOutClose(device);
        device = 0;
        FreeBuffer();
    }

    private void FreeBuffer()
    {
        Marshal.FreeHGlobal(header);
        Marshal.FreeHGlobal(samples);
        header = 0;
        samples = 0;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    private struct WaveFormat
    {
        public ushort Tag;
        public ushort Channels;
        public uint SampleRate;
        public uint BytesPerSecond;
        public ushort BlockAlign;
        public ushort BitsPerSample;
        public ushort ExtraSize;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WaveHeader
    {
        public nint Data;
        public uint Length;
        public uint Recorded;
        public nuint User;
        public uint Flags;
        public uint Loops;
        public nint Next;
        public nuint Reserved;
    }

    [DllImport("winmm.dll", ExactSpelling = true)]
    private static extern uint waveOutOpen(out nint device, uint id, ref WaveFormat format,
        nint callback, nint instance, uint flags);

    [DllImport("winmm.dll", ExactSpelling = true)]
    private static extern uint waveOutPrepareHeader(nint device, nint header, uint size);

    [DllImport("winmm.dll", ExactSpelling = true)]
    private static extern uint waveOutWrite(nint device, nint header, uint size);

    [DllImport("winmm.dll", ExactSpelling = true)]
    private static extern uint waveOutReset(nint device);

    [DllImport("winmm.dll", ExactSpelling = true)]
    private static extern uint waveOutUnprepareHeader(nint device, nint header, uint size);

    [DllImport("winmm.dll", ExactSpelling = true)]
    private static extern uint waveOutClose(nint device);
}

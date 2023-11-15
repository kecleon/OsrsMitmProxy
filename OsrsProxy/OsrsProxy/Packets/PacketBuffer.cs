namespace OsrsProxy.Packets;

public class PacketBuffer : IDisposable
{
	public int Size;
	public int Index;
	public byte[] Bytes;
	private byte[] header;

	public PacketBuffer()
	{
		Size = 4;
		Index = 0;
		header = new byte[Size];
		Bytes = header;
	}

	public void Fit() {
		Size = BitConverter.ToInt32(header, 0);

		if (Size > 2097152) {
			throw new ArgumentException($"New buffer size is too large ({Math.Round(Size / 1024d / 1024d, 2)}MB)\n{Size} Bytes: {Bytes.ToHexString(Bytes.Length)}");
		}

		if (Bytes != header) {
			throw new InvalidOperationException("Buffer already fitted!");
		}

		Bytes = new byte[Size];
		Array.Copy(header, Bytes, header.Length);
	}

	public void Advance(int read)
	{
		Index += read;
	}

	public int BytesRemaining()
	{
		return Size - Index;
	}

	public void Dispose() {
		header = null;
		Bytes = null;
	}
}
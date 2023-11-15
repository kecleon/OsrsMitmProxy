namespace OsrsProxy;

public static class Extensions
{
	public static string ToHexString(this byte[] barray, int length, int start = 0) {
		var c = new char[length * 2];
		for (int i = start; i < length; ++i) {
			var b = ((byte)(barray[i] >> 4));
			c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);
			b = ((byte)(barray[i] & 0xF));
			c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
		}

		return new string(c);
	}

	public static byte[] ToByteArray(this string hex)
	{
		int numberChars = hex.Length;
		byte[] bytes = new byte[numberChars / 2];
		for (int i = 0; i < numberChars; i += 2)
		{
			bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
		}
		return bytes;
	}

	public static string Before(this string full, string partial) {
		var index = full.IndexOf(partial);
		if (index == -1) {
			return full;
		}

		return full.Substring(0, index);
	}

	public static string After(this string full, string partial) {
		var index = full.IndexOf(partial);
		if (index == -1) {
			return full;
		}

		return full.Substring(index + partial.Length);
	}
}
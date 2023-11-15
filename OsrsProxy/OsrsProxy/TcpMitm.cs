using System.Net;
using System.Net.Sockets;
using OsrsProxy.Packets;

namespace OsrsProxy;

public class TcpMitm : Proxy
{
	private int LocalPort;
	private string RemoteHost;
	private int RemotePort;

	public TcpMitm(int localPort, string remoteHost, int remotePort)
	{
		LocalPort = localPort;
		RemoteHost = remoteHost;
		RemotePort = remotePort;
	}

	public override void Listen()
	{
		TcpListener listener = new TcpListener(IPAddress.Any, LocalPort);
		listener.Start();
		Console.WriteLine($"Listening on port {LocalPort}...");

		while (true)
		{
			TcpClient localClient = listener.AcceptTcpClient();
			Console.WriteLine("Connection received. Forwarding to remote...");
			Thread t = new Thread(() => HandleClient(localClient));
			t.Start();
		}
	}

	public override void Send(byte[] bytes)
	{
		throw new NotImplementedException();
	}

	private void HandleClient(TcpClient localClient)
	{
		using (TcpClient remoteClient = new TcpClient(RemoteHost, RemotePort))
		using (localClient)
		{
			NetworkStream localStream = localClient.GetStream();
			NetworkStream remoteStream = remoteClient.GetStream();

			Thread receiveThread = new Thread(() => ForwardData(localStream, remoteStream, false));
			Thread sendThread = new Thread(() => ForwardData(remoteStream, localStream, true));

			receiveThread.Start();
			sendThread.Start();

			receiveThread.Join();
			sendThread.Join();
		}
	}

	public object sync = new();

	private void ForwardData(NetworkStream fromStream, NetworkStream toStream, bool client)
	{
		int bytesRead = 0;

		var prefix = client ? "Recv <<  " : "Sent   >>";
		try
		{
			using var buffer = new PacketBuffer();
			if (!ReadInternal(buffer, fromStream.Socket))
			{
				Console.WriteLine($"Failed reading packet from stream");
				return;
			}
            
			buffer.Fit();
			if (!ReadInternal(buffer, fromStream.Socket))
			{
				Console.WriteLine($"Failed reading packet from stream");
				return;
			}

			var packet = Packet.Create(buffer.Bytes, buffer.Size);
			Console.WriteLine($"{prefix} length {buffer.Size} :: {buffer.Bytes.ToHexString(buffer.Bytes.Length)}");
			
			toStream.Write(buffer.Bytes, 0, buffer.Size);
			toStream.Flush();
			
			//while ((bytesRead = fromStream.Read(buffer, 0, buffer.Length)) > 0)
			//{
			//	var length = (long) BitConverter.ToInt32(buffer, 0);
			//	var data = new byte[length];
			//	Console.WriteLine($"{prefix} length {length} :: {buffer.ToHexString(buffer.Length)}");
			//	Array.Copy(buffer, 4, data, 0, length);
			//	toStream.Write(buffer, 0, bytesRead);
			//	toStream.Flush();
			//}
			
			
			////read int for length
			//bytesRead += fromStream.Read(buffer, 0, 4);
			//var length = BitConverter.ToInt32(buffer, 0);
			//bytesRead += fromStream.Read(buffer, 4, length);

			//Console.WriteLine($"{prefix} length {length} -- {buffer.ToHexString(bytesRead, 4)}");

			//toStream.Write(buffer, 0, bytesRead);
			//toStream.Flush();

			/*while ((bytesRead = fromStream.Read(buffer, 0, buffer.Length)) > 0)
			{
				Console.WriteLine($"{prefix}{buffer.ToHexString(bytesRead)}");
				toStream.Write(buffer, 0, bytesRead);
				toStream.Flush();
			}*/
		}
		catch (SocketException e)
		{
			Console.WriteLine($"Socket exception: {e.Message}");
		}
		catch (Exception e)
		{
			Console.WriteLine($"Error: {e.Message}");
		}
	}
	
	private bool ReadInternal(PacketBuffer buffer, Socket socket)
	{
		while (buffer.BytesRemaining() > 0)
		{
			var read = 0;

			try
			{
				read = socket.Receive(new ArraySegment<byte>(buffer.Bytes, buffer.Index, buffer.BytesRemaining()), SocketFlags.None);
			}
			catch (Exception e)
			{
				Console.WriteLine("Exception reading data from socket: " + e.Message);
				return false;
			}

			if (read == 0)
			{
				return false;
			}

			buffer.Advance(read);
		}

		return true;
	}
}
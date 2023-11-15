using System.Net;
using System.Net.Sockets;

namespace OsrsProxy;

public class UdpMitm : Proxy
{
	private int LocalPort;
	private string RemoteHost;
	private int RemotePort;

	public UdpMitm(int localPort, string remoteHost, int remotePort)
	{
		LocalPort = localPort;
		RemoteHost = remoteHost;
		RemotePort = remotePort;
	}

	public override void Listen()
	{
		UdpClient localClient = new UdpClient(LocalPort);
		Console.WriteLine($"Listening for UDP packets on port {LocalPort}...");

		while (true)
		{
			IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, LocalPort);
			byte[] receivedBytes = localClient.Receive(ref localEndPoint);
			Console.WriteLine($"Received {receivedBytes.Length} bytes. Forwarding to {RemoteHost}:{RemotePort}...");

			Thread t = new Thread(() => ForwardPacket(receivedBytes));
			t.Start();
		}
	}

	public override void Send(byte[] bytes)
	{
		throw new NotImplementedException();
	}

	private void ForwardPacket(byte[] data)
	{
		UdpClient remoteClient = new UdpClient();
		remoteClient.Connect(RemoteHost, RemotePort);
		remoteClient.Send(data, data.Length);
		remoteClient.Close();
	}
}
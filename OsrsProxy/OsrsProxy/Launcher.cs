using System.Reflection;

namespace OsrsProxy;

public class Launcher
{
	enum Protocol
	{
		Tcp,
		Udp,
		Service,
		None
	}

	private static readonly string Project = Assembly.GetCallingAssembly().GetName().Name;
	private static readonly string HelpMessage = $"Usage: {Project} -l:<localport> -r:<remotehost>:<remoteport> -tcp|-udp";

	public static async Task Main(string[] args)
	{
		if (args.Length == 0)
		{
			Console.WriteLine(HelpMessage);
			Console.ReadKey();
			return;
		}
        
        var localport = -1;
		var remotehost = String.Empty;
		var remoteport = -1;
		var protocol = Protocol.None;

		foreach (var arg in args)
		{
			if (arg.StartsWith("-l:"))
			{
				localport = Int32.Parse(arg.Substring(3));
			}
			else if (arg.StartsWith("-r:"))
			{
				var parts = arg.Substring(3).Split(':');
				if (parts.Length != 2)
				{
					Console.WriteLine("Provide remote host as -r:<host>:<port>");
					return;
				}
				remotehost = parts[0];
				remoteport = Int32.Parse(parts[1]);
			}
			else if (arg == "-tcp")
			{
				protocol = Protocol.Tcp;
			}
			else if (arg == "-udp")
			{
				protocol = Protocol.Udp;
			}
			else if (arg == "-service")
			{
				protocol = Protocol.Service;
			}
		}

		if (localport == -1 || remotehost == string.Empty || remoteport == -1 || protocol == Protocol.None)
		{
			Console.WriteLine(HelpMessage);
			Console.ReadKey();
			return;
		}

		Proxy proxy;
		if (protocol == Protocol.Tcp)
		{
			proxy = new TcpMitm(localport, remotehost, remoteport);
		}
		else if (protocol == Protocol.Udp)
		{
			proxy = new UdpMitm(localport, remotehost, remoteport);
		}
		else
		{
			throw new Exception("Invalid protocol");
		}
		
		proxy.Listen();
		// Start listening in a separate thread
		//Thread listenThread = new Thread(proxy.Listen);
		//listenThread.Start();

		Console.WriteLine("Enter hex strings to send or 'exit' to quit:");

		while (true)
		{
			string input = Console.ReadLine();

			if (input.ToLower() == "exit")
			{
				break; // Exit loop and program
			}

			try
			{
				byte[] data = input.Trim().ToByteArray();
				proxy.Send(data);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error processing input: {ex.Message}");
			}
		}
	}
}
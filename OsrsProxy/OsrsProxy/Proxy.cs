namespace OsrsProxy;

public abstract class Proxy
{
	public abstract void Listen();
	public abstract void Send(byte[] bytes);
}
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TinyWeb;
namespace w3serv
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var w3s = new WebServer())
			{
				if (0 < args.Length)
				{
					w3s.EndPoint = _ToEndPoint(args[0]);
				}
				else
				{
					Console.Error.WriteLine("Usage: w3serv <ip>:<port>");
					Console.Error.WriteLine("\t<ip> can be \"*\"");
				}
				w3s.IsStarted = true;
				w3s.ProcessRequest += W3s_ProcessRequest;
				Console.Error.WriteLine("Press any key to stop serving...");
				Console.ReadKey();
			}
		}

		private static void W3s_ProcessRequest(object sender, ProcessRequestEventArgs args)
		{
			// default is text/plain
			args.Response.ContentType = "text/html";
			args.Response.WriteLine("<html><body><h1>Hello World</h1></body>");
		}

		static IPEndPoint _ToEndPoint(string value)
		{
			var s = value as string;
			if (null != s)
			{
				if (0 == s.Length)
					return new IPEndPoint(IPAddress.Any, 80);

				var i = s.LastIndexOf(':');
				var port = 80;
				if (-1< i)
				{
					port = int.Parse(s.Substring(i + 1));
					s = s.Substring(0, i);
				}
				var addr = (0 == s.Length || "*" == s) ? IPAddress.Any : IPAddress.Parse(s);

				return new IPEndPoint(addr, port);
			}
			return null;
		}
	}
}

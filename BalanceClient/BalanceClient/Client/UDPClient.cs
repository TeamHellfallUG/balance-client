using System;
using Lidgren.Network;

using Balance.Utils;

namespace Balance.Client
{
	public class UDPClient : IClient
	{
		public UDPClient()
		{
			
		}

		public event EmptyArgsDelegate OnClose;
		public event EmptyArgsDelegate OnConnect;
		public event ErrorArgsDelegate OnError;
		public event StringArgsDelegate OnMessage;

		public void Close()
		{
			throw new NotImplementedException();
		}

		public void Connect(Config config)
		{
			throw new NotImplementedException();
		}

		public void Send(string data)
		{
			throw new NotImplementedException();
		}
	}
}

using System;

using Balance.Utils;

namespace Balance.Client
{
	public delegate void EmptyArgsDelegate();
	public delegate void StringArgsDelegate(String data);
	public delegate void ErrorArgsDelegate(Exception exception);

	public interface IClient
	{
		void Close();
		void Connect(Config config);
		void Send(String data);

		event EmptyArgsDelegate OnConnect;
		event EmptyArgsDelegate OnClose;
		event StringArgsDelegate OnMessage;
		event ErrorArgsDelegate OnError;
	}
}

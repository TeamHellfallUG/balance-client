using System;
using WebSocketSharp;

using Balance.Utils;

namespace Balance.Client
{
	public class WSClient : IClient
	{
		private WebSocket ws;

		public event EmptyArgsDelegate OnConnect;
		public event EmptyArgsDelegate OnClose;
		public event StringArgsDelegate OnMessage;
		public event ErrorArgsDelegate OnError;

		private void attachListeners() {

			if (ws == null)
			{
				return;
			}

			ws.OnOpen += (sender, e) =>
			{
				if (OnConnect != null) {
					OnConnect();
				}
			};

			ws.OnMessage += (sender, e) =>
			{
				this.onMessage(sender, e);
			};

			ws.OnError += (sender, e) =>
			{
				if (OnError != null)
				{
					OnError(e.Exception);
				}
			};

			ws.OnClose += (sender, e) =>
			{
				if (OnClose != null)
				{
					OnClose();
				}
			};
		}

		private void onMessage(Object sender, MessageEventArgs args) {

			if (args.IsText && args.Data != null) { 
				
				if (OnMessage != null)
				{
					OnMessage(args.Data);
				}	
			}
		}

		public void Close()
		{
			if (ws != null) {
				ws.Close();
			}
		}

		public void Connect(Config config)
		{
			if (ws != null) {
				throw new Exception("wsclient is already connected.");
			}

			this.ws = new WebSocket(config.getWsUrl());
			ws.EmitOnPing = false;
			ws.Origin = config.getHttpUrl();

			attachListeners();
			ws.Connect();
		}

		public void Send(String data) {
			if (ws != null) { 
				ws.Send(data);
			}
		}
	}
}

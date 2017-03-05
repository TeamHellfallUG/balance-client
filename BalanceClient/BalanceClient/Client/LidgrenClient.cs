using System;
using System.Threading;
using Lidgren.Network;

using Balance.Utils;

namespace Balance.Client
{
	public class LidgrenClient : IClient
	{
        private NetClient client;
        private Config config;
        private Boolean connected;

		public LidgrenClient()
		{
            connected = false;

            if (SynchronizationContext.Current == null)
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }

		public event EmptyArgsDelegate OnClose;
		public event EmptyArgsDelegate OnConnect;
		public event ErrorArgsDelegate OnError;
		public event StringArgsDelegate OnMessage;
        public event StringArgsDelegate OnDebug;

        public delegate void NCSArgsDelegate(NetConnectionStatus status);
        public event NCSArgsDelegate OnStatusChange;

        public bool IsConnected()
        {
            return connected;
        }

		public void Close()
		{
            client.Disconnect("{ \"type\": \"internal\", \"header\": \"DISCONNECT\" }");
            client.Shutdown("{ \"type\": \"internal\", \"header\": \"SHUTDOWN\" }");
            client = null;
            this.connected = false;
        }

        public void Connect(Config config)
		{
            if(client != null)
            {
                throw new Exception("a client instance is already active.");
            }

            //init
            NetPeerConfiguration npc = new NetPeerConfiguration("balance");
            npc.AutoFlushSendQueue = false;
            this.config = config;
            this.client = new NetClient(npc);
            client.RegisterReceivedCallback(new SendOrPostCallback(OnMessageReceived));

            //connect
            client.Start();
            NetOutgoingMessage hail = client.CreateMessage("{ \"type\": \"internal\", \"header\": \"HAIL\" }");
            client.Connect(config.hostname, config.port, hail);
		}

		public void Send(string data)
		{
            NetOutgoingMessage om = client.CreateMessage(data);
            client.SendMessage(om, NetDeliveryMethod.ReliableOrdered);
            client.FlushSendQueue();
		}

        public void SendUnreliable(string data)
        {
            NetOutgoingMessage om = client.CreateMessage(data);
            client.SendMessage(om, NetDeliveryMethod.Unreliable);
            client.FlushSendQueue();
        }

        protected void OnMessageReceived(object peer)
        {
            NetIncomingMessage im;

            while((im = client.ReadMessage()) != null)
            {
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.ErrorMessage:
                    case NetIncomingMessageType.WarningMessage:
                        string error = im.ReadString();
                        if (OnError != null)
                        {
                            OnError(new Exception(error));
                        }
                        break;

                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.VerboseDebugMessage:
                        string text = im.ReadString();
                        if(OnDebug != null)
                        {
                            OnDebug(text);
                        }
                        break;

                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)im.ReadByte();

                        if (status == NetConnectionStatus.Connected)
                        {
                            this.connected = true;
                            if (OnConnect != null)
                            {
                                OnConnect();
                            }
                        }

                        if (status == NetConnectionStatus.Disconnected)
                        {
                            this.connected = false;
                            if (OnClose != null)
                            {
                                OnClose();
                            }
                        }

                        if (OnStatusChange != null)
                        {
                            OnStatusChange(status);
                        }

                        //string reason = im.ReadString();
                        //Output(status.ToString() + ": " + reason);
                        break;

                    case NetIncomingMessageType.Data:
                        string data = im.ReadString();
                        if (OnMessage != null)
                        {
                            OnMessage(data);
                        }
                        break;

                    default:
                        string any = im.ReadString();
                        if (OnMessage != null)
                        {
                            OnMessage(any);
                        }
                        break;
                }

                client.Recycle(im);
            }
        }
	}
}

using System;
using System.Threading;
using System.Diagnostics;

using Newtonsoft.Json;

using Balance.Client;
using Balance.Utils;

namespace Balance
{
	public class BalanceClient
	{
		public const String VERSION = "0.0.2";
		public const String INTERNAL = "internal";
		public const String PING_HEADER = "GS:PING";
		public const Int32 PING_INTERVAL = 15500;

		public delegate void LogDelegate(String message);
		public delegate void PacketArgsDelegate(Packet packet);

		protected IClient client;
		protected Config config;
		protected Thread t;
		protected Thread ti;
		protected LogDelegate _log;
		protected Stopwatch pingStopwatch;
		protected double lastPingRoundtrip;

		public event PacketArgsDelegate OnPacket;
		public event PacketArgsDelegate OnInternalPacket;

		public BalanceClient(Config config, IClient client)
		{
			if (config == null)
			{
				throw new Exception("config must not be null.");
			}

			if (client == null)
			{
				throw new Exception("client must not be null.");
			}

			this.config = config;
			this.client = client;
		}

		public BalanceClient(Config config, IClient client, LogDelegate log)
		{
			if (config == null)
			{
				throw new Exception("config must not be null.");
			}

			if (client == null)
			{
				throw new Exception("client must not be null.");
			}

			this.config = config;
			this.client = client;
			this._log = log;

			this.log("[BalanceClient]: " + VERSION);
		}

		protected void log(String message) {
			if (this._log != null) {
				this._log("[BalanceClient]: " + message);
			}
		}

		protected void spawnIntervalThread()
		{
			if (ti != null) {
				throw new Exception("spawnIntervalThread should only be called once per instance.");
			}

			this.ti = new Thread(() => {
				while (true) { 
					ping();
					Thread.Sleep(PING_INTERVAL);
				}
			});

			log("spawning interval thread.");
			this.ti.Start();
		}

		protected void spawnThread()
		{
			if (t != null) {
				throw new Exception("spawnThread should only be called once per instance.");
			}

			this.attachListeners();

			this.t = new Thread(() => {
				Thread.Sleep(15);
				this.prepareConnection();
				this.startConnection();
			});

			log("spawning thread.");
			this.t.Start();
		}

		protected void prepareConnection()
		{
			log("preparing connection.");
		}

		protected void startConnection()
		{
			log("starting connection.");
			this.client.Connect(this.config);
		}

		protected void ping()
		{
			log("sending ping.");
			this.pingStopwatch = Stopwatch.StartNew();
			Send(new Packet(INTERNAL, "GS:PING", "ping"));
		}

		protected void receivePong() {
			
			if (pingStopwatch == null) {
				return;
			}

			pingStopwatch.Stop();
			lastPingRoundtrip = pingStopwatch.Elapsed.TotalMilliseconds;
			pingStopwatch = null;
			log("received pong. Roundtrip took: " + lastPingRoundtrip.ToString() + " ms.");
		}

		private void attachListeners() {

			client.OnConnect += () => {
				log("connected.");
				spawnIntervalThread();
			};

			client.OnClose += () =>
			{
				log("closed.");
			};

			client.OnError += (Exception exception) =>
			{
				log("error: " + exception.Message);
			};

			client.OnMessage += (String data) =>
			{
				log("receiving: " + data);
				Packet packet = this.deserialiseIncomingPacket(data);
				//log("receiving: " + packet.ToString());

				if (packet.Type == INTERNAL) {
					
					if (packet.Header == PING_HEADER) {
						receivePong();
						return;
					}

					if (OnInternalPacket != null) {
						OnInternalPacket(packet);
						return;
					}
				}

				if (OnPacket != null) {
					OnPacket(packet);
				}
			};
		}

		protected Packet deserialiseIncomingPacket(String data) {
			try
			{
				return JsonConvert.DeserializeObject<NetPacket>(data).ToPacket();
			}
			catch (Exception exception){
				log("packet deserialisation failed: " + exception.Message + ", " + data);
				return new Packet();
			}
		}

		protected String serialiseOutgoingPacket(Packet packet) {
			return JsonConvert.SerializeObject(packet.ToNetPacket());
		}

		public void Run()
		{
			this.spawnThread();
		}

		public void Close()
		{
			log("closing.");
			this.client.Close();

			try
			{
				if (t != null && t.IsAlive)
				{
					t.Abort();
				}

				if (ti != null && ti.IsAlive)
				{
					ti.Abort();
				}
			}
			catch (Exception exception)
			{
				log("exception during thread abortion: " + exception.Message);
			}
		}

		public void Send(Packet packet) {

			if (packet == null) {
				throw new Exception("sending a null packet is not allowed.");
			}

			try
			{
				log("sending packet with header: " + packet.Header);
				this.client.Send(this.serialiseOutgoingPacket(packet));
			}
			catch (Exception exception){
				log("exception during send: " + exception.Message);
			}
		}

		public double getCurrentPing() {
			return lastPingRoundtrip;
		}
	}
}
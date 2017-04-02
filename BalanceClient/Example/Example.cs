using System;
using System.Threading;
using System.Net;
using System.Collections.Specialized;

using Balance.Utils;
using Balance.Client;
using Balance.Http;
using Balance.Specialized;

namespace Example
{
	class SomeClass {
		public String One;
		public float Two;
		public Int32[] Three;
	}

	class Example
	{
		public const String VERSION = "1.0.3";

		public static void Main(string[] args)
		{
			Console.WriteLine("[Example]: " + VERSION);

            //HttpExample();
            //WSClientExample();
            //LidgrenClientExample();
            //UDPClientExample();
            RoomGroupClientExample();

            while (true)
			{
				Thread.Sleep(100);
				//empty
			}
		}

		public static void HttpExample()
		{
			Request http = new Request("httpbin.org", "http", Console.WriteLine);

			WebHeaderCollection headers = new WebHeaderCollection();
			headers.Add("Accept: application/json");
			headers.Add("Version: 1");

			http.request(new Inquiry(Methods.GET, "/get", headers), (exception, response) => {

				if (exception != null) {
					Console.WriteLine("web request 1 failed with error: " + exception.Message);
					return;
				}

				Console.WriteLine("web request 1 result: " + response.Body);
			});

			http.request(new Inquiry(Methods.POST, "/post", headers, new NameValueCollection { {"bla", "blup"} }), (exception, response) =>
			{

				if (exception != null)
				{
					Console.WriteLine("web request 2 failed with error: " + exception.Message);
					return;
				}

				Console.WriteLine("web request 2 result: " + response.Body);
			});
		}

		public static void WSClientExample()
		{

			Config config = new Config();
            config.hostname = "192.168.192.52";
			config.port = 8443;

			WSClient ws = new WSClient();

			//BalanceClient client = new BalanceClient(config, ws, Console.WriteLine);
			VectorGroupClient client = new VectorGroupClient(config, ws, Console.WriteLine);

			client.OnConnect += () => {
				//client.CreateGroupRequest();
				client.VectorUpdateRequest(new Vector(20, 10, 30));
			};

			client.OnGroupCreated += groupId => {
				Console.WriteLine(groupId + " created.");
				//client.DeleteGroupRequest(groupId);
			};

			client.OnGroupDeleted += groupId => {
				Console.WriteLine(groupId + " deleted.");
			};

			client.OnVectorPosition += packet => {
				Console.WriteLine("position update: " + packet.ToString());
			};

			client.OnVectorBroadcast += packet =>
			{
				Console.WriteLine("vector broadcast: " + packet.ToString());
			};

			client.Run();

			//starting a second client to check vector/range broadcasts

			WSClient ws2 = new WSClient();

			//BalanceClient client = new BalanceClient(config, ws, Console.WriteLine);
			VectorGroupClient client2 = new VectorGroupClient(config, ws2, Console.WriteLine);

			client2.OnConnect += () => {
				client2.VectorUpdateRequest(new Vector(19, 9, 31)); //pick a close range
			};

			client2.Run();

			//wait a while and broadcast a range message,
			//client(1) should emit on-vector-broadcast
			(new Thread(() => {
				Thread.Sleep(2500);

				SomeClass sc = new SomeClass();
				sc.One = "bla blup";
				sc.Two = 15.6f;
				sc.Three = new Int32[] {1,2,3};

				client2.RangeBroadcastRequest(sc);
			})).Start();
		}

		public static void LidgrenClientExample() {

            LidgrenClient client = new LidgrenClient();
            Config config = new Config();
            config.hostname = "192.168.192.52";
            config.port = 9443;

            client.OnDebug += Console.WriteLine;
            client.OnError += Console.WriteLine;
            client.OnMessage += Console.WriteLine;

            client.Connect(config);

            client.Send("bla");
            client.SendUnreliable("bla bla");
        }

        public static void UDPClientExample()
        {
            UDPClient client = new UDPClient();

            Config config = new Config();
            config.hostname = "192.168.192.52";
            config.port = 9443;

            client.OnError += Console.WriteLine;
            client.OnMessage += Console.WriteLine;

            client.Connect(config);

            client.OnConnect += () =>
            {
                client.Send("derp");
            };
        }

        public static void RoomGroupClientExample()
        {
            WSClient ws = new WSClient();

            Config wsConfig = new Config();
            wsConfig.hostname = "192.168.192.52";
            wsConfig.port = 8443;

            Config udpConfig = new Config();
            udpConfig.hostname = "192.168.192.52";
            udpConfig.port = 9443;

            RoomGroupClient rgc = new RoomGroupClient(wsConfig, ws, udpConfig, Console.WriteLine);

            rgc.Run();
        }
	}
}
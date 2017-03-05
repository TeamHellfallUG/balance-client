using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Balance.Utils;

namespace Balance.Client
{
    public class UDPClient : IClient
    {
        public const String INTERNAL = "internal";
        public const String PING_HEADER = "UDP:PING";
        public const String CONN_HEADER = "UDP:CONN";
        public static DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static int MAX_CONN_ATTEMPTS = 5;
        public static int ACK_THRESHOLD = 3500;
        public static int ACK_INTERVAL = 1000;

        private Thread readThread;
        private UdpClient client;

        public event EmptyArgsDelegate OnConnect;
        public event EmptyArgsDelegate OnClose;
        public event StringArgsDelegate OnMessage;
        public delegate void PacketArgsDelegate(Packet packet);
        public event PacketArgsDelegate OnPacket;
        public event ErrorArgsDelegate OnError;
        public event StringArgsDelegate OnDebug;

        private bool connected;
        private Config config;
        private IPEndPoint serverAddress;

        private Thread connectionThread;
        private Thread ackThread;
        private long lastAckReceived;
        private long currentPing;
        private long startedAt;

        public UDPClient()
        {
            connected = false;
            startedAt = GetCurrentUnixTimestampMillis();
        }

        public void Close()
        {
            this.connected = false;

            stopAckProcess();
            stopConnectionThread();

            if (readThread != null && readThread.IsAlive)
            {
                readThread.Abort();
                client.Close();
            }
        }

        public void Connect(Config config)
        {
            if(readThread != null)
            {
                throw new Exception("readThread is already active.");
            }

            string d = JsonConvert.SerializeObject(new Packet());
            Console.WriteLine(d);


            this.config = config;
            IPAddress serverIp = IPAddress.Parse(config.hostname);
            this.serverAddress = new IPEndPoint(serverIp, config.port);
            this.readThread = new Thread(new ThreadStart(receiveData));

            readThread.IsBackground = true;
            readThread.Start();
        }

        public bool IsConnected()
        {
            return connected;
        }

        public long GetPing()
        {
            return currentPing;
        }

        public void Send(string data)
        {
            send(data);
        }

        private int send(string data)
        {
            if(client == null)
            {
                return -1;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(data);
            return client.Send(bytes, bytes.Length, serverAddress);
        }

        private int send(object data)
        {
            return this.send(JsonConvert.SerializeObject(data));
        }

        private int send(string type, string header, object content)
        {
            return this.send(new Packet(type, header, content).ToNetPacket());
        }

        private void debug(string message)
        {
            if(OnDebug != null)
            {
                OnDebug(message);
            }
        }

        private void startConnectionProcess()
        {
            connectionThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    debug("CONN thread running.");
                    int attempts = 0;
                    while (!connected && attempts < MAX_CONN_ATTEMPTS)
                    {
                        //send an hail request until connected returns true
                        //or we have to make more than 5 requests
                        //this.send(INTERNAL, CONN_HEADER, new Object());
                        debug("sending CONN attempt.");
                        this.send(INTERNAL, CONN_HEADER, new JObject());
                        attempts++;
                        Thread.Sleep(1200); //wait at least 1,2 seconds
                    }

                    if (!connected)
                    {
                        OnError(new Exception("exceeded the maximum connection attempt."));
                        return; //thread will exit
                    }

                    debug(String.Format("CONN good, took: {0} ms.", (GetCurrentUnixTimestampMillis() - startedAt)));
                    //start the ack process and emit connected event
                    this.startAckProcess();
                    if(OnConnect != null)
                    {
                        OnConnect();
                    }
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }));

            connectionThread.IsBackground = true;
            connectionThread.Start();
        }

        private void stopConnectionThread()
        {
            if (connectionThread != null && connectionThread.IsAlive)
            {
                connectionThread.Abort();
                connectionThread = null;
            }
        }

        private void startAckProcess()
        {
            lastAckReceived = GetCurrentUnixTimestampMillis();
            debug(lastAckReceived + " is last receive init.");
            ackThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    debug("ack thread running.");
                    while (true)
                    {
                        if(this.client == null || !this.connected)
                        {
                            return;
                        }

                        debug("checking threshold.");
                        //check for threshold first
                        if (GetCurrentUnixTimestampMillis() - lastAckReceived >= ACK_THRESHOLD)
                        {
                            debug("bad threshold.");
                            this.connected = false;

                            if(OnClose != null)
                            {
                                OnClose();
                            }
                            
                            this.Close();
                            return;
                        }

                        debug("building stamp content.");
                        JObject content = new JObject();
                        content.Add("stamp", GetCurrentUnixTimestampMillis());

                        debug("sending ping.");
                        this.send(INTERNAL, PING_HEADER, content);
                        Thread.Sleep(ACK_INTERVAL);
                    }
                } catch(Exception ex)
                {
                    OnError(ex);
                }
            }));

            ackThread.IsBackground = true;
            ackThread.Start();
        }

        private void stopAckProcess()
        {
            if (ackThread != null && ackThread.IsAlive)
            {
                ackThread.Abort();
                ackThread = null;
            }
        }

        private void handleAckReply(Packet packet)
        {
            debug("received ack reply.");
            lastAckReceived = GetCurrentUnixTimestampMillis();

            try
            {
                long ms = long.Parse(packet.Content.GetValue("stamp").ToString());
                currentPing = GetCurrentUnixTimestampMillis() - ms;
                debug("new ack roundtrip is: " + currentPing);
            } catch(Exception ex)
            {
                OnError(ex);
            }
        }

        private void receiveData()
        {
            client = new UdpClient(0);
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);

            //as soon as client is initiated, start the connection interval
            startConnectionProcess();

            while (true)
            {
                try
                {
                    byte[] data = client.Receive(ref anyIP);
                    debug("received data " + data.Length);
                    string message = Encoding.UTF8.GetString(data);
                    this.handleMessage(message);
                } catch(Exception ex)
                {
                    OnError(ex);
                }

                Thread.Sleep(1);
            }
        }

        protected Packet deserialiseIncomingPacket(String data)
        {
            try
            {
                return JsonConvert.DeserializeObject<NetPacket>(data).ToPacket();
            }
            catch (Exception exception)
            {
                OnError(new Exception("packet deserialisation failed: " + exception.Message + ", " + data));
                return new Packet();
            }
        }

        private void handleMessage(string message)
        {
            Packet packet = deserialiseIncomingPacket(message);

            //handle internal packets
            if (packet.Type == INTERNAL)
            {
                switch (packet.Header)
                {
                    case PING_HEADER:
                        handleAckReply(packet);
                        break;

                    case CONN_HEADER:
                        connected = true; //just set the boolean, interval will take care of the rest
                        break;

                    default:
                        OnError(new Exception("received unknown internal message header: " + packet.Header));
                        break;
                }
                return;
            }

            if (OnMessage != null)
            {
                OnMessage(message);
            }

            if(OnPacket != null)
            {
                OnPacket(packet);
            }
        }

        public static long GetCurrentUnixTimestampMillis()
        {
            DateTime localDateTime, univDateTime;
            localDateTime = DateTime.Now;
            univDateTime = localDateTime.ToUniversalTime();
            return (long)(univDateTime - UnixEpoch).TotalMilliseconds;
        }
    }
}

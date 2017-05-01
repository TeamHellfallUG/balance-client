using System;

using Newtonsoft.Json.Linq;
using System.Collections.Generic;

using Balance.Utils;
using Balance.Client;

namespace Balance.Specialized
{
	public class RoomGroupClient : BalanceClient
	{
		public delegate void StatesArgsDelegate(List<RStateUpdate> states);

		public event PacketArgsDelegate OnQueueBroadcast;
		public event PacketArgsDelegate OnJoinedQueue;
		public event PacketArgsDelegate OnLeftQueue;
		public event PacketArgsDelegate OnConfirmRequest;

		public event PacketArgsDelegate OnMatchDisband; //server only
		public event PacketArgsDelegate OnMatchStart; //server only
		public event PacketArgsDelegate OnMatchEnd; //server only
        public event VoidEventDelegate OnMatchValidated; //server only

        public event VoidEventDelegate OnMatchExit;
        public event StringArgsDelegate OnOtherMatchExit;

		public event StatesArgsDelegate OnStatesUpdate;
		public event PacketArgsDelegate OnMessageUpdate;
		public event PacketArgsDelegate OnWorldUpdate;

        public event StringArgsDelegate OnUdpIdentification; //server only
        public event VoidEventDelegate OnUdpClientConnected;
        public event VoidEventDelegate OnUdpClientClose;

		private Boolean inQueue = false;
		private Boolean inMatch = false;
        private Boolean confirmationOpen = false;
        private Boolean clientConfirmed = false;
		private String currentMatchId = null;
		private String confirmMatchId = null;

		private List<string> lastMatches;

        private Config udpConfig;
        private UDPClient udpClient;
        private String udpIdentifier;
        private String udpGroupId;
        private Boolean udpActive;

        public RoomGroupClient(Config config, IClient client, LogDelegate logDelegate) : 
		base(config, client, logDelegate)
		{
			log("RoomGroupClient active.");
			this.lastMatches = new List<string> ();
            init();
		}

        public RoomGroupClient(Config mainConfig, IClient mainClient, 
            Config udpConfig, LogDelegate logDelegate) :
            base(mainConfig, mainClient, logDelegate)
        {
            log("RoomGroupClient (+sub-client) active.");
            this.lastMatches = new List<string>();

            //will be used to spawn udp clients on demand
            this.udpConfig = udpConfig;

            this.udpIdentifier = "";
            this.udpGroupId = "";
            this.udpActive = false;

            init();
        }

        public bool IsUdpActive()
        {
            return this.udpActive;
        }

        public UDPClient GetUdpSubClient()
        {
            return this.udpClient;
        }

		public bool IsInQueue(){
			return this.inQueue;
		}

		public bool IsInMatch(){
			return this.inMatch;
		}

        public bool IsConfirmationOpen()
        {
            return this.confirmationOpen;
        }

        public bool HasClientConfirmed()
        {
            return this.clientConfirmed;
        }

		public String GetCurrentMatchId(){
			return this.currentMatchId;
		}

		public List<string> GetLastMatches(){
			return this.lastMatches;
		}

		private void handleSearchConfirmation(Packet packet){
			this.inQueue = true;
			log ("joined match making queue.");
		}

		private void handleLeaveConfirmation(Packet packet){
			this.inQueue = false;
			log ("left match making queue.");
		}

		private void handleConfirmRequest(Packet packet){
			this.inQueue = false;

            try
            {
                this.confirmMatchId = packet.Content.GetValue("matchId").ToString();
            }
            catch (Exception ex)
            {
                log("failed to parse matchId from confirmation message");
                log(ex.Message + " .. " + ex.StackTrace);
                return;
            }

            this.confirmationOpen = true;
            this.clientConfirmed = false; //reset
            log ("found match, received confirmation request.");
		}

		private void handleStart(Packet packet){
            this.confirmationOpen = false;
            this.inMatch = true;
            this.currentMatchId = this.confirmMatchId;
			this.confirmMatchId = null;
			log ("match is ready to start.");
		}

		private void handleDisband(Packet packet){
			this.inMatch = false;
            this.confirmationOpen = false;
            this.clientConfirmed = false;
            this.currentMatchId = null;
			this.confirmMatchId = null;
			log ("match group has been disbanded.");
		}

		private void handleEnd(Packet packet){
			this.inMatch = false;
            this.lastMatches.Add (this.currentMatchId);
			this.currentMatchId = null;
			this.confirmMatchId = null;
			log ("match has ended.");
		}

		private void handleMatchExit(Packet packet){

            try
            {
                String leaverId = packet.Content.GetValue("leaver").ToString();
                if(leaverId != this.GetIdentification())
                {
                    log("other " + leaverId + " left the match.");
                    if (OnOtherMatchExit != null)
                    {
                        OnOtherMatchExit(leaverId);
                    }
                    return;
                }
            } catch(Exception ex)
            {
                log("an exception occured during the parsing of a match exit packet: " + ex.Message);
                return;
            }

            this.inMatch = false;
			this.lastMatches.Add (this.currentMatchId);
			this.currentMatchId = null;
			this.confirmMatchId = null;
			log ("client left match.");

            this.CloseUdpSubClient();

            if (OnMatchExit != null)
            {
                OnMatchExit();
            }
        }

		private void handleStatesUpdate(Packet packet){

			if(!this.IsInMatch()){
				log ("client is not in match, cannot process states update.");
				return;
			}

            JToken token = null;
            try
            {
                token = packet.Content.GetValue("states");
            } catch(Exception ex)
            {
                log("failed to retrieve states from su-message: " + ex.Message + ",  " + ex.StackTrace);
                return;
            }

            List<RStateUpdate> states = null;
            try
            {
                states = token.ToObject<List<RStateUpdate>>();
            } catch(Exception ex)
            {
                log("failed to parse/cast states from su-message: " + ex.Message + ",  " + ex.StackTrace);
                return;
            }

			if (OnStatesUpdate != null) {
				OnStatesUpdate(states);
			}
		}

        public void CloseUdpSubClient()
        {
            if (this.udpActive)
            {
                this.log("closing udp sub client.");

                this.udpActive = false;
                this.udpIdentifier = "";
                this.udpGroupId = "";

                if (OnUdpClientClose != null)
                {
                    OnUdpClientClose();
                }

                if (this.udpClient != null)
                {
                    this.udpClient.Close();
                    this.udpClient = null;
                }
            }
        }

        private void spawnUdpSubClient()
        {
            if (this.udpActive)
            {
                this.log("cannot spawn udp sub client as one is already active.");
                return;
            }

            if (OnUdpIdentification != null)
            {
                OnUdpIdentification(this.udpIdentifier);
            }

            this.udpActive = true;
            this.udpClient = new UDPClient();

            this.udpClient.OnConnect += () =>
            {
                this.log("udp sub client is connected.");

                if (OnUdpClientConnected != null)
                {
                    OnUdpClientConnected();
                }
            };

            this.udpClient.OnDebug += str =>
            {
                this.debug(str);
            };

            this.udpClient.OnClose += () =>
            {
                this.CloseUdpSubClient();
            };

            this.udpClient.OnError += error =>
            {
                this.log("udp client error occured: " + error.Message + ", " + error.StackTrace);
            };

            this.udpClient.OnMessage += msg =>
            {
                this.debug("udp msg: " + msg);
            };

            this.udpClient.OnPacket += packet =>
            {
                if(packet.Type == INTERNAL)
                {
                    switch (packet.Header)
                    {
                        case RGSHeader.STATE_UPDATE:
                            this.handleStatesUpdate(packet);
                            return;

                        case RGSHeader.MESSAGE_UPDATE:
                            this.handleMessageUpdate(packet);
                            break;

                        case RGSHeader.WORLD_UPDATE:
                            if (OnWorldUpdate != null)
                            {
                                OnWorldUpdate(packet);
                            }
                            break;

                        default:
                            //empty
                            break;
                    }
                }
            };

            this.log("spawning udp sub client..");
            this.udpClient.Connect(this.udpConfig);
        }

        public int SendUdp(String header, JObject content, String type)
        {
            if(this.udpClient == null)
            {
                throw new Exception("There is no udp client currently spawned.");
            }

            //apply required default fields
            content.Add("gid", this.udpGroupId);
            content.Add("uid", this.udpIdentifier);
            content.Add("tid", this.GetIdentification());

            return this.udpClient.Send(new Packet(type, header, content));
        }

        private void handleMessageUpdate(Packet packet)
        {
            try
            {
                //try to look for identifier packet
                String identifier = packet.Content.GetValue("identifier").ToString();
                String groupId = packet.Content.GetValue("groupId").ToString();

                if(identifier != null && groupId != null)
                {
                    this.log("received udp client identifier: " + identifier);
                    this.udpIdentifier = identifier;
                    this.udpGroupId = groupId;
                    this.spawnUdpSubClient();

                    return; //EOF
                }

                //try to look for info packet
                String info = packet.Content.GetValue("info").ToString();

                if(info != null)
                {
                    this.log("received info -> " + info);

                    if(info == "VALIDATION-TOTAL")
                    {
                        if (OnMatchValidated != null)
                        {
                            OnMatchValidated();
                        }
                    }

                    return; //EOF
                }

            } catch(Exception ex)
            {
                //empty
            }

            if (OnMessageUpdate != null)
            {
                OnMessageUpdate(packet);
            }
        }

        private void init() {

			base.OnInternalPacket += packet => {

				switch (packet.Header) {

					case RGSHeader.BROADCAST:
						if(OnQueueBroadcast != null){
							OnQueueBroadcast(packet);
						}
					break;

					case RGSHeader.SEARCH:
						this.handleSearchConfirmation(packet);
						if(OnJoinedQueue != null){
							OnJoinedQueue(packet);
						}
					break;

					case RGSHeader.LEAVE:
                        this.handleLeaveConfirmation(packet);
						if(OnLeftQueue != null){
							OnLeftQueue(packet);
						}
					break;

					case RGSHeader.CONFIRM:
					    this.handleConfirmRequest(packet);
						if(OnConfirmRequest != null){
							OnConfirmRequest(packet);
						}
					break;

					case RGSHeader.DISBAND:
                        this.CloseUdpSubClient();
                        this.handleDisband(packet);
						if(OnMatchDisband != null){
							OnMatchDisband(packet);
						}
					break;

					case RGSHeader.START:
						this.handleStart(packet);
						if(OnMatchStart != null){
							OnMatchStart(packet);
						}
					break;

					case RGSHeader.END:
                        this.CloseUdpSubClient();
						this.handleEnd(packet);
						if(OnMatchEnd != null){
							OnMatchEnd(packet);
						}
					break;

					case RGSHeader.EXIT:
                        this.handleMatchExit(packet);
					break;

					case RGSHeader.STATE_UPDATE:
						this.handleStatesUpdate(packet);
					return;

					case RGSHeader.MESSAGE_UPDATE:
                        this.handleMessageUpdate(packet);
					break;

					case RGSHeader.WORLD_UPDATE:
						if(OnWorldUpdate != null){
							OnWorldUpdate(packet);
						}
					break;
				}
			};
		}

		public void BroadcastToQueue(String message){
			JObject content = new JObject ();
			content.Add ("delivery", message);
			Send (new Packet(INTERNAL, RGSHeader.BROADCAST, content));
		}

		public void JoinMatchMakingQueue(){

			if (this.IsInQueue ()) {
				throw new Exception ("cannot join mm queue, since client is already in it.");
			}

			if(this.IsInMatch()){
				throw new Exception ("cannot join mm queue, since client is in a match.");
			}

			JObject content = new JObject ();
			Send (new Packet(INTERNAL, RGSHeader.SEARCH, content));
		}

		public void LeaveMatchMakingQueue(){

			if (!this.IsInQueue ()) {
				throw new Exception ("cannot leave mm queue, since client is not in it.");
			}

			JObject content = new JObject ();
			Send (new Packet(INTERNAL, RGSHeader.LEAVE, content));
		}

		public void ConfirmMatchRequest(){

			if(this.confirmMatchId == null && this.confirmationOpen){
				throw new Exception ("cannot confirm match without a present confirmation request.");
			}

            if (this.clientConfirmed)
            {
                throw new Exception("client has already confirmed the latest request.");
            }

            this.clientConfirmed = true;

            JObject content = new JObject ();
			content.Add ("matchId", this.confirmMatchId);
			Send(new Packet(INTERNAL, RGSHeader.CONFIRM, content));
		}

		public void ExitMatch(){

			if (!this.IsInMatch ()) {
				throw new Exception ("cannot exit match, sine client is not a member of one.");
			}

			JObject content = new JObject ();
			content.Add ("matchId", this.currentMatchId);
			Send (new Packet(INTERNAL, RGSHeader.EXIT, content));
		}

		public void SendStateUpdate(StateUpdate stateUpdate){

			if(!this.IsInMatch()){
				throw new Exception ("cannot send state update, since client is not in a match.");
			}

			JObject content = new JObject ();
			content.Add ("state", JObject.FromObject(stateUpdate));
			Send (new Packet(INTERNAL, RGSHeader.STATE_UPDATE, content));
		}

        public void SendUdpStateUpdate(StateUpdate stateUpdate)
        {
            if (!this.IsInMatch())
            {
                throw new Exception("cannot send state update, since client is not in a match.");
            }

            JObject content = new JObject();
            content.Add("state", JObject.FromObject(stateUpdate));
           
            SendUdp(RGSHeader.STATE_UPDATE, content, INTERNAL);
        }

        public new void Close()
        {
            base.Close();
            this.CloseUdpSubClient();
        }

	}
}

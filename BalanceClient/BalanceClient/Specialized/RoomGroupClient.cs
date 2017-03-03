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
		public event PacketArgsDelegate OnMatchExit;

		public event StatesArgsDelegate OnStatesUpdate;
		public event PacketArgsDelegate OnMessageUpdate;
		public event PacketArgsDelegate OnWorldUpdate;

		private bool inQueue = false;
		private bool inMatch = false;
        private bool confirmationOpen = false;
		private String currentMatchId = null;
		private String confirmMatchId = null;

		private List<string> lastMatches;
		
		public RoomGroupClient(Config config, IClient client, LogDelegate logDelegate) : 
		base(config, client, logDelegate)
		{
			log("RoomGroupClient active.");
			this.lastMatches = new List<string> ();
            init();
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
            log ("found match, received confirmation request.");
		}

		private void handleStart(Packet packet){
			this.inMatch = true;
            this.currentMatchId = this.confirmMatchId;
			this.confirmMatchId = null;
			log ("match is ready to start.");
		}

		private void handleDisband(Packet packet){
			this.inMatch = false;
            this.confirmationOpen = false;
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
			this.inMatch = false;
			this.lastMatches.Add (this.currentMatchId);
			this.currentMatchId = null;
			this.confirmMatchId = null;
			log ("client left match.");
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
						this.handleEnd(packet);
						if(OnMatchEnd != null){
							OnMatchEnd(packet);
						}
					break;

					case RGSHeader.EXIT:
						this.handleMatchExit(packet);
						if(OnMatchExit != null){
							OnMatchExit(packet);
						}
					break;

					case RGSHeader.STATE_UPDATE:
						this.handleStatesUpdate(packet);
					return;

					case RGSHeader.MESSAGE_UPDATE:
						if(OnMessageUpdate != null){
							OnMessageUpdate(packet);
						}
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

            this.confirmationOpen = false;

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

	}
}

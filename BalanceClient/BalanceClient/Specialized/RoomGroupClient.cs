using System;

using Balance.Utils;
using Balance.Client;

namespace Balance.Specialized
{
	public class RoomGroupClient : BalanceClient
	{
        public event PacketArgsDelegate OnVectorPosition;
        public event PacketArgsDelegate OnVectorBroadcast;
		
		public RoomGroupClient(Config config, IClient client, LogDelegate logDelegate) : 
		base(config, client, logDelegate)
		{
			log("RoomGroupClient active.");
		}

        private void init() {

			base.OnInternalPacket += packet => {

				switch (packet.Header) {

					case "":
						if (OnVectorPosition != null) {
							OnVectorPosition(packet);
						}
						return;

					case "":
						if (OnVectorBroadcast != null)
						{
							OnVectorBroadcast(packet);
						}
						return;
				}
			};
		}

		public void VectorUpdateRequest(Vector vector)
		{
			JObject content = new JObject();
			content.Add("position", JObject.FromObject(vector));
			Send(new Packet(INTERNAL, V_POSITION_HEADER, content));
		}

		public void RangeBroadcastRequest(Object delivery)
		{
			JObject content = new JObject();
			content.Add("delivery", JObject.FromObject(delivery));
			Send(new Packet(INTERNAL, V_BROADCAST_HEADER, content));
		}

	}
}

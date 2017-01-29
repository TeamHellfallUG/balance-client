using System;

using Newtonsoft.Json.Linq;

using Balance.Utils;
using Balance.Client;

namespace Balance.Specialized
{
	public class VectorGroupClient : GroupClient
	{
		public const String V_HEADER_PREFIX = "VGS:";
		public const String V_POSITION_HEADER = V_HEADER_PREFIX + "POSITION";
		public const String V_BROADCAST_HEADER = V_HEADER_PREFIX + "BROADCAST";

		public event PacketArgsDelegate OnVectorPosition;
		public event PacketArgsDelegate OnVectorBroadcast;
		
		public VectorGroupClient(Config config, IClient client, LogDelegate logDelegate) : 
		base(config, client, logDelegate)
		{
			log("VectorGroupClient active.");
			init();
		}

		private void init() {

			base.OnInternalPacket += packet => {

				switch (packet.Header) {

					case V_POSITION_HEADER:
						if (OnVectorPosition != null) {
							OnVectorPosition(packet);
						}
						return;

					case V_BROADCAST_HEADER:
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

using System;

using Balance.Utils;
using Balance.Client;

using Newtonsoft.Json.Linq;

namespace Balance.Specialized
{
	public class GroupClient : BalanceClient
	{
		public const String HEADER_PREFIX = "GS:";
		public const String JOIN_HEADER = HEADER_PREFIX + "JOIN";
		public const String LEAVE_HEADER = HEADER_PREFIX + "LEAVE";
		public const String CREATE_HEADER = HEADER_PREFIX + "CREATE";
		public const String DELETE_HEADER = HEADER_PREFIX + "DELETE";
		public const String BROADCAST_HEADER = HEADER_PREFIX + "BROADCAST";

		public delegate void StringPacketArgsDelegate(String groupId, Packet packet);

		public event EmptyArgsDelegate OnConnect;
		public event EmptyArgsDelegate OnClose;
		public event ErrorArgsDelegate OnError;

		public event StringArgsDelegate OnGroupCreated;
		public event StringArgsDelegate OnGroupJoined;
		public event StringArgsDelegate OnGroupLeft;
		public event StringArgsDelegate OnGroupDeleted;
		public event StringPacketArgsDelegate OnGroupBroadcast;

		public GroupClient(Config config, IClient client, LogDelegate logDelegate) :
			base(config, client, logDelegate)
		{
			log("GroupClient active.");
			attachGroupListeners();
		}

		protected void attachGroupListeners()
		{

			client.OnConnect += () =>
			{
				if (OnConnect != null) {
					OnConnect();
				}
			};

			client.OnClose += () =>
			{
				if (OnClose != null)
				{
					OnClose();
				}
			};

			client.OnError += (Exception exception) =>
			{
				if (OnError != null)
				{
					OnError(exception);
				}
			};

			base.OnInternalPacket += packet =>
			{
				String groupId = null;

				switch (packet.Header) {
					
					case JOIN_HEADER:
						groupId = readGroupIdFromPacket(packet);
						if (groupId != null && OnGroupJoined != null) {
							OnGroupJoined(groupId);
						}
						return;

					case LEAVE_HEADER:
						groupId = readGroupIdFromPacket(packet);
						if (groupId != null && OnGroupLeft != null)
						{
							OnGroupLeft(groupId);
						}
						return;

					case CREATE_HEADER:
						groupId = readGroupIdFromPacket(packet);
						if (groupId != null && OnGroupCreated != null)
						{
							OnGroupCreated(groupId);
						}
						return;

					case DELETE_HEADER:
						groupId = readGroupIdFromPacket(packet);
						if (groupId != null && OnGroupDeleted != null)
						{
							OnGroupDeleted(groupId);
						}
						return;

					case BROADCAST_HEADER:
						groupId = readGroupIdFromPacket(packet);
						if (groupId != null && OnGroupBroadcast != null)
						{
							OnGroupBroadcast(groupId, packet);
						}
						return;


						//TODO catch for :NOTIFY headers
						//TODO error cases are not handled, they just emit events currently
				}
			};
		}

		private String readGroupIdFromPacket(Packet packet) {
			try
			{
				JToken groupId = null;
				if (!packet.Content.TryGetValue("groupId", out groupId)) {
					return null;
				}
				return groupId.ToString();
			}
			catch (Exception exception) {
				log("failed to parse groupId from packet: " + exception.Message);
				return null;
			}
		}

		public void JoinGroupRequest(String groupId) {
			JObject content = new JObject();
			content.Add("groupId", groupId);
			Send(new Packet(INTERNAL, JOIN_HEADER, content));
		}

		public void LeaveGroupRequest(String groupId) {
			JObject content = new JObject();
			content.Add("groupId", groupId);
			Send(new Packet(INTERNAL, LEAVE_HEADER, content));
		}

		public void CreateGroupRequest() {
			Send(new Packet(INTERNAL, CREATE_HEADER, new Object()));
		}

		public void DeleteGroupRequest(String groupId) {
			JObject content = new JObject();
			content.Add("groupId", groupId);
			Send(new Packet(INTERNAL, DELETE_HEADER, content));
		}

		public void BroadcastToGroupRequest(String groupId, JObject delivery) {
			JObject content = new JObject();
			content.Add("groupId", groupId);
			content.Add("delivery", delivery);
			Send(new Packet(INTERNAL, BROADCAST_HEADER, content));
		}

		public void BroadcastToGroupRequest(String groupId, Object delivery){
			JObject content = new JObject();
			content.Add("groupId", groupId);
			content.Add("delivery", JObject.FromObject(delivery));
			Send(new Packet(INTERNAL, BROADCAST_HEADER, content));
		}

	}
}

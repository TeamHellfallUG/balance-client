using System;

using Newtonsoft.Json.Linq;

namespace Balance.Utils
{
	/* json object on server has lowercase members */
	public class NetPacket {
		
		public String type;
		public String header;
		public JObject content;

		public NetPacket() { }

		public NetPacket(Packet packet) {

			if (packet.Type == null || packet.Header == null ||
			   packet.Content == null) {
				throw new Exception("packet contains null values: " + packet.ToString());
			}

			this.type = packet.Type;
			this.header = packet.Header;
			this.content = packet.Content;
		}

		public Packet ToPacket() {
			return new Packet(this);
		}
	}

	public class Packet
	{
		private String type;
		private String header;
		private JObject content;

		public Packet() {
			this.type = "";
			this.header = "";
			this.content = null;
		}

		public Packet(NetPacket netPacket) {
			this.type = netPacket.type;
			this.header = netPacket.header;
			this.content = netPacket.content;
		}

		public Packet(String type, String header, JObject content)
		{
			this.type = type;
			this.header = header;
			this.content = content;
		}

		public Packet(String type, String header, String content)
		{
			this.type = type;
			this.header = header;

			JObject obj = new JObject();
			JToken token = JToken.FromObject(content);
			obj.Add("content", token);

			this.content = obj;
		}

		public Packet(String type, String header, Object content)
		{
			this.type = type;
			this.header = header;
			this.content = null;

			SetContent(content);
		}

		public String Type { 
			get { return type; } 
			set { throw new Exception("cannot set packet Type of value: " + value); } 
		}

		public String Header {
			get { return header; }
			set { throw new Exception("cannot set packet Header of value: " + value); }
		}

		public JObject Content {
			get { return content; }
			set { throw new Exception("cannot set packet Content of value: " + value); }
		}

		public void SetContent(Object contentObject) {
			this.content = JObject.FromObject(contentObject);
		}

		public override String ToString() {
			Boolean isNotNull = content != null;
			return "{Packet}: type: " + type + ", header: " + header +
				", content-set: " + isNotNull.ToString();
		}

		public NetPacket ToNetPacket() {
			return new NetPacket(this);
		}
	}
}

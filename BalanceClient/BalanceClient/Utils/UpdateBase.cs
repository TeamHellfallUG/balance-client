using System;

using Newtonsoft.Json.Linq;

namespace Balance.Utils
{
    public abstract class UpdatePacketBase
    {
        private Object payload;
        private Boolean shouldBroadcast;

        public UpdatePacketBase(Object payload)
        {
            this.payload = payload;
            this.shouldBroadcast = false;
        }

        public UpdatePacketBase(Object payload, Boolean shouldBroadcast)
        {
            this.payload = payload;
            this.shouldBroadcast = shouldBroadcast;
        }

        public Object Payload
        {
            set { this.payload = value; }
        }

        public JObject ToJObject()
        {
            if (!shouldBroadcast)
            {
                return JObject.FromObject(payload);
            } 

            JObject content = new JObject();
            content.Add("bcast", JObject.FromObject(payload));
            return content;
        }
    }
}

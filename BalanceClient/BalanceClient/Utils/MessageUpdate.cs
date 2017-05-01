using System;

namespace Balance.Utils
{
    public class MessageUpdate : UpdatePacketBase
    {

        public MessageUpdate(Object payload) : base(payload)
        {
        }

        public MessageUpdate(Object payload, Boolean shouldBroadcast) : base(payload, shouldBroadcast)
        {
        }
    }
}

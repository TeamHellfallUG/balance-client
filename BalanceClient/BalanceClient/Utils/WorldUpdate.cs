using System;

namespace Balance.Utils
{
    public class WorldUpdate : UpdatePacketBase
    {

        public WorldUpdate(Object payload) : base(payload)
        {
        }

        public WorldUpdate(Object payload, Boolean shouldBroadcast) : base(payload, shouldBroadcast)
        {
        }
    }
}

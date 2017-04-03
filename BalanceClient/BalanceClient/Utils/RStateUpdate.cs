using System;
using System.Collections.Generic;

namespace Balance.Utils
{
	public class RStateUpdate
	{
		public Vector position;
        public Vector rotation;
        public List<string> animations;
        public String clientId;

        public RStateUpdate() { }

		public RStateUpdate(Vector position, Vector rotation, List<string> animations, String clientId){
			this.position = position;
			this.rotation = rotation;
			this.animations = animations;
			this.clientId = clientId;
		}
	}
}


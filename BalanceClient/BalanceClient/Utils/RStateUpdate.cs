using System;
using System.Collections.Generic;

namespace Balance.Utils
{
	public class RStateUpdate
	{
		private Vector position;
		private Vector rotation;
		private List<string> animations;
		private String clientId;

		public RStateUpdate(Vector position, Vector rotation, List<string> animations, String clientId){
			this.position = position;
			this.rotation = rotation;
			this.animations = animations;
			this.clientId = clientId;
		}

		public Vector Position { 
			get { return position; } 
			set { throw new Exception("cannot set rsu Position of value: " + value); } 
		}

		public Vector Rotation {
			get { return rotation; }
			set { throw new Exception("cannot set rsu Rotation of value: " + value); }
		}

		public List<string> Animations {
			get { return animations; }
			set { throw new Exception("cannot set rsu Animations of value: " + value); }
		}

		public String ClientId {
			get { return clientId; }
			set { throw new Exception("cannot set rsu ClientId of value: " + value); }
		}
	}
}


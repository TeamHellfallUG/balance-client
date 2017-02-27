using System;
using System.Collections.Generic;

namespace Balance.Utils
{
	public class StateUpdate
	{
		public Vector position;
		public Vector rotation;
		public List<string> animations;

		public StateUpdate ()
		{
			this.position = new Vector ();
			this.rotation = new Vector ();
			this.animations = new List<string> ();
		}

		public StateUpdate(Vector position, Vector rotation){
			this.position = position;
			this.rotation = rotation;
			this.animations = new List<string> ();
		}

		public StateUpdate(Vector position, Vector rotation, List<string> animations){
			this.position = position;
			this.rotation = rotation;
			this.animations = animations;
		}

		public void AddAnimation(String animation){
			this.animations.Add (animation);
		}
	}
}


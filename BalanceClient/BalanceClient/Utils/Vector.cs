using System;

namespace Balance.Utils
{
	public class Vector
	{
		public Double x;
		public Double y;
		public Double z;

		public Vector() {}

		public Vector(Double x, Double y, Double z) 
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

        public static Vector Zero()
        {
            return new Vector(0,0,0);
        }
	}
}

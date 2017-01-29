using System;

namespace Balance.Utils
{
	public class Vector
	{
		public Double x;
		public Double y;
		public Double z;

		public Vector()
		{
			x = 0;
			y = 0;
			z = 0;
		}

		public Vector(Double x, Double y, Double z) 
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}
	}
}

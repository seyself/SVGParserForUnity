using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace seyself 
{
	public struct SVGPath
	{
		public string id;
		public Vector3[] points;
		public Vector3 center;
		public SVGOption option;

		public SVGPath(Vector3[] points, string id, SVGOption option)
		{
			this.points = points;
			this.id = id;
			this.option = option;

			if (option.reverse)
			{
				System.Array.Reverse(this.points);
			}
			this.center = new Vector3();
		}
	}
}
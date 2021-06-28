using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace seyself 
{
	public struct SVGCommand
	{
		public string id;
		public string command;
		public float[] path;
		public SVGOption option;

		public SVGCommand(string command, float[] path, string id, SVGOption option)
		{
			this.command = command;
			this.path = path;
			this.id = id;
			this.option = option;
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace seyself 
{
	[RequireComponent(typeof(LineRenderer))]
	public class SVGLineObject : MonoBehaviour
	{
		public LineRenderer renderer { get; private set; }
		public float depthIndex = 0;
		public float timeOffset = 0;
		public bool forceDraw = false;
		public string id;
		// public LineGroup group;
		public SVGPath path;
		public Vector3 center = Vector3.zero;

		void Awake ()
		{
			renderer = GetComponent<LineRenderer>();
			renderer.startColor = Color.white;
			renderer.endColor = Color.white;
			renderer.useWorldSpace = false;
			renderer.positionCount = 0;
		}

		public void ForceDraw()
		{
			forceDraw = true;
		}

		public void Dispose() 
		{
			// MotionObject motion = transform.parent.GetComponent<MotionObject>();
			// motion.OnStart -= ForceDraw;
			// motion.enabled = false;
		}

	}
}

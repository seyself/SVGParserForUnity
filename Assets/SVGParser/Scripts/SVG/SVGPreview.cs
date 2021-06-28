using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace seyself 
{
	public class SVGPreview : SVGPlotter
	{
		
		// override protected bool DrawLine (SVGPath path, LineObject line, int index)
		override protected bool DrawLine (SVGLineObject line)
		{
			SVGPath path = line.path;
			Vector3[] points = path.points;
			float time_ms = (Time.time - _startTime);
			float delay = path.option.delay;
			float time = time_ms - delay + line.timeOffset;
			if (line.forceDraw)
			{
				line.forceDraw = false;
				line.timeOffset = -time - 0.25f;
			}
			float duration = path.option.time + path.option.t_in + path.option.t_out;
			if (time < 0)
			{
				line.renderer.positionCount = 0;
				return false;
			}
			if (_locked && line.timeOffset == 0 && line.forceDraw)
			{
				line.renderer.positionCount = 0;
				return false;
			}

			int numPoints = points.Length;
			int lastIndex = Mathf.FloorToInt(time / path.option.t_in * numPoints);
			if (path.option.t_in == 0) lastIndex = numPoints;
			int startIndex = Mathf.FloorToInt((time - path.option.time - path.option.t_in) / path.option.t_out * numPoints);
			if (path.option.t_out == 0) startIndex = 0;
			int endIndex = Mathf.Min(lastIndex, numPoints);
			int beginIndex = Mathf.Max(startIndex, 0);

			beginIndex = 0;

			int len = endIndex - beginIndex;
			line.renderer.positionCount = len;
			for(int i=beginIndex; i<endIndex; i++) 
			{
				Vector3 pt = points[i];
				pt.y = -pt.y;
				pt *= this.scale;
				line.renderer.SetPosition(i - beginIndex, pt);
			}
			line.renderer.SetPropertyBlock(_props);
			return true;
		}
	}
}

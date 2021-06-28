using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace seyself 
{
	public class SVGConverter
	{
		public SVGConverter()
		{
			
		}

		public static List<Vector3[]> ToArrayList(List<SVGPath> pathList)
		{
			List<Vector3[]> dst = new List<Vector3[]>();

			int len = pathList.Count;
			for(int i=0; i<len; i++)
			{
				SVGPath path = pathList[i];
				Vector3[] points = path.points;
				if (points.Length < 2) continue;
				dst.Add( points );
			}

			return dst;
		}

		public static List<Vector3[]> Scale(List<Vector3[]> pathList, float scale)
		{
			List<Vector3[]> dst = new List<Vector3[]>();

			int len = pathList.Count;
			for(int i=0; i<len; i++)
			{
				Vector3[] points = pathList[i];
				int len2 = points.Length;
				for(int j=0; j<len2; j++)
				{
					Vector3 pt = points[j];
					pt = pt * scale;
					points[j] = pt;
				}
				dst.Add( points );
			}

			return dst;
		}

		public static List<Vector3[]> Flip(List<Vector3[]> pathList, bool flipX, bool flipY)
		{
			List<Vector3[]> dst = new List<Vector3[]>();

			int len = pathList.Count;
			for(int i=0; i<len; i++)
			{
				Vector3[] points = pathList[i];
				int len2 = points.Length;
				for(int j=0; j<len2; j++)
				{
					Vector3 pt = points[j];
					if (flipX) pt.x = -pt.x;
					if (flipY) pt.y = -pt.y;
					points[j] = pt;
				}
				dst.Add( points );
			}

			return dst;
		}

		public static List<Vector3[]> Optimize(List<SVGPath> pathList)
		{
			List<Vector3[]> list = new List<Vector3[]>();

			return list;
		}

		public static List<Vector3[]> Simplify(List<Vector3[]> pathList, float lengthThreshold, float angleThreshold)
		{
			List<Vector3[]> dst = new List<Vector3[]>();

			int len = pathList.Count;
			for(int i=0; i<len; i++)
			{
				Vector3[] points = pathList[i];
				int len2 = points.Length;
				int end = len2 - 1;
				if (len2 == 1) continue;

				List<Vector3> list = new List<Vector3>();
				Vector3 prev = Vector3.zero;
				Vector3 prev2 = Vector3.zero;
				for(int j=0; j<len2; j++)
				{
					Vector3 pt = points[j];
					if (j==0 || j == end)
					{
						prev2 = pt;
						prev = pt;
						list.Add(pt);
					}
					else
					if (j==1)
					{
						list.Add(pt);
						prev2 = prev;
						prev = pt;
					}
					else
					{
						Vector3 p1 = pt - prev;
						Vector3 p2 = prev - prev2;
						float a1 = Mathf.Atan2(p1.y, p1.x) * Mathf.Rad2Deg;
						float a2 = Mathf.Atan2(p2.y, p2.x) * Mathf.Rad2Deg;
						float da = Mathf.Abs( Mathf.DeltaAngle(a2, a1) );
						if (da > angleThreshold)
						{
							list.Add(pt);
							prev2 = prev;
							prev = pt;
						}
						else
						{
							float distance = (pt - prev).magnitude;
							if (distance > lengthThreshold)
							{
								list.Add(pt);
								prev2 = prev;
								prev = pt;
							}
						}
					}
				}

				if (list.Count >= 2)
				{
					dst.Add( list.ToArray() );
				}
			}

			return dst;
		}

		
	}
}
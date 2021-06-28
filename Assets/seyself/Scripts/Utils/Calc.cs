using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace seyself 
{
	public class Calc
	{
		// 3点から法線ベクトルを求める
		public static Vector3 Perp (Vector3 a, Vector3 b, Vector3 c)
		{
			Vector3 side1 = b - a;
			Vector3 side2 = c - a;
			return Vector3.Cross(side1, side2).normalized;
		}

		// value の数値を min から max の範囲で正規化して返す
		public static float Ratio(float min, float max, float value)
		{
			if (value < min) value = min;
			if (value > max) value = max;
			return (value - min) / (max - min);
		}

		// value の数値を min から max の範囲で正規化して、1- で反転させた数値を返す
		public static float InverseRatio(float min, float max, float value)
		{
			return 1.0f - Ratio(min, max, value);
		}

		// 点が多角形の中に入っているかどうかを調べる
		// ref http://edom18.hateblo.jp/entry/2018/11/28/200032
		/// <summary>
		/// 調査点が多角形の内外どちらにあるかを判定する
		/// </summary>
		/// <param name="vertexes">多角形を構成する頂点リスト</param>
		/// <param name="target">調査点</param>
		/// <param name="normal">多角形が存在する平面の法線</param>
		/// <returns>調査点が内側にある場合はtrue</returns>
		public static bool PointInPolygon(Vector3[] vertexes, Vector3 target, Vector3 normal)
		{
			float result = 0;
			for (int i = 0; i < vertexes.Length; i++)
			{
				Vector3 l1 = vertexes[i] - target;
				Vector3 l2 = vertexes[(i + 1) % vertexes.Length] - target;
				float angle = Vector3.Angle(l1, l2);
				Vector3 cross = Vector3.Cross(l1, l2);
				if (Vector3.Dot(cross, normal) < 0)
				{
					angle *= -1;
				}
				result += angle;
			}
			result *= 1f / 360f;
			// 時計回り・反時計回りどちらもありえるため絶対値で判定する
			return Mathf.Abs(result) >= 0.01f;
		}

		// // 3点の外心（外接円の中心点）を求める (2次元)
		// public static Vector3 CenterOfArc2D(Vector3 pt1, Vector3 pt2, Vector3 pt3)
		// {
		// 	float x1 = pt1.x, y1 = pt1.y;
		// 	float x2 = pt2.x, y2 = pt2.y;
		// 	float x3 = pt3.x, y3 = pt3.y;
		// 	float G = ( y2*x1 - y1*x2 + y3*x2 - y2*x3 + y1*x3 - y3*x1 );
		// 	float Xc= ((x1*x1 + y1*y1) * (y2-y3) + (x2*x2 + y2*y2) * (y3-y1) + (x3*x3 + y3*y3) * (y1-y2)) / (2f*G);
		// 	float Yc=-((x1*x1 + y1*y1) * (x2-x3) + (x2*x2 + y2*y2) * (x3-x1) + (x3*x3 + y3*y3) * (x1-x2)) / (2f*G);
		// 	return new Vector3(Xc, Yc);
		// }

		// 3点の外心（外接円の中心点）を求める
		public static Vector3 Circumcenter(Vector3 a, Vector3 b, Vector3 c)
		{
			Vector3 ac = c - a;
			Vector3 ab = b - a;
			Vector3 abXac = Vector3.Cross(ab, ac);
			float acLen2 = ac.x * ac.x + ac.y * ac.y + ac.z * ac.z;
			float abLen2 = ab.x * ab.x + ab.y * ab.y + ab.z * ab.z;
			float abXacLen2 = abXac.x * abXac.x + abXac.y * abXac.y + abXac.z * abXac.z;
			Vector3 toCircumsphereCenter = (Vector3.Cross(abXac, ab) * acLen2 + Vector3.Cross(ac, abXac) * abLen2) / (2f * abXacLen2);
			float circumsphereRadius = toCircumsphereCenter.magnitude;
			Vector3 ccs = a + toCircumsphereCenter;
			return ccs;
		}

		// 3点から法線ベクトルを求めて、法線ベクトルの回転を求める
		public static Quaternion NormalRotation(Vector3 pt1, Vector3 pt2, Vector3 pt3)
		{
			Vector3 axis = Perp(pt1, pt2, pt3);
			if (axis.x == 0 && axis.y == 0 && axis.z == 0) axis.z = 1;
			return Quaternion.LookRotation(axis, Vector3.up);
		}

		// 3点から法線ベクトルを求めて、法線ベクトルの回転を求める
		public static Quaternion NormalRotation(Vector3 axis)
		{
			if (axis.x == 0 && axis.y == 0 && axis.z == 0) axis.z = 1;
			return Quaternion.LookRotation(axis, Vector3.up);
		}

		// 長さが分かっている三辺で構成された三角形ABCの角Bの角度を求める
		float GetAngleOfTriABC (float a, float b, float c) 
		{
			return Mathf.Acos( ((a*a+c*c)-b*b)/(2*a*c) )  / Mathf.PI * 180;
		}

		// // 3つの点(三角形)から外心点(外接円の中心)を求める
		// static Vector3 circumcircle(Vector3 a, Vector3 b, Vector3 c)
		// {
		// 	float x1 = (a.y-c.y)*(a.y*a.y-b.y*b.y+a.x*a.x-b.x*b.x);
		// 	float x2 = (a.y-b.y)*(a.y*a.y-c.y*c.y+a.x*a.x-c.x*c.x);
		// 	float x3 = 2*(a.y-c.y)*(a.x-b.x)-2*(a.y-b.y)*(a.x-c.x);
		// 	float y1 = (a.x-c.x)*(a.x*a.x-b.x*b.x+a.y*a.y-b.y*b.y);
		// 	float y2 = (a.x-b.x)*(a.x*a.x-c.x*c.x+a.y*a.y-c.y*c.y);
		// 	float y3 = 2*(a.x-c.x)*(a.y-b.y)-2*(a.x-b.x)*(a.y-c.y);
		// 	float px = (x1-x2)/x3;
		// 	float py = (y1-y2)/y3;
		// 	return new Vector3(px, py);
		// }
		
		// 2点間の距離を調べる
		static float Length(Vector3 a, Vector3 b)
		{
			return (b - a).magnitude;
		}
		
		// 2点の角度を調べる
		static float Angle(Vector3 a, Vector3 b)
		{
			return Vector3.Angle(a, b);
		}
		
		// 点が線B→Aの右にあるか左にあるか（1=右、-1=左、0=線上）
		static int LineSide(Vector2 pt, Vector2 a, Vector2 b)
		{
			float n = pt.x * (a.y - b.y) + a.x * (b.y - pt.y) + b.x * (pt.y - a.y);
			if (n > 0) return 1;
			if (n < 0) return -1;
			return 0;
		}
		
		// 線分の範囲内にあるか判定
		static bool LineOverlaps(Vector2 pt, Vector2 a, Vector2 b)
		{
			if (((a.x >= pt.x && b.x <= pt.x) || (b.x >= pt.x && a.x <= pt.x)) && ((a.y >= pt.y && b.y <= pt.y) || (b.y >= pt.y && a.y <= pt.y)))
			{
				return true;
			}
			return false;
		}
		
		// 線ABと線CDの角度を調べる
		static float LineAngle(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
		{
			float abX = b.x - a.x;
			float abY = b.y - a.y;
			float cdX = d.x - c.x;
			float cdY = d.y - c.y;
			float r1 = abX * cdX + abY * cdY;
			float r2 = abX * cdY - abY * cdX;
			return Mathf.Atan2(r2, r1);
		}
		
		// 線分ABと線分CDが交差するか調べる
		static bool Intersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
		{
			if (((a.x-b.x)*(c.y-a.y)+(a.y-b.y)*(a.x-c.x)) * ((a.x-b.x)*(d.y-a.y)+(a.y-b.y)*(a.x-d.x)) < 0)
			{
				return (((c.x-d.x)*(a.y-c.y)+(c.y-d.y) * (c.x-a.x))*((c.x-d.x)*(b.y-c.y)+(c.y-d.y)*(c.x-b.x)) < 0);
			}
			return false;
		}
		
		// 直線ABと直線CDの交点を調べる
		static Vector2 CrossPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
		{
			float t1 = a.x-b.x;
			float t2 = c.x-d.x;
			if (t1 == 0) t1 = 0.0000001f;
			if (t2 == 0) t2 = 0.0000001f;
			float ta = (a.y-b.y) / t1;
			float tb = (a.x*b.y - a.y*b.x) / t1;
			float tc = (c.y-d.y) / t2;
			float td = (c.x*d.y - c.y*d.x) / t2;
			float px = (td-tb) / (ta-tc);
			float py = ta * px + tb;
			return new Vector2(px, py);
		}
		
		//直線ABが直線CDに衝突したときの反射角度
		public static float Refrect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
		{
			float aX = b.x-a.x;
			float aY = b.y-a.y;
			float bX = d.x-c.x;
			float bY = d.y-c.y;
			return Mathf.Atan2(aY, aX) + Mathf.PI + Mathf.Atan2(aX*bY-aY*bX, aX*bX+aY*bY) * 2f;
		}
		
		//点ptを線分ABに対して垂直に移動した時に交差する点を調べる
		public static Vector2 PerpPoint(Vector2 pt, Vector2 a, Vector2 b, bool outside=false) 
		{
			Vector2 dest = new Vector2();
			if(a.x == b.x)
			{
				dest.x = a.x;
				dest.y = pt.y;
			}
			else 
			if(a.y == b.y)
			{
				dest.x = pt.x;
				dest.y = a.y;
			}
			else
			{
				float m1 = (b.y - a.y) / (b.x - a.x);
				float b1 = a.y - (m1 * a.x);
				float m2 = -1.0f / m1;
				float b2 = pt.y - (m2 * pt.x);
				dest.x = (b2 - b1) / (m1 - m2);
				dest.y = (b2 * m1 - b1 * m2) / (m1 - m2);
			}
			
			if (outside)
			{
				return dest;
			}
			// 線分の範囲内にあるか判定
			if ((a.x >= dest.x && b.x <= dest.x && a.y >= dest.y && b.y <= dest.y) || (b.x >= dest.x && a.x <= dest.x && b.y >= dest.y && a.y <= dest.y))
			{
				return dest;
			}
			// 範囲外のとき
			return Vector2.zero;
		}
		
		
		// 3次ベジェ曲線の座標を取得する (a=anchorA, b=controlA, c=controlB, d=anchorB, t=[0.0 <= 1.0])
		public static Vector3 BezierCurve(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t) 
		{
			return new Vector3(
				BezierCurve(a.x, b.x, c.x, d.x, t),
				BezierCurve(a.y, b.y, c.y, d.y, t),
				BezierCurve(a.z, b.z, c.z, d.z, t)
			);
		}
		
		// 3次ベジェ曲線の座標を取得する (a=anchorA, b=controlA, c=controlB, d=anchorB, t=[0.0 <= 1.0])
		public static float BezierCurve(float a, float b, float c, float d, float t) 
		{
			float s = 1-t;
			return s*s*s*a + 3*s*s*t*b + 3*s*t*t*c + t*t*t*d;
		}
		
		
		// CatmullRom補間（スプライン曲線）の座標を取得する (a → b → c → d, t=[0.0 <= 1.0])
		public static Vector3 CatmullRom(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t) 
		{
			return new Vector3(
				CatmullRom(a.x, b.x, c.x, d.x, t),
				CatmullRom(a.y, b.y, c.y, d.y, t),
				CatmullRom(a.z, b.z, c.z, d.z, t)
			);
		}
		
		// CatmullRom補間（スプライン曲線）の座標を取得する (a → b → c → d, t=[0.0 <= 1.0])
		public static float CatmullRom(float a, float b, float c, float d, float t) 
		{
			float v0 = (c - a) * 0.5f;
			float v1 = (d - b) * 0.5f;
			float t2 = t * t;
			float t3 = t2 * t;
			return (2*b-2*c+v0+v1) * t3+(-3*b+3*c-2*v0-v1) * t2+v0*t+b;
		}
	}
}
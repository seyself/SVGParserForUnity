using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEngine;

namespace seyself 
{
	public class SVGParser
	{
		List<SVGCommand> _pathList = new List<SVGCommand>();
		Vector3 _position = Vector3.zero;
		Vector3 _control = Vector3.zero;
		Material _material;
		Rect _viewBox;

		public int splitCirclePoints = 100;
		public int splitLinePoints = 20;
		public float curveStep = 0.025f;

		public SVGParser(Material material)
		{
			_material = material;
		}

		public SVGParser()
		{
			
		}

		public List<SVGPath> ParseText(string svgSource) 
		{
			XDocument xml = XDocument.Parse(svgSource);
			return ParseXML(xml);
		}

		public List<SVGPath> Parse(string filePath) 
		{
			XDocument xml = XDocument.Load(filePath);
			return ParseXML(xml);
		}

		public List<SVGPath> ParseXML(XDocument xml) 
		{
			IEnumerable<XElement> xmlList = xml.Elements();
			foreach(XElement item in xmlList)
			{
				if (item.Name.LocalName == "svg")
				{
					_viewBox = GetViewBox(item);
				}
				
				ParseSVGNode(item);
			}

			List<SVGPath> pointsList = GetPointsList();
			List<SVGPath> resultList = new List<SVGPath>();
			int len = pointsList.Count;
			for(int i=0; i<len; i++) 
			{
				SVGPath path = pointsList[i];
				if (path.id.IndexOf("_") == 0) continue;
				
				path.points = OffsetPoints(path.points);
				if (path.option.type == "image")
				{
					path.center = GetCenterOfImage(path.option);
				}
				else
				{
					path.center = GetCenterOfPoints(path.points);
				}
				resultList.Add(path);
			}
			// pointsList.ForEach( path => path.points = OffsetPoints(path.points) );
			// pointsList.ForEach( path => path.center = GetCenterOfPoints(path.points) );
			return resultList;
		}

		Vector3 GetCenterOfPoints(Vector3[] points)
		{
			int len = points.Length;
			int a = 0;
			int b = Mathf.FloorToInt(len / 4);
			int c = Mathf.FloorToInt(len / 8) + b;
			return seyself.Calc.Circumcenter(points[a], points[b], points[c]);
		}

		Vector3 GetCenterOfImage(SVGOption option)
		{
			float vx = -_viewBox.width / 2;
			float vy = -_viewBox.height / 2;
			float x = option.imageX + option.imageWidth / 2 + vx;
			float y = option.imageY + option.imageHeight / 2 + vy;
			return new Vector3(x, y, 0);
		}

		Vector3[] OffsetPoints(Vector3[] points)
		{
			float vx = -_viewBox.width / 2;
			float vy = -_viewBox.height / 2;
			Vector3 offset = new Vector3(vx, vy);
			int len = points.Length;
			for (int i=0; i<len; i++)
			{
				points[i] += offset;
			}
			return points;
		}

		Rect GetViewBox(XElement svg)
		{
			if (svg.Name.LocalName != "svg") return new Rect();

			string viewBox = svg.Attribute("viewBox").Value;
			string[] values = viewBox.Split(' ');
			return new Rect(
				float.Parse(values[0]),
				float.Parse(values[1]),
				float.Parse(values[2]),
				float.Parse(values[3])
			);
		}

		void FlipY(Vector3[] points)
		{
			int len = points.Length;
			for(int i=0; i<len; i++)
			{
				points[i].y = -points[i].y;
			}
		}

		void DrawPointsList(List<Vector3[]> pointsList) 
		{
			int len = pointsList.Count;
			for(int i=0; i<len; i++)
			{
				DrawPoints( pointsList[i] );
			}
		}

		void DrawPoints(Vector3[] points) 
		{
			LineRenderer line = new GameObject().AddComponent<LineRenderer>();
			line.startColor = Color.black;
			line.endColor = Color.black;
			line.startWidth = 1;
			line.endWidth = 1;
			line.material = _material;
			int len = points.Length;
			FlipY(points);
			line.positionCount = len;
			line.SetPositions(points);
		}

		List<SVGPath> GetPointsList()
		{
			Vector3[] points = new Vector3[]{};
			List<SVGPath> pointsList = new List<SVGPath>();
			int len = _pathList.Count;

			string id = null;
			SVGOption option = new SVGOption();
			
			for(int i=0; i<len; i++)
			{
				SVGCommand svgCommand = _pathList[i];
				
				if (svgCommand.command == "M" || svgCommand.command == "m")
				{
					if (points.Length > 0)
					{
						pointsList.Add(new SVGPath(points, id, option));
					}
					else if (option.type == "image")
					{
						pointsList.Add(new SVGPath(points, id, option));
					}
					id = svgCommand.id;
					option = svgCommand.option;
					points = new Vector3[]{};
				}
				if (svgCommand.command == "I")
				{
					if (points.Length > 0)
					{
						pointsList.Add(new SVGPath(points, id, option));
					}
					else if (option.type == "image")
					{
						pointsList.Add(new SVGPath(points, id, option));
					}

					id = svgCommand.id;
					option = svgCommand.option;
					points = new Vector3[]{};
					// pointsList.Add(new SVGPath(points, id, option));
					continue;
				}

				Vector3[] addPoints = GetPoints(svgCommand);
				Vector3[] newPoints = new Vector3[points.Length + addPoints.Length];
				points.CopyTo(newPoints, 0);
				addPoints.CopyTo(newPoints, points.Length);
				points = newPoints;
			}
			if (points.Length > 0)
			{
				pointsList.Add(new SVGPath(points, id, option));
			}
			else 
			if (option.type == "image")
			{
				pointsList.Add(new SVGPath(points, id, option));
			}

			return pointsList;
		}

		void ParseSVGNode(XElement svg)
		{
			if (svg == null) return;

			IEnumerable<XElement> xmlList = svg.Elements();
			foreach(XElement item in xmlList)
			{
				string id = Attr(item, "id", item.Name.LocalName);
				SVGOption option = new SVGOption();
				option.type = item.Name.LocalName;
				option.index = Attr(item, "index", 0);
				option.reverse = Attr(item, "reverse", false);
				option.delay = Attr(item, "delay", 0);
				option.time = Attr(item, "time", 0);
				option.t_in = Attr(item, "in", 0);
				option.t_out = Attr(item, "out", 0);
				option.rotate = Attr(item, "rotate", 0);

				if (item.Name.LocalName == "path")
				{
					XAttribute attr = item.Attribute("d");
					ParsePathAttributeValue(attr.Value, id, option);
				}
				else if (item.Name.LocalName == "circle")
				{
					// <circle class="st0" cx="594.9547729" cy="419.1905212" r="206.5090637"/>
					ParseCircle(item, id, option);
				}
				else if (item.Name.LocalName == "ellipse")
				{
					// <ellipse transform="matrix(0.9870875 -0.1601821 0.1601821 0.9870875 -36.3552895 47.5511322)" 
					//   class="st0" cx="276.7624512" cy="249.2724609" rx="4.979167" ry="4.979157"/>
					ParseEllipse(item, id, option);
				}
				else if (item.Name.LocalName == "rect")
				{
					// <rect x="1168.2526855" y="292.2793579" width="231.9931641" height="54.1723633"/>
					ParseRect(item, id, option);
				}
				else if (item.Name.LocalName == "polygon")
				{
					// <polygon points="873.013916,372.0308228 898.2844238,372.0308228 898.2844238,397.3018188 952.4572754,397.3018188 
					// 	952.4572754,343.1289673 927.1862793,343.1289673 927.1862793,317.550354 901.9152832,317.550354 901.9152832,292.2793579 
					// 	847.4348145,292.2793579 847.4348145,346.4517212 873.013916,346.4517212 		"/>
					ParsePolygon(item, id, option);
				}
				else if (item.Name.LocalName == "polyline")
				{
					// <polyline points="1260.7354736,758.4713135 1259.7723389,778.4641724 1260.902832,778.4641724 
					// 1290.1229248,778.4641724 1291.253418,778.4641724 1290.2929688,758.4840698  "/>
					ParsePolyline(item, id, option);
				}
				else if (item.Name.LocalName == "line")
				{
					// <line class="st0" x1="273.2120667" y1="283.4645691" x2="293.717041" y2="283.4645691"/>
					ParseLine(item, id, option);
				}
				else if (item.Name.LocalName == "image")
				{
					// <image style="overflow:visible;" width="105" height="71" id="obj2" xlink:href="obj2.png"  transform="matrix(1 0 0 1 1584.155 372.5175)"></image>
					ParseImage(item, id, option);
				}
				else if (item.Name.LocalName == "g")
				{
					ParseSVGNode(item);
				}
			}
		}

		bool Attr(XElement item, string name, bool defaultValue)
		{
			var attr = item.Attribute(name);
			if (attr == null) return defaultValue;
			if (attr.Value == "1") return true;
			return false;
		}

		string Attr(XElement item, string name, string defaultValue)
		{
			var attr = item.Attribute(name);
			if (attr == null) return defaultValue;
			return attr.Value;
		}

		float Attr(XElement item, string name, float defaultValue)
		{
			var attr = item.Attribute(name);
			if (attr == null) return defaultValue;
			return float.Parse(attr.Value);
		}

		// Image
		// <image style="overflow:visible;" width="105" height="71" id="obj2" xlink:href="obj2.png"  transform="matrix(1 0 0 1 1584.155 372.5175)"></image>
		void ParseImage(XElement item, string id, SVGOption option)
		{
			XAttribute width = item.Attribute("width");
			XAttribute height = item.Attribute("height");
			XAttribute transform = item.Attribute("transform");
			
			// option.imageSrc = Attr(item, "xlink:href", "");
			option.imageWidth = float.Parse(width.Value);
			option.imageHeight = float.Parse(height.Value);
			
			string d = transform.Value;
			d = new Regex("^\\s+", RegexOptions.Multiline).Replace(d, "");
			d = new Regex("\\s+$", RegexOptions.Multiline).Replace(d, "");
			d = new Regex("^matrix\\(", RegexOptions.Multiline).Replace(d, "");
			d = new Regex("\\)$", RegexOptions.Multiline).Replace(d, "");
			d = new Regex("\\s+", RegexOptions.Multiline).Replace(d, " ");
			d = new Regex("\\s", RegexOptions.Multiline).Replace(d, "/");
			d = new Regex(",", RegexOptions.Multiline).Replace(d, "/");
			string[] values = d.Split('/');

			option.imageScaleX = float.Parse(values[0]);
			option.imageScaleY = float.Parse(values[3]);
			option.imageX = float.Parse(values[4]);
			option.imageY = float.Parse(values[5]);

			SVGCommand cmd = new SVGCommand("I", new float[]{ option.imageX, option.imageY }, id, option);
			_pathList.Add(cmd);
		}

		//polyline
		void ParseLine(XElement item, string id, SVGOption option)
		{
			XAttribute x1 = item.Attribute("x1");
			XAttribute y1 = item.Attribute("y1");
			XAttribute x2 = item.Attribute("x2");
			XAttribute y2 = item.Attribute("y2");
			ParseLine(float.Parse(x1.Value), float.Parse(y1.Value), float.Parse(x2.Value), float.Parse(y2.Value), id, option);
		}

		void ParseLine(float x1, float y1, float x2, float y2, string id, SVGOption option)
		{
			float len = splitLinePoints;
			for(float i=0; i<=len; i++) 
			{
				float x = Mathf.Lerp(x1, x2, i / len);
				float y = Mathf.Lerp(y1, y2, i / len);
				string command = "M";
				if (i > 0) command = "L";
				SVGCommand item = new SVGCommand(command, new float[]{ x, y }, id, option);
				_pathList.Add(item);
			}
		}

		void ParsePolyline(XElement item, string id, SVGOption option)
		{
			// <polyline points="1260.7354736,758.4713135 1259.7723389,778.4641724 1260.902832,778.4641724 
			// 1290.1229248,778.4641724 1291.253418,778.4641724 1290.2929688,758.4840698  "/>
			XAttribute points = item.Attribute("points");
			List<float> list = new List<float>();
			string d = points.Value;
			d = new Regex("^\\s+", RegexOptions.Multiline).Replace(d, "");
			d = new Regex("\\s+$", RegexOptions.Multiline).Replace(d, "");
			d = new Regex("\\s+", RegexOptions.Multiline).Replace(d, " ");
			d = new Regex("\\s", RegexOptions.Multiline).Replace(d, "/");
			d = new Regex(",", RegexOptions.Multiline).Replace(d, "/");
			string[] values = d.Split('/');
			int len = values.Length;
			for(int i=0; i<len; i++)
			{
				list.Add( float.Parse(values[i]) );
			}
			ParsePolyline(list, id, option);
		}

		void ParsePolyline(List<float> points, string id, SVGOption option)
		{
			float len = points.Count;
			for(int i=0; i<len; i+=2) 
			{
				float x = points[i];
				float y = points[i+1];
				string command = "M";
				if (i > 0) command = "L";
				SVGCommand item = new SVGCommand(command, new float[]{ x, y }, id, option);
				_pathList.Add(item);
			}
			// if (len > 1)
			// {
			// 	_pathList.Add( new SVGCommand("L", new float[]{ points[0], points[1] }, id, option) );
			// }
		}

		void ParseEllipse(XElement item, string id, SVGOption option)
		{
			XAttribute cx = item.Attribute("cx");
			XAttribute cy = item.Attribute("cy");
			XAttribute rx = item.Attribute("rx");
			XAttribute ry = item.Attribute("ry");
			ParseEllipse(float.Parse(cx.Value), float.Parse(cy.Value), float.Parse(rx.Value), float.Parse(ry.Value), id, option);
		}

		void ParseEllipse(float cx, float cy, float rx, float ry, string id, SVGOption option)
		{
			float len = splitCirclePoints;
			float rad = Mathf.PI * 2 / len;
			float offset = -Mathf.PI / 2 + (option.rotate * Mathf.Deg2Rad);
			for(float i=0; i<=len; i++) 
			{
				float angle = offset - rad * i;
				float x = cx + Mathf.Cos(angle) * rx;
				float y = cy + Mathf.Sin(angle) * ry;
				string command = "M";
				if (i > 0) command = "L";
				SVGCommand item = new SVGCommand(command, new float[]{ x, y }, id, option);
				_pathList.Add(item);
			}
		}
		
		// <rect x="1168.2526855" y="292.2793579" width="231.9931641" height="54.1723633"/>
		void ParseRect(XElement item, string id, SVGOption option)
		{
			XAttribute x = item.Attribute("x");
			XAttribute y = item.Attribute("y");
			XAttribute width = item.Attribute("width");
			XAttribute height = item.Attribute("height");
			if (x == null) x = new XAttribute("x", 0);
			if (y == null) y = new XAttribute("y", 0);
			if (width == null) width = new XAttribute("width", 0);
			if (height == null) height = new XAttribute("height", 0);
			ParseRect(float.Parse(x.Value), float.Parse(y.Value), float.Parse(width.Value), float.Parse(height.Value), id, option);
		}

		void ParseRect(float x, float y, float width, float height, string id, SVGOption option)
		{
			_pathList.Add( new SVGCommand("M", new float[]{ x, y }, id, option) );
			_pathList.Add( new SVGCommand("L", new float[]{ x + width, y }, id, option) );
			_pathList.Add( new SVGCommand("L", new float[]{ x + width, y + height }, id, option) );
			_pathList.Add( new SVGCommand("L", new float[]{ x, y + height }, id, option) );
			_pathList.Add( new SVGCommand("L", new float[]{ x, y }, id, option) );
		}

		void ParsePolygon(XElement item, string id, SVGOption option)
		{
			// <polygon points="873.013916,372.0308228 898.2844238,372.0308228 898.2844238,397.3018188 952.4572754,397.3018188 
			// 	952.4572754,343.1289673 927.1862793,343.1289673 927.1862793,317.550354 901.9152832,317.550354 901.9152832,292.2793579 
			// 	847.4348145,292.2793579 847.4348145,346.4517212 873.013916,346.4517212 		"/>
			XAttribute points = item.Attribute("points");
			List<float> list = new List<float>();
			string d = points.Value;
			d = new Regex("^\\s+", RegexOptions.Multiline).Replace(d, "");
			d = new Regex("\\s+$", RegexOptions.Multiline).Replace(d, "");
			d = new Regex("\\s+", RegexOptions.Multiline).Replace(d, " ");
			d = new Regex("\\s", RegexOptions.Multiline).Replace(d, "/");
			d = new Regex(",", RegexOptions.Multiline).Replace(d, "/");
			string[] values = d.Split('/');
			int len = values.Length;
			for(int i=0; i<len; i++)
			{
				list.Add( float.Parse(values[i]) );
			}
			ParsePolygon(list, id, option);
		}

		void ParsePolygon(List<float> points, string id, SVGOption option)
		{
			float len = points.Count;
			for(int i=0; i<len; i+=2) 
			{
				float x = points[i];
				float y = points[i+1];
				string command = "M";
				if (i > 0) command = "L";
				SVGCommand item = new SVGCommand(command, new float[]{ x, y }, id, option);
				_pathList.Add(item);
			}
			if (len > 1)
			{
				_pathList.Add( new SVGCommand("L", new float[]{ points[0], points[1] }, id, option) );
			}
		}

		void ParseCircle(XElement item, string id, SVGOption option)
		{
			XAttribute cx = item.Attribute("cx");
			XAttribute cy = item.Attribute("cy");
			XAttribute r = item.Attribute("r");
			ParseCircle(float.Parse(cx.Value), float.Parse(cy.Value), float.Parse(r.Value), id, option);
		}

		void ParseCircle(float cx, float cy, float r, string id, SVGOption option)
		{
			float len = splitCirclePoints;
			float rad = Mathf.PI * 2 / len;
			float offset = -Mathf.PI / 2 + (option.rotate * Mathf.Deg2Rad);
			for(float i=0; i<=len; i++) 
			{
				float angle = offset - rad * i;
				float x = cx + Mathf.Cos(angle) * r;
				float y = cy + Mathf.Sin(angle) * r;
				string command = "M";
				if (i > 0) command = "L";
				SVGCommand item = new SVGCommand(command, new float[]{ x, y }, id, option);
				_pathList.Add(item);
			}
		}


		void ParsePathAttributeValue(string d, string id, SVGOption option)
		{
			d = new Regex("([a-zA-Z])", RegexOptions.Multiline).Replace(d, ",$1,");
			d = new Regex("(\\d)-", RegexOptions.Multiline).Replace(d, "$1,-");
			d = new Regex(",+", RegexOptions.Multiline).Replace(d, ",");
			d = new Regex("-\\.", RegexOptions.Multiline).Replace(d, "-0.");
			d = new Regex(",\\.", RegexOptions.Multiline).Replace(d, ",0.");
			d = new Regex("^,", RegexOptions.Multiline).Replace(d, "");
			d = new Regex(",$", RegexOptions.Multiline).Replace(d, "");
			d = new Regex("(\\n|\\r)", RegexOptions.Multiline).Replace(d, "");
			string[] nums = d.Split(new char[]{','});

			string command = "";
			List<float> path = new List<float>();

			for(int i=0; i<nums.Length; i++)
			{
				string s = nums[i];
				if (s == "") continue;

				// https://triple-underscore.github.io/SVG11/paths.html
				switch(s)
				{
					case "M" : // M（絶対）, m（相対）	moveto	(x y)+
					case "m" : 
					case "L" : // L（絶対）, l（相対） lineto	(x y)+
					case "l" : 
					case "C" : // C（絶対）, c（相対）	curveto	(x1 y1 x2 y2 x y)+
					case "c" : 
					case "S" : // S（絶対）, s（相対）	略式/滑 curveto	(x2 y2 x y)+
					case "s" : 
					case "Q" : // Q（絶対）, q（相対）	二次ベジェ curveto	(x1 y1 x y)+
					case "q" : 
					case "T" : // T（絶対）, t（相対）	略式/滑 二次ベジェ curveto	(x y)+
					case "t" : 
					case "A" : // A（絶対）, a（相対）	楕円弧	(rx ry x-axis-rotation large-arc-flag sweep-flag x y)+
					case "a" : 
					case "Z" : // Z または z	closepath	(なし)
					case "z" : 
					case "V" : // V（絶対）, v（相対） 垂直 lineto x+
					case "v" : 
					case "H" : // H（絶対）, h（相対） 水平 lineto y+
					case "h" : 
					if (command != "")
					{
						SVGCommand item = new SVGCommand(command, path.ToArray(), id, option);
						_pathList.Add(item);
					}

					command = s;
					path.Clear();
					break;

					default : 
					try 
					{
						float n = float.Parse(s);
						path.Add(n);
					}
					catch(System.Exception e)
					{
						// Debug.Log(s);
					}
					break;
				}
			}
			SVGCommand item2 = new SVGCommand(command, path.ToArray(), id, option);
			_pathList.Add(item2);
		}

		public Vector3[] GetPoints(SVGCommand svg)
		{
			List<Vector3> points = new List<Vector3>();
			Vector3 c;
			switch(svg.command)
			{
				case "M" : 
				_position = new Vector3(svg.path[0], svg.path[1]);
				_control = new Vector3(svg.path[0], svg.path[1]);
				points.Add( _position );
				break;
				case "m" : 
				_position = new Vector3(_position.x + svg.path[0], _position.y + svg.path[1]);
				_control = new Vector3(_position.x + svg.path[0], _position.y + svg.path[1]);
				points.Add( _position );
				break;

				case "L" : 
				_position = new Vector3(svg.path[0], svg.path[1]);
				_control = new Vector3(svg.path[0], svg.path[1]);
				points.Add( _position );
				break;
				case "l" : 
				_position = new Vector3(_position.x + svg.path[0], _position.y + svg.path[1]);
				_control = new Vector3(_position.x + svg.path[0], _position.y + svg.path[1]);
				points.Add( _position );
				break;

				case "V" : 
				_control = new Vector3(_position.x, svg.path[0]);
				_position = new Vector3(_position.x, svg.path[0]);
				points.Add( _position );
				break;
				case "v" : 
				_control = new Vector3(_position.x, _position.y + svg.path[0]);
				_position = new Vector3(_position.x, _position.y + svg.path[0]);
				points.Add( _position );
				break;

				case "H" : 
				_control = new Vector3(svg.path[0], _position.y);
				_position = new Vector3(svg.path[0], _position.y);
				points.Add( _position );
				break;
				case "h" : 
				_control = new Vector3(_position.x + svg.path[0], _position.y);
				_position = new Vector3(_position.x + svg.path[0], _position.y);
				points.Add( _position );
				break;
				
				case "C" : 
				for(float t=0; t<1; t+=curveStep) 
				{
					points.Add( new Vector3(
						BezierCurve(_position.x, svg.path[0], svg.path[2], svg.path[4], t),
						BezierCurve(_position.y, svg.path[1], svg.path[3], svg.path[5], t),
						0
					));
				}
				points.Add( new Vector3(
					BezierCurve(_position.x, svg.path[0], svg.path[2], svg.path[4], 1),
					BezierCurve(_position.y, svg.path[1], svg.path[3], svg.path[5], 1),
					0
				));
				_position = new Vector3(svg.path[4], svg.path[5]);
				_control = new Vector3(svg.path[2], svg.path[3]);

				break;

				case "c" : 
				for(float t=0; t<1; t+=curveStep) 
				{
					points.Add( new Vector3(
						BezierCurve(_position.x, _position.x+svg.path[0], _position.x+svg.path[2], _position.x+svg.path[4], t),
						BezierCurve(_position.y, _position.y+svg.path[1], _position.y+svg.path[3], _position.y+svg.path[5], t),
						0
					));
				}
				points.Add( new Vector3(
					BezierCurve(_position.x, _position.x+svg.path[0], _position.x+svg.path[2], _position.x+svg.path[4], 1),
					BezierCurve(_position.y, _position.y+svg.path[1], _position.y+svg.path[3], _position.y+svg.path[5], 1),
					0
				));
				_position = new Vector3(_position.x + svg.path[4], _position.y + svg.path[5]);
				_control = new Vector3(_position.x + svg.path[2], _position.y + svg.path[3]);
				break;

				case "S" : 
				c = _position - (_control - _position);
				for(float t=0; t<1; t+=curveStep) 
				{
					points.Add( new Vector3(
						BezierCurve(_position.x, _position.x, svg.path[0], svg.path[2], t),
						BezierCurve(_position.y, _position.y, svg.path[1], svg.path[3], t),
						0
					));
				}
				points.Add( new Vector3(
					BezierCurve(_position.x, _position.x, svg.path[0], svg.path[2], 1),
					BezierCurve(_position.y, _position.y, svg.path[1], svg.path[3], 1),
					0
				));
				_position = new Vector3(_position.x + svg.path[2], _position.y + svg.path[3]);
				_control = new Vector3(_position.x + svg.path[0], _position.y + svg.path[1]);
				break;

				case "s" : 
				c = _position - (_control - _position);
				for(float t=0; t<1; t+=curveStep) 
				{
					points.Add( new Vector3(
						BezierCurve(_position.x, _position.x, _position.x+svg.path[0], _position.x+svg.path[2], t),
						BezierCurve(_position.y, _position.y, _position.y+svg.path[1], _position.y+svg.path[3], t),
						0
					));
				}
				points.Add( new Vector3(
					BezierCurve(_position.x, _position.x, _position.x+svg.path[0], _position.x+svg.path[2], 1),
					BezierCurve(_position.y, _position.y, _position.y+svg.path[1], _position.y+svg.path[3], 1),
					0
				));

				_position = new Vector3(_position.x + svg.path[2], _position.y + svg.path[3]);
				_control = new Vector3(_position.x + svg.path[0], _position.y + svg.path[1]);
				break;

				case "A" : 
				for(float t=0; t<1; t+=curveStep) 
				{
					points.Add( new Vector3(
						BezierCurve(_position.x, svg.path[0], svg.path[2], svg.path[4], t),
						BezierCurve(_position.y, svg.path[1], svg.path[3], svg.path[5], t),
						0
					));
				}
				points.Add( new Vector3(
					BezierCurve(_position.x, svg.path[0], svg.path[2], svg.path[4], 1),
					BezierCurve(_position.y, svg.path[1], svg.path[3], svg.path[5], 1),
					0
				));
				break;

				case "a" : 
				for(float t=0; t<1; t+=curveStep) 
				{
					points.Add( new Vector3(
						BezierCurve(_position.x, _position.x+svg.path[0], _position.x+svg.path[2], _position.x+svg.path[4], t),
						BezierCurve(_position.y, _position.y+svg.path[1], _position.y+svg.path[3], _position.y+svg.path[5], t),
						0
					));
				}
				points.Add( new Vector3(
					BezierCurve(_position.x, _position.x+svg.path[0], _position.x+svg.path[2], _position.x+svg.path[4], 1),
					BezierCurve(_position.y, _position.y+svg.path[1], _position.y+svg.path[3], _position.y+svg.path[5], 1),
					0
				));
				break;
			}
			return points.ToArray();
		}

		static float BezierCurve(float x1, float x2, float x3, float x4, float t)
		{
			float s = 1f - t;
			return s*s*s * x1 + 3 * s*s*t * x2 + 3 * s*t*t * x3 + t*t*t * x4;
		}
	}
}
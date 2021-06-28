using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace seyself 
{
	public class SVGPlotter : MonoBehaviour
	{
		public delegate void SVGPlotterEvent(SVGPlotter sender);
		public SVGPlotterEvent OnStart;
		public SVGPlotterEvent OnComplete;

		public bool drawEnabled;
		// public string name = "";
		public float scale = 1f;
		public float lineWidth = 1;
		public float alpha = 1;
		public Color color = Color.white;
		public Color emissionColor = Color.black;
		public Material material;
		// public LineGroupContainer container;
		public float depthLength = 0;

		public bool fadeIn;
		public bool fadeOut;


		protected List<SVGPath> _pathList;
		protected MaterialPropertyBlock _props;
		protected List<SVGLineObject> _lines;
		protected bool _isStarted;
		protected bool _isCompleted;
		protected bool _locked;
		protected float _startTime;


		void Start ()
		{
			_props = new MaterialPropertyBlock();
		}

		public void Load(string svgFile) 
		{
			SVGParser parser = new SVGParser(material);
			_pathList = parser.Parse(svgFile);
		}

		public void SetPathList(List<SVGPath> pathList)
		{
			_pathList = pathList;
		}

		public void Unload() 
		{
			if (_pathList != null)
			{
				_pathList.Clear();
			}
			_pathList = null;
		}

		public void Clear() 
		{
			drawEnabled = false;
			if (_lines != null)
			{
				// _lines.ForEach( line => {
				// 	line.group.Remove(line);
				// });
				_lines.Clear();
				_lines = null;
			}
			_isStarted = false;
			_isCompleted = false;
		}

		public void Restart() 
		{
			Clear();
			drawEnabled = true;
		}

		public void Lock() 
		{
			_locked = true;
		}

		void OnDestroy() 
		{
			Clear();
		}
		
		void Update ()
		{
			if (_pathList == null) return;
			if (!drawEnabled) return;

			_props.SetColor("_Color", color);
			_props.SetColor("_EmissionColor", emissionColor);

			Draw();

			// if (_lines != null)
			// {
			// 	DepthUpdate();
			// }
		}

		void DepthUpdate()
		{
			int len = _lines.Count;
			for(int i=0; i<len; i++)
			{
				SVGLineObject line = _lines[i];
				float nz = line.depthIndex * depthLength;
				line.transform.localPosition = new Vector3(0, 0, nz);

				float scale = Camera.main.fieldOfView / (Camera.main.fieldOfView + (nz * 8.5f));
				line.transform.localScale = Vector3.one * (1 / scale);
			}
		}

		void Draw ()
		{
			if (_pathList == null) return;

			int len = _pathList.Count;

			if (_lines == null)
			{
				_startTime = Time.time;
				_lines = new List<SVGLineObject>();
				for(int i=0; i<len; i++)
				{
					SVGPath path = _pathList[i];
					SVGLineObject obj = CreateLineObject( path );
					_lines.Add(obj);
				}
				Debug.Log("Create Lines");
			}

			bool didDraw = false;
			for(int i=0; i<len; i++)
			{
				// didDraw |= DrawLine(_pathList[i], _lines[i], i);
				didDraw |= DrawLine(_lines[i]);
			}

			if (!_isStarted)
			{
				_isStarted = true;
				if (OnStart != null) OnStart(this);
			}
			else
			{
				if (!didDraw && !_isCompleted)
				{
					_isCompleted = true;
					if (OnComplete != null) OnComplete(this);
				}
			}
		}

		SVGLineObject CreateLineObject(SVGPath path)
		{
			SVGLineObject line = new GameObject(name + ":" + path.id).AddComponent<SVGLineObject>();
			// line.gameObject.layer = LayerMask.NameToLayer("NorenRT");
			line.id = path.id;
			line.path = path;
			line.depthIndex = path.option.index;

			line.renderer.startWidth = lineWidth * this.scale;
			line.renderer.endWidth = lineWidth * this.scale;
			line.renderer.material = material;
			line.renderer.positionCount = 0;
			line.renderer.SetPropertyBlock(_props);

			line.center = new Vector3(path.center.x, -path.center.y, 0) * this.scale;

			// container.Add(line);

			// Transform cube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
			// cube.parent = line.transform;
			// cube.localRotation = Quaternion.identity;
			// cube.localScale = Vector3.one * 0.1f;
			// cube.localPosition = new Vector3(path.center.x, -path.center.y, 0) * this.scale;

			return line;
		}

		// virtual protected bool DrawLine (SVGPath path, LineObject line, int index)
		virtual protected bool DrawLine (SVGLineObject line)
		{
			SVGPath path = line.path;
			Vector3[] points = path.points;
			float time_ms = (Time.time - _startTime) * 10f;
			float delay = path.option.delay;
			float time = time_ms - delay + line.timeOffset;
			if (line.forceDraw)
			{
				line.forceDraw = false;
				line.timeOffset = -time - 0.25f;
			}
			float duration = path.option.time + path.option.t_in + path.option.t_out;
			if (time < 0 || time > duration)
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

			int len = endIndex - beginIndex;
			if (len < 0)
			{
				line.renderer.positionCount = 0;
				return false;
			}

			if (fadeIn && endIndex < numPoints)
			{
				color = ColorUtil.ChangeAlpha(color, (float)len / numPoints * alpha);
				_props.SetColor("_Color", color);
				line.renderer.startWidth = (float)len / numPoints * lineWidth * scale;
			}
			else
			if (fadeOut && endIndex == numPoints)
			{
				color = ColorUtil.ChangeAlpha(color, (float)len / numPoints * alpha);
				_props.SetColor("_Color", color);
				line.renderer.startWidth = (float)len / numPoints * lineWidth * scale;
			}
			

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

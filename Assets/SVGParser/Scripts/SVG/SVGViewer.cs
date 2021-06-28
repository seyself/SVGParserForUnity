using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace seyself 
{
	[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
	public class SVGViewer : MonoBehaviour
	{
		MeshFilter _meshFilter;
        MeshRenderer _meshRenderer;
		Mesh _mesh;
		Material _material;
        MaterialPropertyBlock _props;
        List<Vector3> _verteces;
        List<int> _indexes;
		List<SVGPath> _pathList;
		[SerializeField, Range(0, 2)] public float drawScale = 0.005f;

		void Awake ()
		{
			_meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _mesh = new Mesh();
            _props = new MaterialPropertyBlock();
			_mesh.indexFormat = IndexFormat.UInt32;
            _meshFilter.mesh = _mesh;
            if (_material == null) _material = _meshRenderer.material;
			else _meshRenderer.material = _material;
			_meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			_meshRenderer.receiveShadows = false;

			_verteces = new List<Vector3>();
			_indexes = new List<int>();

			_mesh.SetVertices( _verteces );
		}

		void Update ()
		{
			
		}

		public void Draw(List<SVGPath> pathList)
		{
			_pathList = pathList;

			DrawPathAll(pathList);
			MeshUpdate();
		}

		public void Draw(List<Vector3[]> pathList)
		{
			DrawPathAll(pathList);
			MeshUpdate();
		}

		Vector3 Flip(Vector3 v)
		{
			v.y = -v.y;
			return v;
		}

		void DrawPathAll(List<SVGPath> pathList) 
		{
			int len = pathList.Count;
			for(int i=0; i<len; i++)
			{
				DrawPath( pathList[i] );
			}
		}

		void DrawPathAll(List<Vector3[]> pathList) 
		{
			int len = pathList.Count;
			for(int i=0; i<len; i++)
			{
				DrawPath( pathList[i] );
			}
		}

		void DrawPath(SVGPath path)
		{
			float s = drawScale;
			int len = path.points.Length;
			for(int i=1; i<len; i++)
			{
				DrawLine(Flip(path.points[i-1] * s), Flip(path.points[i] * s));
			}
		}

		void DrawPath(Vector3[] points)
		{
			float s = drawScale;
			int len = points.Length;
			for(int i=1; i<len; i++)
			{
				DrawLine(Flip(points[i-1] * s), Flip(points[i] * s));
			}
		}

		void DrawLine(Vector3 begin, Vector3 end) 
		{
			_verteces.Add(begin);
			_verteces.Add(end);
			_indexes.Add(_verteces.Count - 2);
			_indexes.Add(_verteces.Count - 1);
		}

		void MeshUpdate()
		{
			_mesh.SetVertices( _verteces );
			_mesh.SetIndices( _indexes.ToArray(), MeshTopology.Lines, 0);
		}
	}
}

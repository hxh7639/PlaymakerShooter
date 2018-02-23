﻿using UnityEngine;

namespace Dynamite3D.PrefabPlaceNBake{
	[System.Serializable]
	public class ObjectSettings{
		public string name;
		public string path;
		public Mesh renderMesh;
		public string renderMeshPath;
		public Mesh colliderMesh;
		public string colliderMeshPath;

		public bool enablePerObject;
		public LayerMask layerMask = ~0;
		public float density = 0.5f;
		public bool alignToSurface = false;
		public int alignTo = 1;
		public float slopeBias = 90f;

		public bool scaleVar = false;
		public bool scaleUniform = true;
		public float scaleMaxX = 5f;
		public float scaleMinX = 0.1f;
		public float scaleTopX = 2f;
		public float scaleBottomX = 0.5f;
		public float scaleMaxY = 5f;
		public float scaleMinY = 0.1f;
		public float scaleTopY = 2f;
		public float scaleBottomY = 0.5f;
		public float scaleMaxZ = 5f;
		public float scaleMinZ = 0.1f;
		public float scaleTopZ = 2f;
		public float scaleBottomZ = 0.5f;
		public float scaleMax = 5f;
		public float scaleMin = 0.1f;
		public float scaleTop = 2f;
		public float scaleBottom = 0.5f;

		public bool alignToStroke = false;
		public int alignToStrokeAxis = 2;

		public bool rotationVar = false;
		public float rotationMaxX = 180f;
		public float rotationMinX = -180f;
		public float rotationTopX = 5f;
		public float rotationBottomX = 5f;
		public float rotationMaxY = 180f;
		public float rotationMinY = -180f;
		public float rotationTopY = 180f;
		public float rotationBottomY = -180f;
		public float rotationMaxZ = 180f;
		public float rotationMinZ = -180f;
		public float rotationTopZ = 5f;
		public float rotationBottomZ = -5f;

		public bool offsetVar= false;
		public float offsetMaxX = 1f;
		public float offsetMinX = -1f;
		public float offsetTopX = 0f;
		public float offsetBottomX = 0f;
		public float offsetMaxY = 1f;
		public float offsetMinY = -1f;
		public float offsetTopY = 0f;
		public float offsetBottomY = 0f;
		public float offsetMaxZ = 1f;
		public float offsetMinZ = -1f;
		public float offsetTopZ = 0f;
		public float offsetBottomZ = 0f;
		public bool colorVariation = false;

		public Gradient gradient;
	}
}
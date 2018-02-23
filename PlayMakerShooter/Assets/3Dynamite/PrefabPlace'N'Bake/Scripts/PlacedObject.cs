using UnityEngine;
using Dynamite3D.PrefabPlaceNBake;

namespace Dynamite3D.PrefabPlaceNBake{
	[ExecuteInEditMode]
	public class PlacedObject : MonoBehaviour {
		[HideInInspector]
		public int ID;
		[HideInInspector]
		public Collider col;
		[HideInInspector]
		public bool colliderEnabled;
		[HideInInspector]
		public int vertexCount;
		[HideInInspector]
		public PlacedObjectsContainer vertexCountContainer;

		void OnDestroy(){
			vertexCountContainer.vertexCount -= vertexCount;
		}

		void Awake(){
			if (vertexCountContainer)
			vertexCountContainer.vertexCount += vertexCount;
		}

		public void RestoreColliders (){
			if (col)
				col.enabled = colliderEnabled;
		}
	}
}
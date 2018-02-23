using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dynamite3D.PrefabPlaceNBake{
	[System.Serializable]
	public class PlacedObjectsContainer : ScriptableObject{
		[SerializeField] public List<GameObject> placedObjects;
		[SerializeField] public List <int> placedObjectsID;
		[SerializeField] public int vertexCount;
	}
}
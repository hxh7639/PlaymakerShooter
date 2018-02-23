using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Dynamite3D.PrefabPlaceNBake{
	[System.Serializable]
	public class DensitiesContainer : ScriptableObject{
		[SerializeField] public List<float> densities;
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dynamite3D.PrefabPlaceNBake{
	[ExecuteInEditMode]
	public class DroppedObject : MonoBehaviour {

		Rigidbody rb;
		MeshCollider mc;

		// Update is called once per frame
//		void Update () {
//			if (rb.IsSleeping ()) {
//				DestroyImmediate (rb);
//				DestroyImmediate (mc);
//				DestroyImmediate (this);
//			}
//		}
	}
}
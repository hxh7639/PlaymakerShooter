#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using Dynamite3D.PrefabPlaceNBake;

[InitializeOnLoad]
public static class SceneChangeDetector
{
	static string loadedLevel;
	public static EditorWindow ppb;

//	static SceneChangeDetector(){
//		Constructor();
//	}

//	[MenuItem("Detectors/SceneDetector")]
	public static void On(){
		loadedLevel = SceneManager.GetActiveScene ().path;
		EditorApplication.hierarchyWindowChanged += OnHierarchyChange;
	}

	public static void Off(){
		EditorApplication.hierarchyWindowChanged -= OnHierarchyChange;
	}

	static void OnHierarchyChange(){
		if(SceneManager.GetActiveScene ().path != loadedLevel){
			ppb.Close ();
		}
	}
}
#endif
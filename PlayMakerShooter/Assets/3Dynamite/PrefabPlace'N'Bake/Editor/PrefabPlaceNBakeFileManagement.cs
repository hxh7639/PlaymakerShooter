#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using Dynamite3D.PrefabPlaceNBake;
using UnityEditorInternal;
using SFB;

namespace Dynamite3D.PrefabPlaceNBake{
	public partial class PrefabPlaceNBakeWindow : EditorWindow {
		void CreateNewBrush(bool copy){
			PPBSettings settings;
			string windowTitle;
			string brushName;
			string directory;

			if (copy) {
				windowTitle = "Select the location for the copy";
				settings = CreateSettings (selectedBrush);
				brushName = brushesNames[selectedBrush] + " copy";
				directory = Application.dataPath.Replace("Assets", "") + AssetDatabase.GUIDToAssetPath (brushesPaths [selectedBrush]).Replace (brushesNames[selectedBrush] + ".ppbb", "");
			}
			else{
				windowTitle = "Select the location for the new brush";
				settings = new PPBSettings ();
				brushName = "New Brush";
				directory = Application.dataPath;
			}

			string newPath = StandaloneFileBrowser.SaveFilePanel (windowTitle, directory, brushName, "ppbb");
			if (newPath != "") {
				string[] chars = new string[1];
				chars [0] = "Assets";
				newPath = newPath.Split (chars, System.StringSplitOptions.None) [1];
				newPath = "Assets/" + newPath.Substring (1);

				for (int i = 0; i < brushesPaths.Count; i++) {
					if (brushesPaths [i] == AssetDatabase.AssetPathToGUID (newPath)) {
						brushesPaths.RemoveAt (i);
					}
				}
				brushesPaths.Add (newPath);

				string path = brushesPaths [brushesPaths.Count - 1];
				string json = EditorJsonUtility.ToJson (settings);
				System.IO.File.WriteAllText (path, json);

				AssetDatabase.Refresh ();
				AssetDatabase.SaveAssets ();

				brushesPaths [brushesPaths.Count - 1] = AssetDatabase.AssetPathToGUID (brushesPaths [brushesPaths.Count - 1]);
				SaveBrushesPaths ();

				LoadBrushes ();
				selectedBrush = brushes.Count - 1;
				ReadBrush (selectedBrush);
				bakeEnabled = false;
				bakeMessageAccepted = false;
			}
		}

		void LoadBrushesFromDisk(){
			string[] newPaths = StandaloneFileBrowser.OpenFilePanel ("Select the files to import", Application.dataPath, "ppbb", true);

			if (newPaths.Length > 0) {
				string[] chars = new string[1];
				chars [0] = "Assets";

				for (int i = 0; i < newPaths.Length; i++) {
					newPaths [i] = newPaths [i].Split (chars, System.StringSplitOptions.None) [1];
					newPaths [i] = "Assets/" + newPaths [i].Substring (1);

					brushesPaths.Add (AssetDatabase.AssetPathToGUID (newPaths [i]));
				}

				for (int i = 0; i < brushesPaths.Count; i++) {
					for (int o = i + 1; o < brushesPaths.Count; o++) {
						if (brushesPaths [i] == brushesPaths [o]) {
							Debug.Log ("The brush " + AssetDatabase.GUIDToAssetPath (brushesPaths [o]) + " is already in the list");
							brushesPaths.RemoveAt (o);
						}
					}
				}

				SaveBrushesPaths ();
				LoadBrushes ();
				ReadBrush (selectedBrush);
			}
		}

		void LoadBrushes(){
			brushes.Clear ();
			brushesPaths.Clear ();
			brushesNames.Clear ();
			previews.Clear ();
			densitiesContainer.densities.Clear ();

			if (File.Exists (brushesPathsPath)) {
				string jsonPaths = File.ReadAllText (brushesPathsPath);
				PathsContainer pathsContainer = JsonUtility.FromJson<PathsContainer> (jsonPaths);

				for (int i = 0; i < pathsContainer.brushesPaths.Length; i++) {
					if (File.Exists (AssetDatabase.GUIDToAssetPath (pathsContainer.brushesPaths [i]))) {
						brushesPaths.Add (pathsContainer.brushesPaths [i]);
						brushesNames.Add (ExtractFileName (brushesPaths.Count-1));
						string jsonBrush = File.ReadAllText (AssetDatabase.GUIDToAssetPath (brushesPaths [brushesPaths.Count-1]));

						PPBSettings thisBrush = JsonUtility.FromJson<PPBSettings> (jsonBrush);
						brushes.Add (thisBrush);
					} 
					else {
						Debug.LogWarning ("The file with the GUID " + pathsContainer.brushesPaths [i] + " cannot be found. Removed from the list");
					}
				}
			}
			else {
				Debug.LogWarning ("The file BrushesPaths that contains the paths of the brushes files is missing. It will be generated automatically after you add some brushes.");
			}
		}

		void WriteBrush(int id){
			PPBSettings settings = brushes [id];
			string path = AssetDatabase.GUIDToAssetPath (brushesPaths [id]);
			string json = EditorJsonUtility.ToJson (settings);
			System.IO.File.WriteAllText (path, json);
			AssetDatabase.Refresh ();
			AssetDatabase.SaveAssets ();
		}
			
		PPBSettings CreateSettings(int id){
			PPBSettings settings = brushes [id];

			settings.layerMask = layerMask;
			settings.brushSize = brushSize;
			settings.density = density;
			settings.spacing = spacing;
			settings.alignToSurface = alignToSurface;
			settings.alignTo = alignTo;
			settings.slopeBias = slopeBias;

			settings.scaleVar = scaleVar;
			settings.scaleUniform = scaleUniform;
			settings.scaleMaxX = scaleMaxX;
			settings.scaleMinX = scaleMinX;
			settings.scaleTopX = scaleTopX;
			settings.scaleBottomX = scaleBottomX;
			settings.scaleMaxY = scaleMaxY;
			settings.scaleMinY = scaleMinY;
			settings.scaleTopY = scaleTopY;
			settings.scaleBottomY = scaleBottomY;
			settings.scaleMaxZ = scaleMaxZ;
			settings.scaleMinZ = scaleMinZ;
			settings.scaleTopZ = scaleTopZ;
			settings.scaleBottomZ = scaleBottomZ;
			settings.scaleMax = scaleMax;
			settings.scaleMin = scaleMin;
			settings.scaleTop = scaleTop;
			settings.scaleBottom = scaleBottom;

			settings.alignToStroke = alignToStroke;
			settings.alignToStrokeAxis = alignToStrokeAxis;

			settings.rotationVar = rotationVar;
			settings.rotationMaxX = rotationMaxX;
			settings.rotationMinX = rotationMinX;
			settings.rotationTopX = rotationTopX;
			settings.rotationBottomX = rotationBottomX;
			settings.rotationMaxY = rotationMaxY;
			settings.rotationMinY = rotationMinY;
			settings.rotationTopY = rotationTopY;
			settings.rotationBottomY = rotationBottomY;
			settings.rotationMaxZ = rotationMaxZ;
			settings.rotationMinZ = rotationMinZ;
			settings.rotationTopZ = rotationTopZ;
			settings.rotationBottomZ = rotationBottomZ;

			settings.offsetVar = offsetVar;
			settings.offsetMaxX = offsetMaxX;
			settings.offsetMinX = offsetMinX;
			settings.offsetTopX = offsetTopX;
			settings.offsetBottomX = offsetBottomX;
			settings.offsetMaxY = offsetMaxY;
			settings.offsetMinY = offsetMinY;
			settings.offsetTopY = offsetTopY;
			settings.offsetBottomY = offsetBottomY;
			settings.offsetMaxZ = offsetMaxZ;
			settings.offsetMinZ = offsetMinZ;
			settings.offsetTopZ = offsetTopZ;
			settings.offsetBottomZ = offsetBottomZ;
			settings.colorVariation = colorVariation;

			settings.gradient = gradientContainer.gradient;

			settings.physDropHeight = physDropHeight;
			settings.gravity = gravity;

			settings.bakeEnabled = bakeEnabled;
			settings.bakeMaterial = bakeMaterial;
			settings.bakeMeshes = bakeMeshes;
			settings.generateLMUvs = generateLMUvs;
			settings.useExistingUv2 = useExistingUv2;
			settings.generateBakedLod = generateBakedLod;
			settings.groupByObject = groupByObject;
			settings.makeStatic = makeStatic;

			settings.bakedMaterialGUID = brushes [selectedBrush].bakedMaterialGUID;

			return settings;
		}

		void ReadBrush(int id){
			CleanUp ();

			PPBSettings settings = brushes [id];

			RefreshPreviewsAndDensities (settings);

			layerMask = settings.layerMask;		tempLayerMask = layerMask;
			brushSize = settings.brushSize;		tempBrushSize = brushSize;
			density = settings.density;		tempDensity = density;
			spacing = settings.spacing;		tempSpacing = spacing;
			alignToSurface = settings.alignToSurface;		tempAlignToSurface = alignToSurface;
			alignTo = settings.alignTo;		tempAlignTo = alignTo;
			slopeBias = settings.slopeBias;		tempSlopeBias = slopeBias;

			scaleVar = settings.scaleVar;		tempScaleVar = scaleVar;
			scaleUniform = settings.scaleUniform;		tempScaleUniform = scaleUniform;
			scaleMaxX = settings.scaleMaxX;		tempScaleMaxX = scaleMaxX;
			scaleMinX = settings.scaleMinX;		tempScaleMinX = scaleMinX;
			scaleTopX = settings.scaleTopX;		//tempScaleTopX = scaleTopX;
			scaleBottomX = settings.scaleBottomX;		//tempScaleBottomX = scaleBottomX;
			scaleMaxY = settings.scaleMaxY;		tempScaleMaxY = scaleMaxY;
			scaleMinY = settings.scaleMinY;		tempScaleMinY = scaleMinY;
			scaleTopY = settings.scaleTopY;		//tempScaleTopY = scaleTopY;
			scaleBottomY = settings.scaleBottomY;		//tempScaleBottomY = scaleBottomY;
			scaleMaxZ = settings.scaleMaxZ;		tempScaleMaxZ = scaleMaxZ;
			scaleMinZ = settings.scaleMinZ;		tempScaleMinZ = scaleMinZ;
			scaleTopZ = settings.scaleTopZ;		//tempScaleTopZ = scaleTopZ;
			scaleBottomZ = settings.scaleBottomZ;		//tempScaleBottomZ = scaleBottomZ;
			scaleMax = settings.scaleMax;		tempScaleMax = scaleMax;
			scaleMin = settings.scaleMin;		tempScaleMin = scaleMin;
			scaleTop = settings.scaleTop;		//tempScaleTop = scaleTop;
			scaleBottom = settings.scaleBottom;		//tempScaleBottom = scaleBottom;

			alignToStroke = settings.alignToStroke;		tempAlignToStroke = alignToStroke;
			alignToStrokeAxis = settings.alignToStrokeAxis;		tempAlignToStrokeAxis = alignToStrokeAxis;

			rotationVar = settings.rotationVar;		tempRotationVar = rotationVar;
			rotationMaxX = settings.rotationMaxX;		tempRotationMaxX = rotationMaxX;
			rotationMinX = settings.rotationMinX;		tempRotationMinX = rotationMinX;
			rotationTopX = settings.rotationTopX;		//tempRotationTopX = rotationTopX;
			rotationBottomX = settings.rotationBottomX;		//tempRotationBottomX = rotationBottomX;
			rotationMaxY = settings.rotationMaxY;		tempRotationMaxY = rotationMaxY;
			rotationMinY = settings.rotationMinY;		tempRotationMinY = rotationMinY;
			rotationTopY = settings.rotationTopY;		//tempRotationTopY = rotationTopY;
			rotationBottomY = settings.rotationBottomY;		//tempRotationBottomY = rotationBottomY;
			rotationMaxZ = settings.rotationMaxZ;		tempRotationMaxZ = rotationMaxZ;
			rotationMinZ = settings.rotationMinZ;		tempRotationMinZ = rotationMinZ;
			rotationTopZ = settings.rotationTopZ;		//tempRotationTopZ = rotationTopZ;
			rotationBottomZ = settings.rotationBottomZ;		//tempRotationBottomZ = rotationBottomZ;

			offsetVar = settings.offsetVar;		tempOffsetVar = offsetVar;
			offsetMaxX = settings.offsetMaxX;		tempOffsetMaxX = offsetMaxX;
			offsetMinX = settings.offsetMinX;		tempOffsetMinX = offsetMinX;
			offsetTopX = settings.offsetTopX;		//tempOffsetTopX = offsetTopX;
			offsetBottomX = settings.offsetBottomX;		//tempOffsetBottomX = offsetBottomX;
			offsetMaxY = settings.offsetMaxY;		tempOffsetMaxY = offsetMaxY;
			offsetMinY = settings.offsetMinY;		tempOffsetMinY = offsetMinY;
			offsetTopY = settings.offsetTopY;		//tempOffsetTopY = offsetTopY;
			offsetBottomY = settings.offsetBottomY;		//tempOffsetBottomY = offsetBottomY;
			offsetMaxZ = settings.offsetMaxZ;		tempOffsetMaxZ = offsetMaxZ;
			offsetMinZ = settings.offsetMinZ;		tempOffsetMinZ = offsetMinZ;
			offsetTopZ = settings.offsetTopZ;		//tempOffsetTopZ = offsetTopZ;
			offsetBottomZ = settings.offsetBottomZ;		//tempOffsetBottomZ = offsetBottomZ;
			colorVariation = settings.colorVariation;		tempColorVariation = colorVariation;

			gradientContainer = ScriptableObject.CreateInstance<GradientContainer>();
			gradientContainer.gradient = new Gradient ();

			gradientContainer.gradient = settings.gradient;

			serializedGradientContainer = new SerializedObject (gradientContainer);
			serializedGradient = serializedGradientContainer.FindProperty ("gradient");

			physDropHeight = settings.physDropHeight;		tempPhysDropHeight = physDropHeight;
			gravity = settings.gravity;		tempGravity = gravity;

			bakeEnabled = settings.bakeEnabled;		tempBakeEnabled = bakeEnabled;
			bakeMaterial = settings.bakeMaterial;		tempBakeMaterial = bakeMaterial;
			bakeMeshes = settings.bakeMeshes;		tempBakeMeshes = bakeMeshes;
			generateLMUvs = settings.generateLMUvs;		tempGenerateLMUvs = generateLMUvs;
			useExistingUv2 = settings.useExistingUv2;		tempUseExistingUv2 = useExistingUv2;
			generateBakedLod = settings.generateBakedLod;		tempGenerateBakedLod = generateBakedLod;
			groupByObject = settings.groupByObject;		tempGroupByObject = groupByObject;
			makeStatic = settings.makeStatic;		tempMakeStatic = makeStatic;

			bakedMaterial = (Material)AssetDatabase.LoadAssetAtPath (AssetDatabase.GUIDToAssetPath (settings.bakedMaterialGUID), typeof(Material));
			if (!bakedMaterial && bakeEnabled) {
				Debug.LogWarning("Baked Material for the brush " + brushesNames[selectedBrush] + " is missing. Bake mode disabled.");
				bakeEnabled = false;
			}

			if (bakeEnabled) {
				bakeMessageAccepted = true;
			} 
			else {
				bakeMessageAccepted = false;
			}
			if (colorVariation) {
				vertexColorMessageAccepted = true;
			}
			else {
				vertexColorMessageAccepted = false;
			}
		}

		string ExtractFileName(int id){
			string path = AssetDatabase.GUIDToAssetPath (brushesPaths [id]);
			path = Path.GetFileName (path);
			string[] chars = new string[1]; chars [0] = ".";
			path = path.Split (chars, System.StringSplitOptions.None)[0];
			return path;
		}

		void SaveBrushesPaths(){
			PathsContainer pathsContainer = new PathsContainer ();
			pathsContainer.brushesPaths = brushesPaths.ToArray ();
			string jsonPaths = JsonUtility.ToJson (pathsContainer);
			System.IO.File.WriteAllText (brushesPathsPath, jsonPaths);
		}

		void RemoveBrush (){
			Debug.Log (brushesNames [selectedBrush] + " removed from the list. The associated file persist in your proyect for further uses.");
			brushesPaths.RemoveAt (selectedBrush);
			selectedBrush -= 1;
			if (selectedBrush == -1) {
				selectedBrush = 0;
			}
			SaveBrushesPaths ();
			LoadBrushes ();
			if (brushes.Count > 0) {
				ReadBrush (selectedBrush);
			}
		}

		void NewObject(Object dragged_object){
			GameObject go = dragged_object as GameObject;

			//TODO Cuidao que estás cogiendo las mallas y el material pa mierda
			if (go.GetComponent<MeshFilter> ()) {
				brushes [selectedBrush].objects.Add (new ObjectSettings ());
				brushes [selectedBrush].objects [brushes [selectedBrush].objects.Count - 1].path = AssetDatabase.GetAssetPath (dragged_object);
				brushes [selectedBrush].objects [brushes [selectedBrush].objects.Count - 1].renderMesh = go.GetComponent<MeshFilter> ().sharedMesh;
				brushes [selectedBrush].objects [brushes [selectedBrush].objects.Count - 1].renderMeshPath =  AssetDatabase.GetAssetPath (go.GetComponent<MeshFilter> ().sharedMesh);

				densitiesContainer.densities.Add (0.5f);
				previews.Add (AssetPreview.GetAssetPreview (dragged_object));
				brushes[selectedBrush].objects [brushes[selectedBrush].objects.Count - 1].name = dragged_object.name;

				if (go.GetComponent<MeshCollider> ()) {
					brushes [selectedBrush].objects [brushes [selectedBrush].objects.Count - 1].colliderMesh = go.GetComponent<MeshCollider> ().sharedMesh;
					brushes [selectedBrush].objects [brushes [selectedBrush].objects.Count - 1].colliderMeshPath = AssetDatabase.GetAssetPath (go.GetComponent<MeshCollider> ().sharedMesh);
				}
				else {
					Debug.Log ("The prefab has no MeshCollider. If you bake this object, it will not have collider.", dragged_object);
				}

				RefreshPreviewsAndDensities (brushes [selectedBrush]);
			}
			else {
				Debug.Log ("The prefab has no MeshFilter. Nothing added.", dragged_object);
			}
		}

		public void RemoveObject(int id){
			brushes [selectedBrush].objects.RemoveAt (id);
			densitiesContainer.densities.RemoveAt (id);
			RefreshPreviewsAndDensities (brushes [selectedBrush]);
		}

		void RefreshPreviewsAndDensities(PPBSettings settings){
			previews.Clear ();
			densitiesContainer.densities.Clear ();
			foreach (ObjectSettings ob in settings.objects) {
				Object myObject = AssetDatabase.LoadAssetAtPath<Object> (ob.path) as Object;
				previews.Add (AssetPreview.GetAssetPreview(myObject));
				densitiesContainer.densities.Add (ob.density);
			}
		}

		void SaveDensities(){
			for (int o = 0; o < brushes [selectedBrush].objects.Count; o++) {
				brushes[selectedBrush].objects[o].density = densitiesContainer.densities[o];
			}
		}
	}
}
#endif
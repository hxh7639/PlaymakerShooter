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
		[MenuItem ("Tools/3Dynamite/PrefabPlace'N'Bake")]
		public static void Init () {
			PrefabPlaceNBakeWindow myWindow = (PrefabPlaceNBakeWindow)EditorWindow.GetWindow(typeof (PrefabPlaceNBakeWindow));
			myWindow.maxSize = new Vector2(840f, 585f);
			myWindow.minSize = new Vector2(300f, 0f);
			myWindow.Show();
			myWindow.autoRepaintOnSceneChange = true;
			GUIContent content = new GUIContent ("Place & Bake!");
			myWindow.titleContent = content;
			EditorApplication.modifierKeysChanged -= myWindow.Repaint;
			EditorApplication.modifierKeysChanged += myWindow.Repaint;
			EditorApplication.playModeStateChanged -= myWindow.PlayModeChanged;
			EditorApplication.playModeStateChanged += myWindow.PlayModeChanged;
			SceneView.onSceneGUIDelegate -= myWindow.OnSceneGUI;
			SceneView.onSceneGUIDelegate += myWindow.OnSceneGUI;
			Undo.undoRedoPerformed -= myWindow.VertexCount;
			Undo.undoRedoPerformed += myWindow.VertexCount;
		}

		void VertexCount(){
//			if (bakeEnabled){
//				FindPlacedObjects ();
//				placedObjectsContainer.vertexCount = 0;
//				for (int u = 0; u < placedObjectsContainer.placedObjects.Count; u++) {
//					if (placedObjectsContainer.placedObjects [u])
//						placedObjectsContainer.vertexCount += objectVertexCount [placedObjectsContainer.placedObjectsID [u]];				
//				}
//			}
		}

		void PlayModeChanged(PlayModeStateChange state){
			this.Close ();
		}

		void Awake(){
			Tools.current = Tool.None;

			SceneChangeDetector.On ();
			SceneChangeDetector.ppb = this;

			guiSkin = (GUISkin)AssetDatabase.LoadAssetAtPath("Assets/3Dynamite/PrefabPlace'N'Bake/GuiSkins/GuiSkins/GuiSkin_1.guiskin", typeof(GUISkin));
			brush = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/3Dynamite/PrefabPlace'N'Bake/GuiSkins/Icons/Brush.psd", typeof(Texture2D));
			arrow = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/3Dynamite/PrefabPlace'N'Bake/GuiSkins/Icons/Arrow.psd", typeof(Texture2D));
			settings = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/3Dynamite/PrefabPlace'N'Bake/GuiSkins/Icons/Settings.psd", typeof(Texture2D));
			objects = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/3Dynamite/PrefabPlace'N'Bake/GuiSkins/Icons/Objects.psd", typeof(Texture2D));

			gradientContainer = ScriptableObject.CreateInstance<GradientContainer>();
			gradientContainer.gradient = new Gradient ();

			serializedGradientContainer = new SerializedObject(gradientContainer);
			serializedGradient = serializedGradientContainer.FindProperty ("gradient");

			densitiesContainer = ScriptableObject.CreateInstance<DensitiesContainer> ();
			densitiesContainer.densities = new List<float> ();

			serializedDensitiesContainer = new SerializedObject (densitiesContainer);
			serializedDensities = serializedDensitiesContainer.FindProperty ("densities");

			placedObjectsContainer = ScriptableObject.CreateInstance<PlacedObjectsContainer> ();
			placedObjectsContainer.placedObjects = new List<GameObject> ();
			placedObjectsContainer.placedObjectsID = new List<int> ();
		
			sectionColor = new Color (0f, 0f, 0f, 0.09f);
			sectionLimitColor = new Color (0f, 0f, 0f, 0.4f);
			subSectionColor = new Color (1f, 1f, 1f, 0.2f);

			LoadBrushes ();
			if (brushes.Count > 0) {
				ReadBrush (0);
				selectedBrush = 0;
				bakedMaterial = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(brushes[selectedBrush].bakedMaterialGUID)) as Material;
				RefreshPreviewsAndDensities (brushes [selectedBrush]);
			}

			//Estas líneas deberían estar en el fichero PrefabPlaceNBakeBehaviour
			brushQuad = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/3Dynamite/PrefabPlace'N'Bake/GraphicResources/Quad.fbx") as Mesh;
//			brushMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/3Dynamite/PrefabPlace'N'Bake/GraphicResources/BrushHalo.mat") as Material;
//			eraseMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/3Dynamite/PrefabPlace'N'Bake/GraphicResources/Eraser.mat") as Material;
//			precisionMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/3Dynamite/PrefabPlace'N'Bake/GraphicResources/Precision.mat") as Material;
			brushMatProj = AssetDatabase.LoadAssetAtPath<Material>("Assets/3Dynamite/PrefabPlace'N'Bake/GraphicResources/BrushHaloProj.mat") as Material;
			eraseMatProj = AssetDatabase.LoadAssetAtPath<Material>("Assets/3Dynamite/PrefabPlace'N'Bake/GraphicResources/EraserProj.mat") as Material;
			precisionMatProj = AssetDatabase.LoadAssetAtPath<Material>("Assets/3Dynamite/PrefabPlace'N'Bake/GraphicResources/PrecisionProj.mat") as Material;
			dropMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/3Dynamite/PrefabPlace'N'Bake/GraphicResources/Drop.mat") as Material;

			originalVertices [0] = new Vector3 (0.5f, -0.5f, 0f);
			originalVertices [1] = new Vector3 (0.5f, 0.5f, 0f);
			originalVertices [2] = new Vector3 (-0.5f, 0.5f, 0f);
			originalVertices [3] = new Vector3 (-0.5f, -0.5f, 0f);

			oldAutoSimulation = Physics.autoSimulation;
			oldGravity = Physics.gravity;

			projector = new GameObject ();
			projectorComp = projector.AddComponent<Projector> ();
			projectorComp.material = brushMatProj;
			projectorComp.orthographic = true;
			projectorComp.orthographicSize = brushSize;
			projectorComp.farClipPlane = 6f;
			projectorComp.nearClipPlane = -6f;
			projector.hideFlags = HideFlags.HideAndDontSave;
		}

		void OnDestroy(){
			if (brushes.Count>0){
				SaveDensities ();
				brushes[selectedBrush] = CreateSettings (selectedBrush);
				WriteBrush (selectedBrush);
			}
			SaveBrushesPaths ();
			EditorApplication.modifierKeysChanged -= this.Repaint;
			EditorApplication.playModeStateChanged -= this.PlayModeChanged;
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			Undo.undoRedoPerformed -= this.VertexCount;

			CleanUp ();

			Physics.autoSimulation = oldAutoSimulation;
			Physics.gravity = oldGravity;

			SceneChangeDetector.Off ();

			DestroyImmediate (projector);
		}

		void OnGUI () {
				RefreshTabs ();
				if (!objectWindow) {
					guiDisabled = false;
				}

				if (alignTo == alignToStrokeAxis && alignToSurface && alignToStroke) {
					alignToStrokeAxis += 1;
					tempAlignToStrokeAxis = alignToStrokeAxis;
					if (alignToStrokeAxis == 3) {
						alignToStrokeAxis = 0;
						tempAlignToStrokeAxis = alignToStrokeAxis;

					}
					Debug.LogWarning ("You cannot align to surface and to stroke the same axis. Stroke Alignment Axis was changed automatically.");
				}

				EditorGUI.BeginDisabledGroup (guiDisabledByMessageBox);
				EditorGUI.BeginDisabledGroup (guiDisabled);
				EditorGUI.BeginChangeCheck ();

				brushesMainScroll = GUI.BeginScrollView (new Rect (0f, 0f, brushesColumnWidth + 24f, this.position.height), brushesMainScroll, new Rect (0f, 0f, brushesColumnWidth, this.maxSize.y - 16f));

				GUILayout.BeginArea (new Rect (5f, 5f, brushesColumnWidth * brushesColumnFactor, columnHeight + 38f));
				GUI.Box (new Rect (0f, 0f, brushesColumnWidth * brushesColumnFactor, 32f), "", guiSkin.GetStyle ("Tab"));
				GUI.DrawTexture (new Rect (8f, 4f, 24f, 24f), brush);
				GUI.Label (new Rect (0, 0, brushesColumnWidth, 32f), "Brushes", guiSkin.GetStyle ("Title"));

				if (brushesColumnFactor > 0.15f) {
					GUI.Box (new Rect (0, 38f, brushesColumnWidth, columnHeight), "", guiSkin.GetStyle ("Box"));
					brushesScroll = GUI.BeginScrollView (new Rect (0f, 40f, brushesColumnWidth, columnHeight - 138f), brushesScroll, new Rect (0f, 0f, brushesColumnWidth - 16f, brushes.Count * 60f + 10f));
					GUILayout.BeginArea (new Rect (5f, 10f, brushesColumnWidth - 26f, brushes.Count * 60f + 10f));
					DrawBrushesContent ();
					GUILayout.EndArea ();
					GUI.EndScrollView ();
					EditorGUI.DrawRect (new Rect (0, columnHeight - 98f, brushesColumnWidth, 2f), sectionLimitColor);
					EditorGUI.DrawRect (new Rect (0, columnHeight - 98f, brushesColumnWidth, 528f), sectionColor);
					GUILayout.BeginArea (new Rect (5f, columnHeight - 91f, brushesColumnWidth - 12f, columnHeight));
					DrawBrushesButtons ();
					GUILayout.EndArea ();
				}
				if (brushesColumnFactor < 1f) {
					Color hideColor = new Color (0.761f, 0.761f, 0.761f, Mathf.Lerp (0f, 1f, 0.15f / brushesColumnFactor));
					EditorGUI.DrawRect (new Rect (0, 38f, brushesColumnWidth, columnHeight), hideColor);
				}
				GUILayout.EndArea ();

				EditorGUI.DrawRect (new Rect (brushesColumnWidth + 18f, 0f, 2f, this.position.height), Color.gray);

				GUI.EndScrollView ();

			if (brushes.Count > 0) {
				mainScroll = GUI.BeginScrollView (new Rect (brushesColumnWidth + 20f, 0f, this.position.width - (brushesColumnWidth + 20f), this.position.height), mainScroll, new Rect (0f, 0f, Mathf.Lerp (33f, objectsColumnWidth, objectsColumnFactor) + bakeColumnWidth + 35f, this.maxSize.y - 16f));

				GUI.Box (new Rect (10f, 5f, brushesColumnWidth * brushesColumnFactor + 15f, 32f), "", guiSkin.GetStyle ("Tab"));
				GUI.DrawTexture (new Rect (18f, 9f, 24f, 24f), settings);
				GUI.Label (new Rect (10f, 5f, brushesColumnWidth + 15f, 32f), "Settings", guiSkin.GetStyle ("Title"));

				settingsScroll = GUI.BeginScrollView (new Rect (10f, 45f, 270f, this.position.height - 68f), settingsScroll, new Rect (brushesColumnWidth + 30f, 0f, 240f, Mathf.Lerp (30f, sectionPaintHeight, paintColumnFactor) + Mathf.Lerp (30f, sectionBakeHeight, bakeColumnFactor) + 15f));

				GUI.BeginGroup (new Rect (brushesColumnWidth + 30f, 5f, 250f, Mathf.Lerp (30f, sectionPaintHeight, paintColumnFactor)));

//					GUI.Box (new Rect (0f, 5f, paintColumnWidth - 6f, 25f), "", guiSkin.GetStyle ("Button"));
				//				GUI.DrawTexture (new Rect (8f, 3f, 24f, 24f), brush);
				if (GUI.Button (new Rect (0, 0f, paintColumnWidth - 6f, 25f), "Paint Settings", guiSkin.GetStyle ("Button"))) {
					if (!paintColumnCollapsed) {
						paintColumnFactor = 0.95f;
					} else {
						paintColumnFactor = 0.05f;
					}
				}
				GUI.Box (new Rect (0f, 32f, paintColumnWidth - 6f, sectionPaintHeight - 32f), "", guiSkin.GetStyle ("Box"));

				#region PaintBackground
				sectionPaintHeight = 30f;

				if (!alignToSurface) {
					EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 3f, paintColumnWidth, 1f), sectionLimitColor);
					EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 5f, paintColumnWidth, 170f), sectionColor);
					sectionPaintHeight += 175f;
				} else {
					EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 3f, paintColumnWidth, 1f), sectionLimitColor);
					EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 5f, paintColumnWidth, 200f), sectionColor);
					EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 147f, paintColumnWidth, 25f), subSectionColor);
					sectionPaintHeight += 204f;
				}

				EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 10f, paintColumnWidth, 1f), sectionLimitColor);
				EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 12f, paintColumnWidth, 20f), sectionColor);

				if (scaleVar) {
					EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 32f, paintColumnWidth, 80f), sectionColor);
					EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 62f, paintColumnWidth, 46f), subSectionColor);
					sectionPaintHeight += 80f;

					if (!scaleUniform) {
						EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 32f, paintColumnWidth, 105f), sectionColor);
						EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 82f, paintColumnWidth, 46f), subSectionColor);
						sectionPaintHeight += 129f;
					} else {
						sectionPaintHeight += 29f;
					}
				} else {
					sectionPaintHeight += 39f;
				}

				EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 10f, paintColumnWidth, 1f), sectionLimitColor);
				EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 12f, paintColumnWidth, 20f), sectionColor);

				if (alignToStroke) {
					EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 32f, paintColumnWidth, 35f), sectionColor);
					EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 36f, paintColumnWidth, 25f), subSectionColor);
					sectionPaintHeight += 65f;
				} else {
					sectionPaintHeight += 38f;
				}

				EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 10f, paintColumnWidth, 1f), sectionLimitColor);
				EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 12f, paintColumnWidth, 20f), sectionColor);

				if (rotationVar) {
					EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 32f, paintColumnWidth, 155f), sectionColor);
					EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 32f, paintColumnWidth, 46f), subSectionColor);
					EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 132f, paintColumnWidth, 46f), subSectionColor);
					sectionPaintHeight += 180f;
				} else {
					sectionPaintHeight += 38f;
				}

				EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 10f, paintColumnWidth, 1f), sectionLimitColor);
				EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 12f, paintColumnWidth, 20f), sectionColor);

				if (offsetVar) {
					EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 32f, paintColumnWidth, 155f), sectionColor);
					EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 32f, paintColumnWidth, 46f), subSectionColor);
					EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 132f, paintColumnWidth, 46f), subSectionColor);
					sectionPaintHeight += 180f;
				} else {
					sectionPaintHeight += 38f;
				}

				EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 10f, paintColumnWidth, 1f), sectionLimitColor);
				EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 12f, paintColumnWidth, 20f), sectionColor); 

				if (colorVariation) {
					EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 32f, paintColumnWidth, 25f), sectionColor);
					sectionPaintHeight += 60f;
				} else {
					sectionPaintHeight += 40f;
				}

				EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 4f, paintColumnWidth, 1f), sectionLimitColor);
				EditorGUI.DrawRect (new Rect (0f, sectionPaintHeight + 6f, paintColumnWidth, 80f), sectionColor);
				sectionPaintHeight += 86f;
				#endregion

				GUILayout.BeginArea (new Rect (0f, 40f, 230f, sectionPaintHeight));
				DrawPaintContent ();
				GUILayout.EndArea ();

				GUI.EndGroup ();

				GUI.BeginGroup (new Rect (brushesColumnWidth + 30f, Mathf.Lerp (40f, sectionPaintHeight + 10f, paintColumnFactor) + 5f, 250f, Mathf.Lerp (30f, sectionBakeHeight, bakeColumnFactor)));

//				GUI.Box (new Rect (0f, 5f, bakeColumnWidth - 6f, 25f), "", guiSkin.GetStyle ("Button"));
//				GUI.DrawTexture (new Rect (8f, 3f, 24f, 24f), brush);
				if (GUI.Button (new Rect (0, 5f, bakeColumnWidth - 6f, 25f), "Other Settings", guiSkin.GetStyle ("Button"))) {
					if (!bakeColumnCollapsed) {
						bakeColumnFactor = 0.95f;
					} else {
						bakeColumnFactor = 0.05f;
					}
				}

				GUI.Box (new Rect (0f, 32f, paintColumnWidth - 6f, sectionBakeHeight - 32f), "", guiSkin.GetStyle ("Box"));

				#region drawBG
				sectionBakeHeight = 30f;

				if (!bakeEnabled) {
					EditorGUI.DrawRect (new Rect (0f, sectionBakeHeight + 3f, paintColumnWidth, 1f), sectionLimitColor);
					EditorGUI.DrawRect (new Rect (0f, sectionBakeHeight + 5f, paintColumnWidth, 28f), sectionColor);
					sectionBakeHeight += 45f;
				} else {
					if (!generateLMUvs) {
						EditorGUI.DrawRect (new Rect (0f, sectionBakeHeight + 3f, paintColumnWidth, 1f), sectionLimitColor);
						EditorGUI.DrawRect (new Rect (0f, sectionBakeHeight + 5f, paintColumnWidth, 60f), sectionColor);
						sectionBakeHeight += 73f;
					} else {
						EditorGUI.DrawRect (new Rect (0f, sectionBakeHeight + 3f, paintColumnWidth, 1f), sectionLimitColor);
						EditorGUI.DrawRect (new Rect (0f, sectionBakeHeight + 5f, paintColumnWidth, 85f), sectionColor);
						EditorGUI.DrawRect (new Rect (0f, sectionBakeHeight + 66f, paintColumnWidth, 20f), subSectionColor);
						sectionBakeHeight += 100f;
					}
				}
				EditorGUI.DrawRect (new Rect (0f, sectionBakeHeight, paintColumnWidth, 1f), sectionLimitColor);
				EditorGUI.DrawRect (new Rect (0f, sectionBakeHeight, paintColumnWidth, 55f), sectionColor);
				sectionBakeHeight += 55f;
				#endregion

				GUILayout.BeginArea (new Rect (0f, 40f, 230f, sectionPaintHeight));
				DrawBakeContent ();
				GUILayout.EndArea ();

				GUI.EndGroup ();

				GUI.EndScrollView ();

				GUILayout.BeginArea (new Rect (290f, 5f, Mathf.Lerp (33f, objectsColumnWidth, objectsColumnFactor), columnHeight + 38f));
				GUI.Box (new Rect (0f, 0f, Mathf.Lerp (33f, objectsColumnWidth, objectsColumnFactor), 32f), "", guiSkin.GetStyle ("Tab"));
				GUI.DrawTexture (new Rect (5f, 4f, 24f, 24f), objects);
				GUI.DrawTexture (new Rect (objectsColumnWidth - 29f, 4f, 24f, 24f), arrow);
				if (GUI.Button (new Rect (0f, 0f, objectsColumnWidth, 32f), "Objects", guiSkin.GetStyle ("Title"))) {
					if (!objectsColumnCollapsed) {
						objectsColumnFactor = 0.95f;
					} else {
						objectsColumnFactor = 0.05f;
					}
				}

				if (objectsColumnFactor > 0.15f) {
					GUI.Box (new Rect (0f, 38f, objectsColumnWidth, columnHeight), "", guiSkin.GetStyle ("Box"));
					DrawObjectsContent ();
				}
				if (objectsColumnFactor < 1f) {
					Color hideColor = new Color (0.761f, 0.761f, 0.761f, Mathf.Lerp (0f, 1f, 0.15f / objectsColumnFactor));
					EditorGUI.DrawRect (new Rect (0, 38f, objectsColumnWidth, columnHeight), hideColor);
				}
				GUILayout.EndArea ();

				GUI.EndScrollView ();

				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (this, "PrefabPlaceNBake GUI");
					Undo.RegisterCompleteObjectUndo (gradientContainer, "PrefabPlaceNBake GUI");

					layerMask = tempLayerMask;
					brushSize = tempBrushSize;
					density = tempDensity;
					spacing = tempSpacing;
					alignToSurface = tempAlignToSurface;
					alignTo = tempAlignTo;
					slopeBias = tempSlopeBias;

					scaleVar = tempScaleVar;
					scaleUniform = tempScaleUniform;
					scaleMaxX = tempScaleMaxX;
					scaleMinX = tempScaleMinX;
					scaleMaxY = tempScaleMaxY;
					scaleMinY = tempScaleMinY;
					scaleMaxZ = tempScaleMaxZ;
					scaleMinZ = tempScaleMinZ;
					scaleMax = tempScaleMax;
					scaleMin = tempScaleMin;

					alignToStroke = tempAlignToStroke;
					alignToStrokeAxis = tempAlignToStrokeAxis;

					rotationVar = tempRotationVar;
					rotationMaxX = tempRotationMaxX;
					rotationMinX = tempRotationMinX;
					rotationMaxY = tempRotationMaxY;
					rotationMinY = tempRotationMinY;
					rotationMaxZ = tempRotationMaxZ;
					rotationMinZ = tempRotationMinZ;

					offsetVar = tempOffsetVar;
					offsetMaxX = tempOffsetMaxX;
					offsetMinX = tempOffsetMinX;
					offsetMaxY = tempOffsetMaxY;
					offsetMinY = tempOffsetMinY;
					offsetMaxZ = tempOffsetMaxZ;
					offsetMinZ = tempOffsetMinZ;

					colorVariation = tempColorVariation;
					serializedGradientContainer.ApplyModifiedProperties ();

					physDropHeight = tempPhysDropHeight;
					gravity = tempGravity;

					bakeEnabled = tempBakeEnabled;
					bakeMaterial = tempBakeMaterial;
					bakeMeshes = tempBakeMeshes;
					generateLMUvs = tempGenerateLMUvs;
					useExistingUv2 = tempUseExistingUv2;
					generateBakedLod = tempGenerateBakedLod;
					groupByObject = tempGroupByObject;
					makeStatic = tempMakeStatic;
				}
				EditorGUI.EndDisabledGroup ();
				EditorGUI.EndDisabledGroup ();

				DrawMessageBoxes ();

			}
			else {
				GUI.Label (new Rect (brushesColumnWidth + 50f, this.position.height/2 - 37.5f, 250f, 75f), "Create or load a brush to begin...", guiSkin.box);
			}
		}

		void DrawMessageBoxes(){
			if (bakeEnabled && !bakeMessageAccepted) {
				guiDisabledByMessageBox = true;

				Rect bakeMessageRect = new Rect (this.position.width / 2 - 150f, this.position.height / 2 - 150f, 300f, 250f);

				GUI.Box (bakeMessageRect, "Caution!", guiSkin.window);
				EditorGUI.TextArea (new Rect (bakeMessageRect.x + 15f, bakeMessageRect.y + 40f, 230f, 80f),"After setting ON the Bake Mode some data will be calculated and stored in your project.", guiSkin.GetStyle("labellittle"));
				EditorGUI.TextArea (new Rect (bakeMessageRect.x + 15f, bakeMessageRect.y + 100f, 230f, 80f),"After this you won't be able of add or remove objects to this brush.", guiSkin.GetStyle("labellittle"));
				EditorGUI.TextArea (new Rect (bakeMessageRect.x + 15f, bakeMessageRect.y + 140f, 230f, 80f),"This is a non undoable operation.", guiSkin.GetStyle("labellittle"));
				if (GUI.Button (new Rect (bakeMessageRect.x + 20f, bakeMessageRect.y + 215f, 100f, 20f), "Accept",  guiSkin.GetStyle("button"))) {
					bakeMessageAccepted = true;
					guiDisabledByMessageBox = false;
					BakeMaterials ();
				}
				if (GUI.Button (new Rect (bakeMessageRect.x + 180f, bakeMessageRect.y + 215f, 100f, 20f), "Cancel", guiSkin.GetStyle("button"))) {
					bakeEnabled = false;
					guiDisabledByMessageBox = false;
				}
			}
			if (!cloneBakeMessageAccepted && cloningWithBake) {
				guiDisabledByMessageBox = true;

				Rect cloneMessageRecet = new Rect (this.position.width / 2 - 200f, this.position.height / 2 - 35f, 400f, 100f);

				GUI.Box (cloneMessageRecet, "", guiSkin.window);
				GUI.Label (new Rect (cloneMessageRecet.x + 45f, cloneMessageRecet.y + 10f, 100f, 20f), "Cloning the brush WITHOUT bake mode", guiSkin.label);
				GUI.Label (new Rect (cloneMessageRecet.x + 125f, cloneMessageRecet.y + 40f, 150f, 20f), "You can enable it later.", guiSkin.GetStyle("labellittle"));
				if (GUI.Button (new Rect (cloneMessageRecet.x + 150f, cloneMessageRecet.y + 70f, 100f, 20f), "Accept", guiSkin.GetStyle("button"))) {
					cloneBakeMessageAccepted = true;
					SaveDensities ();
					brushes [selectedBrush] = CreateSettings (selectedBrush);
					WriteBrush (selectedBrush);
					CreateNewBrush (true);


					cloningWithBake = false;
					guiDisabledByMessageBox = false;
				}
			}
			if (colorVariation && !vertexColorMessageAccepted && !bakeEnabled) {
				guiDisabledByMessageBox = true;

				Rect colorMessageRect = new Rect (this.position.width / 2 - 165f, this.position.height / 2 - 100f, 330f, 165f);

				GUI.Box (colorMessageRect, "Caution!", guiSkin.window);

				EditorGUI.TextArea (new Rect (colorMessageRect.x + 15f, colorMessageRect.y + 45f, 280f, 80f),"Enabling Color Variation without baking the mesh will generate unique mesh for each placed object.", guiSkin.GetStyle("labellittle"));
				if (GUI.Button (new Rect (colorMessageRect.x + 115f, colorMessageRect.y + 130f, 100f, 20f), "Accept", guiSkin.GetStyle("button"))) {
					vertexColorMessageAccepted = true;
					guiDisabledByMessageBox = false;
				}
			}
			if (!colorVariation && vertexColorMessageAccepted) {
				vertexColorMessageAccepted = false;
			}
		}

		void DrawBrushesContent(){
			for (int i = 0; i < brushes.Count; i++) {
				if (selectedBrush == i) {
					if (GUILayout.Button (brushesNames [i], guiSkin.GetStyle ("ButtonBigSelected"), GUILayout.Height (50f))) {
						brushes [selectedBrush] = CreateSettings (selectedBrush);
						SaveDensities ();
						WriteBrush (selectedBrush);
						ReadBrush (i);
						selectedBrush = i;
						previews.Clear ();
						densitiesContainer.densities.Clear ();
						RefreshPreviewsAndDensities (brushes [selectedBrush]);
						soloMode = -1;
						Undo.ClearUndo (this);
						placedObjectsContainer.placedObjects.Clear ();
						placedObjectsContainer.placedObjectsID.Clear ();
						droppedObjectsDO.Clear ();
						droppedObjectsRB.Clear ();
						droppedObjectsMC.Clear ();
					}
				}
				else {
					if (GUILayout.Button (brushesNames [i], guiSkin.GetStyle ("ButtonBig"), GUILayout.Height (50f))) {
						brushes [selectedBrush] = CreateSettings (selectedBrush);
						SaveDensities ();
						WriteBrush (selectedBrush);
						ReadBrush (i);
						selectedBrush = i;
						previews.Clear ();
						densitiesContainer.densities.Clear ();
						RefreshPreviewsAndDensities (brushes [selectedBrush]);
						soloMode = -1;
						Undo.ClearUndo (this);
						placedObjectsContainer.placedObjects.Clear ();
						placedObjectsContainer.placedObjectsID.Clear ();
						droppedObjectsDO.Clear ();
						droppedObjectsRB.Clear ();
						droppedObjectsMC.Clear ();
					}

				}
				GUILayout.Space (10f);
			}
		}

		void DrawBrushesButtons(){
			if (GUILayout.Button ("New Brush", guiSkin.GetStyle("Button"), GUILayout.Height(28f))) {
				if (brushes.Count > 0) {
					SaveDensities ();
					brushes [selectedBrush] = CreateSettings (selectedBrush);
					WriteBrush (selectedBrush);
				}
				CreateNewBrush (false);
			}
			if (GUILayout.Button ("Clone Brush", guiSkin.GetStyle("Button"), GUILayout.Height(28f))) {
				if (brushes.Count > 0) {
					if (!bakeEnabled) {
						SaveDensities ();
						brushes [selectedBrush] = CreateSettings (selectedBrush);
						WriteBrush (selectedBrush);
						CreateNewBrush (true);
					} 
					else {
						cloneBakeMessageAccepted = false;
						cloningWithBake = true;
						//ir al mensaje porque la mandanga sigue allí
					}
				}
			}
			if (GUILayout.Button ("Load Brushes", guiSkin.GetStyle("Button"), GUILayout.Height(28f))) {
				LoadBrushesFromDisk ();
			}
			if (GUILayout.Button ("Remove Brush", guiSkin.GetStyle("Button"), GUILayout.Height(28f))) {
				if (brushes.Count > 0) {
					SaveDensities ();
					brushes [selectedBrush] = CreateSettings (selectedBrush);
					WriteBrush (selectedBrush);
					RemoveBrush ();
				}
			}
		}

		void DrawPaintContent(){

			GUILayout.Label("Layer Mask", guiSkin.GetStyle("label"), GUILayout.Height(20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			lastLayouRect = new Rect (lastLayouRect.x + 100f, lastLayouRect.y, lastLayouRect.width - 100f, lastLayouRect.height);
			tempLayerMask = EditorGUI.MaskField(lastLayouRect, InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layerMask), InternalEditorUtility.layers, guiSkin.GetStyle("button"));
			tempLayerMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempLayerMask);

			GUILayout.Label("Brush Size", guiSkin.GetStyle("label"), GUILayout.Height(20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			tempBrushSize = EditorGUI.FloatField (lastLayouRect, " ", brushSize, guiSkin.GetStyle("textfield"));

			GUILayout.Label ("Density", guiSkin.GetStyle("label"), GUILayout.Height(20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			lastLayouRect = new Rect (paintColumnWidth / 2 - 30f, lastLayouRect.y, lastLayouRect.width / 3f, lastLayouRect.height);
			tempDensity = GUI.HorizontalSlider (lastLayouRect, density, 0f, 1f, guiSkin.GetStyle ("horizontalslider"), guiSkin.GetStyle ("horizontalsliderthumb"));
			lastLayouRect = new Rect (paintColumnWidth - 60f, lastLayouRect.y, lastLayouRect.width / 3f, lastLayouRect.height);
			EditorGUI.LabelField(lastLayouRect, density.ToString("F2"), guiSkin.GetStyle("LabelLittle"));

			GUILayout.Label("Spacing (pixels)", guiSkin.GetStyle("label"), GUILayout.Height(20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			tempSpacing = EditorGUI.IntField (lastLayouRect, " ", spacing, guiSkin.GetStyle("textfield"));

			GUILayout.Label ("Align to Surface", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			lastLayouRect = new Rect (paintColumnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
			tempAlignToSurface = EditorGUI.Toggle (lastLayouRect, alignToSurface, guiSkin.GetStyle ("Toggle"));

			if (alignToSurface) {
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("      X", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (lastLayouRect.x + 50f, lastLayouRect.y, 20f, lastLayouRect.height);
				if (alignTo == 0) {
					if (GUI.Button (lastLayouRect, "", guiSkin.GetStyle ("OnToggle"))) {
					
					}
				} else {
					if (GUI.Button (lastLayouRect, "", guiSkin.toggle)) {
						if (tempAlignToStrokeAxis == 0 && alignToStroke) {
							Debug.LogWarning ("You cannot align to surface and to stroke the same axis");
						} else {
							tempAlignTo = 0;
						}
					}
				}
				GUILayout.Label ("      Y", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (lastLayouRect.x + 50f, lastLayouRect.y, 20f, lastLayouRect.height);
				if (alignTo == 1) {
					if (GUI.Button (lastLayouRect, "", guiSkin.GetStyle ("OnToggle"))) {

					}
				} else {
					if (GUI.Button (lastLayouRect, "", guiSkin.toggle)) {
						if (tempAlignToStrokeAxis == 1 && alignToStroke) {
							Debug.LogWarning ("You cannot align to surface and to stroke the same axis");
						} else {
							tempAlignTo = 1;
						}
					}
				}
				GUILayout.Label ("      Z", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (lastLayouRect.x + 50f, lastLayouRect.y, 20f, lastLayouRect.height);
				if (alignTo == 2) {
					if (GUI.Button (lastLayouRect, "", guiSkin.GetStyle ("OnToggle"))) {

					}
				} else {
					if (GUI.Button (lastLayouRect, "", guiSkin.toggle)) {
						if (tempAlignToStrokeAxis == 2 && alignToStroke) {
							Debug.LogWarning ("You cannot align to surface and to stroke the same axis");
						} else {
							tempAlignTo = 2;
						}
					}
				}
				GUILayout.EndHorizontal ();
			}

			GUILayout.Label ("Slope Bias", guiSkin.GetStyle("label"), GUILayout.Height(20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			lastLayouRect = new Rect (paintColumnWidth / 2 - 30f, lastLayouRect.y, lastLayouRect.width / 3f, lastLayouRect.height);
			tempSlopeBias = GUI.HorizontalSlider (lastLayouRect, slopeBias, 0f, 90f, guiSkin.GetStyle ("horizontalslider"), guiSkin.GetStyle ("horizontalsliderthumb"));
			lastLayouRect = new Rect (paintColumnWidth - 60f, lastLayouRect.y, lastLayouRect.width / 3f, lastLayouRect.height);
			EditorGUI.LabelField(lastLayouRect, slopeBias.ToString("F1"), guiSkin.GetStyle("LabelLittle"));

			GUILayout.Space (10f);

			GUILayout.Label ("Scale Variation", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			lastLayouRect = new Rect (paintColumnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
			tempScaleVar = EditorGUI.Toggle (lastLayouRect, scaleVar, guiSkin.GetStyle ("Toggle"));

			if (scaleVar) {
				GUILayout.Label ("  Uniform Scale", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (paintColumnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
				tempScaleUniform = EditorGUI.Toggle (lastLayouRect, scaleUniform, guiSkin.GetStyle ("Toggle"));

				if (scaleUniform) {
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("     ", guiSkin.GetStyle ("label3"));
					GUILayout.Label ("Min", guiSkin.GetStyle ("label2"));
					tempScaleMin = EditorGUILayout.FloatField (scaleMin, guiSkin.GetStyle ("textfieldlittle"));
					GUILayout.Label ("Max", guiSkin.GetStyle ("label2"));
					tempScaleMax = EditorGUILayout.FloatField (scaleMax, guiSkin.GetStyle ("textfieldlittle"));
					GUILayout.EndHorizontal ();

					EditorGUILayout.LabelField (scaleBottom.ToString ("F1") + " - " + scaleTop.ToString ("F1"), guiSkin.GetStyle("LabelLittle2"));
					lastLayouRect = GUILayoutUtility.GetLastRect ();
					lastLayouRect = new Rect (lastLayouRect.x + 80f, lastLayouRect.y, lastLayouRect.width - 80f, lastLayouRect.height);
					EditorGUI.MinMaxSlider (lastLayouRect, ref scaleBottom, ref scaleTop, scaleMin, scaleMax);
				}
				else {
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("X    ", guiSkin.GetStyle("label3"));
					GUILayout.Label("Min", guiSkin.GetStyle("label2"));
					tempScaleMinX = EditorGUILayout.FloatField (scaleMinX,guiSkin.GetStyle("textfieldlittle"));
					GUILayout.Label("Max", guiSkin.GetStyle("label2"));
					tempScaleMaxX = EditorGUILayout.FloatField (scaleMaxX, guiSkin.GetStyle("textfieldlittle"));
					GUILayout.EndHorizontal ();

					EditorGUILayout.LabelField (scaleBottomX.ToString("F1") + " - " + scaleTopX.ToString("F1"), guiSkin.GetStyle("LabelLittle2"));
					lastLayouRect = GUILayoutUtility.GetLastRect ();
					lastLayouRect = new Rect (lastLayouRect.x + 80f, lastLayouRect.y, lastLayouRect.width - 80f, lastLayouRect.height);
					EditorGUI.MinMaxSlider (lastLayouRect, ref scaleBottomX, ref scaleTopX, scaleMinX, scaleMaxX);
					GUILayout.Space (8f);

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Y    ", guiSkin.GetStyle("label3"));
					GUILayout.Label("Min", guiSkin.GetStyle("label2"));
					tempScaleMinY = EditorGUILayout.FloatField (scaleMinY,guiSkin.GetStyle("textfieldlittle"));
					GUILayout.Label("Max", guiSkin.GetStyle("label2"));
					tempScaleMaxY = EditorGUILayout.FloatField (scaleMaxY, guiSkin.GetStyle("textfieldlittle"));
					GUILayout.EndHorizontal ();

					EditorGUILayout.LabelField (scaleBottomY.ToString("F1") + " - " + scaleTopY.ToString("F1"), guiSkin.GetStyle("LabelLittle2"));
					lastLayouRect = GUILayoutUtility.GetLastRect ();
					lastLayouRect = new Rect (lastLayouRect.x + 80f, lastLayouRect.y, lastLayouRect.width - 80f, lastLayouRect.height);
					EditorGUI.MinMaxSlider (lastLayouRect, ref scaleBottomY, ref scaleTopY, scaleMinY, scaleMaxY);
					GUILayout.Space (8f);

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Z    ", guiSkin.GetStyle("label3"));
					GUILayout.Label("Min", guiSkin.GetStyle("label2"));
					tempScaleMinZ = EditorGUILayout.FloatField (scaleMinZ,guiSkin.GetStyle("textfieldlittle"));
					GUILayout.Label("Max", guiSkin.GetStyle("label2"));
					tempScaleMaxZ = EditorGUILayout.FloatField (scaleMaxZ, guiSkin.GetStyle("textfieldlittle"));
					GUILayout.EndHorizontal ();

					EditorGUILayout.LabelField (scaleBottomZ.ToString("F1") + " - " + scaleTopZ.ToString("F1"), guiSkin.GetStyle("LabelLittle2"));
					lastLayouRect = GUILayoutUtility.GetLastRect ();
					lastLayouRect = new Rect (lastLayouRect.x + 80f, lastLayouRect.y, lastLayouRect.width - 80f, lastLayouRect.height);
					EditorGUI.MinMaxSlider (lastLayouRect, ref scaleBottomZ, ref scaleTopZ, scaleMinZ, scaleMaxZ);
				}
			}

			GUILayout.Space (10f);

			GUILayout.Label ("Align to Stroke", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			lastLayouRect = new Rect (paintColumnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
			tempAlignToStroke = EditorGUI.Toggle (lastLayouRect, alignToStroke, guiSkin.GetStyle ("Toggle"));

			if (alignToStroke) {
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("      X", guiSkin.GetStyle("label"), GUILayout.Height(20f));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (lastLayouRect.x + 50f, lastLayouRect.y, 20f, lastLayouRect.height);
				if (alignToStrokeAxis == 0) {
					if (GUI.Button (lastLayouRect, "", guiSkin.GetStyle("OnToggle"))) {

					}
				} else {
					if (GUI.Button (lastLayouRect, "", guiSkin.toggle)) {
						if (tempAlignTo == 0 && alignToSurface) {
							Debug.LogWarning ("You cannot align to surface and to stroke the same axis");
						}
						else {
							tempAlignToStrokeAxis = 0;
						}
					}
				}
				GUILayout.Label ("      Y", guiSkin.GetStyle("label"), GUILayout.Height(20f));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (lastLayouRect.x + 50f, lastLayouRect.y, 20f, lastLayouRect.height);
				if (alignToStrokeAxis == 1) {
					if (GUI.Button (lastLayouRect, "", guiSkin.GetStyle("OnToggle"))) {

					}
				} else {
					if (GUI.Button (lastLayouRect, "", guiSkin.toggle)) {
						if (tempAlignTo == 1 && alignToSurface) {
							Debug.LogWarning ("You cannot align to surface and to stroke the same axis");
						}
						else {
							tempAlignToStrokeAxis = 1;
						}
					}
				}
				GUILayout.Label ("      Z", guiSkin.GetStyle("label"), GUILayout.Height(20f));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (lastLayouRect.x + 50f, lastLayouRect.y, 20f, lastLayouRect.height);
				if (alignToStrokeAxis == 2) {
					if (GUI.Button (lastLayouRect, "", guiSkin.GetStyle("OnToggle"))) {

					}
				} else {
					if (GUI.Button (lastLayouRect, "", guiSkin.toggle)) {
						if (tempAlignTo == 2 && alignToSurface) {
							Debug.LogWarning ("You cannot align to surface and to stroke the same axis");
						}
						else {
							tempAlignToStrokeAxis = 2;
						}
					}
				}
				GUILayout.EndHorizontal ();
			}

			GUILayout.Space (10f);

			GUILayout.Label ("Rotation Variation", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			lastLayouRect = new Rect (paintColumnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
			tempRotationVar = EditorGUI.Toggle (lastLayouRect, rotationVar, guiSkin.GetStyle ("Toggle"));

			if (rotationVar) {
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("X    ", guiSkin.GetStyle ("label3"));
				GUILayout.Label ("Min", guiSkin.GetStyle ("label2"));
				tempRotationMinX = EditorGUILayout.FloatField (rotationMinX, guiSkin.GetStyle ("textfieldlittle"));
				GUILayout.Label ("Max", guiSkin.GetStyle ("label2"));
				tempRotationMaxX = EditorGUILayout.FloatField (rotationMaxX, guiSkin.GetStyle ("textfieldlittle"));
				GUILayout.EndHorizontal ();

				EditorGUILayout.LabelField (rotationBottomX.ToString ("F1") + " - " + rotationTopX.ToString ("F1"), guiSkin.GetStyle("LabelLittle2"));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (lastLayouRect.x + 80f, lastLayouRect.y, lastLayouRect.width - 80f, lastLayouRect.height);
				EditorGUI.MinMaxSlider (lastLayouRect, ref rotationBottomX, ref rotationTopX, rotationMinX, rotationMaxX);
				GUILayout.Space (8f);

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Y    ", guiSkin.GetStyle ("label3"));
				GUILayout.Label ("Min", guiSkin.GetStyle ("label2"));
				tempRotationMinY = EditorGUILayout.FloatField (rotationMinY, guiSkin.GetStyle ("textfieldlittle"));
				GUILayout.Label ("Max", guiSkin.GetStyle ("label2"));
				tempRotationMaxY = EditorGUILayout.FloatField (rotationMaxY, guiSkin.GetStyle ("textfieldlittle"));
				GUILayout.EndHorizontal ();

				EditorGUILayout.LabelField (rotationBottomY.ToString ("F1") + " - " + rotationTopY.ToString ("F1"), guiSkin.GetStyle("LabelLittle2"));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (lastLayouRect.x + 80f, lastLayouRect.y, lastLayouRect.width - 80f, lastLayouRect.height);
				EditorGUI.MinMaxSlider (lastLayouRect, ref rotationBottomY, ref rotationTopY, rotationMinY, rotationMaxY);
				GUILayout.Space (8f);

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Z    ", guiSkin.GetStyle ("label3"));
				GUILayout.Label ("Min", guiSkin.GetStyle ("label2"));
				tempRotationMinZ = EditorGUILayout.FloatField (rotationMinZ, guiSkin.GetStyle ("textfieldlittle"));
				GUILayout.Label ("Max", guiSkin.GetStyle ("label2"));
				tempRotationMaxZ = EditorGUILayout.FloatField (rotationMaxZ, guiSkin.GetStyle ("textfieldlittle"));
				GUILayout.EndHorizontal ();

				EditorGUILayout.LabelField (rotationBottomZ.ToString ("F1") + " - " + rotationTopZ.ToString ("F1"), guiSkin.GetStyle("LabelLittle2"));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (lastLayouRect.x + 80f, lastLayouRect.y, lastLayouRect.width - 80f, lastLayouRect.height);
				EditorGUI.MinMaxSlider (lastLayouRect, ref rotationBottomZ, ref rotationTopZ, rotationMinZ, rotationMaxZ);
			}

			GUILayout.Space (10f);

			GUILayout.Label ("Offset Variation", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			lastLayouRect = new Rect (paintColumnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
			tempOffsetVar = EditorGUI.Toggle (lastLayouRect, offsetVar, guiSkin.GetStyle ("Toggle"));
			if (offsetVar) {
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("X    ", guiSkin.GetStyle ("label3"));
				GUILayout.Label ("Min", guiSkin.GetStyle ("label2"));
				tempOffsetMinX = EditorGUILayout.FloatField (offsetMinX, guiSkin.GetStyle ("textfieldlittle"));
				GUILayout.Label ("Max", guiSkin.GetStyle ("label2"));
				tempOffsetMaxX = EditorGUILayout.FloatField (offsetMaxX, guiSkin.GetStyle ("textfieldlittle"));
				GUILayout.EndHorizontal ();

				EditorGUILayout.LabelField (offsetBottomX.ToString ("F1") + " - " + offsetTopX.ToString ("F1"), guiSkin.GetStyle("LabelLittle2"));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (lastLayouRect.x + 80f, lastLayouRect.y, lastLayouRect.width - 80f, lastLayouRect.height);
				EditorGUI.MinMaxSlider (lastLayouRect, ref offsetBottomX, ref offsetTopX, offsetMinX, offsetMaxX);
				GUILayout.Space (8f);

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Y    ", guiSkin.GetStyle ("label3"));
				GUILayout.Label ("Min", guiSkin.GetStyle ("label2"));
				tempOffsetMinY = EditorGUILayout.FloatField (offsetMinY, guiSkin.GetStyle ("textfieldlittle"));
				GUILayout.Label ("Max", guiSkin.GetStyle ("label2"));
				tempOffsetMaxY = EditorGUILayout.FloatField (offsetMaxY, guiSkin.GetStyle ("textfieldlittle"));
				GUILayout.EndHorizontal ();

				EditorGUILayout.LabelField (offsetBottomY.ToString ("F1") + " - " + offsetTopY.ToString ("F1"), guiSkin.GetStyle("LabelLittle2"));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (lastLayouRect.x + 80f, lastLayouRect.y, lastLayouRect.width - 80f, lastLayouRect.height);
				EditorGUI.MinMaxSlider (lastLayouRect, ref offsetBottomY, ref offsetTopY, offsetMinY, offsetMaxY);
				GUILayout.Space (8f);

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Z    ", guiSkin.GetStyle ("label3"));
				GUILayout.Label ("Min", guiSkin.GetStyle ("label2"));
				tempOffsetMinZ = EditorGUILayout.FloatField (offsetMinZ, guiSkin.GetStyle ("textfieldlittle"));
				GUILayout.Label ("Max", guiSkin.GetStyle ("label2"));
				tempOffsetMaxZ = EditorGUILayout.FloatField (offsetMaxZ, guiSkin.GetStyle ("textfieldlittle"));
				GUILayout.EndHorizontal ();

				EditorGUILayout.LabelField (offsetBottomZ.ToString ("F1") + " - " + offsetTopZ.ToString ("F1"), guiSkin.GetStyle("LabelLittle2"));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (lastLayouRect.x + 80f, lastLayouRect.y, lastLayouRect.width - 80f, lastLayouRect.height);
				EditorGUI.MinMaxSlider (lastLayouRect, ref offsetBottomZ, ref offsetTopZ, offsetMinZ, offsetMaxZ);
			}

			GUILayout.Space (10f);

			GUILayout.Label ("Vertex Color Variation", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			lastLayouRect = new Rect (paintColumnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
			tempColorVariation = EditorGUI.Toggle (lastLayouRect, colorVariation, guiSkin.GetStyle ("Toggle"));
			if(colorVariation){
				GUILayout.Space (2f);
				EditorGUILayout.PropertyField(serializedGradient);
			}

			GUILayout.Space (10f);

			GUILayout.Label("Physical Drop Height", guiSkin.GetStyle("label"), GUILayout.Height(20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			lastLayouRect = new Rect (lastLayouRect.x + 30f, lastLayouRect.y, lastLayouRect.width - 30f, lastLayouRect.height);
			tempPhysDropHeight = EditorGUI.FloatField (lastLayouRect, " ", physDropHeight, guiSkin.GetStyle("textfield"));

			GUILayout.Label("Gravity", guiSkin.GetStyle("label"), GUILayout.Height(20f));
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("X", guiSkin.GetStyle ("label2"));
			tempGravity.x = EditorGUILayout.FloatField (gravity.x, guiSkin.GetStyle ("textfieldlittle"));
			GUILayout.Label ("Y", guiSkin.GetStyle ("label2"));
			tempGravity.y = EditorGUILayout.FloatField (gravity.y, guiSkin.GetStyle ("textfieldlittle"));
			GUILayout.Label ("Z", guiSkin.GetStyle ("label2"));
			tempGravity.z = EditorGUILayout.FloatField (gravity.z, guiSkin.GetStyle ("textfieldlittle"));
			GUILayout.EndHorizontal ();
		}

		void DrawBakeContent(){
			EditorGUI.BeginDisabledGroup (bakeEnabled);
			GUILayout.Label ("Bake Mode Enabled", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			lastLayouRect = new Rect (paintColumnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
			tempBakeEnabled = EditorGUI.Toggle (lastLayouRect, bakeEnabled, guiSkin.GetStyle ("Toggle"));
			EditorGUI.EndDisabledGroup ();

			if (bakeEnabled) {
//				GUILayout.Label ("  Bake Material", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
//				lastLayouRect = GUILayoutUtility.GetLastRect ();
//				lastLayouRect = new Rect (paintColumnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
//				tempBakeMaterial = EditorGUI.Toggle (lastLayouRect, bakeMaterial, guiSkin.GetStyle ("Toggle"));

//				GUILayout.Label ("  Bake Meshes", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
//				lastLayouRect = GUILayoutUtility.GetLastRect ();
//				lastLayouRect = new Rect (paintColumnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
//				tempBakeMeshes = EditorGUI.Toggle (lastLayouRect, bakeMeshes, guiSkin.GetStyle ("Toggle"));

				GUILayout.Label ("  Generate Lightmap Uvs", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (paintColumnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
				tempGenerateLMUvs = EditorGUI.Toggle (lastLayouRect, generateLMUvs, guiSkin.GetStyle ("Toggle"));

				if (generateLMUvs) {
					GUILayout.Label ("    Use Existing Uv2", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
					lastLayouRect = GUILayoutUtility.GetLastRect ();
					lastLayouRect = new Rect (paintColumnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
					tempUseExistingUv2 = EditorGUI.Toggle (lastLayouRect, useExistingUv2, guiSkin.GetStyle ("Toggle"));
				}

//				GUILayout.Label ("  Generate Baked LOD", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
//				lastLayouRect = GUILayoutUtility.GetLastRect ();
//				lastLayouRect = new Rect (paintColumnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
//				tempGenerateBakedLod = EditorGUI.Toggle (lastLayouRect, generateBakedLod, guiSkin.GetStyle ("Toggle"));
			}

			GUILayout.Space (10);

			GUILayout.Label ("Group By Object", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			lastLayouRect = new Rect (paintColumnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
			tempGroupByObject = EditorGUI.Toggle (lastLayouRect, groupByObject, guiSkin.GetStyle ("Toggle"));

			GUILayout.Label ("Make Objects Static", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			lastLayouRect = new Rect (paintColumnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
			tempMakeStatic = EditorGUI.Toggle (lastLayouRect, makeStatic, guiSkin.GetStyle ("Toggle"));
		}

		void DrawObjectsContent(){
			if (brushes.Count > 0) {
				DragAndDropObjects ();
				if (brushes [selectedBrush].objects.Count > 0) {
					objectsScroll = GUI.BeginScrollView (new Rect (0f, 117f, objectsColumnWidth, columnHeight - 80f), objectsScroll, new Rect (0f, 0f, objectsColumnWidth - 16f, 130f * brushes [selectedBrush].objects.Count + 20f));
					GUI.BeginGroup (new Rect (5f, 10f, objectsColumnWidth - 26f, brushes [selectedBrush].objects.Count * 130f));

					serializedDensitiesContainer.Update ();

					for (int i = 0; i < brushes [selectedBrush].objects.Count; i++) {
						Rect objectRect = new Rect (5f, 130f * i, objectsColumnWidth - 5f, 125f);
						EditorGUI.DrawRect (objectRect, sectionColor);

						Rect nameRect = new Rect (objectRect.x + objectsColumnWidth / 2f - 35f, objectRect.y + 25f, objectRect.width / 2f - 10f, 25f);
						GUI.Label (nameRect, brushes [selectedBrush].objects [i].name, guiSkin.GetStyle ("label"));

						nameRect = new Rect (objectRect.x + objectsColumnWidth / 2f + 38f, objectRect.y + 5f, objectRect.width / 2f - 10f, 25f);
						GUI.Label (nameRect, "Solo", guiSkin.GetStyle ("labellittle"));
						nameRect = new Rect (nameRect.x + 35f, nameRect.y + 5f, 15f, 15f);
						if (soloMode == i) {
							if (GUI.Button (nameRect, "", guiSkin.GetStyle ("OnToggle"))) {
								soloMode = -1;
							}
						}
						else {
							if (GUI.Button (nameRect, "", guiSkin.toggle)) {
								soloMode = i;
							}
						}

						nameRect = new Rect (objectRect.x + objectsColumnWidth / 2f - 30f, objectRect.y + 15f + 40f, objectRect.width / 2f - 10f, 25f);
						if (GUI.Button (nameRect, "Options", guiSkin.button)) {
							SaveDensities ();
							if (!objectWindow) {
								guiDisabled = true;
								objectWindow = (ObjectSettingsWindow)EditorWindow.CreateInstance<ObjectSettingsWindow> ();

								objectWindow.mySettings = brushes [selectedBrush].objects [i];
								GUIContent content = new GUIContent (objectWindow.mySettings.name + " options");
								objectWindow.titleContent = content;
								objectWindow.preview = previews [i];
								objectWindow.objectId = i;
								objectWindow.parentWindow = this;
								objectWindow.maxSize = new Vector2 (282f, 900f);
								objectWindow.minSize = new Vector2 (282f, 200);
								objectWindow.ReadSettings ();
								EditorWindow.GetWindow<ObjectSettingsWindow> ();
							} else {
								EditorWindow.GetWindow<ObjectSettingsWindow> ();
							}
						}

						if (previews [i] != null) {
							Rect previewRect = new Rect (objectRect.x + 10f, objectRect.y + 10f, 80f, 80f);
							GUI.DrawTexture (previewRect, previews [i]);
							previewRect = new Rect (previewRect.x + 2.5f, previewRect.y + 2.5f, previewRect.width * 0.25f, previewRect.height * 0.25f);
							if (GUI.Button(previewRect, "", guiSkin.GetStyle("ButtonSearch"))){
								EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(brushes [selectedBrush].objects[i].path) as GameObject);
							}
						}

						Rect densityRect = new Rect (objectRect.x + 5f, objectRect.y + 100f, objectRect.width - 35f, 20f);
						EditorGUI.Slider (densityRect, serializedDensities.GetArrayElementAtIndex (i), 0f, 1f);

						Rect boxRect = new Rect (densityRect.x, densityRect.y, densityRect.width - 55f, densityRect.height);
						GUI.Box (boxRect, "", guiSkin.box);
						boxRect = new Rect (densityRect.x, densityRect.y, (densityRect.width - 55f) * serializedDensities.GetArrayElementAtIndex (i).floatValue, densityRect.height);
						GUI.Box (boxRect, "", guiSkin.textArea);

						GUI.Label (boxRect, "Density", guiSkin.GetStyle ("label"));
					}
					serializedDensitiesContainer.ApplyModifiedProperties ();

					GUI.EndGroup ();
					GUI.EndScrollView ();
				}
				else {
					GUI.Box (new Rect (10f, 160f, objectsColumnWidth - 26f, 90f), "In case you bake the brush, the system will use the first object's material as template for build the baked textures and material.", guiSkin.box);
				}
			} else {
				Rect drop_area = new Rect (0f, 43f, objectsColumnWidth, 50f);
				GUI.Box (drop_area, "Create or load a brush.", guiSkin.box);
			}
		}

		void DragAndDropObjects(){
			Event evt = Event.current;
			Rect drop_area = new Rect (0f, 43f, objectsColumnWidth, 75f);
			if (!bakeEnabled) {
				GUI.Box (drop_area, "Drag & Drop prefabs here to add objects to the selected brush.", guiSkin.box);
				switch (evt.type) {
				case EventType.DragUpdated:
				case EventType.DragPerform:
					if (!drop_area.Contains (evt.mousePosition))
						return;

					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

					if (evt.type == EventType.DragPerform) {
						DragAndDrop.AcceptDrag ();

						foreach (Object dragged_object in DragAndDrop.objectReferences) {
							if (PrefabUtility.GetPrefabType (dragged_object) == PrefabType.Prefab){ //|| PrefabUtility.GetPrefabType (dragged_object) == PrefabType.ModelPrefab) {
								NewObject(dragged_object);
							}
							else {
								Debug.LogWarning (dragged_object.name + " is not a prefab. Please, add only prefabs to the list.");
							}
						}
					}
					break;
				}
			} else {
				GUI.Box (drop_area, "This brush has Bake Mode enabled. You cannot add or remove objects on this brush.", guiSkin.box);
			}
		}

		void RefreshTabs(){
			if (brushesColumnFactor < 1f && !brushesColumnCollapsed) {
				brushesColumnFactor -= collapseSpeed;
				this.Repaint ();
				if (brushesColumnFactor <= 0.15f) {
					brushesColumnFactor = 0.15f;
					brushesColumnCollapsed = true;
				}
			}
			if (brushesColumnFactor > 0.15f && brushesColumnCollapsed) {
				brushesColumnFactor += collapseSpeed;
				this.Repaint ();
				if (brushesColumnFactor >= 1f) {
					brushesColumnFactor = 1f;
					brushesColumnCollapsed = false;
				}
			}
			if (paintColumnFactor < 1f && !paintColumnCollapsed) {
				paintColumnFactor -= collapseSpeed;
				this.Repaint ();
				if (paintColumnFactor <= 0.05f) {
					paintColumnFactor = 0.00f;
					paintColumnCollapsed = true;
				}
			}
			if (paintColumnFactor > 0.00f && paintColumnCollapsed) {
				paintColumnFactor += collapseSpeed;
				this.Repaint ();
				if (paintColumnFactor >= 1f) {
					paintColumnFactor = 1f;
					paintColumnCollapsed = false;
				}
			}
			if (bakeColumnFactor < 1f && !bakeColumnCollapsed) {
				bakeColumnFactor -= collapseSpeed;
				this.Repaint ();
				if (bakeColumnFactor <= 0.05f) {
					bakeColumnFactor = 0.00f;
					bakeColumnCollapsed = true;
				}
			}
			if (bakeColumnFactor > 0.00f && bakeColumnCollapsed) {
				bakeColumnFactor += collapseSpeed;
				this.Repaint ();
				if (bakeColumnFactor >= 1f) {
					bakeColumnFactor = 1f;
					bakeColumnCollapsed = false;
				}
			}
			if (objectsColumnFactor < 1f && !objectsColumnCollapsed) {
				objectsColumnFactor -= collapseSpeed;
				this.Repaint ();
				if (objectsColumnFactor <= 0.05f) {
					objectsColumnFactor = 0.00f;
					objectsColumnCollapsed = true;
				}
			}
			if (objectsColumnFactor > 0.00f && objectsColumnCollapsed) {
				objectsColumnFactor += collapseSpeed;
				this.Repaint ();
				if (objectsColumnFactor >= 1f) {
					objectsColumnFactor = 1f;
					objectsColumnCollapsed = false;
				}
			}
		}

		public void GetObjectSettings(ObjectSettings inputSettings, int objectId){
			brushes [selectedBrush].objects [objectId] = inputSettings;
			densitiesContainer.densities [objectId] = inputSettings.density;

		}
	}
}
#endif
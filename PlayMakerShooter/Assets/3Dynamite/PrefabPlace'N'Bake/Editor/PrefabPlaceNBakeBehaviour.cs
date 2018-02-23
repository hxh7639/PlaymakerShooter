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

		#region Variables
		int controlID;
		Event current;
		bool rayCast;
		bool painting;
		bool paintActivated = true;
		bool physicalDrop;
		bool precisionPlace;
		bool eraser;
		bool canCast;
		Vector3 mousePoint;
		Vector3 mouseNormal;
		Vector3 lastMouse;
		Vector3 mouseDirection;
		Vector3 PlacedObjectParentScale;
		GameObject precisionPlaceObject;
		Vector3 precisionPlaceParentScale;
		int precisionPlaceObjectID;
		Quaternion precisionPlaceOriginalRot;
		bool precisionPlaceFlag = false;
		Vector3 lastPrecisionPlaceDir = Vector3.one;

//		Material brushMat;
//		Material eraseMat;
//		Material precisionMat;
		Material brushMatProj;
		Material eraseMatProj;
		Material precisionMatProj;
		Material dropMat;
		Mesh brushQuad;

		bool oldAutoSimulation;
		Vector3 oldGravity;
		Vector3[] originalVertices = new Vector3[4];
		Vector3[] newVertices = new Vector3[4];

		Collider[] objectsToCast;

		GameObject[] objectsToPlace;

//		List <GameObject> placedObjects = new List<GameObject> ();
//		List <int> placedObjectsID = new List<int>();
//		List<GameObject> placedObjectsWithCol = new List<GameObject> ();
//		List<Collider> placedObjectsColliders = new List<Collider> ();
//		List<bool> placedObjectsColBool = new List<bool> ();
		List<PlacedObject> placedObjectsPO = new List<PlacedObject> ();
		List<DroppedObject> droppedObjectsDO = new List<DroppedObject> ();
		List <Rigidbody> droppedObjectsRB = new List<Rigidbody> ();
		List <MeshCollider> droppedObjectsMC = new List<MeshCollider> ();

		Vector2 initialMouse;
		int skip;
		int skipped;

		GameObject parent;
		GameObject[] subparents;

		float eraserDensity = 1f;
		Rect eraserDensityRect;

		List <List<Texture2D>> textures = new List<List<Texture2D>>();
		Material bakedMaterial;
		List <Material> bakedMaterials = new List<Material> ();
		List <Texture2D> newTextures = new List<Texture2D>();
		List<int> dummyNewUvsPosition = new List<int> ();
		List<Rect> newUvs = new List<Rect> ();
		Vector3 bakedObjectPosition;
		int[] objectVertexCount;
//		public int vertexCount;

		GameObject projector;
		Projector projectorComp;

		//TODO borrame
		List <Rect>rects = new List<Rect>();

		bool hideInfo = false;
		Rect forbiddenRect;
		Rect forbiddenRect2 = new Rect(0f,0f,0f,0f);
		#endregion

		void OnFocus() {
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
		}

		void OnSceneGUI(SceneView sceneView) {
			if (brushes.Count > 0) {
				current = Event.current;
				controlID = GUIUtility.GetControlID (FocusType.Passive);

				if (current.type == EventType.KeyDown) {
					if (current.keyCode == KeyCode.CapsLock) {
						paintActivated = !paintActivated;
						projectorComp.enabled = !projectorComp.enabled;
						if (paintActivated) {
							Tools.current = Tool.None;
						}
					}
				}

				if (paintActivated) {
					if (!current.control && !current.shift) {
						physicalDrop = false;
						eraser = false;
						precisionPlace = false;
					}
					if (current.control && !current.shift) {
						physicalDrop = true;
						eraser = false;
						precisionPlace = false;
						Physics.autoSimulation = false;
						Physics.gravity = gravity * 10f;
					}
					if (current.shift && !current.control) {
						if (!eraser) {
							FindPlacedObjects ();
						}
						physicalDrop = false;
						eraser = true;
						precisionPlace = false;
					}
					if (current.shift && current.control) {
						physicalDrop = false;
						eraser = false;
						precisionPlace = true;
					}
					if (!current.control) {
						Physics.autoSimulation = oldAutoSimulation;
						Physics.gravity = oldGravity;
					}

					if (current.type == EventType.MouseMove) {
						RayCastSceneView ();
					}
					MouseMechanics ();
					DrawSceneGraphics ();
				}	
			}
			Handles.BeginGUI ();

//			GUILayout.BeginArea (new Rect (500f, 5f, 500f, 500f));
//			if (GUILayout.Button("lolololo")){
//				List <Vector2> isleSizes = new List<Vector2> ();
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one / 2);
//				isleSizes.Add (Vector2.one / 2);
//				isleSizes.Add (Vector2.one / 2);
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one/ 2);
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one / 2);
//				isleSizes.Add (Vector2.one / 2);
//				isleSizes.Add (Vector2.one / 2);
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one/ 2);
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one / 2);
//				isleSizes.Add (Vector2.one / 2);
//				isleSizes.Add (Vector2.one / 2);
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one/ 2);
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one / 2);
//				isleSizes.Add (Vector2.one / 2);
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one / 2);
//				isleSizes.Add (Vector2.one / 2);
//				isleSizes.Add (Vector2.one / 2);
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one); 
//				isleSizes.Add (Vector2.one * 2);
//				isleSizes.Add (Vector2.one * 2);
//				isleSizes.Add (Vector2.one * 2);
//				isleSizes.Add (Vector2.one * 2);
//				isleSizes.Add (Vector2.one/ 2);
//				isleSizes.Add (Vector2.one/ 8);
//				isleSizes.Add (Vector2.one/ 8);
//				isleSizes.Add (Vector2.one/ 8);
//				isleSizes.Add (Vector2.one/ 4);
//				isleSizes.Add (Vector2.one/ 4);
//				isleSizes.Add (Vector2.one/ 4);
//				isleSizes.Add (Vector2.one/ 4);
//
//				rects = GenerateAtlas (isleSizes);
//			}
//			float scale = 400f;
//			for (int i = 0; i < rects.Count; i++){
//				GUI.Box (new Rect (rects[i].x * scale, rects[i].y * scale, rects[i].width * scale, rects[i].height * scale), rects.IndexOf (rects[i]).ToString());
//			}
//			GUILayout.EndArea ();

			forbiddenRect = new Rect (10f, sceneView.position.height - 135f, 310f, 130f);
			GUILayout.BeginArea (new Rect (10f, sceneView.position.height - 135f, 310f, 130f));
			Color bgColor = new Color (0.8f, 0.8f, 0.8f, 0.4f);

			if (!hideInfo && sceneView.position.width > 650f) {
				EditorGUI.DrawRect (new Rect (0f, 0f, 310f, 130f), bgColor);
				if (GUI.Button(new Rect( 5f, 100f, 43f, 15f), "Hide")){
					hideInfo = !hideInfo;
				}
			}
			else {
				forbiddenRect = new Rect (15f, sceneView.position.height - 35f, 43f, 15f);
				if (GUI.Button(new Rect( 5f, 100f, 43f, 15f), "Show")){
					hideInfo = !hideInfo;
				}
			}

			if (sceneView.position.width > 650f && !hideInfo) {
				GUILayout.Space (5f);

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Caps Lock", guiSkin.GetStyle ("LabelLittleBold"));
				GUILayout.Label ("                       Enable/Disable Tool", guiSkin.GetStyle ("LabelLittleSceneView"));
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Shift (Hold)", guiSkin.GetStyle ("LabelLittleBold"));
				GUILayout.Label ("Eraser", guiSkin.GetStyle ("LabelLittleSceneView"));
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Eraser Density", guiSkin.GetStyle ("LabelLittleBold"));
				eraserDensityRect = GUILayoutUtility.GetLastRect ();
				eraserDensityRect = new Rect (eraserDensityRect.x + 130f, eraserDensityRect.y, eraserDensityRect.width - 160f, eraserDensityRect.height);
				eraserDensity = GUI.HorizontalSlider (eraserDensityRect, eraserDensity, 0f, 1f);
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Control (Hold)", guiSkin.GetStyle ("LabelLittleBold"));
				GUILayout.Label ("        Physical Drop", guiSkin.GetStyle ("LabelLittleSceneView"));
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Control + Shift (Hold)", guiSkin.GetStyle ("LabelLittleBold"));
				GUILayout.Label ("       Precision Placement", guiSkin.GetStyle ("LabelLittleSceneView"));
				GUILayout.EndHorizontal ();

			}
			GUILayout.EndArea ();

			if (bakeEnabled) {
				forbiddenRect2 = new Rect (sceneView.position.width - 385f, sceneView.position.height - 70f, 375f, 30f);
				GUI.BeginGroup (forbiddenRect2);
				if (GUI.Button (new Rect (0f, 0f, 210f, 30f), "Bake stroke into a single mesh", guiSkin.button)) {
					BakeMeshes ();
				}
				GUI.Label (new Rect (215f, 0f, 250f, 30f), placedObjectsContainer.vertexCount.ToString () + " / 65536 vertices", guiSkin.GetStyle ("labellittle"));
				GUI.EndGroup ();
			}
			else {
				forbiddenRect2 = new Rect (0f, 0f, 0f, 0f);
			}

			Handles.EndGUI ();
		}

		void Update(){
			if (physicalDrop) {
				Physics.Simulate(0.02f);
			}
		}

		void CleanUp(){
			for (int i = 0; i < placedObjectsContainer.placedObjects.Count; i++) {
				if (placedObjectsContainer.placedObjects [i]) {
					PlacedObject PO = placedObjectsContainer.placedObjects [i].GetComponent<PlacedObject> ();
					DestroyImmediate (PO);
				}
			}
			for (int i = 0; i < droppedObjectsRB.Count; i++) {
				DestroyImmediate (droppedObjectsRB [i]);
			}
			for (int i = 0; i < placedObjectsPO.Count; i++) {
				DestroyImmediate (placedObjectsPO [i]);
			}
			for (int i = 0; i < droppedObjectsMC.Count; i++) {
				DestroyImmediate (droppedObjectsMC [i]);
			}
			RestoreColliders ();
			bakedMaterials.Clear ();
		}

		void RestoreColliders(){
			FindPlacedObjects ();
				
			for (int i = 0; i < placedObjectsContainer.placedObjects.Count; i++) {
				PlacedObject PO = placedObjectsContainer.placedObjects [i].GetComponent<PlacedObject>();
				PO.RestoreColliders ();
			}
		}

		void DrawSceneGraphics(){
			if (rayCast) {
				projectorComp.enabled = true;
				if (eraser) {
					Handles.color = new Color (1f, 0.4f, 0.4f, 1f);
					Handles.DrawWireDisc (mousePoint, mouseNormal, brushSize);
//					eraseMat.SetPass (0);
					if (projector)
						projectorComp.material = eraseMatProj;
				}
				if (precisionPlace) {
					
					if (!painting) {
						Handles.color = new Color (0.4f, 1f, 0.4f, 1f);
						Handles.DrawWireDisc (mousePoint, mouseNormal, brushSize);
						Handles.color = Color.black;
						Handles.DrawWireDisc (mousePoint, mouseNormal, brushSize / 2f);
//					precisionMat.SetPass (0);
						if (projector) {
							projectorComp.material = precisionMatProj;
						}						
					}
					else {
						projectorComp.enabled = false;
					}
				}
				if (!eraser && !precisionPlace) {
					Handles.color = new Color (0.4f, 0.4f, 1f, 1f);
					Handles.DrawWireDisc (mousePoint, mouseNormal, brushSize);
//					brushMat.SetPass (0);
					if (projector)
						projectorComp.material = brushMatProj;
				}

				//Las cosas comentadas son de la forma antigua de pintar, con malla en vez de proyector
//				for (int i = 0; i < brushQuad.vertexCount; i++) {
//					newVertices [i] = originalVertices [i] * brushSize * 2;
//				}
//				brushQuad.vertices = newVertices;
//				Graphics.DrawMeshNow (brushQuad, mousePoint + mouseNormal * 0.05f, Quaternion.LookRotation (mouseNormal));

				if (physicalDrop) {
					projectorComp.enabled = false;
//					Handles.color = new Color (0.4f, 0.4f, 1f, 1f);
//					Handles.DrawWireDisc (mousePoint, mouseNormal, brushSize);
					for (int i = 0; i < brushQuad.vertexCount; i++) {
						newVertices [i] = originalVertices [i] * brushSize * 2;
					}
					brushQuad.vertices = newVertices;
					dropMat.SetPass (0);
					Vector3 lookAtVector = (mousePoint + -Vector3.Normalize (gravity) * physDropHeight) - SceneView.currentDrawingSceneView.camera.transform.position;
					Graphics.DrawMeshNow (brushQuad, mousePoint + -Vector3.Normalize (gravity) * physDropHeight, Quaternion.LookRotation (lookAtVector));
				}
			}
		}

		void MouseMechanics(){
			if (!forbiddenRect.Contains(current.mousePosition) && !forbiddenRect2.Contains(current.mousePosition)) {
				switch (current.type) {
				case EventType.MouseUp:
					{
						if (painting && current.button == 0) {
							StopPaint ();
						}
						break;
					}
				case EventType.MouseDown:
					{
						if (rayCast && current.button == 0 && !current.alt) {
							StartPaint ();
						}
						break;
					}
				case EventType.MouseDrag:
					{
						if (rayCast && current.button == 0 && painting && !precisionPlace) {
							Paint (false);
							RayCastSceneView ();
						}
						else if (precisionPlace) {
							PrecisionPlace ();
							RayCastSceneView ();
						}
						break;
					}
				//Aquí dice que mientras estña la ventana dibujandose o algo similar, quien tiene el control del sceneview es esta ventana (creo)
				case EventType.Layout:
					HandleUtility.AddDefaultControl (controlID);
					break;
				}
			}
		}

		void PrecisionPlace(){
			if (!precisionPlaceFlag) {
				precisionPlaceObject.transform.rotation = precisionPlaceOriginalRot;
				precisionPlaceFlag = true;
			}

			Vector2 distance = initialMouse - current.mousePosition;
			float mouseDistance = Mathf.Sqrt (Mathf.Pow (distance.x, 2) + Mathf.Pow (distance.y, 2)) / 1000f;
			mouseDistance *= Vector3.Distance (SceneView.currentDrawingSceneView.camera.transform.position, precisionPlaceObject.transform.position);

			precisionPlaceObject.transform.localScale = new Vector3 (mouseDistance * precisionPlaceParentScale.x, mouseDistance * precisionPlaceParentScale.y, mouseDistance * precisionPlaceParentScale.z);

			float angle;
			Vector3 PrecisionPlaceDir;
			ObjectSettings objectSettings = brushes [selectedBrush].objects [precisionPlaceObjectID];

			if (!objectSettings.enablePerObject) {
				if (alignToSurface) {
					if (alignTo == 0) {
						PrecisionPlaceDir = Vector3.ProjectOnPlane (mousePoint - precisionPlaceObject.transform.position, precisionPlaceObject.transform.right);
						angle = -Vector3.SignedAngle (PrecisionPlaceDir, lastPrecisionPlaceDir, precisionPlaceObject.transform.right);

						precisionPlaceObject.transform.RotateAround (precisionPlaceObject.transform.position, precisionPlaceObject.transform.right, angle);

						lastPrecisionPlaceDir = PrecisionPlaceDir;
					}
					if (alignTo == 1) {
						PrecisionPlaceDir = Vector3.ProjectOnPlane (mousePoint - precisionPlaceObject.transform.position, precisionPlaceObject.transform.up);
						angle = -Vector3.SignedAngle (PrecisionPlaceDir, lastPrecisionPlaceDir, precisionPlaceObject.transform.up);

						precisionPlaceObject.transform.RotateAround (precisionPlaceObject.transform.position, precisionPlaceObject.transform.up, angle);

						lastPrecisionPlaceDir = PrecisionPlaceDir;
					}
					if (alignTo == 2) {
						PrecisionPlaceDir = Vector3.ProjectOnPlane (mousePoint - precisionPlaceObject.transform.position, precisionPlaceObject.transform.forward);
						angle = -Vector3.SignedAngle (PrecisionPlaceDir, lastPrecisionPlaceDir, precisionPlaceObject.transform.forward);

						precisionPlaceObject.transform.RotateAround (precisionPlaceObject.transform.position, precisionPlaceObject.transform.forward, angle);

						lastPrecisionPlaceDir = PrecisionPlaceDir;
					}
				} else {
					PrecisionPlaceDir = Vector3.ProjectOnPlane (mousePoint - precisionPlaceObject.transform.position, precisionPlaceObject.transform.up);
					angle = -Vector3.SignedAngle (PrecisionPlaceDir, lastPrecisionPlaceDir, precisionPlaceObject.transform.up);

					precisionPlaceObject.transform.RotateAround (precisionPlaceObject.transform.position, precisionPlaceObject.transform.up, angle);

					lastPrecisionPlaceDir = PrecisionPlaceDir;
				}
			}
			else {
				if (objectSettings.alignToSurface) {
					if (objectSettings.alignTo == 0) {
						PrecisionPlaceDir = Vector3.ProjectOnPlane (mousePoint - precisionPlaceObject.transform.position, precisionPlaceObject.transform.right);
						angle = -Vector3.SignedAngle (PrecisionPlaceDir, lastPrecisionPlaceDir, precisionPlaceObject.transform.right);

						precisionPlaceObject.transform.RotateAround (precisionPlaceObject.transform.position, precisionPlaceObject.transform.right, angle);

						lastPrecisionPlaceDir = PrecisionPlaceDir;
					}
					if (objectSettings.alignTo == 1) {
						PrecisionPlaceDir = Vector3.ProjectOnPlane (mousePoint - precisionPlaceObject.transform.position, precisionPlaceObject.transform.up);
						angle = -Vector3.SignedAngle (PrecisionPlaceDir, lastPrecisionPlaceDir, precisionPlaceObject.transform.up);

						precisionPlaceObject.transform.RotateAround (precisionPlaceObject.transform.position, precisionPlaceObject.transform.up, angle);

						lastPrecisionPlaceDir = PrecisionPlaceDir;
					}
					if (objectSettings.alignTo == 2) {
						PrecisionPlaceDir = Vector3.ProjectOnPlane (mousePoint - precisionPlaceObject.transform.position, precisionPlaceObject.transform.forward);
						angle = -Vector3.SignedAngle (PrecisionPlaceDir, lastPrecisionPlaceDir, precisionPlaceObject.transform.forward);

						precisionPlaceObject.transform.RotateAround (precisionPlaceObject.transform.position, precisionPlaceObject.transform.forward, angle);

						lastPrecisionPlaceDir = PrecisionPlaceDir;
					}
				} else {
					PrecisionPlaceDir = Vector3.ProjectOnPlane (mousePoint - precisionPlaceObject.transform.position, precisionPlaceObject.transform.up);
					angle = -Vector3.SignedAngle (PrecisionPlaceDir, lastPrecisionPlaceDir, precisionPlaceObject.transform.up);

					precisionPlaceObject.transform.RotateAround (precisionPlaceObject.transform.position, precisionPlaceObject.transform.up, angle);

					lastPrecisionPlaceDir = PrecisionPlaceDir;
				}
			}
		}

		void StopPaint(){
			if (!physicalDrop) {
				RestoreColliders ();
			}
			painting = false;
			precisionPlaceFlag = false;
			Resources.UnloadUnusedAssets ();
			System.GC.Collect ();
		}

		void StartPaint(){
			painting = true;
			initialMouse = Event.current.mousePosition;
			lastMouse = mousePoint;
			bakedObjectPosition = mousePoint;

			if (!eraser) {
				skip = spacing;

				subparents = new GameObject[brushes[selectedBrush].objects.Count];
				parent = GameObject.Find ("PPB_" + brushesNames [selectedBrush]);

				if (parent == null) {
					parent = new GameObject ();
					parent.name = "PPB_" + brushesNames [selectedBrush];
					parent.transform.position = mousePoint;
					Undo.RegisterCreatedObjectUndo (parent, "PPB Created Parent");
				}

				for (int i = 0; i < subparents.Length; i++) {
					subparents [i] = GameObject.Find ("PPB_" + brushesNames [selectedBrush] + "_" + brushes [selectedBrush].objects [i].name);

					if (subparents [i] == null && groupByObject) {
						subparents [i] = new GameObject ();
						subparents [i].name = "PPB_" + brushesNames [selectedBrush] + "_" + brushes [selectedBrush].objects [i].name;
						subparents [i].transform.parent = parent.transform;
						Undo.RegisterCreatedObjectUndo (subparents [i], "PPB Created Subparent");
					}
				}
			}
			else {				
				skip = Mathf.CeilToInt (Mathf.Abs (1f - eraserDensity) * 100);
			}
			skipped = skip;
			LoadObjects ();
			if (precisionPlace) {
				PlaceObject (mousePoint, mouseNormal, false);
				initialMouse = current.mousePosition;
			}
			else {
				Paint (true);
			}
		}

		void Paint(bool firstPaint){
			if (!eraser) {
				foreach (float den in densitiesContainer.densities) {
					if (den > 0) {
						GeneratePoints (firstPaint);
						break;
					}
				}
			}
			else {
				ErasePlacedObjects ();
			}
		}
			
		void BakeMaterials(){
			if (brushes [selectedBrush].objects.Count > 0) {
				bakeMaterial = true;
				bakeMeshes = true;
				textures.Clear ();
				newTextures.Clear ();
				brushes [selectedBrush].duplicatedObjects.Clear ();
				brushes [selectedBrush].duplicatedObjectsRef.Clear ();
				dummyNewUvsPosition.Clear ();
				newUvs.Clear ();

				bool alreadyExists;
				for (int i = 0; i < brushes [selectedBrush].objects.Count; i++) {
					GameObject prefabSource = (GameObject)AssetDatabase.LoadAssetAtPath (brushes [selectedBrush].objects [i].path, typeof(GameObject));

					if (prefabSource.GetComponent<MeshRenderer> ().sharedMaterials.Length > 1) {
						Debug.LogError ("Detected more than one material in a single object. Multimaterial is not supported by the bake system.", prefabSource);
						ReadBrush (selectedBrush);
						return;
					}

					Material materialSource = prefabSource.GetComponent<MeshRenderer> ().sharedMaterial;
					if (i == 0) {
						bakedMaterials.Add (materialSource);
					} 
					else {
						alreadyExists = false;
						for (int o = 0; o < bakedMaterials.Count; o++) {
							if (materialSource == bakedMaterials [o]) {
								alreadyExists = true;
//							Debug.Log ("Detected duplicated material at index " + i + ". Using data of object at index " + o + "." );
								brushes [selectedBrush].duplicatedObjects.Add (i);
								brushes [selectedBrush].duplicatedObjectsRef.Add (o);
								dummyNewUvsPosition.Add (o);
								break;
							}
						}
						if (!alreadyExists) {
							bakedMaterials.Add (materialSource);
						}
					}
				}

				List <string> propertiesId = new List<string> ();
				List <TextureImporterType> textureType = new List<TextureImporterType> ();
				List <bool> hasAlpha = new List<bool> ();

				GameObject go = (GameObject)AssetDatabase.LoadAssetAtPath (brushes [selectedBrush].objects [0].path, typeof(GameObject));
				Material material = go.GetComponent<MeshRenderer> ().sharedMaterial;

				if (material) {
					Shader shader = material.shader;

					bakedMaterial = Instantiate (material);

					int toRemove = brushesNames [selectedBrush].Length + 6;
					string path = AssetDatabase.CreateFolder (AssetDatabase.GUIDToAssetPath (brushesPaths [selectedBrush]).Substring (0, AssetDatabase.GUIDToAssetPath (brushesPaths [selectedBrush]).Length - toRemove), brushesNames [selectedBrush]);

					for (int o = 0; o < ShaderUtil.GetPropertyCount (shader); o++) {
						if (ShaderUtil.GetPropertyType (shader, o) == ShaderUtil.ShaderPropertyType.TexEnv) {
							string propertyName = ShaderUtil.GetPropertyName (shader, o);
//							
							Texture2D texture = material.GetTexture (propertyName) as Texture2D;
							if (texture) {
								if (propertyName != "_DetailNormalMap" && propertyName != "_DetailAlbedoMap" && !propertyName.Contains("Detail") && !propertyName.Contains("detail")  && !propertyName.Contains("DETAIL")) {
									propertiesId.Add (propertyName);
									brushes [selectedBrush].hasBakedTextures = true;
								}
								else {
									Debug.Log ("Detail maps found in the main object. Maybe the tiling in the resulting material isn't right. Cheack it yourself for a better looking");
								}
							}
						}
					}
				
					for (int i = 0; i < propertiesId.Count; i++) {
						textures.Add (new List<Texture2D> ());
						for (int o = 0; o < brushes [selectedBrush].objects.Count; o++) {
							if (!brushes [selectedBrush].duplicatedObjects.Contains (o)) {
								go = (GameObject)AssetDatabase.LoadAssetAtPath (brushes [selectedBrush].objects [o].path, typeof(GameObject));
								material = go.GetComponent<MeshRenderer> ().sharedMaterial;
								Color fillColor = Color.gray;
								if (material) {
									if (material.HasProperty (propertiesId [i])) {
										Texture2D texture = material.GetTexture (propertiesId [i]) as Texture2D;
										if (texture) {
											EditorUtility.DisplayProgressBar ("Baking materials...", "Computing property: " + propertiesId [i] + ". " + "Texture: " + texture.name, (float)o / (float)brushes [selectedBrush].objects.Count);
											string texturePath = AssetDatabase.GetAssetPath (texture);
											TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath (texturePath);
											if (o == 0) {
												hasAlpha.Add (importer.DoesSourceTextureHaveAlpha ());
												textureType.Add (importer.textureType);
											}
											bool oldIsReadable = importer.isReadable;
											TextureImporterCompression oldCompression = importer.textureCompression;
											bool oldConvertToNM = importer.convertToNormalmap;
											if (oldConvertToNM) {
												fillColor = new Color (0.5f, 0.5f, 1f);
											}
											TextureImporterType oldTextureType = importer.textureType;

											importer.isReadable = true;
											importer.textureCompression = TextureImporterCompression.Uncompressed;
											importer.convertToNormalmap = false;
											importer.textureType = TextureImporterType.Default;
											AssetDatabase.ImportAsset (texturePath, ImportAssetOptions.ForceUpdate);

											textures [i].Add (Texture2D.Instantiate (texture));

											importer.isReadable = oldIsReadable;
											importer.textureCompression = oldCompression;
											importer.convertToNormalmap = oldConvertToNM;
											importer.textureType = oldTextureType;
											AssetDatabase.ImportAsset (texturePath, ImportAssetOptions.ForceUpdate);
										} else {
											textures [i].Add (new Texture2D (textures [i] [0].width, textures [i] [0].height, TextureFormat.RGB24, true));
											Color[] colors = new Color[textures [i] [0].width * textures [i] [0].height];
											for (int u = 0; u < colors.Length; u++) {
												colors [u] = fillColor;
											}
											textures [i] [textures [i].Count - 1].SetPixels (colors);
										}
									} else {
										//Lo mismo que si no hay textura pero sí material
										textures [i].Add (new Texture2D (textures [i] [0].width, textures [i] [0].height, TextureFormat.RGB24, true));
										Color[] colors = new Color[textures [i] [0].width * textures [i] [0].height];
										for (int u = 0; u < colors.Length; u++) {
											colors [u] = fillColor;
										}
										textures [i] [textures [i].Count - 1].SetPixels (colors);
									}
								}
								else {
									//Lo mismo que si no hay textura ni material
									textures [i].Add (new Texture2D (textures [i] [0].width, textures [i] [0].height, TextureFormat.RGB24, true));
									Color[] colors = new Color[textures [i] [0].width * textures [i] [0].height];
									for (int u = 0; u < colors.Length; u++) {
										colors [u] = fillColor;
									}
									textures [i] [textures [i].Count - 1].SetPixels (colors);
								}
							}
						}
					}

					if (propertiesId.Count > 0) {
						string newTexturePath;
						TextureImporter importer;

						Texture2D mainAtlasTexture;
						mainAtlasTexture = new Texture2D (256, 256, TextureFormat.ARGB32, true);
						brushes [selectedBrush].bakedUvs = mainAtlasTexture.PackTextures (textures [0].ToArray (), 0);
						brushes [selectedBrush].bakedUvsWithDummies = brushes [selectedBrush].bakedUvs;

						Texture2D finalmainAtlasTexture;
						if (hasAlpha [0]) {
							finalmainAtlasTexture = new Texture2D (mainAtlasTexture.width, mainAtlasTexture.height, TextureFormat.ARGB32, true);
							finalmainAtlasTexture.SetPixels32 (mainAtlasTexture.GetPixels32 ());
						} 
						else {
							finalmainAtlasTexture = new Texture2D (mainAtlasTexture.width, mainAtlasTexture.height, TextureFormat.RGB24, true);
							finalmainAtlasTexture.SetPixels (mainAtlasTexture.GetPixels ());
						}

						finalmainAtlasTexture.Apply ();
						byte[] bytes = finalmainAtlasTexture.EncodeToPNG ();
						newTexturePath = AssetDatabase.GUIDToAssetPath (path).Substring (6) + "/" + brushesNames [selectedBrush] + " " + propertiesId [0] + ".png";
						File.WriteAllBytes (Application.dataPath + newTexturePath, bytes);

						AssetDatabase.SaveAssets ();
						AssetDatabase.Refresh ();

						newTextures.Add ((Texture2D)AssetDatabase.LoadAssetAtPath (AssetDatabase.GUIDToAssetPath (path) + "/" + brushesNames [selectedBrush] + " " + propertiesId [0] + ".png", typeof(Texture2D)));
						importer = (TextureImporter)TextureImporter.GetAtPath ("Assets" + newTexturePath);
						importer.textureType = textureType [0];
						AssetDatabase.ImportAsset ("Assets" + newTexturePath, ImportAssetOptions.ForceUpdate);

						for (int i = 1; i < textures.Count; i++) {
							EditorUtility.DisplayProgressBar ("Baking materials...", "Generating atlas for property " + propertiesId [i], (float)i / (float)textures.Count);
							for (int o = 0; o < textures [i].Count; o++) {
								Texture2D tempTex = new Texture2D (textures [i] [o].width, textures [i] [o].height, TextureFormat.ARGB32, true);
								Color32[] colors = textures [i] [o].GetPixels32 ();
								tempTex.SetPixels32 (colors);

								TextureScale.Bilinear (tempTex, textures [0] [o].width, textures [0] [o].height);
								textures [i] [o] = tempTex;
							}

							Texture2D atlasTexture;
							atlasTexture = new Texture2D (256, 256, TextureFormat.ARGB32, true);
							atlasTexture.PackTextures (textures [i].ToArray (), 0);

							Texture2D finalAtlasTexture = new Texture2D (atlasTexture.width, atlasTexture.height, TextureFormat.ARGB32, true);
							if (hasAlpha [i]) {
								finalAtlasTexture = new Texture2D (atlasTexture.width, atlasTexture.height, TextureFormat.ARGB32, true);
								finalAtlasTexture.SetPixels32 (atlasTexture.GetPixels32 ());
							} else {
								finalAtlasTexture = new Texture2D (atlasTexture.width, atlasTexture.height, TextureFormat.RGB24, true);
								finalAtlasTexture.SetPixels (atlasTexture.GetPixels ());
							}

							finalAtlasTexture.Apply ();
							bytes = finalAtlasTexture.EncodeToPNG ();
							newTexturePath = AssetDatabase.GUIDToAssetPath (path).Substring (6) + "/" + brushesNames [selectedBrush] + " " + propertiesId [i] + ".png";
							File.WriteAllBytes (Application.dataPath + newTexturePath, bytes);

							AssetDatabase.SaveAssets ();
							AssetDatabase.Refresh ();

							newTextures.Add ((Texture2D)AssetDatabase.LoadAssetAtPath ("Assets" + newTexturePath, typeof(Texture2D)));

							importer = (TextureImporter)TextureImporter.GetAtPath ("Assets" + newTexturePath);
							importer.textureType = textureType [i];
							AssetDatabase.ImportAsset ("Assets" + newTexturePath, ImportAssetOptions.ForceUpdate);
						}

						AssetDatabase.SaveAssets ();
						AssetDatabase.Refresh ();

						for (int i = 0; i < propertiesId.Count; i++) {
							bakedMaterial.SetTexture (propertiesId [i], newTextures [i]);
						}
					} 
					else {
						Debug.LogWarning ("Main Texture not found in the first object of the brush. The texture bake system is based on the Main Texture of the first object's material.");
					}

					AssetDatabase.CreateAsset (bakedMaterial, AssetDatabase.GUIDToAssetPath (path) + "/" + brushesNames [selectedBrush] + ".mat");
					brushes [selectedBrush].bakedMaterialGUID = AssetDatabase.AssetPathToGUID (AssetDatabase.GUIDToAssetPath (path) + "/" + brushesNames [selectedBrush] + ".mat");

					AssetDatabase.Refresh ();

				}
				else {
					Debug.LogError ("Material not found in the first object of the brush. The bake material system is based on the first material of the first object");
					bakeEnabled = false;
					bakeMessageAccepted = false;
				}

				textures.Clear ();
				newTextures.Clear ();

				if (brushes [selectedBrush].hasBakedTextures) {
					for (int i = 0; i < brushes [selectedBrush].bakedUvsWithDummies.Length; i++) {
						newUvs.Add (brushes [selectedBrush].bakedUvsWithDummies [i]);
					}
					
					for (int i = dummyNewUvsPosition.Count - 1; i >= 0; i--) {
						newUvs.Insert (dummyNewUvsPosition [i] + 1, new Rect ());
					}
				}

				brushes [selectedBrush].bakedUvsWithDummies = newUvs.ToArray ();

				EditorUtility.ClearProgressBar ();
				EditorGUIUtility.PingObject (bakedMaterial);
			}
			else {
				Debug.LogWarning ("No objects in the current brush. Nothing to bake");
			}
		}

		List <Rect> GenerateAtlas(List<Vector2> islesSizes){


			float maxX = 0;
			float maxY = 0;
			bool xY = false;
			int stripe = 0;

			for (int i = 0; i < islesSizes.Count; i++) {
				
				if (i == 0) {
					rects.Add (new Rect (Vector2.zero, islesSizes [i]));
					maxX += islesSizes [i].x;
					maxY += islesSizes [i].y;
					continue;
				}
				else {
					if (maxX == maxY || maxY > maxX) {
						if (!xY) {
							rects.Add (new Rect (maxX, 0f, islesSizes [i].x, islesSizes [i].y));
							maxX += islesSizes [i].x;
							continue;
						}
						else {
							if (rects [rects.Count - 1].x + rects [rects.Count - 1].width + islesSizes [i].x <= maxX) {
								rects.Add (new Rect (rects [rects.Count - 1].x + rects [rects.Count - 1].width, rects [rects.Count - 1].y, islesSizes [i].x, islesSizes [i].y));
								stripe += 1;
								continue;
							}
							else {
								if (rects [rects.Count - 1].y + rects [rects.Count - 1].height + islesSizes [i].y <= maxY) {
									rects.Add (new Rect (rects [rects.Count - stripe].x, rects [rects.Count - stripe].y + rects [rects.Count - stripe].height, islesSizes [i].x, islesSizes [i].y));
									stripe = 1;
									continue;
								}
								else {
									rects.Add (new Rect (maxX, 0f, islesSizes [i].x, islesSizes [i].y));
									maxX += islesSizes [i].x;
									stripe = 1;
									continue;
								}
							}
						}
					}
					if (maxX > maxY) {
						if (rects [rects.Count - 1].x + rects [rects.Count - 1].width + islesSizes [i].x <= maxX) {
							rects.Add (new Rect (rects [rects.Count - 1].x + rects [rects.Count - 1].width, rects [rects.Count - 1].y, islesSizes [i].x, islesSizes [i].y));
							continue;
						}
						if (rects [rects.Count - 1].y + rects [rects.Count - 1].height + islesSizes [i].y <= maxY) {
							rects.Add (new Rect (rects [rects.Count - 1].x, rects [rects.Count -1].y + rects [rects.Count -1].height, islesSizes [i].x, islesSizes [i].y));
							continue;
						}
						if ( !(rects [rects.Count - 1].x + rects [rects.Count - 1].width + islesSizes [i].x <= maxX) && !(rects [rects.Count - 1].y + rects [rects.Count - 1].height + islesSizes [i].y <= maxY)){
							rects.Add (new Rect (0f, maxY, islesSizes [i].x, islesSizes [i].y));
							maxY += islesSizes [i].y;
							xY = true;
							continue;
						}
					}
				}
			}

			for (int i = 0; i < rects.Count; i++) {
				rects [i] = new Rect (rects [i].x / maxX, rects [i].y / maxY, rects [i].width / maxX, rects [i].height / maxY);
			}

//			Debug.Log (rects.Count);

			return rects;
		}

		void FindPlacedObjects(){
			List <GameObject> placedObjects = new List<GameObject>();
			List <int> placedObjectsID = new List<int>();
			PlacedObject pO;

			if (parent) {
				for (int i = 0; i < parent.transform.childCount; i++) {
					pO = parent.transform.GetChild (i).GetComponent<PlacedObject> ();
					if (pO) {
						placedObjects.Add (parent.transform.GetChild (i).gameObject);
						placedObjectsID.Add (pO.ID);
					}
				}
			
			foreach (GameObject subParent in subparents) {
				if (subParent) {
					for (int i = 0; i < subParent.transform.childCount; i++) {
						pO = subParent.transform.GetChild (i).GetComponent<PlacedObject> ();
						if (pO) {
							placedObjects.Add (subParent.transform.GetChild (i).gameObject);
							placedObjectsID.Add (pO.ID);
						}
					}
				}
				}
			}

			placedObjectsContainer.placedObjects = placedObjects;
			placedObjectsContainer.placedObjectsID = placedObjectsID;
		}

		void BakeMeshes(){
			if (placedObjectsContainer.vertexCount < 65536) {
				FindPlacedObjects ();
			
				if (placedObjectsContainer.placedObjects.Count > 0 && placedObjectsContainer.vertexCount > 0) {
					Mesh bakedMesh = new Mesh ();
					bakedMesh.name = brushesNames [selectedBrush] + "_BakedStroke";
					Mesh bakedCollisionMesh = new Mesh ();
					bakedCollisionMesh.name = brushesNames [selectedBrush] + "_BakedStrokeCol";
					GameObject bakedObject = new GameObject ();
					if (makeStatic) {
						bakedObject.isStatic = true;
					}
					Undo.RegisterCreatedObjectUndo (bakedObject, "PPB Bake Stroke");
					bakedObject.transform.parent = parent.transform;
					bakedObject.transform.position = bakedObjectPosition;
					bakedObject.name = brushesNames [selectedBrush] + "_Stroke";
					MeshFilter bakedMF = bakedObject.AddComponent<MeshFilter> ();
					MeshRenderer bakedMR = bakedObject.AddComponent<MeshRenderer> ();
					MeshCollider bakedMC = null;

					CombineInstance[] combine;

					List <GameObject> placedObjectsSorted = new List<GameObject> ();
					List <int> placedObjectsOldOrder = new List<int> ();
					List <Rect> uvs2Rects = new List<Rect> ();
					List <Vector2> islesSizes = new List<Vector2> ();
					if (generateLMUvs && useExistingUv2) {
						for (int i = 0; i < placedObjectsContainer.placedObjects.Count; i++) {
							if (placedObjectsContainer.placedObjects [i]) {							
								placedObjectsSorted.Add (placedObjectsContainer.placedObjects [i]);
							}
						}

						placedObjectsSorted.Sort (delegate(GameObject a, GameObject b) {
							return (b.GetComponent<MeshFilter> ().sharedMesh.bounds.size.magnitude * b.transform.localScale.magnitude).CompareTo (a.GetComponent<MeshFilter> ().sharedMesh.bounds.size.magnitude * a.transform.localScale.magnitude);
						}
						);

						for (int i = 0; i < placedObjectsContainer.placedObjects.Count; i++) {
							if (placedObjectsContainer.placedObjects [i]) {							
								placedObjectsOldOrder.Add (placedObjectsSorted.IndexOf (placedObjectsContainer.placedObjects [i]));
							}
						}

						for (int i = 0; i < placedObjectsSorted.Count; i++) {
							Mesh mesh = Mesh.Instantiate (placedObjectsSorted [i].GetComponent<MeshFilter> ().sharedMesh);
							if (mesh.uv2.Length > 0) {
								GameObject currentObject = placedObjectsSorted [i];

								islesSizes.Add (new Vector2 (Vector3.Magnitude (mesh.bounds.size / 3) * Vector3.Magnitude (currentObject.transform.localScale), Vector3.Magnitude (mesh.bounds.size / 3) * Vector3.Magnitude (currentObject.transform.localScale)));
							} else {
								break;
							}
						}
						uvs2Rects = GenerateAtlas (islesSizes);
					}


					for (int i = 0; i < placedObjectsContainer.placedObjects.Count; i++) {
						GameObject currentObject = placedObjectsContainer.placedObjects [i];
						if (placedObjectsContainer.placedObjects [i]) {
							EditorUtility.DisplayProgressBar ("Baking meshes...", "Mesh: " + placedObjectsContainer.placedObjects [i].name, (float)i / (float)placedObjectsContainer.placedObjects.Count);

							Rect bakedUv = new Rect ();
							if (brushes [selectedBrush].hasBakedTextures) {
								if (!brushes [selectedBrush].duplicatedObjects.Contains (placedObjectsContainer.placedObjectsID [i])) {
									bakedUv = brushes [selectedBrush].bakedUvsWithDummies [placedObjectsContainer.placedObjectsID [i]];
								} else {
									bakedUv = brushes [selectedBrush].bakedUvs [brushes [selectedBrush].duplicatedObjectsRef [brushes [selectedBrush].duplicatedObjects.IndexOf (placedObjectsContainer.placedObjectsID [i])]];
								}
							}

							Mesh mesh = Mesh.Instantiate (placedObjectsContainer.placedObjects [i].GetComponent<MeshFilter> ().sharedMesh);

							Mesh colMesh = null;
							if (placedObjectsContainer.placedObjects [i].GetComponent<MeshCollider> ()) {
								colMesh = Mesh.Instantiate (placedObjectsContainer.placedObjects [i].GetComponent<MeshCollider> ().sharedMesh);						
								if (colMesh && !bakedMC) {
									bakedMC = bakedObject.AddComponent<MeshCollider> ();
								}
							}

							List <Vector3> vertices = new List<Vector3> ();
							List <Vector3> normals = new List<Vector3> ();
							List <Vector2> uvs = new List<Vector2> ();
							List <Vector2> uvs2 = new List<Vector2> ();
							List <Vector3> collisionVertices = new List<Vector3> ();

							for (int v = 0; v < mesh.vertexCount; v++) {
								Vector3 vertex = mesh.vertices [v];
								vertex = new Vector3 (vertex.x * currentObject.transform.localScale.x, vertex.y * currentObject.transform.localScale.y, vertex.z * currentObject.transform.localScale.z);
								vertex = currentObject.transform.rotation * vertex;
								vertex = vertex + currentObject.transform.position - bakedObjectPosition;

								if (colMesh) {
									Vector3 colVertex = mesh.vertices [v];
									colVertex = new Vector3 (colVertex.x * currentObject.transform.localScale.x, colVertex.y * currentObject.transform.localScale.y, colVertex.z * currentObject.transform.localScale.z);
									colVertex = currentObject.transform.rotation * colVertex;
									colVertex = colVertex + currentObject.transform.position - bakedObjectPosition;
									collisionVertices.Add (colVertex);
								}

								vertices.Add (vertex);

								Vector3 normal = currentObject.transform.rotation * mesh.normals [v];
								normals.Add (normal);

								if (brushes [selectedBrush].hasBakedTextures && mesh.uv.Length > 0) {
									Vector2 uv = mesh.uv [v];
									uv = new Vector2 (uv.x * bakedUv.width + bakedUv.x, uv.y * bakedUv.height + bakedUv.y);
									uvs.Add (uv);
								}
								
								if (generateLMUvs && useExistingUv2) {
									if (mesh.uv2.Length > 0) {
										Vector2 uv2 = mesh.uv2 [v];
										uv2 = new Vector2 (uv2.x * uvs2Rects [placedObjectsOldOrder [i]].width + uvs2Rects [placedObjectsOldOrder [i]].x, uv2.y * uvs2Rects [placedObjectsOldOrder [i]].height + uvs2Rects [placedObjectsOldOrder [i]].y);
										uvs2.Add (uv2);
									} else {
										Debug.LogWarning ("Use Existing Uv2 for lightmap Uvs generation is ON, but no Uv2 set found in all meshes your're trying to bake");									
									}
								}
							}

							mesh.vertices = vertices.ToArray ();
							mesh.normals = normals.ToArray ();
							mesh.uv = uvs.ToArray ();
							if (generateLMUvs && useExistingUv2 && mesh.uv2.Length > 0) {
								mesh.uv2 = uvs2.ToArray ();
							}

							Mesh tempMesh = Mesh.Instantiate (bakedMesh);
							combine = new CombineInstance[2];
							combine [0].mesh = tempMesh;
							combine [1].mesh = mesh;

							bakedMesh.CombineMeshes (combine, true, false);

							if (colMesh) {
								colMesh.vertices = collisionVertices.ToArray ();

								Mesh tempColMesh = Mesh.Instantiate (bakedCollisionMesh);
								combine = new CombineInstance[2];
								combine [0].mesh = tempColMesh;
								combine [1].mesh = colMesh;

								bakedCollisionMesh.CombineMeshes (combine, true, false);
							}

							Undo.DestroyObjectImmediate (placedObjectsContainer.placedObjects [i]);
						}
					}

					if (generateLMUvs && !useExistingUv2) {
						EditorUtility.DisplayProgressBar ("Generating Lightmap UVs...", "", 0.5f);
						Unwrapping.GenerateSecondaryUVSet (bakedMesh);
						EditorUtility.ClearProgressBar ();
					}

					bakedMesh.RecalculateBounds ();
					bakedMesh.RecalculateTangents ();
					bakedMF.mesh = bakedMesh;
					bakedMR.material = bakedMaterial;

					if (bakedMC) {
						bakedMC.sharedMesh = bakedCollisionMesh;
					}

					placedObjectsSorted.Clear ();
					placedObjectsOldOrder.Clear ();
					uvs2Rects.Clear ();
					islesSizes.Clear ();
					placedObjectsContainer.vertexCount = 0;
					EditorUtility.ClearProgressBar ();

					for (int i = 0; i < subparents.Length; i++) {
						if (subparents [i]) {
							if (subparents [i].transform.childCount == 0) {
								DestroyImmediate (subparents [i]);
							}
						}
					}
				}
			}
			else {
				Debug.LogError ("Current stroke has too many vertices. Unity's limit for a single mesh is 65536. Please, remove elements before baking");
			}
		}
			
		Collider[] GetEnvironmentObjects(Vector3 point, Vector3 normal, float radius, LayerMask layerMask){
			Vector3 myVector3 = new Vector3 (radius, radius, 0.05f);
			Collider[] objectsColliders = Physics.OverlapBox (point, myVector3, Quaternion.LookRotation (normal), layerMask);

			return objectsColliders;
		}

		void ErasePlacedObjects(){
			skipped = Mathf.CeilToInt (Vector2.Distance (Event.current.mousePosition, initialMouse));
			if (skipped < skip) {

			} else {
				Bounds bound = new Bounds (mousePoint, Vector3.one * brushSize);

				for (int i = 0; i < placedObjectsContainer.placedObjects.Count; i++) {
					if (placedObjectsContainer.placedObjects [i]) {
						if (Random.value < eraserDensity) {
							if (bound.Contains (placedObjectsContainer.placedObjects [i].transform.position) && Vector3.Magnitude (placedObjectsContainer.placedObjects [i].transform.position - mousePoint) < brushSize) {
								
								Undo.DestroyObjectImmediate (placedObjectsContainer.placedObjects [i]);
//								DestroyImmediate (placedObjects [i]);
//								placedObjects.Remove (placedObjects [i]);
							}
						}
					}
				}
				skipped = 0;
				lastMouse = mousePoint;
				initialMouse = Event.current.mousePosition;
			}
		}

		void RayCastSceneView(){
			Vector2 mouseScreenPos = Event.current.mousePosition;
			Ray ray = HandleUtility.GUIPointToWorldRay (mouseScreenPos);
			RaycastHit RC;
			if (Physics.Raycast (ray, out RC, 2000f, layerMask.value)) {
				SceneView.lastActiveSceneView.Repaint ();
				mousePoint = RC.point;
				mouseNormal = RC.normal;

				rayCast = true;

				objectsToCast = GetEnvironmentObjects (mousePoint, mouseNormal, brushSize, layerMask);

				if (projector) {
					projector.transform.position = mousePoint;
					projector.transform.rotation = Quaternion.LookRotation (-mouseNormal);
					projectorComp.orthographicSize = brushSize;
				}
			}
			else {
				rayCast = false;
			}
		}

		void GeneratePoints(bool firstPaint){
			if (!firstPaint) {
				skipped = Mathf.CeilToInt (Vector2.Distance (Event.current.mousePosition, initialMouse));
			}
			else {
				skipped = skip;
			}
			if (skipped < skip) {

			} else {				
				mouseDirection = Vector3.Normalize (mousePoint - lastMouse);

				Quaternion rotation = Quaternion.FromToRotation (Vector3.forward, mouseNormal);

				float brushArea = Mathf.Pow (brushSize, 2f) * Mathf.PI;
				int objectsAmount = Mathf.CeilToInt (brushArea * density);

				for (int i = 0; i < objectsAmount; i++) {
					Vector3 randomPoint = rotation * (new Vector3 (Random.insideUnitCircle.x, Random.insideUnitCircle.y, 0f) * brushSize) + mousePoint;

					Ray ray = new Ray (randomPoint + mouseNormal, -mouseNormal);
					RaycastHit RC;

					for (int o = 0; o < objectsToCast.Length; o++) {
						if (objectsToCast [o].Raycast (ray, out RC, 200f)) {
							if (!physicalDrop) {
								if (Vector3.Angle (RC.normal, mouseNormal) < slopeBias) {
									PlaceObject (RC.point, RC.normal, firstPaint);
									break;
								}
							} else {								
								PlaceObject (RC.point - Vector3.Normalize (gravity) * physDropHeight, Random.onUnitSphere, firstPaint);
								break;
							}
						}	
					}
					skipped = 0;
					lastMouse = mousePoint;
					initialMouse = Event.current.mousePosition;
				}
			}
		}

		void LoadObjects(){
			objectsToPlace = new GameObject[brushes[selectedBrush].objects.Count];
			objectVertexCount = new int[brushes[selectedBrush].objects.Count];

			for (int i = 0; i < objectsToPlace.Length; i++) {
				objectsToPlace [i] = AssetDatabase.LoadAssetAtPath<GameObject> (brushes [selectedBrush].objects [i].path) as GameObject;
				objectVertexCount [i] = objectsToPlace [i].GetComponent<MeshFilter> ().sharedMesh.vertexCount;
			}
		}

		void PlaceObject(Vector3 position, Vector3 orientation, bool firstPaint){
			float total = 0;
			float[] probabilities = new float[densitiesContainer.densities.Count];

			for (int i = 0; i < densitiesContainer.densities.Count; i++) {
				total += densitiesContainer.densities [i];
			}

			for (int i = 0; i < densitiesContainer.densities.Count; i++) {
				probabilities [i] = densitiesContainer.densities [i] / total;
			}

			float myRandom = Random.value;
			float count = 0;

			for (int i = 0; i < densitiesContainer.densities.Count; i++) {
				int indexOfCreatedObject = -1;
				if (myRandom < probabilities [i] + count || soloMode != -1) {
					if (soloMode == -1) {
						indexOfCreatedObject = i;
					}
					else {
						indexOfCreatedObject = soloMode;
					}

					if (placedObjectsContainer.vertexCount + objectVertexCount [indexOfCreatedObject] >= 65536 && bakeEnabled) {
						Debug.LogWarning ("Unity's limit for vertexcount (65536 vertices) reached for a single mesh. Please, bake the current stroke before continue.");
					} else {
						if ((firstPaint && alignToStroke) || (firstPaint && brushes [selectedBrush].objects [indexOfCreatedObject].enablePerObject && brushes [selectedBrush].objects [indexOfCreatedObject].alignToStroke))
							{
								
							}
						else{
							GameObject go = PrefabUtility.InstantiatePrefab (objectsToPlace [indexOfCreatedObject]) as GameObject;
							PlacedObjectParentScale = new Vector3 (objectsToPlace [indexOfCreatedObject].transform.localScale.x, objectsToPlace [indexOfCreatedObject].transform.localScale.y, objectsToPlace [indexOfCreatedObject].transform.localScale.z);
							PlacedObject placedObject = go.AddComponent<PlacedObject> ();

							placedObjectsContainer.vertexCount += objectVertexCount [indexOfCreatedObject];

							placedObjectsPO.Add (placedObject);
							placedObject.ID = indexOfCreatedObject;
							placedObject.vertexCount = objectVertexCount [indexOfCreatedObject];
							placedObject.vertexCountContainer = placedObjectsContainer;

							if (!physicalDrop) {
								Collider collider = go.GetComponent<Collider> ();
								if (collider) {
									placedObject.col = collider;
									placedObject.colliderEnabled = collider.enabled;
									collider.enabled = false;
								}
							}

							Undo.RegisterCreatedObjectUndo (go, "PPB Stroke");

							if (!physicalDrop && !precisionPlace) {
								if (!brushes [selectedBrush].objects [indexOfCreatedObject].enablePerObject) {
									CalculateScale (ref go);
									CalculateOffset (position, ref go);
									CalculateRotation (orientation, ref go);
									CalculateColor (ref go);
								} else {
									CalculateScalePerObject (ref go, indexOfCreatedObject);
									CalculateOffsetPerObject (position, ref go, indexOfCreatedObject);
									CalculateRotationPerObject (orientation, ref go, indexOfCreatedObject);
									CalculateColorPerObject (ref go, indexOfCreatedObject);
								}
								if (makeStatic) {
									go.isStatic = true;
								}

							}
							if (physicalDrop && !precisionPlace) {
								MeshCollider MC = go.GetComponent<MeshCollider> ();
								BoxCollider BC = go.GetComponent<BoxCollider> ();
								SphereCollider SC = go.GetComponent<SphereCollider> ();
								CapsuleCollider CC = go.GetComponent<CapsuleCollider> ();
								if (MC || BC || SC || CC) {
									if (MC) {
										if (MC.convex) {
											RigPhysics (position, orientation, indexOfCreatedObject, ref go);
										} else {
											Debug.LogWarning ("The object " + brushes [selectedBrush].objects [indexOfCreatedObject].name + " has no CONVEX mesh collider. Add a CONVEX mesh collider in order to simulate physical drop", (GameObject)AssetDatabase.LoadAssetAtPath<GameObject> (brushes [selectedBrush].objects [indexOfCreatedObject].path));
											DestroyImmediate (go);
										}
									} else if (BC || SC || CC) {
										RigPhysics (position, orientation, indexOfCreatedObject, ref go);
									}
								} else {
									Debug.LogWarning ("The object " + brushes [selectedBrush].objects [indexOfCreatedObject].name + " has no collider. Convex mesh collider automatically added. It will be removed on tool close.", (GameObject)AssetDatabase.LoadAssetAtPath<GameObject> (brushes [selectedBrush].objects [indexOfCreatedObject].path));
									MeshCollider newMC = go.AddComponent<MeshCollider> ();
									newMC.convex = true;
									droppedObjectsMC.Add (newMC);
									RigPhysics (position, orientation, indexOfCreatedObject, ref go);
								}
							}
							if (precisionPlace && !physicalDrop) {
								precisionPlaceObject = go;
								GameObject parent = PrefabUtility.GetPrefabParent (go) as GameObject;
								precisionPlaceParentScale = parent.transform.localScale;
								precisionPlaceObjectID = indexOfCreatedObject;
								precisionPlaceObject.transform.position = mousePoint;

								if (!brushes [selectedBrush].objects [indexOfCreatedObject].enablePerObject) {
									CalculateOffset (position, ref precisionPlaceObject);
									CalculateScale (ref precisionPlaceObject);
									CalculateRotation (orientation, ref precisionPlaceObject);
									CalculateColor (ref precisionPlaceObject);
								} else {
									CalculateOffsetPerObject (position, ref precisionPlaceObject, indexOfCreatedObject);
									CalculateRotationPerObject (orientation, ref precisionPlaceObject, indexOfCreatedObject);
									CalculateScalePerObject (ref precisionPlaceObject, indexOfCreatedObject);
									CalculateColorPerObject (ref precisionPlaceObject, indexOfCreatedObject);

									Quaternion quat = Quaternion.identity;

									if (brushes [selectedBrush].objects [indexOfCreatedObject].alignToSurface && !brushes [selectedBrush].objects [indexOfCreatedObject].alignToStroke) {
										if (Vector3.Magnitude (mouseDirection) > 0f) {
											quat = Quaternion.LookRotation (orientation);
										}
										go.transform.rotation = quat;
										if (brushes [selectedBrush].objects [indexOfCreatedObject].alignTo == 0) {
											go.transform.RotateAround (go.transform.position, go.transform.forward, 90f);
										}
										if (brushes [selectedBrush].objects [indexOfCreatedObject].alignTo == 2) {
											go.transform.RotateAround (go.transform.position, go.transform.right, -90f);
										}
									}
								}
							}

							if (groupByObject) {
								if (go)
									go.transform.parent = subparents [indexOfCreatedObject].transform;
							} else {
								if (go)
									go.transform.parent = parent.transform;	
							}
							break;
						}
					}
				}
				else {
					count += probabilities [i];
				}
			}
		}

		void RigPhysics(Vector3 position, Vector3 orientation, int i, ref GameObject go){
			go.transform.position = position;
			go.transform.rotation = Quaternion.LookRotation (orientation);
			Rigidbody RB = go.AddComponent<Rigidbody> ();
//			DroppedObject DO = go.AddComponent<DroppedObject> ();
//			DO.rb = RB;

//			droppedObjectsDO.Add (DO);
			droppedObjectsRB.Add (RB);
			RB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			RB.interpolation = RigidbodyInterpolation.Interpolate;

			if (!brushes [selectedBrush].objects [i].enablePerObject) {
				CalculateScale (ref go);
				CalculateColor (ref go);
			} else {
				CalculateColorPerObject (ref go, i);
				CalculateColorPerObject (ref go, i);
			}
		}

		void CalculateRotation(Vector3 orientation, ref GameObject go){
			Quaternion quat = Quaternion.identity;

			if ((alignToSurface && !alignToStroke) || (alignToSurface && precisionPlace)) {
//				if (Vector3.Magnitude (mouseDirection) > 0f) {
					quat = Quaternion.LookRotation (orientation);
//				}
//				if (!precisionPlace)
					quat = quat * Quaternion.AngleAxis (90f, go.transform.right);
				go.transform.rotation = quat;
				if (alignTo == 0) {
					go.transform.RotateAround (go.transform.position, go.transform.forward, 90f);
				}
				if (alignTo == 2) {
					go.transform.RotateAround (go.transform.position, go.transform.right, -90f);
				}
			}
			precisionPlaceOriginalRot = go.transform.rotation;
			if (!precisionPlace) {
				if (alignToStroke && !alignToSurface) {
					if (Vector3.Magnitude (mouseDirection) > 0f) {
						quat = Quaternion.LookRotation (mouseDirection);
					}
					quat = quat * Quaternion.AngleAxis (90f, go.transform.right);
					go.transform.rotation = quat;
					if (alignToStrokeAxis == 0) {
						go.transform.RotateAround (go.transform.position, go.transform.forward, 90f);
					}
					if (alignToStrokeAxis == 2) {
						go.transform.RotateAround (go.transform.position, go.transform.right, -90f);
					}
				}

				if (alignToStroke && alignToSurface) {
					if (Vector3.Magnitude (mouseDirection) > 0f) {
						quat = Quaternion.LookRotation (mouseDirection, orientation);
					}
					go.transform.rotation = quat;

					if (alignTo == 1) {
						if (alignToStrokeAxis == 0) {
							go.transform.RotateAround (go.transform.position, go.transform.up, -90f);
						}
					}

					if (alignTo == 0) {
						go.transform.RotateAround (go.transform.position, go.transform.forward, 90f);
						if (alignToStrokeAxis == 1) {
							go.transform.RotateAround (go.transform.position, go.transform.right, 90f);
						}
					}
					if (alignTo == 2) {
						go.transform.RotateAround (go.transform.position, go.transform.right, -90f);
						go.transform.RotateAround (go.transform.position, go.transform.right, 180f);
						if (alignToStrokeAxis == 0) {
							go.transform.RotateAround (go.transform.position, go.transform.forward, 90f);
						}
					}
				}
			}
			if (rotationVar) {
				go.transform.RotateAround (go.transform.position, go.transform.right, Random.Range (rotationBottomX, rotationTopX));
				go.transform.RotateAround (go.transform.position, go.transform.up, Random.Range (rotationBottomY, rotationTopY));
				go.transform.RotateAround (go.transform.position, go.transform.forward, Random.Range (rotationBottomZ, rotationTopZ));
			}
		}

		void CalculateOffset(Vector3 position, ref GameObject go){
			go.transform.position = position;

			if (offsetVar) {
				go.transform.position += go.transform.right * Random.Range (offsetBottomX, offsetTopX);
				go.transform.position += go.transform.up * Random.Range (offsetBottomY, offsetTopY);
				go.transform.position += go.transform.forward * Random.Range (offsetBottomZ, offsetTopZ);
			}
		}

		void CalculateScale(ref GameObject go){
			if (scaleVar) {
				if (scaleUniform) {
					float uniformVariation = Random.Range (scaleBottom, scaleTop);
					go.transform.localScale = new Vector3 (uniformVariation * PlacedObjectParentScale.x, uniformVariation * PlacedObjectParentScale.y, uniformVariation * PlacedObjectParentScale.z);
				}
				else {
					go.transform.localScale = new Vector3 (Random.Range (scaleBottomX, scaleTopX) * PlacedObjectParentScale.x, Random.Range (scaleBottomY, scaleTopY) * PlacedObjectParentScale.y, Random.Range (scaleBottomZ, scaleTopZ) * PlacedObjectParentScale.z);
				}
			}
		}

		void CalculateColor(ref GameObject go){
			if (colorVariation) {
				MeshFilter mf = go.GetComponent<MeshFilter> ();
				Mesh mesh = Mesh.Instantiate (mf.sharedMesh);

				Color[] myColors = new Color[mesh.vertexCount];

				Color randColor = gradientContainer.gradient.Evaluate (Random.value);
				for (int i = 0; i < myColors.Length; i++) {
					myColors [i] = randColor;
				}

				mesh.colors = myColors;
				mf.mesh = mesh;
			}
		}

		void CalculateRotationPerObject(Vector3 orientation, ref GameObject go, int objectID){
			Quaternion quat = Quaternion.identity;

			if ((brushes [selectedBrush].objects [objectID].alignToSurface && !brushes [selectedBrush].objects [objectID].alignToStroke) ||
				(brushes [selectedBrush].objects [objectID].alignToSurface && precisionPlace)) {
//				if (Vector3.Magnitude (mouseDirection) > 0f) {
					quat = Quaternion.LookRotation (orientation);
//				}
//				if (!precisionPlace)
					quat = quat * Quaternion.AngleAxis (90f, go.transform.right);
				go.transform.rotation = quat;
				if (brushes [selectedBrush].objects [objectID].alignTo == 0) {
					go.transform.RotateAround (go.transform.position, go.transform.forward, 90f);
				}
				if (brushes [selectedBrush].objects [objectID].alignTo == 2) {
					go.transform.RotateAround (go.transform.position, go.transform.right, -90f);
				}
			}
			precisionPlaceOriginalRot = go.transform.rotation;

			if (!precisionPlace) {
				if (brushes [selectedBrush].objects [objectID].alignToStroke && !brushes [selectedBrush].objects [objectID].alignToSurface) {
					if (Vector3.Magnitude (mouseDirection) > 0f) {
						quat = Quaternion.LookRotation (mouseDirection);
					}
					quat = quat * Quaternion.AngleAxis (90f, go.transform.right);
					go.transform.rotation = quat;
					if (brushes [selectedBrush].objects [objectID].alignToStrokeAxis == 0) {
						go.transform.RotateAround (go.transform.position, go.transform.forward, 90f);
					}
					if (brushes [selectedBrush].objects [objectID].alignToStrokeAxis == 2) {
						go.transform.RotateAround (go.transform.position, go.transform.right, -90f);
					}
				}

				if (brushes [selectedBrush].objects [objectID].alignToStroke && brushes [selectedBrush].objects [objectID].alignToSurface) {
					if (Vector3.Magnitude (mouseDirection) > 0f) {
						quat = Quaternion.LookRotation (mouseDirection, orientation);
					}
					go.transform.rotation = quat;

					if (brushes [selectedBrush].objects [objectID].alignTo == 1) {
						if (brushes [selectedBrush].objects [objectID].alignToStrokeAxis == 0) {
							go.transform.RotateAround (go.transform.position, go.transform.up, -90f);
						}
					}

					if (brushes [selectedBrush].objects [objectID].alignTo == 0) {
						go.transform.RotateAround (go.transform.position, go.transform.forward, 90f);
						if (brushes [selectedBrush].objects [objectID].alignToStrokeAxis == 1) {
							go.transform.RotateAround (go.transform.position, go.transform.right, 90f);
						}
					}
					if (brushes [selectedBrush].objects [objectID].alignTo == 2) {
						go.transform.RotateAround (go.transform.position, go.transform.right, -90f);
						go.transform.RotateAround (go.transform.position, go.transform.right, 180f);
						if (brushes [selectedBrush].objects [objectID].alignToStrokeAxis == 0) {
							go.transform.RotateAround (go.transform.position, go.transform.forward, 90f);
						}
					}
				}
			}

			if (brushes[selectedBrush].objects[objectID].rotationVar) {
				go.transform.RotateAround (go.transform.position, go.transform.right, Random.Range (brushes[selectedBrush].objects[objectID].rotationBottomX, brushes[selectedBrush].objects[objectID].rotationTopX));
				go.transform.RotateAround (go.transform.position, go.transform.up, Random.Range (brushes[selectedBrush].objects[objectID].rotationBottomY, brushes[selectedBrush].objects[objectID].rotationTopY));
				go.transform.RotateAround (go.transform.position, go.transform.forward, Random.Range (brushes[selectedBrush].objects[objectID].rotationBottomZ, brushes[selectedBrush].objects[objectID].rotationTopZ));
			}
		}

		void CalculateOffsetPerObject(Vector3 position, ref GameObject go, int objectID){
			go.transform.position = position;

			if (brushes[selectedBrush].objects[objectID].offsetVar) {
				go.transform.position += go.transform.right * Random.Range (brushes[selectedBrush].objects[objectID].offsetBottomX, brushes[selectedBrush].objects[objectID].offsetTopX);
				go.transform.position += go.transform.up * Random.Range (brushes[selectedBrush].objects[objectID].offsetBottomY, brushes[selectedBrush].objects[objectID].offsetTopY);
				go.transform.position += go.transform.forward * Random.Range (brushes[selectedBrush].objects[objectID].offsetBottomZ, brushes[selectedBrush].objects[objectID].offsetTopZ);
			}
		}

		void CalculateScalePerObject(ref GameObject go, int objectID){
			if (brushes[selectedBrush].objects[objectID].scaleVar) {
				if (brushes[selectedBrush].objects[objectID].scaleUniform) {
					float uniformVariation = Random.Range (brushes[selectedBrush].objects[objectID].scaleBottom , brushes[selectedBrush].objects[objectID].scaleTop);
					go.transform.localScale = new Vector3 (uniformVariation * PlacedObjectParentScale.x, uniformVariation * PlacedObjectParentScale.y, uniformVariation * PlacedObjectParentScale.z);
				}
				else {
					go.transform.localScale = new Vector3 (Random.Range (brushes[selectedBrush].objects[objectID].scaleBottomX, brushes[selectedBrush].objects[objectID].scaleTopX) * PlacedObjectParentScale.x, Random.Range (brushes[selectedBrush].objects[objectID].scaleBottomY, brushes[selectedBrush].objects[objectID].scaleTopY)  * PlacedObjectParentScale.y, Random.Range (brushes[selectedBrush].objects[objectID].scaleBottomZ, brushes[selectedBrush].objects[objectID].scaleTopZ) * PlacedObjectParentScale.z);
				}
			}
		}

		void CalculateColorPerObject(ref GameObject go, int objectID){
			if (brushes[selectedBrush].objects[objectID].colorVariation) {
				MeshFilter mf = go.GetComponent<MeshFilter> ();
				Mesh mesh = Mesh.Instantiate (mf.sharedMesh);

				Color[] myColors = new Color[mesh.vertexCount];

				Color randColor = brushes[selectedBrush].objects[objectID].gradient.Evaluate (Random.value);
				for (int i = 0; i < myColors.Length; i++) {
					myColors [i] = randColor;
				}

				mesh.colors = myColors;
				mf.mesh = mesh;
			}
		}
	}
}
#endif
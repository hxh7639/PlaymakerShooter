#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Dynamite3D.PrefabPlaceNBake;
using UnityEditorInternal;
using SFB;

namespace Dynamite3D.PrefabPlaceNBake{
	public class ObjectSettingsWindow : EditorWindow {
		
		#region GUIVariables
		public Texture2D preview;
		GUISkin guiSkin;
		Color sectionColor;
		Color sectionLimitColor;
		Color subSectionColor;
		Rect lastLayouRect;
		Rect sectionRect;
		float sectionHeight;
		float columnWidth = 256f;
		float settingsHeight = 500f;
		Vector2 settingsScroll;
		#endregion

		#region SettingVariables

		[SerializeField] bool enablePerObject;
		bool tempEnablePerObject;

		[SerializeField] LayerMask layerMask;
		LayerMask tempLayerMask;
		[SerializeField] float density;
		float tempDensity;
		[SerializeField] bool alignToSurface;
		bool tempAlignToSurface;
		[SerializeField] int alignTo;
		int tempAlignTo;
		[SerializeField] float slopeBias;
		float tempSlopeBias;

		[SerializeField] bool scaleVar;
		bool tempScaleVar;
		[SerializeField] bool scaleUniform;
		bool tempScaleUniform;
		[SerializeField] float scaleMaxX;
		float tempScaleMaxX;
		[SerializeField] float scaleMinX;
		float tempScaleMinX;
		[SerializeField] float scaleTopX;
//		float tempScaleTopX;
		[SerializeField] float scaleBottomX;
//		float tempScaleBottomX;
		[SerializeField] float scaleMaxY;
		float tempScaleMaxY;
		[SerializeField] float scaleMinY;
		float tempScaleMinY;
		[SerializeField] float scaleTopY;
//		float tempScaleTopY;
		[SerializeField] float scaleBottomY;
//		float tempScaleBottomY;
		[SerializeField] float scaleMaxZ;
		float tempScaleMaxZ;
		[SerializeField] float scaleMinZ;
		float tempScaleMinZ;
		[SerializeField] float scaleTopZ;
//		float tempScaleTopZ;
		[SerializeField] float scaleBottomZ;
//		float tempScaleBottomZ;
		[SerializeField] float scaleMax;
		float tempScaleMax;
		[SerializeField] float scaleMin;
		float tempScaleMin;
		[SerializeField] float scaleTop;
//		float tempScaleTop;
		[SerializeField] float scaleBottom;
//		float tempScaleBottom;

		[SerializeField] bool alignToStroke;
		bool tempAlignToStroke;
		[SerializeField] int alignToStrokeAxis;
		int tempAlignToStrokeAxis;

		[SerializeField] bool rotationVar;
		bool tempRotationVar;
		[SerializeField] float rotationMaxX;
		float tempRotationMaxX;
		[SerializeField] float rotationMinX;
		float tempRotationMinX;
		[SerializeField] float rotationTopX;
//		float tempRotationTopX;
		[SerializeField] float rotationBottomX;
//		float tempRotationBottomX;
		[SerializeField] float rotationMaxY;
		float tempRotationMaxY;
		[SerializeField] float rotationMinY;
		float tempRotationMinY;
		[SerializeField] float rotationTopY;
//		float tempRotationTopY;
		[SerializeField] float rotationBottomY;
//		float tempRotationBottomY;
		[SerializeField] float rotationMaxZ;
		float tempRotationMaxZ;
		[SerializeField] float rotationMinZ;
		float tempRotationMinZ;
		[SerializeField] float rotationTopZ;
//		float tempRotationTopZ;
		[SerializeField] float rotationBottomZ;
//		float tempRotationBottomZ;

		[SerializeField] bool offsetVar;
		bool tempOffsetVar;
		[SerializeField] float offsetMaxX;
		float tempOffsetMaxX;
		[SerializeField] float offsetMinX;
		float tempOffsetMinX;
		[SerializeField] float offsetTopX;
//		float tempOffsetTopX;
		[SerializeField] float offsetBottomX;
//		float tempOffsetBottomX;
		[SerializeField] float offsetMaxY;
		float tempOffsetMaxY;
		[SerializeField] float offsetMinY;
		float tempOffsetMinY;
		[SerializeField] float offsetTopY;
//		float tempOffsetTopY;
		[SerializeField] float offsetBottomY;
//		float tempOffsetBottomY;
		[SerializeField] float offsetMaxZ;
		float tempOffsetMaxZ;
		[SerializeField] float offsetMinZ;
		float tempOffsetMinZ;
		[SerializeField] float offsetTopZ;
//		float tempOffsetTopZ;
		[SerializeField] float offsetBottomZ;
//		float tempOffsetBottomZ;
		[SerializeField] bool colorVariation;
		bool tempColorVariation;

		[SerializeField] GradientContainer gradientContainer;
		SerializedObject serializedGradientContainer;
		SerializedProperty serializedGradient;
		#endregion

		public ObjectSettings mySettings;
		public int objectId;
		public PrefabPlaceNBakeWindow parentWindow;
		bool removing = false;
		bool guiDisabled = false;

		void Awake(){
			guiSkin = (GUISkin)AssetDatabase.LoadAssetAtPath("Assets/3Dynamite/PrefabPlace'N'Bake/GuiSkins/GuiSkins/GuiSkin_1.guiskin", typeof(GUISkin));

			gradientContainer = ScriptableObject.CreateInstance<GradientContainer>();
			gradientContainer.gradient = new Gradient ();

			serializedGradientContainer = new SerializedObject(gradientContainer);
			serializedGradient = serializedGradientContainer.FindProperty ("gradient");
		
			sectionColor = new Color (0f, 0f, 0f, 0.12f);
			sectionLimitColor = new Color (0f, 0f, 0f, 0.4f);
			subSectionColor = new Color (1f, 1f, 1f, 0.2f);

			EditorApplication.modifierKeysChanged += this.Repaint;
		}

		void OnEnable(){
			
		}

		void OnDestroy(){
			if (!removing) {
				SaveSettings ();
				parentWindow.GetObjectSettings (mySettings, objectId);
			}
			parentWindow.guiDisabled = false;
			parentWindow.Repaint ();
		}

		void OnLostFocus(){
			if (!removing) {
				SaveSettings ();
				parentWindow.GetObjectSettings (mySettings, objectId);
			}
		}

		void OnGUI () {
			if (alignTo == alignToStrokeAxis && alignToSurface && alignToStroke) {
				alignToStrokeAxis += 1;
				tempAlignToStrokeAxis = alignToStrokeAxis;
				if (alignToStrokeAxis == 3) {
					alignToStrokeAxis = 0;
					tempAlignToStrokeAxis = alignToStrokeAxis;
				}
				Debug.LogWarning ("You cannot align to surface and to stroke the same axis. Stroke Alignment was changed automatically.");
			}

			EditorGUI.BeginChangeCheck ();

			GUILayout.BeginArea(new Rect(5f, 5f, columnWidth + 5f, 130f));
			GUI.DrawTexture (new Rect (10f, 10f, 100f, 100f), preview);
			GUI.Label (new Rect (120f, 15f, 120f, 30f), mySettings.name, guiSkin.label);

			if (parentWindow.bakeEnabled) {
				GUI.Label (new Rect (120f, 50f, 120f, 60f), "Remove disabled in Bake Mode", guiSkin.GetStyle("box"));
			} 
			else {
				if (!removing) {
					if (GUI.Button (new Rect (125f, 65f, 130f, 30f), "Remove from brush")) {
						removing = true;
						guiDisabled = true;
					}
				} else {
					GUI.Label (new Rect (120f, 45f, 120f, 30f), "Are you sure?", guiSkin.label);
					if (GUI.Button (new Rect (125f, 75f, 60f, 20f), "Yes")) {
						parentWindow.RemoveObject (objectId);
						parentWindow.Repaint ();
						this.Close ();
					}
					if (GUI.Button (new Rect (195f, 75f, 60f, 20f), "No")) {
						removing = false;
						guiDisabled = false;
					}
				}
			}

			GUILayout.EndArea ();

			EditorGUI.DrawRect (new Rect (5f, 5f, columnWidth + 10f, 125f), sectionColor);
			EditorGUI.DrawRect (new Rect (5f, 140f, columnWidth + 10f, 2f), sectionLimitColor);

			EditorGUI.BeginDisabledGroup (removing);

			GUILayout.BeginArea(new Rect (5f, 150f, columnWidth + 20f, 175));
			GUILayout.Label ("Density", guiSkin.GetStyle("label"), GUILayout.Height(20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();

			lastLayouRect = new Rect (columnWidth / 2 - 30f, lastLayouRect.y, lastLayouRect.width / 3f, lastLayouRect.height);
			tempDensity = GUI.HorizontalSlider (lastLayouRect, density, 0f, 1f, guiSkin.GetStyle ("horizontalslider"), guiSkin.GetStyle ("horizontalsliderthumb"));
			lastLayouRect = new Rect (columnWidth - 60f, lastLayouRect.y, lastLayouRect.width / 3f, lastLayouRect.height);
			EditorGUI.LabelField(lastLayouRect, density.ToString("F2"), guiSkin.GetStyle("LabelLittle"));
			lastLayouRect = GUILayoutUtility.GetLastRect ();

			GUILayout.Space (20f);

			GUILayout.Label ("Enable Per Object Settings", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			lastLayouRect = new Rect (columnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
			tempEnablePerObject = EditorGUI.Toggle (lastLayouRect, enablePerObject, guiSkin.GetStyle ("Toggle"));

			GUILayout.EndArea ();

			EditorGUI.EndDisabledGroup ();

			settingsScroll = GUI.BeginScrollView (new Rect (5f, 240f, columnWidth + 20f, this.position.height - 240f), settingsScroll, new Rect (0f, 0f, columnWidth, settingsHeight));
			EditorGUI.BeginDisabledGroup (!enablePerObject || removing);
			GUILayout.BeginArea(new Rect (0f, 0f, columnWidth, 900f));
			DrawObjectSettings ();
			GUILayout.EndArea ();
			EditorGUI.EndDisabledGroup ();
			GUI.EndScrollView ();

			if (!guiDisabled) {
				if (EditorGUI.EndChangeCheck ()) {
					Undo.RegisterCompleteObjectUndo (this, "ObjectSettings GUI");
					Undo.RegisterCompleteObjectUndo (gradientContainer, "ObjectSettings GUI");

					enablePerObject = tempEnablePerObject;

					layerMask = tempLayerMask;
					density = tempDensity;
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
				}
			}
		}

		void DrawObjectSettings(){
			sectionHeight = 0f;
			settingsHeight = 100f;

			GUILayout.Space (10f);

//			GUILayout.Label("Layer Mask", guiSkin.GetStyle("label"), GUILayout.Height(20f));
//			lastLayouRect = GUILayoutUtility.GetLastRect ();
//			sectionRect = lastLayouRect;
//			lastLayouRect = new Rect (lastLayouRect.x + 100f, lastLayouRect.y, lastLayouRect.width - 105f, lastLayouRect.height);
//			tempLayerMask = EditorGUI.MaskField(lastLayouRect, InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layerMask), InternalEditorUtility.layers, guiSkin.GetStyle("button"));
//			tempLayerMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempLayerMask);

			GUILayout.Label ("Align to Surface", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			sectionRect = lastLayouRect;
			lastLayouRect = new Rect (columnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
			tempAlignToSurface = EditorGUI.Toggle (lastLayouRect, alignToSurface, guiSkin.GetStyle ("Toggle"));

			if (alignToSurface) {
				sectionHeight += 30f;
				settingsHeight += 30f;
				EditorGUI.DrawRect (new Rect (sectionRect.x, sectionRect.y + 25f, sectionRect.width, 25f), subSectionColor);
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("      X", guiSkin.GetStyle("label"), GUILayout.Height(20f));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (lastLayouRect.x + 50f, lastLayouRect.y, 20f, lastLayouRect.height);
				if (alignTo == 0) {
					if (GUI.Button (lastLayouRect, "", guiSkin.GetStyle("OnToggle"))) {

					}
				} else {
					if (GUI.Button (lastLayouRect, "", guiSkin.toggle)) {
						if (tempAlignToStrokeAxis == 0 && alignToStroke) {
							Debug.LogWarning ("You cannot align to surface and to stroke the same axis");
						}
						else {
							tempAlignTo = 0;
						}
					}
				}
				GUILayout.Label ("      Y", guiSkin.GetStyle("label"), GUILayout.Height(20f));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (lastLayouRect.x + 50f, lastLayouRect.y, 20f, lastLayouRect.height);
				if (alignTo == 1) {
					if (GUI.Button (lastLayouRect, "", guiSkin.GetStyle("OnToggle"))) {

					}
				} else {
					if (GUI.Button (lastLayouRect, "", guiSkin.toggle)) {
						if (tempAlignToStrokeAxis == 1 && alignToStroke) {
							Debug.LogWarning ("You cannot align to surface and to stroke the same axis");
						}
						else {
							tempAlignTo = 1;
						}
					}
				}
				GUILayout.Label ("      Z", guiSkin.GetStyle("label"), GUILayout.Height(20f));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (lastLayouRect.x + 50f, lastLayouRect.y, 20f, lastLayouRect.height);
				if (alignTo == 2) {
					if (GUI.Button (lastLayouRect, "", guiSkin.GetStyle("OnToggle"))) {

					}
				} else {
					if (GUI.Button (lastLayouRect, "", guiSkin.toggle)) {
						if (tempAlignToStrokeAxis == 2 && alignToStroke) {
							Debug.LogWarning ("You cannot align to surface and to stroke the same axis");
						}
						else {
							tempAlignTo = 2;
						}
					}
				}
				GUILayout.EndHorizontal ();
			}

//			GUILayout.Label ("Slope Bias", guiSkin.GetStyle("label"), GUILayout.Height(20f));
//			lastLayouRect = GUILayoutUtility.GetLastRect ();
//			lastLayouRect = new Rect (columnWidth / 2 - 30f, lastLayouRect.y, lastLayouRect.width / 3f, lastLayouRect.height);
//			tempSlopeBias = GUI.HorizontalSlider (lastLayouRect, slopeBias, 0f, 90f, guiSkin.GetStyle ("horizontalslider"), guiSkin.GetStyle ("horizontalsliderthumb"));
//			lastLayouRect = new Rect (columnWidth - 60f, lastLayouRect.y, lastLayouRect.width / 3f, lastLayouRect.height);
//			EditorGUI.LabelField(lastLayouRect, slopeBias.ToString("F1"), guiSkin.GetStyle("LabelLittle"));

			sectionHeight += 30f;
			settingsHeight += 30f;
			sectionRect = new Rect (sectionRect.x, sectionRect.y - 7f, sectionRect.width, sectionHeight);
			EditorGUI.DrawRect (sectionRect, sectionColor);
			EditorGUI.DrawRect (new Rect(sectionRect.x, sectionRect.y, sectionRect.width, 2f), sectionLimitColor);

			GUILayout.Space (10f);


			GUILayout.Label ("Scale Variation", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			sectionRect = lastLayouRect;
			sectionHeight = 20f;
			lastLayouRect = new Rect (columnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
			tempScaleVar = EditorGUI.Toggle (lastLayouRect, scaleVar, guiSkin.GetStyle ("Toggle"));

			if (scaleVar) {
				GUILayout.Label ("  Uniform Scale", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
				lastLayouRect = GUILayoutUtility.GetLastRect ();
				lastLayouRect = new Rect (columnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
				tempScaleUniform = EditorGUI.Toggle (lastLayouRect, scaleUniform, guiSkin.GetStyle ("Toggle"));
				sectionHeight += 75f;
				settingsHeight += 75f;
				EditorGUI.DrawRect(new Rect(sectionRect.x, sectionRect.y + 50f, sectionRect.width, 45f), subSectionColor);
				EditorGUI.DrawRect(new Rect(sectionRect.x, sectionRect.y + 152f, sectionRect.width, 45f), subSectionColor);

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
					sectionHeight += 105f;
					settingsHeight += 105f;
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

			EditorGUI.DrawRect (new Rect (sectionRect.x, sectionRect.y - 5f, sectionRect.width, sectionHeight + 8f), sectionColor);
			EditorGUI.DrawRect (new Rect (sectionRect.x, sectionRect.y - 5f, sectionRect.width, 2f), sectionLimitColor);

			GUILayout.Space (10f);

			sectionHeight = 0f;
			GUILayout.Label ("Align to Stroke", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			sectionRect = lastLayouRect;
			sectionHeight += 25f;
			settingsHeight += 25f;
			lastLayouRect = new Rect (columnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
			tempAlignToStroke = EditorGUI.Toggle (lastLayouRect, alignToStroke, guiSkin.GetStyle ("Toggle"));

			if (alignToStroke) {
				sectionHeight += 33f;
				settingsHeight += 33f;
				EditorGUI.DrawRect (new Rect (sectionRect.x, sectionRect.y + 25f, sectionRect.width, 25f), subSectionColor);
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

			EditorGUI.DrawRect (new Rect (sectionRect.x, sectionRect.y - 2f, sectionRect.width, sectionHeight), sectionColor);
			EditorGUI.DrawRect (new Rect (sectionRect.x, sectionRect.y - 2f, sectionRect.width, 2f), sectionLimitColor);
			GUILayout.Space (10f);

			sectionHeight = 0f;
			GUILayout.Label ("Rotation Variation", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			sectionRect = lastLayouRect;
			sectionHeight += 25f;
			settingsHeight += 25f;
			lastLayouRect = new Rect (columnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
			tempRotationVar = EditorGUI.Toggle (lastLayouRect, rotationVar, guiSkin.GetStyle ("Toggle"));

			if (rotationVar) {
				sectionHeight = 178f;
				settingsHeight += 178f;
				EditorGUI.DrawRect (new Rect (sectionRect.x, sectionRect.y + 20f, sectionRect.width, 48f), subSectionColor);
				EditorGUI.DrawRect (new Rect (sectionRect.x, sectionRect.y + 122f, sectionRect.width, 48f), subSectionColor);
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

			EditorGUI.DrawRect (new Rect (sectionRect.x, sectionRect.y - 2f, sectionRect.width, sectionHeight), sectionColor);
			EditorGUI.DrawRect (new Rect (sectionRect.x, sectionRect.y - 2f, sectionRect.width, 2f), sectionLimitColor);
			GUILayout.Space (10f);

			sectionHeight = 0f;
			GUILayout.Label ("Offset Variation", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			sectionRect = lastLayouRect;
			sectionHeight += 25f; 
			settingsHeight += 25f;
			lastLayouRect = new Rect (columnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
			tempOffsetVar = EditorGUI.Toggle (lastLayouRect, offsetVar, guiSkin.GetStyle ("Toggle"));
			if (offsetVar) {
				sectionHeight = 178f;
				settingsHeight += 178f;
				EditorGUI.DrawRect (new Rect (sectionRect.x, sectionRect.y + 20f, sectionRect.width, 48f), subSectionColor);
				EditorGUI.DrawRect (new Rect (sectionRect.x, sectionRect.y + 122f, sectionRect.width, 48f), subSectionColor);
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

			EditorGUI.DrawRect (new Rect (sectionRect.x, sectionRect.y - 2f, sectionRect.width, sectionHeight), sectionColor);
			EditorGUI.DrawRect (new Rect (sectionRect.x, sectionRect.y - 2f, sectionRect.width, 2f), sectionLimitColor);
			GUILayout.Space (10f);

			sectionHeight = 0f;
			GUILayout.Label ("Vertex Color Variation", guiSkin.GetStyle ("label"), GUILayout.Height (20f));
			lastLayouRect = GUILayoutUtility.GetLastRect ();
			sectionRect = lastLayouRect;
			sectionHeight += 25f;
			settingsHeight += 25f;
			lastLayouRect = new Rect (columnWidth - 50f, lastLayouRect.y, 20f, lastLayouRect.height);
			tempColorVariation = EditorGUI.Toggle (lastLayouRect, colorVariation, guiSkin.GetStyle ("Toggle"));
			if(colorVariation){
				sectionHeight += 25f;
				settingsHeight += 25f;
				GUILayout.Space (2f);
				EditorGUILayout.PropertyField(serializedGradient);
			}
			EditorGUI.DrawRect (new Rect (sectionRect.x, sectionRect.y - 2f, sectionRect.width, sectionHeight), sectionColor);
			EditorGUI.DrawRect (new Rect (sectionRect.x, sectionRect.y - 2f, sectionRect.width, 2f), sectionLimitColor);
			GUILayout.Space (10f);
		}

		public void ReadSettings(){
			ObjectSettings settings = mySettings;

			enablePerObject = settings.enablePerObject; 	tempEnablePerObject = enablePerObject;
			layerMask = settings.layerMask;		tempLayerMask = layerMask;
			density = settings.density;		tempDensity = density;
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
		}

		public void SaveSettings(){
			mySettings.enablePerObject = enablePerObject;
			mySettings.layerMask = layerMask;
			mySettings.density = density;
			mySettings.alignToSurface = alignToSurface;
			mySettings.alignTo = alignTo;
			mySettings.slopeBias = slopeBias;

			mySettings.scaleVar = scaleVar;
			mySettings.scaleUniform = scaleUniform;
			mySettings.scaleMaxX = scaleMaxX;
			mySettings.scaleMinX = scaleMinX;
			mySettings.scaleTopX = scaleTopX;
			mySettings.scaleBottomX = scaleBottomX;
			mySettings.scaleMaxY = scaleMaxY;
			mySettings.scaleMinY = scaleMinY;
			mySettings.scaleTopY = scaleTopY;
			mySettings.scaleBottomY = scaleBottomY;
			mySettings.scaleMaxZ = scaleMaxZ;
			mySettings.scaleMinZ = scaleMinZ;
			mySettings.scaleTopZ = scaleTopZ;
			mySettings.scaleBottomZ = scaleBottomZ;
			mySettings.scaleMax = scaleMax;
			mySettings.scaleMin = scaleMin;
			mySettings.scaleTop = scaleTop;
			mySettings.scaleBottom = scaleBottom;

			mySettings.alignToStroke = alignToStroke;
			mySettings.alignToStrokeAxis = alignToStrokeAxis;

			mySettings.rotationVar = rotationVar;
			mySettings.rotationMaxX = rotationMaxX;
			mySettings.rotationMinX = rotationMinX;
			mySettings.rotationTopX = rotationTopX;
			mySettings.rotationBottomX = rotationBottomX;
			mySettings.rotationMaxY = rotationMaxY;
			mySettings.rotationMinY = rotationMinY;
			mySettings.rotationTopY = rotationTopY;
			mySettings.rotationBottomY = rotationBottomY;
			mySettings.rotationMaxZ = rotationMaxZ;
			mySettings.rotationMinZ = rotationMinZ;
			mySettings.rotationTopZ = rotationTopZ;
			mySettings.rotationBottomZ = rotationBottomZ;

			mySettings.offsetVar = offsetVar;
			mySettings.offsetMaxX = offsetMaxX;
			mySettings.offsetMinX = offsetMinX;
			mySettings.offsetTopX = offsetTopX;
			mySettings.offsetBottomX = offsetBottomX;
			mySettings.offsetMaxY = offsetMaxY;
			mySettings.offsetMinY = offsetMinY;
			mySettings.offsetTopY = offsetTopY;
			mySettings.offsetBottomY = offsetBottomY;
			mySettings.offsetMaxZ = offsetMaxZ;
			mySettings.offsetMinZ = offsetMinZ;
			mySettings.offsetTopZ = offsetTopZ;
			mySettings.offsetBottomZ = offsetBottomZ;
			mySettings.colorVariation = colorVariation;

			mySettings.gradient = gradientContainer.gradient;
		}

	}
}
#endif
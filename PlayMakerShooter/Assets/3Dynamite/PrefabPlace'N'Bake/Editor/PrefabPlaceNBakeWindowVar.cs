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
		#region Gui Variables
		GUISkin guiSkin;
		Texture2D brush;
		Texture2D arrow;
		Texture2D settings;
		Texture2D objects;
		float columnHeight = 520f;
		float brushesColumnWidth = 256f;
		float paintColumnWidth = 256f;
		float bakeColumnWidth = 256f;
		float objectsColumnWidth = 256f;
		float brushesColumnFactor = 1f;
		float paintColumnFactor = 0f;
		float bakeColumnFactor = 0f;
		float objectsColumnFactor = 1f;
		bool brushesColumnCollapsed = false;
		bool paintColumnCollapsed = true;
		bool bakeColumnCollapsed = true;
		bool objectsColumnCollapsed = false;
		float collapseSpeed = 0.015f;
		Rect lastLayouRect;

		Vector2 brushesScroll;
		Vector2 paintScroll;
		Vector2 bakeScroll;
		Vector2 objectsScroll;
		Vector2 mainScroll;
		Vector2 brushesMainScroll;
		Color sectionColor;
		Color sectionLimitColor;
		Color subSectionColor;
		float sectionPaintHeight;
		float sectionBakeHeight;
		public bool guiDisabled;
		public bool guiDisabledByMessageBox;

		Vector2 settingsScroll;
		float settingsHeight;
		#endregion

		#region FilesVariables
		string brushesPathsPath = "Assets/3Dynamite/PrefabPlace'N'Bake/BrushesPaths.json";
		List<string> brushesPaths = new List<string> ();
		List<PPBSettings> brushes = new List<PPBSettings> ();
		int selectedBrush;
		List<string> brushesNames = new List<string> ();
		#endregion

		#region PaintColumnVariables
		[SerializeField] LayerMask layerMask;
		LayerMask tempLayerMask;
		[SerializeField] float brushSize;
		float tempBrushSize;
		[SerializeField] float density;
		float tempDensity;
		[SerializeField] int spacing;
		int tempSpacing;
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
		[SerializeField] float scaleMinY ;
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

		[SerializeField] float physDropHeight;
		float tempPhysDropHeight;

		[SerializeField] Vector3 gravity;
		Vector3 tempGravity;
		#endregion

		#region BakeColumnVariables

		[SerializeField] public bool bakeEnabled;
		bool tempBakeEnabled;
		[SerializeField] bool bakeMaterial;
		bool tempBakeMaterial;
		[SerializeField] bool bakeMeshes;
		bool tempBakeMeshes;
		[SerializeField] bool generateLMUvs;
		bool tempGenerateLMUvs;
		[SerializeField] bool useExistingUv2;
		bool tempUseExistingUv2;
		[SerializeField] bool generateBakedLod;
		bool tempGenerateBakedLod;
		[SerializeField] bool groupByObject;
		bool tempGroupByObject;
		[SerializeField] bool makeStatic;
		bool tempMakeStatic;

		#endregion

		#region ObjectsColumnVariables
		[SerializeField] DensitiesContainer densitiesContainer;
		SerializedObject serializedDensitiesContainer;
		SerializedProperty serializedDensities;

		[SerializeField] PlacedObjectsContainer placedObjectsContainer;
//		SerializedObject serializedPlacedObjectsContainer;
//		SerializedProperty serializedPlacedObjects;

		List <Texture2D> previews = new List<Texture2D>();

		ObjectSettingsWindow objectWindow;

		int soloMode = -1;
		#endregion

		#region MessageBoxesVariables
		bool bakeMessageAccepted;
		bool cloneBakeMessageAccepted = true;
		bool vertexColorMessageAccepted;
		bool cloningWithBake;
		#endregion
	}
}
#endif
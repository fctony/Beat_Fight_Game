#if UNITY_EDITOR 
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

public class GameSettingsEditor : EditorWindow {

	private GameSettings settings;
	private string databasePath = "Assets/BeatEmUp_GameTemplate3D/Settings/Resources/GameSettings.asset";
	[MenuItem("Tools/Game Settings")]

	//create window
	public static void Init() {
		GameSettingsEditor window = EditorWindow.GetWindow<GameSettingsEditor>();
		window.minSize = new Vector2(500, 200);
		window.Show();
	}

	//load database
	void OnEnable() {
		if (settings == null) LoadSettings();
	}

	//Save database
	void OnDisable(){
		EditorUtility.SetDirty(settings);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	void OnGUI() {
		EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
		DisplayMainArea();
		EditorGUILayout.EndVertical();
	}
	
	//load or create the database
	void LoadSettings() {
		settings = (GameSettings)AssetDatabase.LoadAssetAtPath(databasePath, typeof(GameSettings));
		if (settings == null) CreateDatabase();
	}
	
	//create the database
	void CreateDatabase() {
		settings = ScriptableObject.CreateInstance<GameSettings>();
		AssetDatabase.CreateAsset(settings, databasePath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}
	
	//Header
	private void BeginHeader(string label){
		EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
	}
	
	//warning text
	private void DisplayWarningText(string label){
		GUIStyle style2 = new GUIStyle(EditorStyles.wordWrappedLabel);
		style2.wordWrap = true;
		style2.stretchHeight = true;
		style2.normal.textColor = Color.red;
		EditorGUILayout.LabelField(label, style2);
	}
	
	//save the database
	void SaveSettings(){
		AssetDatabase.SaveAssets();
		EditorUtility.SetDirty(settings);
	}

	//label for items
	GUIContent label(string label){
		return new GUIContent(label);
	}
	
	//Window to Edit an existing item
	void DisplayMainArea(){
		EditorGUIUtility.labelWidth = 270;

		EditorGUILayout.Space();
		BeginHeader("Global Game Settings");
		settings.timeScale = EditorGUILayout.FloatField( new GUIContent("TimeScale: "), settings.timeScale);
		settings.framerate = EditorGUILayout.IntField( new GUIContent("Framerate: "), settings.framerate);
		settings.showFPSCounter = EditorGUILayout.Toggle( new GUIContent("Show FPS counter: "), settings.showFPSCounter);
		EditorGUILayout.Space();

		EditorGUILayout.Space();
		BeginHeader("Global Audio Settings");
		settings.SFXVolume = EditorGUILayout.FloatField( new GUIContent("SFX Volume: "), Mathf.Clamp(settings.SFXVolume, 0f, 1f));
		settings.MusicVolume = EditorGUILayout.FloatField( new GUIContent("Music Volume: "), Mathf.Clamp(settings.MusicVolume, 0f, 1f));
		EditorGUILayout.Space();
	}
}
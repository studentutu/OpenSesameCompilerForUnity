﻿using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Coffee.OpenSesameCompilers
{
    [System.Serializable]
    public class OpenSesameSetting
    {
        const string k_KeyPublishOrigin = "OpenSesame_PublishOrigin";

        public static string PublishOrigin
        {
            get { return EditorPrefs.GetString(k_KeyPublishOrigin, "") ?? ""; }
            set { EditorPrefs.SetString(k_KeyPublishOrigin, value); }
        }

        public bool OpenSesame = false;

        public static OpenSesameSetting GetAtPathOrDefault(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(Path.GetFullPath(path)))
                return new OpenSesameSetting();

            var importer = AssetImporter.GetAtPath(path);
            if (importer == null || string.IsNullOrEmpty(importer.userData))
                return new OpenSesameSetting();

            try
            {
                return JsonUtility.FromJson<OpenSesameSetting>(importer.userData);
            }
            catch
            {
                return new OpenSesameSetting();
            }
        }

        public static bool IsEnabled(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(Path.GetFullPath(path)))
                return false;

            var importer = AssetImporter.GetAtPath(path);
            if (importer == null || string.IsNullOrEmpty(importer.userData))
                return false;

            try
            {
                return JsonUtility.FromJson<OpenSesameSetting>(importer.userData).OpenSesame;
            }
            catch
            {
                return false;
            }
        }
    }

    [InitializeOnLoad]
    static class OpenSesameInspectorGUI
    {
        static GUIContent s_OpenSesameText;
        static GUIContent s_PublishText;
        static GUIContent s_HelpText;

        static OpenSesameInspectorGUI()
        {
            s_OpenSesameText = new GUIContent("Open Sesame", "Use OpenSesameCompiler instead of default csc. In other words, allow access to internals/privates to other assemblies.");
            s_PublishText = new GUIContent("Publish", "Publish this assembly as dll to the parent directory.");
            s_HelpText = new GUIContent("Help", "Open help page on browser.");
            Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
        }

        static void OnPostHeaderGUI(Editor editor)
        {
            var importer = editor.target as AssemblyDefinitionImporter;
            if (!importer)
                return;

            OpenSesameSetting setting = null;
            var userdata = importer.userData;
            try
            {
                setting = string.IsNullOrEmpty(userdata)
                        ? new OpenSesameSetting()
                        : JsonUtility.FromJson<OpenSesameSetting>(userdata);
            }
            catch
            {
                setting = new OpenSesameSetting();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                setting.OpenSesame = EditorGUILayout.ToggleLeft(s_OpenSesameText, setting.OpenSesame, GUILayout.MaxWidth(100));
                if (EditorGUI.EndChangeCheck())
                {
                    importer.userData = JsonUtility.ToJson(setting);
                    importer.SaveAndReimport();
                }

                GUILayout.FlexibleSpace();
                if (GUILayout.Button(s_PublishText, EditorStyles.miniButtonMid))
                {
                    OpenSesameSetting.PublishOrigin = Path.GetDirectoryName(importer.assetPath) + "/";
                    importer.SaveAndReimport();
                }

                if (GUILayout.Button(s_HelpText, EditorStyles.miniButtonRight))
                {
                    Application.OpenURL("https://github.com/mob-sakai/OpenSesameCompilerForUnity");
                }
            }
        }
    }
}

///* ScalingGroupEditor.cs
// * 
// * Inherits from Unity Editor and is only used in the Unity Editor to control a ScalingGroup
// * 
// * Controls childed ScalingGroup.cs child components and Block.cs child components 
// * scaling within a given view, and provides convenience options 
// * for creating new ScalingGroup BlockGroups and scaled/scalable blocks in the editor
// * 
// * This is primarily used when building an XR experience to allow child blocks to behave responsively
// * as the customer adds new blocks to the view in the editor.
// */

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;

//namespace BrandXR
//{
//    [CustomEditor(typeof(ScalingGroup))]
//    [CanEditMultipleObjects]
//    public class ScalingGroupEditor : Editor
//    {
//        SerializedProperty maxWidth;
//        SerializedProperty maxHeight;
//        SerializedProperty width;
//        SerializedProperty height;
//        SerializedProperty padding;
//        SerializedProperty children;
//        //SerializedProperty rightHandle;
//        //SerializedProperty topHandle;
//        //SerializedProperty bottomHandle;
//        //SerializedProperty leftHandle;

//        public void OnEnable()
//        {
//            maxWidth = serializedObject.FindProperty("maxWidth");
//            maxHeight = serializedObject.FindProperty("maxHeight");
//            width = serializedObject.FindProperty("width");
//            height = serializedObject.FindProperty("height");
//            padding = serializedObject.FindProperty("padding");
//            children = serializedObject.FindProperty("children");
//            //rightHandle = serializedObject.FindProperty("rightHandle");
//            //topHandle = serializedObject.FindProperty("topHandle");
//            //bottomHandle = serializedObject.FindProperty("bottomHandle");
//            //leftHandle = serializedObject.FindProperty("leftHandle");
//        }
        
//        public override void OnInspectorGUI()
//        {
//            serializedObject.Update();
//            if (width != null)
//            {
//                float newMaxWidth = maxWidth.floatValue;
//                EditorGUILayout.Slider(width, 1, newMaxWidth);
//            }
//            if (height != null)
//            {
//                float newMaxHeight = maxHeight.floatValue;
//                EditorGUILayout.Slider(height, 1, newMaxHeight);
//            }
//            if (padding != null)
//            {
//                EditorGUILayout.Slider(padding, 1, 100);
//            }
//            if (children != null)
//            {
//                EditorGUILayout.PropertyField(children);
//            }

//            //if (rightHandle != null)
//            //{
//            //    EditorGUILayout.ObjectField(rightHandle);
//            //}
//            //if (topHandle != null)
//            //{
//            //    EditorGUILayout.ObjectField(topHandle);
//            //}
//            //if (bottomHandle != null)
//            //{
//            //    EditorGUILayout.ObjectField(bottomHandle);
//            //}
//            //if (leftHandle != null)
//            //{
//            //    EditorGUILayout.ObjectField(leftHandle);
//            //}

//            serializedObject.ApplyModifiedProperties();
//        }

//        public void OnSceneGUI()
//        {
//            //ScalingGroup sg = (ScalingGroup)target;
//            //Transform child = sg.GetChildTransform();
//            //EditorGUI.BeginChangeCheck();

//            //Handles.PositionHandle()
//        }
        
//    } //END Class

//} //END Namespace
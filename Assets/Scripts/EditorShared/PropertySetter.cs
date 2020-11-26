//
// Author: Jonatan Johansson
// Created: 2019-XX-XX
//

using UnityEngine;

public enum PropertySetterMode {
	EditMode,
	PlayMode,
	Always
}

/// <summary>
/// When a serialized field is changed in the inspector, redirect the new value into a property setter.
/// </summary>
public class PropertySetterAttribute : PropertyAttribute {
	public string Property { get; set; }
	public PropertySetterMode Mode { get; set; } = PropertySetterMode.PlayMode;

	/// <param name="property">Name of the property whose setter should be used.</param>
	/// <param name="mode">When the setter should be used.</param>
	public PropertySetterAttribute(string property, PropertySetterMode mode) {
		Property = property;
		Mode = mode;
	}

	/// <summary>
	/// Mode defaults to: <see cref="PropertySetterMode.PlayMode"/>.
	/// </summary>
	/// <param name="property">Name of the property whose setter should be used.</param>
	public PropertySetterAttribute(string property) {
		Property = property;
	}
}


#if UNITY_EDITOR
namespace PropertyDrawers {
	using System;
	using System.Reflection;
	using UnityEditor;
	using UnityEngine.Assertions;

	[CustomPropertyDrawer(typeof(PropertySetterAttribute))]
	public class PropertySetterPropertyDrawer : PropertyDrawer {
		private const BindingFlags PropBindingFlags =
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			PropertySetterAttribute attr = (PropertySetterAttribute)attribute;

			if (attr.Mode == PropertySetterMode.Always ||
				(attr.Mode == PropertySetterMode.PlayMode && EditorApplication.isPlaying) ||
				(attr.Mode == PropertySetterMode.EditMode && !EditorApplication.isPlaying)) {
				SerializedObject obj = property.serializedObject;

				label.text = '*' + label.text;
				label.tooltip += " *Property";

				Color oldColor = GUI.color;
				GUI.color = new Color(oldColor.r + 0.1f, oldColor.g, oldColor.b, oldColor.a);

				EditorGUI.BeginChangeCheck();
				EditorGUI.PropertyField(position, property, label, true);

				GUI.color = oldColor;
				if (EditorGUI.EndChangeCheck()) {
					try {
						Type declType = fieldInfo.DeclaringType;
						Assert.IsNotNull(declType, "DeclaringType was null");
						PropertyInfo prop = declType.GetProperty(attr.Property, PropBindingFlags);
						Assert.IsNotNull(prop, "GetProperty returned null");
						prop.SetValue(obj.targetObject, GetSerializedPropertyValue(property));
					}
					finally {
						// Ensure field is not written to.
						obj.Update();
					}
				}
			}
			else {
				// not active
				EditorGUI.PropertyField(position, property, label, true);
			}
		}

		private static object GetSerializedPropertyValue(SerializedProperty p) {
			switch (p.propertyType) {
				case SerializedPropertyType.Enum:
				case SerializedPropertyType.ArraySize:
				case SerializedPropertyType.FixedBufferSize:
				case SerializedPropertyType.Integer:
					return p.intValue;
				case SerializedPropertyType.Boolean:
					return p.boolValue;
				case SerializedPropertyType.Float:
					return p.floatValue;
				case SerializedPropertyType.String:
					return p.stringValue;
				case SerializedPropertyType.ObjectReference:
					return p.objectReferenceValue;
				case SerializedPropertyType.Color:
					return p.colorValue;
				case SerializedPropertyType.LayerMask:
					return (LayerMask)p.intValue;
				case SerializedPropertyType.Vector2:
					return p.vector2Value;
				case SerializedPropertyType.Vector3:
					return p.vector3Value;
				case SerializedPropertyType.Vector4:
					return p.vector4Value;
				case SerializedPropertyType.Rect:
					return p.rectValue;
				case SerializedPropertyType.Character:
					return (char)p.intValue;
				case SerializedPropertyType.AnimationCurve:
					return p.animationCurveValue;
				case SerializedPropertyType.Bounds:
					return p.boundsValue;
				case SerializedPropertyType.Quaternion:
					return p.quaternionValue;
				case SerializedPropertyType.ExposedReference:
					return p.exposedReferenceValue;
				case SerializedPropertyType.Vector2Int:
					return p.vector2IntValue;
				case SerializedPropertyType.Vector3Int:
					return p.vector3IntValue;
				case SerializedPropertyType.RectInt:
					return p.rectIntValue;
				case SerializedPropertyType.BoundsInt:
					return p.boundsIntValue;

				case SerializedPropertyType.Gradient:
				case SerializedPropertyType.Generic:
					throw new NotImplementedException();
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
#endif
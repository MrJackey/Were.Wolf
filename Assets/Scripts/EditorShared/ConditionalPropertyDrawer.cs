/*
 * Created on: 2020-05-06
 * Author: Jonatan Johansson
 * Description: Unity 'EnableIf' and 'ShowIf' property attributes.
 */

using UnityEngine;

public abstract class ConditionalPropertyAttribute : PropertyAttribute {
	/// <summary>The name of the field to use.</summary>
	public string Reference { get; }

	/// <summary>The value to compare against.</summary>
	public object Comparand { get; }

	/// <summary>Invert the condition.</summary>
	public bool Invert { get; set; }

	public bool IsComparison { get; }

	public ConditionalPropertyAttribute(string reference) {
		Reference = reference;
	}

	public ConditionalPropertyAttribute(string reference, object comparand) {
		Reference = reference;
		Comparand = comparand;
		IsComparison = true;
	}
}

/// <summary>
/// Disables a serialized field in the inspector unless a certain condition evaluates to true. The condition can be
/// inverted with <see cref="ConditionalPropertyAttribute.Invert"/>.
/// </summary>
public class EnableIfAttribute : ConditionalPropertyAttribute {
	/// <summary>
	/// Disable the serialized field in the inspector unless the field with the name specified by <paramref name="reference"/>
	/// contains a truthy value.
	/// </summary>
	/// <param name="reference">The name of the reference field.</param>
	/// <remarks>
	/// 	<para>A value is considered truthy if it is neither null nor 0.</para>
	/// 	<para>The referenced field must be accessible from within the type in which the attribute is used.</para>
	/// </remarks>
	public EnableIfAttribute(string reference) : base(reference) { }

	/// <summary>
	/// Disable the serialized field in the inspector unless the value contained in the reference field is equal to
	/// <paramref name="comparand"/>.
	/// </summary>
	/// <param name="reference">The name of the reference field.</param>
	/// <param name="comparand">The value to compare against.</param>
	/// <remarks>
	/// 	<para>The referenced field must be accessible from within the type in which the attribute is used.</para>
	/// </remarks>
	public EnableIfAttribute(string reference, object comparand) : base(reference, comparand) { }
}

/// <summary>
/// Hide a serialized field in the inspector unless a certain condition evaluates to true. The condition can be
/// inverted with <see cref="ConditionalPropertyAttribute.Invert"/>.
/// </summary>
public class ShowIfAttribute : ConditionalPropertyAttribute {
	/// <summary>
	/// Hide the serialized field in the inspector unless the field with the name specified by <paramref name="reference"/>
	/// contains a truthy value.
	/// </summary>
	/// <param name="reference">The name of the reference field.</param>
	/// <remarks>
	/// 	<para>A value is considered truthy if it is neither null nor 0.</para>
	/// 	<para>The referenced field must be accessible from within the type in which the attribute is used.</para>
	/// </remarks>
	public ShowIfAttribute(string reference) : base(reference) { }

	/// <summary>
	/// Hide the serialized field in the inspector unless the value contained in the reference field is equal to
	/// <paramref name="comparand" />.
	/// </summary>
	/// <param name="reference">The name of the reference field.</param>
	/// <param name="comparand">The value to compare against.</param>
	/// <remarks>
	/// 	<para>The referenced field must be accessible from within the type in which the attribute is used.</para>
	/// </remarks>
	public ShowIfAttribute(string reference, object comparand) : base(reference, comparand) { }
}


#if UNITY_EDITOR
namespace PropertyDrawers {
	using System;
	using System.Reflection;
	using UnityEditor;
	using Object = UnityEngine.Object;

	[CustomPropertyDrawer(typeof(ConditionalPropertyAttribute), true)]
	public class ConditionalPropertyDrawer : PropertyDrawer {
		private const int HelpBoxPadding = 2;
		private const int HelpBoxMinHeight = 38; // Magic constant.

		private Type cachedReferenceFieldType;
		private FieldInfo cachedReferenceField;
		private bool cachedReferenceFieldStatic;

		private bool ok;
		private bool needsUpdate;
		private bool conditionResult;
		private string errorMessage;

		private float propHeight;

		public override bool CanCacheInspectorGUI(SerializedProperty property) {
			return false; // Probably not?
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			EvaluateCondition(property);

			if (ok && attribute is ShowIfAttribute && !conditionResult)
				return 0f; // Hidden.

			float height = propHeight = EditorGUI.GetPropertyHeight(property, label, true);
			if (!ok && errorMessage != null)
				height += HelpBoxPadding + Mathf.Max(HelpBoxMinHeight,
				                                     EditorStyles.helpBox.CalcHeight(
					                                     new GUIContent(errorMessage),
					                                     EditorGUIUtility.currentViewWidth));

			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			if (needsUpdate) {
				Debug.Assert(false, "GetPropertyHeight did not execute before OnGUI.");
				EvaluateCondition(property);
			}

			needsUpdate = true;

			if (ok) {
				if (attribute is EnableIfAttribute) {
					EditorGUI.BeginDisabledGroup(!conditionResult);
					EditorGUI.PropertyField(position, property, label, true);
					EditorGUI.EndDisabledGroup();
					return;
				}
				if (attribute is ShowIfAttribute && !conditionResult)
					return; // Hidden.
			}
			else {
				if (errorMessage != null) {
					EditorGUI.PropertyField(position, property, label, true);
					position.yMin += HelpBoxPadding + propHeight;
					EditorGUI.HelpBox(position, errorMessage, MessageType.Error);
					return;
				}
			}

			// Fallback to default drawer.
			EditorGUI.PropertyField(position, property, label, true);
		}

		private void EvaluateCondition(SerializedProperty property) {
			ConditionalPropertyAttribute attr = attribute as ConditionalPropertyAttribute;

			ok = false;
			errorMessage = null;
			needsUpdate = false;

			// CHECK STUFF
			if (attr == null) return;
			if (attr.Reference == null) {
				errorMessage = "Field reference is null.";
				return;
			}

			SerializedObject serializedObject = property.serializedObject;
			if (serializedObject.isEditingMultipleObjects) return; // Show default when multi editing objects.

			Object target = serializedObject.targetObject;
			if (target == null) return;

			// GET REFERENCE FIELD INFO
			Type declType = fieldInfo.DeclaringType;
			if (cachedReferenceField == null) {
				cachedReferenceField = declType.GetField(attr.Reference,
				                                         BindingFlags.Public | BindingFlags.NonPublic |
				                                         BindingFlags.Instance | BindingFlags.Static);
				if (cachedReferenceField == null) {
					errorMessage = $"No field named '{attr.Reference}' found in type '{declType.Name}'.";
					return;
				}

				cachedReferenceFieldType = cachedReferenceField.FieldType;
				cachedReferenceFieldStatic = cachedReferenceField.IsStatic;
			}

			// GET REFERENCE FIELD VALUE AND CHECK IT
			object value = cachedReferenceField.GetValue(cachedReferenceFieldStatic ? null : target);
			bool result = attr.IsComparison
				? Equals(value, attr.Comparand)
				: IsTruthy(value, cachedReferenceFieldType);

			// STORE RESULT
			conditionResult = attr.Invert ? !result : result;
			ok = true;
		}

		private static bool IsTruthy(object value, Type type) {
			if (type.IsValueType) {
				if (value is IConvertible v) {
					TypeCode tc = v.GetTypeCode();
					if (tc == TypeCode.Object || tc == TypeCode.DateTime) return true;
					try { return v.ToDouble(null) != default; }
					catch { /* ignored */ }
				}
			}

			return value != null && !value.Equals(null);
		}
	}
}
#endif
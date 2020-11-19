//
// Author: Jonatan Johansson
// Created: XX-XX-2019
//

using System;
using System.Reflection;
using UnityEngine;

public class EnableIfAttribute : PropertyAttribute {
	public string MemberName { get; set; }
	public object Comparand { get; set; }
	public bool Not { get; set; }

	public EnableIfAttribute(string memberName) {
		MemberName = memberName;
	}

	public EnableIfAttribute(string memberName, object comparand) {
		MemberName = memberName;
		Comparand = comparand;
	}
}

#if UNITY_EDITOR
namespace PropertyDrawers {
	using UnityEditor;

	[CustomPropertyDrawer(typeof(EnableIfAttribute))]
	public class EnableIfPropertyDrawer : PropertyDrawer {
		// TODO: Cache reflection results.

		private const BindingFlags BFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
			BindingFlags.Static;

		private MemberInfo FindAppropriateMember(Type type, string name, object context) {
			EnableIfAttribute attr = (EnableIfAttribute)attribute;
			Type neededType = attr.Comparand != null ? attr.Comparand.GetType() : typeof(bool);

			// field
			var field = type.GetField(name, BFlags);
			if (field != null) {
				if (field.FieldType != neededType) {
					Debug.LogError("field type needs to be the same as comparand type or bool");
					return null;
				}

				return field;
			}

			// property
			var property = type.GetProperty(name, BFlags, null, neededType, Type.EmptyTypes, null);
			if (property != null) {
				if (property.PropertyType != neededType) {
					Debug.LogError("property type needs to be the same as comparand type or bool");
					return null;
				}

				if (!property.CanRead) {
					Debug.LogError("property needs to have a getter");
					return null;
				}

				return property;
			}

			// method
			var method = type.GetMethod(name, BFlags, null, Type.EmptyTypes, null);
			if (method != null) {
				if (method.ReturnType != neededType) {
					Debug.LogError("return type needs to be the same as comparand type or bool");
					return null;
				}

				return method;
			}

			Debug.LogError("no appropriate member found");
			return null;
		}

		private static bool TryGetMemberValue(MemberInfo member, object target, out object value) {
			switch (member) {
				case FieldInfo field:
					value = field.GetValue(field.IsStatic ? null : target);
					return true;
				case PropertyInfo property:
					MethodInfo getter = property.GetMethod;
					value = getter.Invoke(getter.IsStatic ? null : target, null);
					return true;
				case MethodInfo method:
					value = method.Invoke(method.IsStatic ? null : target, null);
					return true;
				default:
					Debug.LogAssertion(null);
					value = null;
					return false;
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return EditorGUI.GetPropertyHeight(property, label);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			bool enabled = true;
			var attr = (EnableIfAttribute)attribute;
			if (attr.MemberName != null) {
				object target = property.serializedObject.targetObject;
				Type type = target.GetType();

				var member = FindAppropriateMember(type, attr.MemberName, target);
				if (member != null && TryGetMemberValue(member, target, out object memberValue)) {
					if (attr.Comparand != null)
						enabled = memberValue == attr.Comparand;
					else
						// TODO: Assert bool type
						enabled = memberValue is true;
					//enabled = memberValue == (attr.Comparand ?? true);
					if (attr.Not) enabled = !enabled;
				}
			}

			if (!enabled) EditorGUI.BeginDisabledGroup(true);

			// draw default
			EditorGUI.PropertyField(position, property, label, true);

			if (!enabled) EditorGUI.EndDisabledGroup();
		}
	}
}
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;

namespace Extension
{
    public static class Extension
    {
		private static readonly Dictionary<Extension.FieldKey, FieldInfo> _fieldCache = new Dictionary<Extension.FieldKey, FieldInfo>();
		private struct FieldKey
		{
			public FieldKey(Type inType, string inName)
			{
				this.type = inType;
				this.name = inName;
				this._hashCode = (this.type.GetHashCode() ^ this.name.GetHashCode());
			}
			public override int GetHashCode()
			{
				return this._hashCode;
			}
			public readonly Type type;
			public readonly string name;
			private readonly int _hashCode;
		}

		public static object GetField(this object self, string name, Type type = null)
		{
			if (type == null)
			{
				type = self.GetType();
			}
			if (!self.SearchForFields(name))
			{
				Console.WriteLine("[KK_Extension] Field Not Found: " + name);
				return false;
			}
			Extension.FieldKey fieldKey = new Extension.FieldKey(type, name);
			FieldInfo field;
			if (!Extension._fieldCache.TryGetValue(fieldKey, out field))
			{
				field = fieldKey.type.GetField(fieldKey.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				Extension._fieldCache.Add(fieldKey, field);
			}
			return field.GetValue(self);
		}
		public static bool SetField(this object self, string name, object value, Type type = null)
		{
			if (type == null)
			{
				type = self.GetType();
			}
			if (!self.SearchForFields(name))
			{
				Console.WriteLine("[KK_Extension] Field Not Found: " + name);
				return false;
			}
			Extension.FieldKey fieldKey = new Extension.FieldKey(type, name);
			FieldInfo field;
			if (!Extension._fieldCache.TryGetValue(fieldKey, out field))
			{
				field = fieldKey.type.GetField(fieldKey.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				if (field != null)
				{
					Extension._fieldCache.Add(fieldKey, field);
					field.SetValue(self, value);
					return true;
				}
				Console.WriteLine("[KK_Extension] Set Field Not Found: " + name);
			}
			return false;
		}
		public static bool SetProperty(this object self, string name, object value)
		{
			if (!self.SearchForProperties(name))
			{
				Console.WriteLine("[KK_Extension] Field Not Found: " + name);
				return false;
			}
			PropertyInfo property = self.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty);
			if (property != null)
			{
				property.SetValue(self, value, null);
				return true;
			}
			Console.WriteLine("[KK_Extension] Set Property Not Found: " + name);
			return false;
		}
		public static object GetProperty(this object self, string name)
		{
			if (!self.SearchForProperties(name))
			{
				Console.WriteLine("[KK_Extension] Property Not Found: " + name);
				return false;
			}
			return self.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty).GetValue(self, null);
		}
		public static object Invoke(this object self, string name, object[] p = null)
		{
			object result;
			try
			{
				result = ((self != null) ? self.GetType().InvokeMember(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod, null, self, p) : null);
			}
			catch (MissingMethodException ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.InnerException);
				MemberInfo[] array = (self != null) ? self.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod) : null;
				List<string> list = new List<string>();
				foreach (MemberInfo memberInfo in array)
				{
					if (memberInfo.Name == name)
					{
						return true;
					}
					list.Add("[KK_Extension] Member Name/Type: " + memberInfo.Name + " / " + memberInfo.MemberType.ToString());
				}
				foreach (string value in list)
				{
					Console.WriteLine(value);
				}
				Console.WriteLine("[KK_Extension] Get " + array.Length.ToString() + " Members.");
				result = false;
			}
			return result;
		}
		public static bool SearchForFields(this object self, string name)
		{
			FieldInfo[] fields = self.GetType().GetFields(AccessTools.all);
			List<string> list = new List<string>();
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.Name == name)
				{
					return true;
				}
				List<string> list2 = list;
				string str = "[KK_Extension] Field Name/Type: ";
				string name2 = fieldInfo.Name;
				string str2 = " / ";
				Type fieldType = fieldInfo.FieldType;
				list2.Add(str + name2 + str2 + ((fieldType != null) ? fieldType.ToString() : null));
			}
			Console.WriteLine("[KK_Extension] Get " + fields.Length.ToString() + " Fields.");
			foreach (string value in list)
			{
				Console.WriteLine(value);
			}
			return false;
		}

		public static bool SearchForProperties(this object self, string name)
		{
			PropertyInfo[] properties = self.GetType().GetProperties(AccessTools.all);
			List<string> list = new List<string>();
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.Name == name)
				{
					return true;
				}
				List<string> list2 = list;
				string str = "[KK_Extension] Property Name/Type: ";
				string name2 = propertyInfo.Name;
				string str2 = " / ";
				Type propertyType = propertyInfo.PropertyType;
				list2.Add(str + name2 + str2 + ((propertyType != null) ? propertyType.ToString() : null));
			}
			Console.WriteLine("[KK_Extension] Get " + properties.Length.ToString() + " Properties.");
			foreach (string value in list)
			{
				Console.WriteLine(value);
			}
			return false;
		}
		public static Sprite LoadNewSprite(string FilePath, int width, int height, float PixelsPerUnit = 100f)
		{
			Texture2D texture2D = Extension.LoadTexture(FilePath);
			if (null == texture2D)
			{
				texture2D = Extension.LoadDllResource(FilePath, width, height);
			}
			return Sprite.Create(texture2D, new Rect(0f, 0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0f, 0f), PixelsPerUnit);
		}
		public static Texture2D LoadTexture(string FilePath)
		{
			if (File.Exists(FilePath))
			{
				byte[] data = File.ReadAllBytes(FilePath);
				Texture2D texture2D = new Texture2D(2, 2);
				if (texture2D.LoadImage(data))
				{
					return texture2D;
				}
			}
			return null;
		}
		public static Texture2D LoadDllResource(string FilePath, int width, int height)
		{
			Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(FilePath);
			Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
			texture2D.LoadImage(Extension.ReadToEnd(manifestResourceStream));
			if (texture2D == null)
			{
				Console.WriteLine("Missing Dll resource: " + FilePath);
			}
			return texture2D;
		}
		private static byte[] ReadToEnd(Stream stream)
		{
			long position = stream.Position;
			stream.Position = 0L;
			byte[] result;
			try
			{
				byte[] array = new byte[4096];
				int num = 0;
				int num2;
				while ((num2 = stream.Read(array, num, array.Length - num)) > 0)
				{
					num += num2;
					if (num == array.Length)
					{
						int num3 = stream.ReadByte();
						if (num3 != -1)
						{
							byte[] array2 = new byte[array.Length * 2];
							Buffer.BlockCopy(array, 0, array2, 0, array.Length);
							Buffer.SetByte(array2, num, (byte)num3);
							array = array2;
							num++;
						}
					}
				}
				byte[] array3 = array;
				if (array.Length != num)
				{
					array3 = new byte[num];
					Buffer.BlockCopy(array, 0, array3, 0, num);
				}
				result = array3;
			}
			finally
			{
				stream.Position = position;
			}
			return result;
		}
		public static Texture2D AddWatermark(Texture2D background, Texture2D watermark, int startX, int startY)
		{
			Texture2D texture2D = new Texture2D(background.width, background.height, background.format, false);
			for (int i = 0; i < background.width; i++)
			{
				for (int j = 0; j < background.height; j++)
				{
					if (i >= startX && j >= startY && i - startX < watermark.width && j - startY < watermark.height)
					{
						Color pixel = background.GetPixel(i, j);
						Color pixel2 = watermark.GetPixel(i - startX, j - startY);
						Color color = Color.Lerp(pixel, pixel2, pixel2.a / 1f);
						texture2D.SetPixel(i, j, color);
					}
					else
					{
						texture2D.SetPixel(i, j, background.GetPixel(i, j));
					}
				}
			}
			texture2D.Apply();
			return texture2D;
		}
		//public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this object self)
		//{
		//	IDictionary dictionary = self as IDictionary;
		//	if (dictionary == null)
		//	{
		//		Console.WriteLine("[KK_Extension] Faild to cast to Dictionary!");
		//		return null;
		//	}
		//	return Extension.<ToDictionary>g__CastDict|14_2<TKey, TValue>(dictionary).ToDictionary((DictionaryEntry entry) => (TKey)((object)entry.Key), (DictionaryEntry entry) => (TValue)((object)entry.Value));
		//}
		public static List<T> ToList<T>(this object self)
		{
			IEnumerable<T> enumerable = self as IEnumerable<T>;
			if (enumerable == null)
			{
				Console.WriteLine("[KK_Extension] Faild to cast to List!");
				return null;
			}
			return new List<T>(enumerable);
		}
		public static object ToListWithoutType(this object self)
		{
			Type type = self.GetType();
			foreach (Type type2 in type.GetInterfaces())
			{
				if (type2.IsGenericType && type2.GetGenericTypeDefinition() == typeof(IList<>))
				{
					Type type3 = type.GetGenericArguments()[0];
					MethodInfo methodInfo = typeof(Extension).GetMethod("ToList", BindingFlags.Static | BindingFlags.Public);
					if (type3 != null)
					{
						methodInfo = methodInfo.MakeGenericMethod(new Type[]
						{
							type3
						});
					}
					return methodInfo.Invoke(null, new object[]
					{
						self
					});
				}
			}
			Console.WriteLine("[KK_Extension] Faild to cast to List<unknown>!");
			return null;
		}
		public static int RemoveAll(this object self, Predicate<object> match)
		{
			int num = 0;
			IList list = self as IList;
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (match(list[i]))
					{
						list.RemoveAt(i);
						num++;
						i--;
					}
				}
			}
			else
			{
				Console.WriteLine("[KK_Extension] RemoveAll: Input Object is not type of List<unknown>!");
			}
			return num;
		}
		public static int Count(this object self)
		{
			IList list = self as IList;
			if (list != null)
			{
				return list.Count;
			}
			Console.WriteLine("[KK_Extension] Count: Input Object is not type of List<unknown>!");
			return -1;
		}
		public static object Where(this object self, Predicate<object> match)
		{
			if (self is IList)
			{
				Type type = self.GetType();
				Type type2 = null;
				foreach (Type type3 in type.GetInterfaces())
				{
					if (type3.IsGenericType && type3.GetGenericTypeDefinition() == typeof(IList<>))
					{
						type2 = type.GetGenericArguments()[0];
					}
				}
				MethodInfo methodInfo = typeof(Extension).GetMethod("ToList", BindingFlags.Static | BindingFlags.Public);
				if (type2 != null)
				{
					methodInfo = methodInfo.MakeGenericMethod(new Type[]
					{
						type2
					});
				}
				IList list = (IList)methodInfo.Invoke(null, new object[]
				{
					self
				});
				list.RemoveAll((object x) => !match(x));
				return list;
			}
			Console.WriteLine("[KK_Extension] Where: Input Object is not type of List<unknown>!");
			return null;
		}
		public static void Add(this object self, object obj2Add)
		{
			IList list = self as IList;
			if (list != null)
			{
				Type type = null;
				foreach (Type type2 in self.GetType().GetInterfaces())
				{
					if (type2.IsGenericType && type2.GetGenericTypeDefinition() == typeof(IList<>))
					{
						type = self.GetType().GetGenericArguments()[0];
					}
				}
				if (type != null && obj2Add.GetType() == type)
				{
					list.Add(obj2Add);
					return;
				}
				Console.WriteLine("[KK_Extension] Type not Match! Cannot Add " + obj2Add.GetType().FullName + " into " + type.FullName);
			}
		}
		//public static void AddRange(this object self, object obj2Add)
		//{
		//	IList list = self as IList;
		//	if (list != null)
		//	{
		//		IList list2 = obj2Add as IList;
		//		if (list2 != null)
		//		{
		//			Type type = null;
		//			foreach (Type type2 in self.GetType().GetInterfaces())
		//			{
		//				if (type2.IsGenericType && type2.GetGenericTypeDefinition() == typeof(IList<>))
		//				{
		//					type = self.GetType().GetGenericArguments()[0];
		//				}
		//			}
		//			Type type3 = null;
		//			foreach (Type type4 in obj2Add.GetType().GetInterfaces())
		//			{
		//				if (type4.IsGenericType && type4.GetGenericTypeDefinition() == typeof(IList<>))
		//				{
		//					type3 = obj2Add.GetType().GetGenericArguments()[0];
		//				}
		//			}
		//			if (type != null && type == type3)
		//			{
		//				using (IEnumerator enumerator = list2.GetEnumerator())
		//				{
		//					while (enumerator.MoveNext())
		//					{
		//						object value = enumerator.Current;
		//						list.Add(value);
		//					}
		//					return;
		//				}
		//			}
		//			Console.WriteLine("[KK_Extension] Type not Match! Cannot Add " + type3.FullName + " into " + type.FullName);
		//		}
		//	}
		//}
		public static BaseUnityPlugin TryGetPluginInstance(string pluginName, Version minimumVersion = null)
		{
			PluginInfo pluginInfo;
			Chainloader.PluginInfos.TryGetValue(pluginName, out pluginInfo);
			if (pluginInfo != null)
			{
				if (pluginInfo.Metadata.Version >= minimumVersion)
				{
					return pluginInfo.Instance;
				}
				Console.WriteLine(string.Concat(new string[]
				{
					"[KK_Extension] ",
					pluginName,
					" v",
					pluginInfo.Metadata.Version.ToString(),
					" is detacted OUTDATED."
				}));
				Console.WriteLine(string.Concat(new string[]
				{
					"[KK_Extension] Please update ",
					pluginName,
					" to at least v",
					minimumVersion.ToString(),
					" to enable related feature."
				}));
			}
			return null;
		}
	}
}

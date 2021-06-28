using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;

namespace seyself
{
	public class FileIO
	{
		public FileIO()
		{
			
		}

		public static bool Exists(string filePath)
		{
			return File.Exists(filePath);
		}

		public static void WriteText(string path, string text)
		{
			Thread thread = new Thread(()=>{
				StreamWriter sw = new StreamWriter(path, false);
				sw.WriteLine(text);
				sw.Flush();
				sw.Close();
			});
			thread.IsBackground = true;
			thread.Start();
		}

		public static string ReadText(string path)
		{
			FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			StreamReader reader = new StreamReader(fileStream);
			string text = reader.ReadToEnd();
			reader.Close();
			fileStream.Close();
			return text;
		}

		public static void WriteBinary(string path, byte[] bytes)
		{
			File.WriteAllBytes(path, bytes);
		}

		public static byte[] ReadBinary(string path)
		{
			FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
			BinaryReader bin = new BinaryReader(fileStream);
			byte[] bytes = bin.ReadBytes((int)bin.BaseStream.Length);
			bin.Close();
			fileStream.Close();
			return bytes;
		}

		public static void WriteImage(string path, Texture2D texture, string fileType="png")
		{
			Thread thread = new Thread(()=>{
				fileType = fileType.ToLower();
				if (fileType == "jpeg" || fileType == "jpg")
				{
					byte[] bytes = ImageConversion.EncodeToJPG(texture);
					WriteBinary(path, bytes);
				}
				else
				{
					byte[] bytes = ImageConversion.EncodeToPNG(texture);
					WriteBinary(path, bytes);
				}
			});
			thread.IsBackground = true;
			thread.Start();
		}

		public static Texture2D ReadImage(string path)
		{
			byte[] bytes = ReadBinary( path );
			return BinaryToTexture( bytes );
		}
		
		public static Texture2D BinaryToTexture(byte[] bytes)
		{
			Texture2D tex = new Texture2D(2, 2);
			ImageConversion.LoadImage(tex, bytes);
			return tex;
		}

		public static bool IsPNG(byte[] bytes)
		{
			if (bytes[1] != 0x50) return false;
			if (bytes[2] != 0x4E) return false;
			if (bytes[3] != 0x47) return false;
			return true;
		}

		public static bool IsJPEG(byte[] bytes)
		{
			if (bytes[6] != 0x4A) return false;
			if (bytes[7] != 0x46) return false;
			if (bytes[8] != 0x49) return false;
			if (bytes[9] != 0x46) return false;
			return true;
		}
		
		public static void Serialize<T>(string path, T obj)
		{
			Serialize<T>(path, obj, false);
		}

		public static void Serialize<T>(string path, T obj, bool prettyPrint)
		{
			string json = JsonUtility.ToJson(obj, prettyPrint);
			WriteText(path, json);
		}

		public static T Deserialize<T>(string path)
		{
			try 
			{
				string json = ReadText(path);
				return JsonUtility.FromJson<T>(json);
			}
			catch (System.Exception e) 
			{
				Debug.LogWarning(e);
			}
			return default(T);
		}
	}
}

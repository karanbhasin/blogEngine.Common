using System;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Foundation.Common.Util {
	/// <summary>
	/// Summary description for SerializationUtil.
	/// </summary>
	public class SerializationUtil {
		#region Binary
		public static byte[] SerializeToBinary(object source) { 
			byte[] buffer;

			using (MemoryStream ms = new MemoryStream()) {
				BinaryFormatter bFormatter = new BinaryFormatter(); 
				// Serialize the class into the MemoryStream 
				bFormatter.Serialize(ms, source); 
				//ms.Seek(0,0); 
				// Write the stream contents to a byte array
				buffer = ms.ToArray();
			}

			return buffer;
		} 
		public static object DeserializeFromBinary(byte[] buffer) { 
			Object deserializedObject = null;
			
			if (buffer != null) {
				using (MemoryStream ms = new MemoryStream()) {
					// Write the byte array to a stream
					ms.Write (buffer, 0, buffer.Length);
					ms.Seek(0, 0); 
					BinaryFormatter b = new BinaryFormatter(); 
					// Deserialize the stream to an object
					deserializedObject = b.Deserialize(ms);
				}
			}

			return deserializedObject;
		} 
		#endregion Binary
	}
}


using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using AppmetrS2S.Persister;
using AppmetrS2S.Serializations;
using Common.Logging;

namespace AppmetrS2S
{
    internal class Utils
    {
        private static readonly ILog Log = LogManager.GetLogger<FileBatchPersister>();

        public static long GetNowUnixTimestamp()
        {
            return (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public static byte[] SerializeBatch(Batch batch, IJsonSerializer serializer)
        {
            var json = serializer.Serialize(batch);
            byte[] data = Encoding.UTF8.GetBytes(json);
            return data;
        }

        public static void WriteData(Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public static void WriteBatch(Stream stream, Batch batch, IJsonSerializer serializer)
        {
            Log.TraceFormat("Starting serialize batch with id={0}", batch.GetBatchId());
            var json = serializer.Serialize(batch);
            Log.TraceFormat("Get bytes from serialized batch with id={0}", batch.GetBatchId());
            var data = Encoding.UTF8.GetBytes(json);
            Log.TraceFormat("Write bytes to stream. Batch id={0}", batch.GetBatchId());
            stream.Write(data, 0, data.Length);
            Log.TraceFormat("Flush stream. Batch id={0}", batch.GetBatchId());
            stream.Flush();
        }

        public static bool TryReadBatch(Stream stream, IJsonSerializer serializer, out Batch batch)
        {
            try
            {
                batch = serializer.Deserialize<Batch>(new StreamReader(stream).ReadToEnd());
                Log.TraceFormat("Successfully read the batch {0}", batch.GetBatchId());
                return true;
            }
            catch (Exception e)
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error("Error while deserialization batch", e);    
                }
                
                batch = null;
                return false;
            }
        }

        public static string UrlEncode(string str, Encoding e) => 
	        !string.IsNullOrWhiteSpace(str)
		        ? Encoding.ASCII.GetString(UrlEncodeToBytes(str, e)) 
		        : null;

        private static byte[] UrlEncodeToBytes(string str, Encoding e)
        {
	        var bytes = e.GetBytes(str);
	        return UrlEncode(bytes, bytes.Length);
        }

        private static byte[] UrlEncode(byte[] bytes, int count)
        {
	        var num1 = 0;
            var num2 = 0;
            for (var index = 0; index < count; ++index)
            {
                var ch = (char)bytes[index];
                if (ch == ' ')
                    ++num1;
                else if (!IsUrlSafeChar(ch))
                    ++num2;
            }
            if (num1 == 0 && num2 == 0)
            {
                if (bytes.Length == count)
                    return bytes;
                byte[] numArray = new byte[count];
                Buffer.BlockCopy(bytes, 0, numArray, 0, count);
                return numArray;
            }
            byte[] numArray1 = new byte[count + num2 * 2];
            int num3 = 0;
            for (int index1 = 0; index1 < count; ++index1)
            {
                byte num4 = bytes[index1];
                char ch = (char)num4;
                if (IsUrlSafeChar(ch))
                    numArray1[num3++] = num4;
                else if (ch == ' ')
                {
                    numArray1[num3++] = (byte)43;
                }
                else
                {
                    byte[] numArray2 = numArray1;
                    int index2 = num3;
                    int num5 = index2 + 1;
                    numArray2[index2] = 37;
                    byte[] numArray3 = numArray1;
                    int index3 = num5;
                    int num6 = index3 + 1;
                    int charLower1 = (int)(byte)ToCharLower((int)num4 >> 4);
                    numArray3[index3] = (byte)charLower1;
                    byte[] numArray4 = numArray1;
                    int index4 = num6;
                    num3 = index4 + 1;
                    int charLower2 = (int)(byte)ToCharLower((int)num4);
                    numArray4[index4] = (byte)charLower2;
                }
            }
            return numArray1;
        }

        public static bool IsUrlSafeChar(char ch)
        {
	        if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || (ch >= '0' && ch <= '9' || ch == '!'))
		        return true;
	        switch (ch)
	        {
		        case '(':
		        case ')':
		        case '*':
		        case '-':
		        case '.':
		        case '_':
			        return true;
		        default:
			        return false;
	        }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char ToCharLower(int value)
        {
	        value &= 15;
	        value += 48;
	        if (value > 57)
		        value += 39;
	        return (char)value;
        }
    }
}
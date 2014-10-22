using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PillarAPI.Utilities
{
    internal class StringManipulation
    {
        /// <summary>
        ///     Converts byte[] to string.
        /// </summary>
        /// <param name="buff">Byte[] to be converted</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <returns>The string that represents the byte[]</returns>
        public static string ByteToString(IEnumerable<byte> buff)
        {
            return buff.Aggregate("", (current, t) => current + t.ToString("X2"));
        }

        /// <summary>
        ///     Converts string to byte[].
        /// </summary>
        /// <param name="str">String to be converted</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.Text.DecoderFallbackException"></exception>
        /// <returns>The byte[] that represents the string</returns>
        public static byte[] StringToByteArray(string str)
        {
            var encoding = new UTF8Encoding();
            return encoding.GetBytes(str);
        }

        /// <summary>
        ///     Converts byte[] to a base64 string.
        /// </summary>
        /// <param name="bytes">Byte[] to be converted</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <returns>The string that represents the byte[]</returns>
        public static string BytesToBase64String(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        ///     Converts string to a base64 byte[].
        /// </summary>
        /// <param name="inputString"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.FormatException"></exception>
        /// <returns></returns>
        public static byte[] StringToBase64ByteArray(string inputString)
        {
            return Convert.FromBase64String(inputString);
        }
    }
}
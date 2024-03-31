using System;

namespace MFPS.ULogin
{
    public class GCM
    {
        /// <summary>
        /// Concatening given array of bytes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static byte[] Concat(byte[] a, byte[] b)
        {
            byte[] output = new byte[a.Length + b.Length];

            for (int i = 0; i < a.Length; i++)
            {
                output[i] = a[i];
            }

            for (int j = 0; j < b.Length; j++)
            {
                output[a.Length + j] = b[j];
            }

            return output;
        }

        /// <summary>
        /// Return subarray of bytes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] SubArray(byte[] data, int start, int length)
        {
            byte[] result = new byte[length];

            Array.Copy(data, start, result, 0, length);

            return result;
        }

    }
}
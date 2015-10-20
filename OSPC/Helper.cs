using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSPC
{
    public static class Extensions
    {
        /// <summary>
        /// Returns the string with at most "maxLength" characters. If the length is exceeded, suffix is added.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxLength"></param>
        /// <param name="suffix">Suffix to add when string length exceedes maxLength</param>
        /// <param name="trimStart">cut off first chars</param>
        /// <returns></returns>
        public static string MaxLength(this string str, int maxLength, string suffix = "", bool trimStart = false)
        {
            if (!string.IsNullOrEmpty(str) && str.Length > maxLength)
            {
                if(trimStart)
                    return (suffix ?? "") + str.Substring(str.Length - maxLength, maxLength);
                else
                    return str.Substring(0, maxLength) + suffix;
            }
            else
            {
                return str;
            }
        }
    }
}

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace OwlCore.Wpf.Extensions
{
    public static class ResourceDictionaryExtensions
    {
        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">
        /// The key of the value to get.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified key,
        /// if the key is found; otherwise, <c>null</c>. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <see cref="ResourceDictionary"/> contains an element with the specified key; otherwise, <c>false</c>.
        /// </returns>
        public static bool TryGetValue(this ResourceDictionary dict, object key, [MaybeNullWhen(false)] out object value)
        {
            try
            {
                value = dict[key];
                return true;
            }
            catch (KeyNotFoundException)
            {
                value = null;
                return false;
            }
        }
    }
}

namespace System.Collections.Generic {
    /// <summary>
    /// Provides extension methods for the System.Collections.Generic.List class.
    /// </summary>
    public static class ListExtensions {
        /// <summary>
        /// Adds the elements of the specified array to the end of the System.Collections.Generic.List&lt;T&gt;.
        /// </summary>
        /// <typeparam name="T">
        /// The type of elements in the array.
        /// </typeparam>
        /// <param name="array">
        /// The array whose elements should be added to the end of the System.Collections.Generic.List&lt;T&gt;.
        /// </param>
        public static void AddRange<T>(this List<T> list, T[] array) {
            foreach (var element in array) {
                list.Add(element);
            }
        }

		public static void ForEachWithIndex<T>(this List<T> elems, Action<int, T> action) {
			int i = 0;
			elems.ForEach(elem => action(i++, elem));
		}
    }
}

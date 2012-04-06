namespace System.Data {
    /// <summary>
    /// Provides extension methods for the System.Data.IDataReader class.
    /// </summary>
    public static class DataReaderExtensions {
        /// <summary>
        /// Determines whether the IDataReader contains the specified column.
        /// </summary>
        /// <param name="columnName">
        /// The name of the column to find.
        /// </param>
        /// <returns>
        /// A boolean determining whether the column exists in the IDataReader.
        /// </returns>
        public static bool ContainsColumn(this IDataReader dr, string columnName) {
            //
            // Determine how many columns the IDataReader has and loop through them.
            //
            for (int i = 0; i < dr.FieldCount; i++) {
                //
                // Get the name of the current column and return true if it matches the specified column name.
                //
                if (dr.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }

            return false;
        }
    }
}

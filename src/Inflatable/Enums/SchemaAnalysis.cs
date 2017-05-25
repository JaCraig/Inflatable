namespace Inflatable.Enums
{
    /// <summary>
    /// Schema analysis enum
    /// </summary>
    public enum SchemaAnalysis
    {
        /// <summary>
        /// Do not analyze
        /// </summary>
        NoAnalysis = 0x0,

        /// <summary>
        /// Generate analysis for the source
        /// </summary>
        GenerateAnalysis = 0x1,

        /// <summary>
        /// Automatically apply analysis found
        /// </summary>
        ApplyAnalysis = 0x2
    }
}
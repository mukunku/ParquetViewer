using ParquetViewer.Engine.Types;

namespace ParquetViewer.Engine
{
    //Global settings, what can go wrong? It's convenient, though.
    public static class ParquetEngineSettings
    {
        /// <summary>
        /// By default Parquet Engine will render Dates using the system culture's format.
        /// By setting this value a custom date format can be used instead.
        /// </summary>
        /// <remarks>Parquet Engine renders dates when converting <see cref="ListValue"/>, 
        /// <see cref="StructValue"/>, and <see cref="MapValue"/> types to string.</remarks>
        public static string? DateDisplayFormat { get; set; }
    }
}

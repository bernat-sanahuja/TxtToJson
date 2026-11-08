using System.IO;

namespace TxtToJson.Models
{
    /// <summary>
    /// Un .txt ya convertido a JSON, listo para mostrarse y guardarse.
    /// </summary>
    public class ResultadoJson
    {
        public ResultadoJson(Archivo origen, string json)
        {
            Origen = origen;
            Json = json;
        }

        public Archivo Origen { get; }

        /// <summary>Nombre del .json resultante (mismo nombre que el .txt).</summary>
        public string Nombre => Path.GetFileNameWithoutExtension(Origen.Ruta) + ".json";

        /// <summary>Ruta del .txt de origen, para sugerir la carpeta al guardar.</summary>
        public string RutaOrigen => Origen.Ruta;

        /// <summary>El JSON ya formateado con sangría.</summary>
        public string Json { get; }
    }
}

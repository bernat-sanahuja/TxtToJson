using System.IO;

namespace TxtToJson.Models
{
    /// <summary>
    /// Datos de un archivo .txt soltado en la ventana.
    /// No sabe nada de WPF: solo guarda la ruta y expone el nombre.
    /// </summary>
    public class Archivo
    {
        public Archivo(string ruta)
        {
            Ruta = ruta;
        }

        public string Ruta { get; }

        public string Nombre => Path.GetFileName(Ruta);
    }
}

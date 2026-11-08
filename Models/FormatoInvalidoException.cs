using System;

namespace TxtToJson.Models
{
    /// <summary>
    /// Error de formato en el .txt de entrada. Lleva el número de línea
    /// para que el usuario sepa exactamente dónde está el problema.
    /// </summary>
    public class FormatoInvalidoException : Exception
    {
        public FormatoInvalidoException(int linea, string mensaje)
            : base($"line {linea}: {mensaje}")
        {
            Linea = linea;
        }

        public int Linea { get; }
    }
}

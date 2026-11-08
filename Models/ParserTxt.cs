using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;

namespace TxtToJson.Models
{
    /// <summary>
    /// Convierte el formato .txt propio del programa en un árbol JSON.
    ///
    /// Reglas:
    ///   - La sangría decide el anidamiento (tab = 4 columnas).
    ///   - "clave:" sin valor abre un objeto hijo (o una lista, si los hijos empiezan por "-").
    ///   - "-clave:valor" abre un elemento de una lista de objetos.
    ///   - "-valor" abre un elemento de una lista de escalares.
    ///   - "[a, b, c]" es una lista corta en una sola línea.
    ///   - Las comillas fuerzan string: "28" es texto, 28 es número.
    ///   - true/false/null (sin comillas) son booleano y null.
    ///   - Número entero -> int; con decimales (o coma) -> double.
    ///   - Todo lo demás es string, sin necesidad de comillas.
    /// </summary>
    public static class ParserTxt
    {
        private const int AnchoTab = 4;

        public static JsonNode Parsear(string texto)
        {
            List<Linea> lineas = Analizar(texto);

            if (lineas.Count == 0)
            {
                throw new FormatoInvalidoException(1, "el archivo está vacío");
            }

            if (lineas[0].Texto.StartsWith("-"))
            {
                throw new FormatoInvalidoException(
                    lineas[0].Numero,
                    "el archivo debe empezar por clave:valor, no por un elemento de lista");
            }

            int indice = 0;
            JsonNode raiz = ParsearObjeto(lineas, ref indice, -1);

            if (indice < lineas.Count)
            {
                throw new FormatoInvalidoException(lineas[indice].Numero, "no se pudo interpretar esta línea");
            }

            return raiz;
        }

        // ------------------------------------------------------------------
        // Estructuras internas
        // ------------------------------------------------------------------

        private sealed class Linea
        {
            public int Numero;
            public int Sangria;
            public string Texto = "";
        }

        // ------------------------------------------------------------------
        // Análisis línea a línea
        // ------------------------------------------------------------------

        private static List<Linea> Analizar(string texto)
        {
            List<Linea> resultado = new List<Linea>();

            if (string.IsNullOrEmpty(texto))
            {
                return resultado;
            }

            // Quitar BOM si lo hay.
            if (texto[0] == '\uFEFF')
            {
                texto = texto.Substring(1);
            }

            string normalizado = texto.Replace("\r\n", "\n").Replace('\r', '\n');
            string[] crudas = normalizado.Split('\n');

            for (int n = 0; n < crudas.Length; n++)
            {
                string cruda = crudas[n];
                string sinEspacios = cruda.Trim();

                // Líneas en blanco: se ignoran.
                if (sinEspacios.Length == 0)
                {
                    continue;
                }

                resultado.Add(new Linea
                {
                    Numero = n + 1,
                    Sangria = CalcularSangria(cruda),
                    Texto = sinEspacios
                });
            }

            return resultado;
        }

        private static int CalcularSangria(string linea)
        {
            int ancho = 0;

            foreach (char caracter in linea)
            {
                if (caracter == ' ')
                {
                    ancho += 1;
                }
                else if (caracter == '\t')
                {
                    ancho += AnchoTab;
                }
                else
                {
                    break;
                }
            }

            return ancho;
        }

        // ------------------------------------------------------------------
        // Objetos
        // ------------------------------------------------------------------

        private static JsonObject ParsearObjeto(List<Linea> lineas, ref int indice, int sangriaDelPadre)
        {
            JsonObject objeto = new JsonObject();

            while (indice < lineas.Count && lineas[indice].Sangria > sangriaDelPadre)
            {
                Linea linea = lineas[indice];

                if (linea.Texto.StartsWith("-"))
                {
                    throw new FormatoInvalidoException(linea.Numero, "elemento de lista fuera de una lista");
                }

                int dosPuntos = linea.Texto.IndexOf(':');
                if (dosPuntos < 0)
                {
                    throw new FormatoInvalidoException(linea.Numero, "se esperaba clave:valor");
                }

                string clave = linea.Texto.Substring(0, dosPuntos).Trim();
                string valor = linea.Texto.Substring(dosPuntos + 1).Trim();

                if (clave.Length == 0)
                {
                    throw new FormatoInvalidoException(linea.Numero, "la clave está vacía");
                }

                indice++;

                if (valor.Length == 0)
                {
                    objeto[clave] = ParsearBloqueHijo(lineas, ref indice, linea.Sangria);
                }
                else
                {
                    objeto[clave] = Escalar(valor, linea.Numero);
                }
            }

            return objeto;
        }

        /// <summary>
        /// Tras una línea "clave:", decide si lo que viene es una lista (empieza por "-")
        /// o un objeto, y lo parsea.
        /// </summary>
        private static JsonNode ParsearBloqueHijo(List<Linea> lineas, ref int indice, int sangriaDeLaClave)
        {
            if (indice < lineas.Count
                && lineas[indice].Sangria > sangriaDeLaClave
                && lineas[indice].Texto.StartsWith("-"))
            {
                return ParsearLista(lineas, ref indice, sangriaDeLaClave);
            }

            return ParsearObjeto(lineas, ref indice, sangriaDeLaClave);
        }

        // ------------------------------------------------------------------
        // Listas
        // ------------------------------------------------------------------

        private static JsonArray ParsearLista(List<Linea> lineas, ref int indice, int sangriaDelPadre)
        {
            JsonArray lista = new JsonArray();

            while (indice < lineas.Count && lineas[indice].Sangria > sangriaDelPadre)
            {
                Linea linea = lineas[indice];

                if (!linea.Texto.StartsWith("-"))
                {
                    throw new FormatoInvalidoException(
                        linea.Numero,
                        "se esperaba un elemento de lista que empiece por '-'");
                }

                string resto = linea.Texto.Substring(1).Trim();

                if (resto.Length == 0)
                {
                    throw new FormatoInvalidoException(linea.Numero, "el elemento de lista está vacío");
                }

                int dosPuntos = resto.IndexOf(':');

                // Elemento simple: "-leer"
                if (dosPuntos < 0)
                {
                    lista.Add(Escalar(resto, linea.Numero));
                    indice++;
                    continue;
                }

                // Elemento con forma de objeto: "-nombre:conan"
                string clave = resto.Substring(0, dosPuntos).Trim();
                string valor = resto.Substring(dosPuntos + 1).Trim();

                if (clave.Length == 0)
                {
                    throw new FormatoInvalidoException(linea.Numero, "la clave del elemento está vacía");
                }

                JsonObject elemento = new JsonObject();
                indice++;

                if (valor.Length == 0)
                {
                    elemento[clave] = ParsearBloqueHijo(lineas, ref indice, linea.Sangria);
                }
                else
                {
                    elemento[clave] = Escalar(valor, linea.Numero);
                }

                // Claves hermanas del mismo elemento: líneas que no empiezan por "-"
                // y están al menos tan indentadas como el "-".
                while (indice < lineas.Count
                       && lineas[indice].Sangria >= linea.Sangria
                       && !lineas[indice].Texto.StartsWith("-"))
                {
                    Linea hermana = lineas[indice];

                    int dp = hermana.Texto.IndexOf(':');
                    if (dp < 0)
                    {
                        throw new FormatoInvalidoException(hermana.Numero, "se esperaba clave:valor");
                    }

                    string claveHermana = hermana.Texto.Substring(0, dp).Trim();
                    string valorHermano = hermana.Texto.Substring(dp + 1).Trim();

                    if (claveHermana.Length == 0)
                    {
                        throw new FormatoInvalidoException(hermana.Numero, "la clave está vacía");
                    }

                    indice++;

                    if (valorHermano.Length == 0)
                    {
                        elemento[claveHermana] = ParsearBloqueHijo(lineas, ref indice, hermana.Sangria);
                    }
                    else
                    {
                        elemento[claveHermana] = Escalar(valorHermano, hermana.Numero);
                    }
                }

                lista.Add(elemento);
            }

            return lista;
        }

        // ------------------------------------------------------------------
        // Valores sueltos
        // ------------------------------------------------------------------

        private static JsonNode? Escalar(string valor, int numeroLinea)
        {
            // "..." fuerza string
            if (valor.Length >= 2 && valor[0] == '"' && valor[valor.Length - 1] == '"')
            {
                return JsonValue.Create(Desescapar(valor.Substring(1, valor.Length - 2)));
            }

            // [a, b, c] lista corta
            if (valor.StartsWith("["))
            {
                if (!valor.EndsWith("]"))
                {
                    throw new FormatoInvalidoException(numeroLinea, "la lista con corchetes no está cerrada con ']'");
                }

                string interior = valor.Substring(1, valor.Length - 2).Trim();
                JsonArray arreglo = new JsonArray();

                if (interior.Length > 0)
                {
                    foreach (string trozo in PartirPorComas(interior))
                    {
                        string elemento = trozo.Trim();

                        if (elemento.Length == 0)
                        {
                            throw new FormatoInvalidoException(numeroLinea, "hay un elemento vacío en la lista");
                        }

                        arreglo.Add(Escalar(elemento, numeroLinea));
                    }
                }

                return arreglo;
            }

            if (valor.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return JsonValue.Create(true);
            }

            if (valor.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                return JsonValue.Create(false);
            }

            if (valor.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (int.TryParse(valor, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int entero))
            {
                return JsonValue.Create(entero);
            }

            if (long.TryParse(valor, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out long largo))
            {
                return JsonValue.Create(largo);
            }

            string conPunto = valor.Replace(',', '.');

            if (double.TryParse(conPunto,
                                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
                                CultureInfo.InvariantCulture,
                                out double real))
            {
                return JsonValue.Create(real);
            }

            return JsonValue.Create(valor);
        }

        private static string Desescapar(string texto)
        {
            StringBuilder resultado = new StringBuilder(texto.Length);

            for (int i = 0; i < texto.Length; i++)
            {
                if (texto[i] == '\\' && i + 1 < texto.Length)
                {
                    i++;
                    resultado.Append(texto[i] switch
                    {
                        'n' => '\n',
                        't' => '\t',
                        _ => texto[i]
                    });
                }
                else
                {
                    resultado.Append(texto[i]);
                }
            }

            return resultado.ToString();
        }

        private static List<string> PartirPorComas(string interior)
        {
            List<string> partes = new List<string>();
            StringBuilder actual = new StringBuilder();
            bool enComillas = false;

            for (int i = 0; i < interior.Length; i++)
            {
                char caracter = interior[i];

                if (caracter == '\\' && enComillas && i + 1 < interior.Length)
                {
                    actual.Append(caracter).Append(interior[i + 1]);
                    i++;
                    continue;
                }

                if (caracter == '"')
                {
                    enComillas = !enComillas;
                    actual.Append(caracter);
                    continue;
                }

                if (caracter == ',' && !enComillas)
                {
                    partes.Add(actual.ToString());
                    actual.Clear();
                    continue;
                }

                actual.Append(caracter);
            }

            partes.Add(actual.ToString());
            return partes;
        }
    }
}

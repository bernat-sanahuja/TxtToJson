using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using TxtToJson.Models;

namespace TxtToJson
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        /// <summary>Máximo de archivos permitidos (6 columnas x 4 filas).</summary>
        public const int MaxArchivos = 24;

        private static readonly JsonSerializerOptions OpcionesJson = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private bool _mostrandoResultados;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Archivos .txt sueltos en la ventana.</summary>
        public ObservableCollection<Archivo> Archivos { get; } = new ObservableCollection<Archivo>();

        /// <summary>JSON ya generados, uno por .txt convertido.</summary>
        public ObservableCollection<ResultadoJson> Resultados { get; } = new ObservableCollection<ResultadoJson>();

        /// <summary>true = se ve la pantalla de resultados; false = la de archivos.</summary>
        public bool MostrandoResultados
        {
            get => _mostrandoResultados;
            set
            {
                if (_mostrandoResultados == value)
                {
                    return;
                }

                _mostrandoResultados = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MostrandoResultados)));
            }
        }

        // ------------------------------------------------------------------
        // Arrastrar y soltar
        // ------------------------------------------------------------------

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            Grid1.Background = (Brush)FindResource("DropHoverBrush");
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            e.Handled = true;
        }

        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            Grid1.Background = Brushes.Transparent;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            Grid1.Background = Brushes.Transparent;

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            string[] rutas = (string[])e.Data.GetData(DataFormats.FileDrop);

            int noTxt = 0;
            int repetidos = 0;
            int sobrantes = 0;

            foreach (string ruta in rutas)
            {
                if (!Path.GetExtension(ruta).Equals(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    noTxt++;
                    continue;
                }

                if (Archivos.Any(a => string.Equals(a.Ruta, ruta, StringComparison.OrdinalIgnoreCase)))
                {
                    repetidos++;
                    continue;
                }

                if (Archivos.Count >= MaxArchivos)
                {
                    sobrantes++;
                    continue;
                }

                Archivos.Add(new Archivo(ruta));
            }

            // Al soltar archivos nuevos volvemos a la vista de archivos.
            MostrandoResultados = false;

            MostrarAvisos(noTxt, repetidos, sobrantes);
        }

        private void EliminarArchivo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elemento && elemento.DataContext is Archivo archivo)
            {
                Archivos.Remove(archivo);
            }
        }

        // ------------------------------------------------------------------
        // Navegación entre vistas
        // ------------------------------------------------------------------

        private void Volver_Click(object sender, RoutedEventArgs e)
        {
            MostrandoResultados = false;
        }

        // ------------------------------------------------------------------
        // Execute: convierte todos los .txt y muestra los .json
        // ------------------------------------------------------------------

        private void Execute_Click(object sender, RoutedEventArgs e)
        {
            if (Archivos.Count == 0)
            {
                MessageBox.Show("Add some .txt files first.",
                                "Nothing to convert",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            List<ResultadoJson> nuevos = new List<ResultadoJson>();
            List<string> errores = new List<string>();

            foreach (Archivo archivo in Archivos)
            {
                try
                {
                    string texto = File.ReadAllText(archivo.Ruta);
                    JsonNode? nodo = ParserTxt.Parsear(texto);
                    string json = nodo is null ? "null" : nodo.ToJsonString(OpcionesJson);

                    nuevos.Add(new ResultadoJson(archivo, json));
                }
                catch (FormatoInvalidoException error)
                {
                    errores.Add($"{Path.GetFileName(archivo.Ruta)} → {error.Message}");
                }
                catch (Exception error)
                {
                    errores.Add($"{Path.GetFileName(archivo.Ruta)} → could not be read ({error.Message})");
                }
            }

            if (nuevos.Count > 0)
            {
                Resultados.Clear();

                foreach (ResultadoJson resultado in nuevos)
                {
                    Resultados.Add(resultado);
                }

                MostrandoResultados = true;
            }

            if (errores.Count > 0)
            {
                MostrarErrores("Some files could not be converted", errores);
            }
        }

        // ------------------------------------------------------------------
        // Guardar un .json individual: clic en su tarjeta
        // ------------------------------------------------------------------

        private void GuardarJson_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement elemento || elemento.DataContext is not ResultadoJson resultado)
            {
                return;
            }

            SaveFileDialog dialogo = new SaveFileDialog
            {
                Title = "Save JSON",
                FileName = resultado.Nombre,
                DefaultExt = ".json",
                Filter = "JSON (*.json)|*.json|All files (*.*)|*.*",
                InitialDirectory = Path.GetDirectoryName(resultado.RutaOrigen) ?? ""
            };

            if (dialogo.ShowDialog() != true)
            {
                return;
            }

            try
            {
                File.WriteAllText(dialogo.FileName, resultado.Json, new UTF8Encoding(false));
            }
            catch (Exception error)
            {
                MessageBox.Show($"Could not save the file:{Environment.NewLine}{error.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        // ------------------------------------------------------------------
        // Guardar todos los .json en una carpeta
        // ------------------------------------------------------------------

        private void DescargarTodo_Click(object sender, RoutedEventArgs e)
        {
            if (Resultados.Count == 0)
            {
                return;
            }

            OpenFolderDialog dialogo = new OpenFolderDialog
            {
                Title = "Choose a folder for the JSON files"
            };

            if (dialogo.ShowDialog() != true)
            {
                return;
            }

            string carpeta = dialogo.FolderName;

            // Evitar que dos .txt con el mismo nombre se pisen dentro del lote.
            List<string> destinos = new List<string>();
            HashSet<string> usados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (ResultadoJson resultado in Resultados)
            {
                string nombre = resultado.Nombre;
                string sinExtension = Path.GetFileNameWithoutExtension(nombre);
                int copia = 2;

                while (!usados.Add(nombre))
                {
                    nombre = $"{sinExtension} ({copia}).json";
                    copia++;
                }

                destinos.Add(Path.Combine(carpeta, nombre));
            }

            int existentes = destinos.Count(ruta => File.Exists(ruta));

            if (existentes > 0)
            {
                MessageBoxResult respuesta = MessageBox.Show(
                    $"{existentes} file(s) already exist in that folder.{Environment.NewLine}Overwrite them?",
                    "Confirm",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (respuesta != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            int guardados = 0;
            List<string> errores = new List<string>();

            for (int i = 0; i < Resultados.Count; i++)
            {
                try
                {
                    File.WriteAllText(destinos[i], Resultados[i].Json, new UTF8Encoding(false));
                    guardados++;
                }
                catch (Exception error)
                {
                    errores.Add($"{Path.GetFileName(destinos[i])} → {error.Message}");
                }
            }

            if (errores.Count > 0)
            {
                MostrarErrores($"Saved {guardados} of {Resultados.Count} file(s)", errores);
                return;
            }

            MessageBox.Show($"Saved {guardados} file(s) to:{Environment.NewLine}{carpeta}",
                            "Download complete",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        // ------------------------------------------------------------------
        // Avisos
        // ------------------------------------------------------------------

        private static void MostrarErrores(string titulo, List<string> errores)
        {
            const int MaximoAMostrar = 8;

            string detalle = string.Join(Environment.NewLine, errores.Take(MaximoAMostrar));

            if (errores.Count > MaximoAMostrar)
            {
                detalle += Environment.NewLine + $"... and {errores.Count - MaximoAMostrar} more";
            }

            MessageBox.Show(detalle,
                            titulo,
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
        }

        private static void MostrarAvisos(int noTxt, int repetidos, int sobrantes)
        {
            List<string> avisos = new List<string>();

            if (noTxt > 0)
            {
                avisos.Add($"Only .txt files are accepted ({noTxt} ignored).");
            }

            if (repetidos > 0)
            {
                avisos.Add($"{repetidos} file(s) were already in the list.");
            }

            if (sobrantes > 0)
            {
                avisos.Add($"Only {MaxArchivos} files can be added ({sobrantes} ignored).");
            }

            if (avisos.Count > 0)
            {
                MessageBox.Show(string.Join(Environment.NewLine, avisos),
                                "Warning",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using TxtToJson.Models;

namespace TxtToJson
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>Máximo de archivos permitidos (6 columnas x 4 filas).</summary>
        public const int MaxArchivos = 24;

        /// <summary>
        /// Única fuente de verdad de la interfaz.
        /// Al añadir o quitar elementos, WPF actualiza la lista por su cuenta.
        /// </summary>
        public ObservableCollection<Archivo> Archivos { get; } = new ObservableCollection<Archivo>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

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
                // Solo .txt
                if (!Path.GetExtension(ruta).Equals(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    noTxt++;
                    continue;
                }

                // No duplicar
                if (Archivos.Any(a => string.Equals(a.Ruta, ruta, StringComparison.OrdinalIgnoreCase)))
                {
                    repetidos++;
                    continue;
                }

                // Respetar el límite
                if (Archivos.Count >= MaxArchivos)
                {
                    sobrantes++;
                    continue;
                }

                Archivos.Add(new Archivo(ruta));
            }

            MostrarAvisos(noTxt, repetidos, sobrantes);
        }

        private void EliminarArchivo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement elemento && elemento.DataContext is Archivo archivo)
            {
                Archivos.Remove(archivo);
            }
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
                avisos.Add($"{repetidos} file(s) are already in the list.");
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

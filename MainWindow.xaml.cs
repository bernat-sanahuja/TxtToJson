using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace TxtToJson
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> rutas = new List<string>();
        private LinearGradientBrush colorDrag = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1)
        };


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            // Cambiar color
            colorDrag.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#F5F7FB"), 0));
            colorDrag.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#C7D6FA"), 1));

            Grid1.Background = colorDrag;


            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Guardar en una lista los elementos arrastrados
                string[] rutasArray = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string rutaGuardada in rutas)
                {
                    foreach (string rutaNueva in rutasArray)
                    {
                        if (rutaGuardada.Equals(rutaNueva, StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("You have already added this file.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                }
                rutas.AddRange(rutasArray);

                // Borrar los elementos que no sean .txt
                int eliminados = rutas.RemoveAll(r => !System.IO.Path.GetExtension(r).Equals(".txt", StringComparison.OrdinalIgnoreCase));

                if (eliminados > 0)
                {
                    MessageBox.Show("You can only drop .txt files.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                OrganizarArchivos(rutas);
            }
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            colorDrag.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#DCE8FF"), 0));
            colorDrag.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#C7D6FA"), 1));

            Grid1.Background = colorDrag;
        }

        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            colorDrag.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#F5F7FB"), 0));
            colorDrag.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#C7D6FA"), 1));

            Grid1.Background = colorDrag;
        }

        private void OrganizarArchivos(List<string> rutas)
        {
            int row = 0;
            int column = 0;

            foreach (string ruta in rutas)
            {
                StackPanel panel = new StackPanel();
                panel.HorizontalAlignment = HorizontalAlignment.Center;
                panel.VerticalAlignment = VerticalAlignment.Center;

                Image imagen = new Image();
                imagen.Width = 48;
                imagen.Height = 48;
                imagen.Source = new BitmapImage(new Uri("/Resources/Icons/txt_icon.png", UriKind.Relative));

                Label label = new Label();
                label.Content = System.IO.Path.GetFileName(ruta);
                label.HorizontalAlignment = HorizontalAlignment.Center;
                label.FontFamily = new FontFamily("Segoe UI");

                panel.Children.Add(imagen);
                panel.Children.Add(label);

                Grid.SetRow(panel, row);
                Grid.SetColumn(panel, column);
                Grid1.Children.Add(panel);

                if (column == 5)
                {
                    column = 0;
                    row++;
                }
                else
                {
                    column++;
                }
            }
        }
    }
}
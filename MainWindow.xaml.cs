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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] rutas = (string[])e.Data.GetData(DataFormats.FileDrop);
                string ruta = rutas[0];
             

                foreach (string rutaArchivo in rutas)
                {
                    TextBox1.Text += rutaArchivo + Environment.NewLine;
                }


  
            }
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            var colorDrag = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };

            colorDrag.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#4776E6"), 0));
            colorDrag.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#8E54E9"), 1));

            Grid1.Background = colorDrag;
        }

        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            var colorDrag = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };

            colorDrag.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#1F1C2C"), 0));
            colorDrag.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#2D3A5C"), 1));

            Grid1.Background = colorDrag;

        }
    }
}
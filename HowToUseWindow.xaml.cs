using System.Windows;

namespace TxtToJson
{
    /// <summary>
    /// Ventana de ayuda. Se abre con Show(), no con ShowDialog(),
    /// así que no bloquea la ventana principal.
    /// </summary>
    public partial class HowToUseWindow : Window
    {
        private const string EjemploTxt =
"""
nombre:marta
edad:28
activa:true
aficiones: [leer, cocinar]
direccion:
    ciudad:madrid
    cp: "28001"
mascotas:
    -perro:conan
     edad:2
""";

        private const string EjemploJson =
"""
{
  "nombre": "marta",
  "edad": 28,
  "activa": true,
  "aficiones": ["leer", "cocinar"],
  "direccion": {
    "ciudad": "madrid",
    "cp": "28001"
  },
  "mascotas": [
    { "perro": "conan", "edad": 2 }
  ]
}
""";

        public HowToUseWindow()
        {
            InitializeComponent();
            TextoTxt.Text = EjemploTxt;
            TextoJson.Text = EjemploJson;
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

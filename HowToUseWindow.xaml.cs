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
name:John
age:28
active:true
hobbies: [read, cook]
address:
    city:Madrid
    cp: "28001"
pets:
    -dog:Conan
     age:2
    -cat:Fluffy
     age:1
""";

        private const string EjemploJson =
"""
{
  "name": "John",
  "age": 28,
  "active": true,
  "hobbies": ["read", "cook"],
  "address": {
    "city": "Madrid",
    "cp": "28001"
  },
  "pets": [
    { "dog": "Conan", "age": 2 },
    { "cat": "Fluffy", "age": 1 }
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

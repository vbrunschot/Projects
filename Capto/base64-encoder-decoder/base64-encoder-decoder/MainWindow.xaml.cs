using System;
using System.Text;
using System.Windows;

namespace base64_encoder_decoder
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnEncode_Click(object sender, RoutedEventArgs e)
        {
            tbOutput.Text = Convert.ToBase64String(Encoding.UTF8.GetBytes(tbInput.Text));
        }

        private void btnDecode_Click(object sender, RoutedEventArgs e)
        {
            tbOutput.Text = Encoding.UTF8.GetString(Convert.FromBase64String(tbInput.Text));
        }
    }
}

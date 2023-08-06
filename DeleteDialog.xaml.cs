using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TeleTrader
{
    /// <summary>
    /// Interaction logic for DeleteDialog.xaml
    /// </summary>
    public partial class DeleteDialog : Window
    {
        public bool Delete;
        public DeleteDialog()
        {
            InitializeComponent();
            Delete = false;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Delete = true;
            this.Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Delete = false;
            this.Close();
        }
    }
}

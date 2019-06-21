using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Fttd
{
    /// <summary>
    /// Логика взаимодействия для Window2.xaml
    /// </summary>
    public partial class Window2 : Window
    {
        public Window2()
        {
            InitializeComponent();
            DataGridTechnology.ItemsSource = State.detailColl.OrderByDescending(item => item.Inventory);
            DataGridTechnology.Items.Refresh();
        }

        private void gridtop_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { this.DragMove(); }
            catch { }
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace DragDropTest
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ListBox dragSource = null;

        public ObservableCollection<string> LbFirst = new ObservableCollection<string>();
        public ObservableCollection<string> LbSecond = new ObservableCollection<string>();       
      
        public MainWindow()
        {          
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LbFirst.Add("1");
            LbFirst.Add("2");
            LbFirst.Add("3");

            Lb1.ItemsSource = LbFirst;
            Lb2.ItemsSource = LbSecond;
        }

        private void ListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            
            ListBox parent = (ListBox)sender;
            dragSource = parent;
            object data = parent.SelectedItem;
            //object data = GetDataFromListBox(dragSource, e.GetPosition(parent));

            if (data != null)
            {
                DragDrop.DoDragDrop(parent, data, DragDropEffects.Move);
            }
        }
        private void ListBox_Drop(object sender, DragEventArgs e)
        {         
            ListBox parent = (ListBox)sender;
            object data = e.Data.GetData(typeof(string));

            //((IList)dragSource.ItemsSource).Remove(data);
            //parent.Items.Add(data);


            //MessageBox.Show(e.OriginalSource.ToString());
            //e.


            if (!parent.Items.Contains(data))
            {
                if (sender == Lb1)
                    LbFirst.Add(data as string);
                else
                    LbSecond.Add(data as string);
            }

        }
        private static object GetDataFromListBox(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);

                    if (data == DependencyProperty.UnsetValue)
                    {
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    }

                    if (element == source)
                    {
                        return null;
                    }
                }

                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }

            return null;
        }
    }
}

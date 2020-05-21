using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DragDropTest
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private KeyboardListener KListener = new KeyboardListener();
        private List<Key> pressedKeys = new List<Key>();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            KListener.KeyDown += new RawKeyEventHandler(KListener_KeyDown);
            KListener.KeyUp += new RawKeyEventHandler(KListener_KeyUp);
        }

        private void KListener_KeyUp(object sender, RawKeyEventArgs args) => pressedKeys.Remove(args.Key);
                  
        void KListener_KeyDown(object sender, RawKeyEventArgs args)
        {
            if(!pressedKeys.Contains(args.Key))
                pressedKeys.Add(args.Key);

            foreach (var item in pressedKeys)
            {
                Console.Write(item.ToString() + " ");
            }
            Console.WriteLine();


            if ((pressedKeys.Contains(Key.LeftCtrl) || pressedKeys.Contains(Key.RightCtrl)) && pressedKeys.Contains(Key.C))
                Console.WriteLine("Copy");
            if ((pressedKeys.Contains(Key.LeftCtrl) || pressedKeys.Contains(Key.RightCtrl)) && pressedKeys.Contains(Key.X))
                Console.WriteLine("Move");
            if(pressedKeys.Contains(Key.Delete))
                Console.WriteLine("Delete");
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            KListener.Dispose();
        }
    }
}

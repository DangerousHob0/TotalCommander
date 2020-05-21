using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TotalCommander.Model;
using System.IO;
using System.Windows.Input;
using System.Drawing;
using TotalCommander;
using System.Windows;
using System.Diagnostics;
using System.Collections;

namespace TotalCommander.ViewModel
{
    class FileExplorerViewModel : BindableBase
    {
        private FileExplorerModel model;

        #region Props
        public List<ListItem> ExplorerItems
        {
            get { return model.explorerItems; }
            set { model.explorerItems = value; OnPropertyChanged();  }
        }
        public List<ComboBoxItem> LogicalDrives
        {
            get { return model.logicalDrives; }
            set { model.logicalDrives = value; OnPropertyChanged(); }
        }
        public string DiskInfo
        {
            get { return model.diskInfo; }
            set { model.diskInfo = value; OnPropertyChanged(); }
        }

        public string FilesQuantityInDir
        {
            get { return model.filesQuantityInDir; }
            set { model.filesQuantityInDir = value; OnPropertyChanged(); }
        }

        public DirectoryInfo CurrentDirectory
        {
            get { return model.currentDirectory; }
            set { model.currentDirectory = value; OnPropertyChanged(); }
        }

        public FileExplorerModel Model
        {
            get { return model; }
        }

        #endregion

        private bool _IsSelectedExplorer;
        public bool IsSelectedExplorer
        {
            get { return _IsSelectedExplorer; }
            set 
            {
                _IsSelectedExplorer = value;
                if (value)
                     (Application.Current.MainWindow.DataContext as MainWindowViewModel).SelectedItemInExplorer(this);
                else
                    ExplorerItems.ForEach(i => i.IsSelected = false);
            }
        }
        public bool IsDragSource { get; set; }


        # region Ctor
        public FileExplorerViewModel()
        {
            model = new FileExplorerModel();
            IsSelectedExplorer = false;
        }
        #endregion
        #region Commands
        public ICommand Update => new RelayCommand<object>(obj =>
        {
            //LogicalDrives = Directory.GetLogicalDrives().ToList();                     
            //LogicalDrives.Add(string.Format(@"{0}Users\{1}\Desktop", Path.GetPathRoot(Environment.SystemDirectory),Environment.UserName));
            //LogicalDrives.Add(string.Format(@"{0}Users\{1}\Documents", Path.GetPathRoot(Environment.SystemDirectory),Environment.UserName));
        });
        public ICommand LVitemClick => new RelayCommand<ListItem>((selectedItem) =>
        {
            if (selectedItem == null)
                return;

            if (selectedItem.File is DirectoryInfo)
            {
                if ((selectedItem.File as DirectoryInfo).Exists)
                    OpenDir(selectedItem.File as DirectoryInfo);
            }
            else
                Process.Start(selectedItem.File.FullName);
        });

        public ICommand LVselectedChanged => new RelayCommand<object>((obj) =>
        {
              IsSelectedExplorer = true;
        });

        public ICommand CBitemSelected => new RelayCommand<ComboBoxItem>(SelectedItem =>
        {
            //todo решить отключение хард диска в ходе работы программы (DriveNotFoundExeption)
            DriveInfo drive = new DriveInfo(SelectedItem.Path);
            DiskInfo = string.Format($"[{drive.DriveFormat}] " +
                $"{drive.AvailableFreeSpace.ToString("0,0", System.Globalization.CultureInfo.InvariantCulture)} byte of " +
                $"{drive.TotalSize.ToString("0,0", System.Globalization.CultureInfo.InvariantCulture)} free");

            OpenDir(new DirectoryInfo(SelectedItem.Path));
        });


        public ICommand SortItems => new RelayCommand<string>(feature =>
        {
            if (ExplorerItems.Count == 0)
                return; 

            if (feature == "Name")
                ExplorerItems = ExplorerItems.OrderBy((i) => i.Hint).ThenBy((i) => i.File.Name).ToList();
            else if (feature == "Date")
                ExplorerItems = ExplorerItems.OrderBy((i) => i.Hint).ThenBy((i) => i.File.CreationTime).ToList();
            else if (feature == "Type")
                OpenDir(new DirectoryInfo(Path.GetDirectoryName(ExplorerItems.LastOrDefault().File.FullName)));
        });

        public ICommand FileDrag => new RelayCommand<System.Windows.Controls.ListView>(lv =>
        {

            //lv.Sel
            (App.Current.MainWindow.DataContext as MainWindowViewModel).DragExplorer(this);
            DragDrop.DoDragDrop(lv, ExplorerItems.Where((i)=>i.IsSelected).ToList(), DragDropEffects.Move);
        });
        public ICommand Drop => new RelayCommand<DragEventArgs>(e =>
        {
            (App.Current.MainWindow.DataContext as MainWindowViewModel).DropFiles(this, e);
        
           
           
            //if(dat is IEnumerable)
            //MessageBox.Show(data.GetType().ToString());
        });
        #endregion


        #region Methods
        public void RefreshExplorer() => OpenDir(CurrentDirectory);

        #endregion
        #region Tools

        private System.Windows.Media.Brush GetColorByAttributes(in FileAttributes atr)
        {
            if (atr.HasFlag(FileAttributes.Hidden))
                return System.Windows.Media.Brushes.Blue;

            return System.Windows.Media.Brushes.Black;
        }

        #endregion

        #region Navigation
        public void OpenDir(DirectoryInfo dir)
        {           
            try
            {
                List<ListItem> temp = new List<ListItem>();

                if (dir.Parent != null)
                    temp.Add(new ListItem(dir.Parent, MahApps.Metro.IconPacks.PackIconModernKind.UndoCurve.ToImageSource(System.Windows.Media.Brushes.Green), null, GetColorByAttributes(dir.Attributes)));
                    //temp.Add(new ListItem(dir.Parent, SystemIcons.Asterisk.ToImageSource(), string.Empty, GetColorByAttributes(dir.Attributes)));

                foreach (var item in dir.GetDirectories())
                    temp.Add(new ListItem(item, MahApps.Metro.IconPacks.PackIconModernKind.Folder.ToImageSource(System.Windows.Media.Brushes.Gray), "Directory", GetColorByAttributes(item.Attributes)));
                //temp.Add(new ListItem(item, ShellIcon.GetLargeFolderIcon().ToImageSource(), "Directory", GetColorByAttributes(item.Attributes)));

                foreach (var item in dir.GetFiles())
                    temp.Add(new ListItem(item, ShellIcon.GetLargeIconFromExtension(item.Extension).ToImageSource(), "File", GetColorByAttributes(item.Attributes)));

                ExplorerItems.Clear();
                ExplorerItems = temp;
                CurrentDirectory = dir;
                FilesQuantityInDir = string.Format($"{Directory.GetDirectories(dir.FullName).Length} dir(s) / {Directory.GetFiles(dir.FullName).Length} file(s)");
            }
            catch (Exception rx)
            {
                MessageBox.Show("Отказано в доступе", "",MessageBoxButton.OK, MessageBoxImage.Exclamation);
#if DEBUG
                MessageBox.Show(rx.ToString());
#endif
            }
        }
        public void ResetFolderToDefault(List<ListItem> list)
        {
            list.Clear();
            list = new List<ListItem>();
            FilesQuantityInDir = string.Empty;
        }
        #endregion

    }
}

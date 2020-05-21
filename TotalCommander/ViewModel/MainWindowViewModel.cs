//#define DEBUG


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using TotalCommander.Model;


namespace TotalCommander.ViewModel
{
    class MainWindowViewModel : BindableBase
    {
        private MainWindowModel model;
        private FileExplorerViewModel LeftExplorer;
        private FileExplorerViewModel RightExplorer;
        #region props       
        public View.FileExplorer LeftExplorerView { get; set; }
        public View.FileExplorer RightExplorerView { get; set; }
        #endregion

        public ImageSource Icon { get; set; }

        public MainWindowViewModel()
        {
            LeftExplorerView = new View.FileExplorer();
            RightExplorerView = new View.FileExplorer();

            LeftExplorer = LeftExplorerView.DataContext as FileExplorerViewModel;
            RightExplorer = RightExplorerView.DataContext as FileExplorerViewModel;

            Icon = MahApps.Metro.IconPacks.PackIconModernKind.CabinetFiles.ToImageSource(System.Windows.Media.Brushes.DimGray);
        }

        #region Commands      
        public ICommand CopyOrMove => new RelayCommand<string>(operation =>
        {
            //На случай если какой то експлорер не открыт и происходит попытка копирования
            if (LeftExplorer.ExplorerItems.Count == 0 || RightExplorer.ExplorerItems.Count == 0)
                return;

            //На случай если какой то експлорер не открыт и происходит попытка копирования
            if (LeftExplorer.ExplorerItems.Count == 0 || RightExplorer.ExplorerItems.Count == 0)
                return;

            FileExplorerViewModel sourceExplorer = null;
            FileExplorerViewModel destExplorer = null;

            if (LeftExplorer.IsSelectedExplorer)
            {
                destExplorer = RightExplorer;
                sourceExplorer = LeftExplorer;
            }

            else
            {
                destExplorer = LeftExplorer;
                sourceExplorer = RightExplorer;
            }


            if (sourceExplorer != null && destExplorer != null)
            {
                string[] files = sourceExplorer.ExplorerItems.Where(i => i.IsSelected).Select(i => i.File.FullName).ToArray();

                try
                {
                    if (operation == "Copy")
                    {

                        foreach (var item in files)
                            Copyring(item, destExplorer.CurrentDirectory.FullName);
                    }
                    else if (operation == "Move")
                    {
                        foreach (var item in files)
                            Moving(item, destExplorer.CurrentDirectory.FullName);

                        sourceExplorer.RefreshExplorer();
                    }
                }
                catch (Exception ex)
                {
                    //на случай отмены операции
#if DEBUG
                    MessageBox.Show(ex.Message);
#endif
                }

                destExplorer.RefreshExplorer();
            }
        });

        public ICommand Delete => new RelayCommand<object>(obj =>
        {
            FileExplorerViewModel Explorer = null;

            if (LeftExplorer.IsSelectedExplorer)
                Explorer = LeftExplorer;
            else
                Explorer = RightExplorer;

            if (Explorer != null)
            {
                string[] files = Explorer.ExplorerItems.Where(i => i.IsSelected).Select(i => i.File.FullName).ToArray();
                foreach (var item in files)
                    Remove(item);
                Explorer.RefreshExplorer();
            }
        });

        public ICommand AddToArhive => new RelayCommand<object>(obj =>
        {
            FileExplorerViewModel Explorer = null;

            if (LeftExplorer.IsSelectedExplorer)
                Explorer = LeftExplorer;

            else if (RightExplorer.IsSelectedExplorer)
                Explorer = RightExplorer;

            if (Explorer == null)
                return;

            var files = Explorer.ExplorerItems.Where(i => i.IsSelected).Select(i => i.File.FullName).ToArray();
            if (files.Length > 0)
            {
                string arhiveName = Directory.Exists(files[0]) ? Path.GetFileName(files[0]) : Path.GetFileNameWithoutExtension(files[0]);
                arhiveName = Path.Combine(Explorer.CurrentDirectory.FullName, arhiveName)/* + ".zip"*/;
                int numberCopy = 1;

                while (File.Exists(arhiveName+".zip"))
                {
                    arhiveName = arhiveName.Replace($" копия({numberCopy -1})", "");
                    arhiveName = string.Format($"{arhiveName} копия({numberCopy++})");
                }
                arhiveName += ".zip";

                using (var zip = ZipFile.Open(arhiveName, ZipArchiveMode.Create))
                {
                    foreach (var item in files)
                        Archiving(Explorer.CurrentDirectory.FullName + "\\", item, zip);
                }
                Explorer.RefreshExplorer();
            }
        });

        #endregion


        #region Methods
        /// Костыли
        public void SelectedItemInExplorer(FileExplorerViewModel Explorer)
        {
            if (Explorer == LeftExplorer)
                RightExplorer.IsSelectedExplorer = false;
            else if (Explorer == RightExplorer)
                LeftExplorer.IsSelectedExplorer = false;
        }

        public void DragExplorer(FileExplorerViewModel Explorer)
        {
            if (Explorer == LeftExplorer)
            {
                LeftExplorer.IsDragSource = true;
                RightExplorer.IsDragSource = false;
            }
            else if (Explorer == RightExplorer)
            {
                RightExplorer.IsDragSource = true;
                LeftExplorer.IsDragSource = false;
            }
        }
        public void DropFiles(FileExplorerViewModel Receiving, DragEventArgs e)
        {
            string[] files = null;

            if (e.Data.GetDataPresent(typeof(List<ListItem>)))
            {
                if (!Receiving.IsDragSource)
                {
                    files = (e.Data.GetData(typeof(List<ListItem>)) as List<ListItem>).
                                            Where(i => (i.File.Attributes.HasFlag(FileAttributes.Directory) ? Directory.Exists(Path.Combine(Receiving.CurrentDirectory.FullName, i.File.Name))
                                                : File.Exists(Path.Combine(Receiving.CurrentDirectory.FullName, i.File.Name))) == false).
                                            Select(i => i.File.FullName).ToArray();
                }
            }
            else if (e.Data.GetDataPresent("FileNameW"))
            {
                string[] filesInCurrentDirectory = Receiving.ExplorerItems.Select(i => i.File.FullName).ToArray();
                files = (e.Data.GetData("FileNameW") as string[]).Where(i => filesInCurrentDirectory.Contains(i) == false).ToArray();
            }

            if (files != null)
            {
                try
                {
                    foreach (var item in files)
                        Copyring(item, Receiving.CurrentDirectory.FullName);
                }
                catch (Exception ex)
                {
#if DEBUG
                    MessageBox.Show(ex.Message);
#endif
                }

                Receiving.RefreshExplorer();
            }

        }
        private void Copyring(string sourcePath, string destPath)
        {
            #region OLD
            //if (Directory.Exists(sourcePath))
            //{              
            //var newDir = Directory.CreateDirectory(Path.Combine(destPath, Path.GetFileName(sourcePath)));

            //foreach (var item in Directory.GetDirectories(sourcePath))
            //    Copyring(item, newDir.FullName);

            //foreach (var item in Directory.GetFiles(sourcePath))
            //    File.Copy(item, Path.Combine(newDir.FullName, Path.GetFileName(item)));
            //}
            //else
            //File.Copy(sourcePath, Path.Combine(destPath, Path.GetFileName(sourcePath)));
            #endregion
            if (Directory.Exists(sourcePath))
            {
                var newDir = Directory.CreateDirectory(Path.Combine(destPath, Path.GetFileName(sourcePath)));
                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(sourcePath, newDir.FullName, Microsoft.VisualBasic.FileIO.UIOption.AllDialogs);
            }
            else
                Microsoft.VisualBasic.FileIO.FileSystem.CopyFile(sourcePath, Path.Combine(destPath, Path.GetFileName(sourcePath)), Microsoft.VisualBasic.FileIO.UIOption.AllDialogs);
        }
        private void Moving(string sourcePath, string destPath)
        {
            #region OLD
            //if (Directory.Exists(sourcePath))
            //{
            //    var newDir = Directory.CreateDirectory(Path.Combine(destPath, Path.GetFileName(sourcePath)));

            //    foreach (var item in Directory.GetDirectories(sourcePath))
            //        Moving(item, newDir.FullName);

            //    foreach (var item in Directory.GetFiles(sourcePath))
            //        File.Move(item, Path.Combine(newDir.FullName, Path.GetFileName(item)));

            //    Directory.Delete(sourcePath);
            //}
            //else
            //    File.Move(sourcePath, Path.Combine(destPath, Path.GetFileName(sourcePath)));
            #endregion
            if (Directory.Exists(sourcePath))
            {
                var newDir = Directory.CreateDirectory(Path.Combine(destPath, Path.GetFileName(sourcePath)));
                Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(sourcePath, newDir.FullName, Microsoft.VisualBasic.FileIO.UIOption.AllDialogs);
            }
            else
                Microsoft.VisualBasic.FileIO.FileSystem.MoveFile(sourcePath, Path.Combine(destPath, Path.GetFileName(sourcePath)), Microsoft.VisualBasic.FileIO.UIOption.AllDialogs);
        }
        private void Remove(string Path)
        {
            //todo спросить у пользователя уверен ли он и отправить в корзину или удалить совсем, вроде бы ресайкл опшн позволяет это
            #region OLD
            //if (Directory.Exists(Path))
            //{
            //    foreach (var item in Directory.GetDirectories(Path))
            //        Remove(item);

            //    foreach (var item in Directory.GetFiles(Path))
            //        File.Delete(item);

            //    Directory.Delete(Path);
            //}
            //else
            //    File.Delete(Path);
            #endregion
            if (Directory.Exists(Path))
            {
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(Path, Microsoft.VisualBasic.FileIO.UIOption.AllDialogs, Microsoft.VisualBasic.FileIO.RecycleOption.DeletePermanently);
            }
            else
                Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(Path);
        }
        private void Archiving(string destPath, string file, ZipArchive zip)
        {               
            if (Directory.Exists(file))
            {
                foreach (var item in Directory.GetDirectories(file))                   
                   Archiving(destPath, item, zip);

                foreach (var item in Directory.GetFiles(file))
                    zip.CreateEntryFromFile(item, item.Replace(destPath, ""));

            }
            else
                zip.CreateEntryFromFile(file, file.Replace(destPath, ""));           
        }
        #endregion
    }
}

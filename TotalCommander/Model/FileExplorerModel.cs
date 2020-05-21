using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TotalCommander.Model
{
    class FileExplorerModel
    {
        public List<ComboBoxItem> logicalDrives = new List<ComboBoxItem>();
        public List<ListItem> explorerItems = new List<ListItem>();
        public string diskInfo;
        public string filesQuantityInDir;
        public System.IO.DirectoryInfo currentDirectory;

        public FileExplorerModel()
        {
            var drives = Directory.GetLogicalDrives();


            foreach (var item in drives)
                logicalDrives.Add(new ComboBoxItem { Icon = MahApps.Metro.IconPacks.PackIconModernKind.Input, Path = item, Title = item });

            logicalDrives.Add(new ComboBoxItem 
            { 
              Icon = MahApps.Metro.IconPacks.PackIconModernKind.CabinetFiles,
              Path = string.Format(@"{0}Users\{1}\Documents", Path.GetPathRoot(Environment.SystemDirectory), Environment.UserName), 
              Title = "Мои документы" 
            });

            logicalDrives.Add(new ComboBoxItem
            {
                Icon = MahApps.Metro.IconPacks.PackIconModernKind.Monitor,
                Path = string.Format(@"{0}Users\{1}\Desktop", Path.GetPathRoot(Environment.SystemDirectory), Environment.UserName),
                Title = "Рабочий стол"
            });
        }
    }
}

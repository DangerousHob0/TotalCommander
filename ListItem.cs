using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace TotalCommander
{
    class ListItem : BindableBase
    {
        public FileSystemInfo File { get; set; }       
        public string CreationTime { get => File.CreationTime.ToString(System.Globalization.CultureInfo.CurrentCulture); }                 
        public ImageSource Icon { get; set; }
        public String Hint{ get; set; }
        public System.Windows.Media.Brush TextColor { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();                
            }
        }
        public ListItem(FileSystemInfo file, ImageSource icon, String Hint, System.Windows.Media.Brush TextColor)
        {           
            this.File = file;
            this.Icon = icon;
            this.TextColor = TextColor;
            this.Hint = Hint;
        }
    }
}
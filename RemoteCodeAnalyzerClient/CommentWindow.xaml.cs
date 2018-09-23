using System;
using System.Collections;
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

namespace RemoteCodeAnalyzerClient
{
    /// <summary>
    /// Interaction logic for CommentWindow.xaml
    /// </summary>
    public partial class CommentWindow : Window
    {
        private ArrayList commentList { get; set; }
        public CommentWindow(ArrayList commentList)
        {
            InitializeComponent();
            this.commentList = commentList;

            CommentListBox.ItemsSource = commentList;
        }
        
    }
}

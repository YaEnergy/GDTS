using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GD_Texture_Swapper
{
    public partial class LoadingBarForm : Form
    {
        public LoadingBarForm(string caption = "LoadingBarForm")
        {
            InitializeComponent();
            Text = caption;
        }

        public delegate void CancelLoadEvent();

        public event CancelLoadEvent? CancelLoad; 

        public void cancelButton_click(object sender, EventArgs e) => CancelLoad?.Invoke();
    }
}

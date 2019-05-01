using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lepingud
{
    public partial class Form3 : Form
    {
        List<Leping> workers;
        public Form3(List<Leping> wList)
        {
            InitializeComponent();
            workers = wList;
            dataGridView1.DataSource = workers;
        }
    }
}

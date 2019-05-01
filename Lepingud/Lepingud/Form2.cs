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
    public partial class Form2 : Form
    {
        public Leping GetText { get { return new Leping(textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text, Double.Parse(textBox5.Text)); } }
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
    }
}

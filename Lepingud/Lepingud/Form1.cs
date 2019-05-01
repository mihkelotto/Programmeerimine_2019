using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace Lepingud
{
    public partial class Form1 : Form
    {
        private List<Leping> listLeping = new List<Leping>();
        private List<Leping> workersList;
        string fileName = "lepingud.xml";
        public Form1()
        {
            InitializeComponent();
            lepingBindingSource.DataSource = typeof(Leping);
            label6.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lepingBindingSource.MoveFirst();
            ShowInfo();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            lepingBindingSource.MovePrevious();
            ShowInfo();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            lepingBindingSource.MoveNext();
            ShowInfo();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            lepingBindingSource.MoveLast();
            ShowInfo();
        }

        private void ShowInfo()
        {
            if (listLeping.Count != 0)
            {
                label6.Text = "Element number " + (lepingBindingSource.Position + 1).ToString() + " of " + lepingBindingSource.Count.ToString();
            }
            else label6.Text = "No elements in list";
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            using (var reader = new StreamReader(fileName))
            {
                XmlSerializer deser = new XmlSerializer(typeof(List<Leping>));
                listLeping = (List<Leping>)deser.Deserialize(reader);
                lepingBindingSource.DataSource = listLeping;
            }
            ShowInfo();
        }

        private void fileOpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Cursor Files|*.xml";
            openFileDialog1.Title = "Select a Cursor File";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (var reader = new StreamReader(openFileDialog1.FileName))
                {
                    XmlSerializer deser = new XmlSerializer(typeof(List<Leping>));
                    if (!listLeping.Any())
                    {
                        List<Leping> listLeping2 = new List<Leping>();
                        listLeping2.AddRange((List<Leping>)deser.Deserialize(reader));
                        listLeping = listLeping2;
                    }
                    else
                    {
                        listLeping.AddRange((List<Leping>)deser.Deserialize(reader));
                    }
                    lepingBindingSource.DataSource = listLeping;
                }
                ShowInfo();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.ShowHelp = true;
            saveFileDialog1.FileName = fileName;
            openFileDialog1.Filter = "Cursor Files|*.xml";
            openFileDialog1.Title = "Select a Cursor File";

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                using (FileStream writer = new FileStream(openFileDialog1.FileName, FileMode.Create))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(List<Leping>));
                    ser.Serialize(writer, listLeping);
                }
            }
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /*
         * Väljastada töötajad kelle lepping lõppeb 5 päeva pärast ja kelle palk on 2 korda suurem kui eesti keskmine.
         * */
        private static bool Calculations(Leping lp)
        {
            DateTime date = DateTime.ParseExact(lp.Kestvus, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture); DateTime.ParseExact(lp.Kestvus, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
            int d = (Convert.ToInt32((date - DateTime.UtcNow.Date).TotalDays));
            double wage = 1455 * 2;
            if (lp.Palk >= wage && d == 5)
            {
                return true;
            }

            else
            {
                return false;
            }

        }

        private void button9_Click(object sender, EventArgs e)
        {

            workersList = listLeping.FindAll(Calculations);
            Form3 frm3 = new Form3(workersList);
            frm3.ShowDialog();
        }

        private void insertDemoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 frm2 = new Form2();
            frm2.ShowDialog();
            Leping l = frm2.GetText;
            frm2.Close();
            if (!listLeping.Any())
            {
                List<Leping> listLeping2 = new List<Leping>();
                listLeping2.Add(l);
                listLeping = listLeping2;
            }
            else
            {
                listLeping.Add(l);
            }
            lepingBindingSource.DataSource = listLeping;
            ShowInfo();
        }

        private void insertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Leping l = new Leping(textBox1.Text, textBox2.Text, textBox3.Text, textBox4.Text, Double.Parse(textBox5.Text));
            int m = (lepingBindingSource.Position);
            listLeping[m] = l;
            lepingBindingSource.DataSource = listLeping;
            ShowInfo();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listLeping != null)
            {
                lepingBindingSource.RemoveCurrent();
            }
            lepingBindingSource.DataSource = listLeping;
            ShowInfo();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            lepingBindingSource.Clear();
            lepingBindingSource.DataSource = listLeping;
            ShowInfo();
        }
    }
}

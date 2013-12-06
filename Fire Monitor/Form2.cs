using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Fire_Monitor
{
    public partial class Form2 : Form
    {
        bool firstrun;
        bool loading = true;
        private DataSet1 dataSet1;
        Form1 f1;

        public DataSet1 ds
        {
            get
            {
                return dataSet1;
            }
            set
            {
                dataSet1 = value;
            }
        }

        public Form2(Form1 f1_tmp)
        {
            InitializeComponent();
            f1 = f1_tmp;
        }

        public void Form2_Load(object sender, EventArgs e)
        {
            string xmlfile = Application.StartupPath + "\\db.xml";
            string schemafile = Application.StartupPath + "\\db.xsd";
            dataSet1.ReadXml(xmlfile);
            dataSet1.ReadXmlSchema(schemafile);
            firstrun = Convert.ToBoolean(dataSet1.Tables["settings"].Rows[0][7]);
            check_table();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0)
            {
                ToolTip tip = new ToolTip();
                tip.IsBalloon = true;
                tip.ToolTipIcon = ToolTipIcon.Error;
                tip.ToolTipTitle = "Грешка!";
                tip.Show("Моля въведете IP адрес или Hostname!", textBox1, 0, -65, 1000);
                textBox1.Focus();
                return;
            }

            if (firstrun)
            {
                dataSet1.Tables["settings"].Rows[0]["first_run"] = false;
            }
            //dataSet1.Tables["settings"].Rows[0]["hist"] = numericUpDown2.Value;
            //dataSet1.Tables["settings"].Rows[0]["upper_limit"] = numericUpDown4.Value;
            //dataSet1.Tables["settings"].Rows[0]["lower_limit"] = numericUpDown3.Value;

            dataSet1.AcceptChanges();
            string xmlfile = Application.StartupPath + "\\db.xml";
            dataSet1.WriteXml(xmlfile);
            //Form1 f1 = new Form1();
            //f1.reload_db();
            //f1.Show();
            //Application.Restart();
            f1.dataSet1 = (DataSet1)dataSet1.Copy();
            f1.reload_db();
            this.Close();
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            ToolTip tip = new ToolTip();
            tip.IsBalloon = true;
            tip.ToolTipIcon = ToolTipIcon.Error;
            tip.ToolTipTitle = "Грешка!";
            if (textBox1.Text.Length == 0)
            {
                tip.Show("Моля въведете IP адрес или Hostname!", textBox1, 0, -65, 1000);
                textBox1.Focus();
                return;
            }
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            int r;
            if (!Int32.TryParse(textBox3.Text, out r))
            {
                ToolTip tip = new ToolTip();
                tip.IsBalloon = true;
                tip.ToolTipIcon = ToolTipIcon.Error;
                tip.ToolTipTitle = "Грешка!";
                tip.Show("Моля въвеждайте само цифри", textBox3, 0, -65, 1000);
                textBox3.Focus();
                return;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (loading)
                return;
            if (e.ColumnIndex == 1)
            {
                check_table();
            }
            if (e.ColumnIndex == 3)
            {
                if (dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString() == "")
                {
                    dataGridView1.Rows[e.RowIndex].Cells[3].Value = 0;
                }
               if (Convert.ToDouble(dataGridView1.Rows[e.RowIndex].Cells[3].Value) >= Convert.ToDouble(dataGridView1.Rows[e.RowIndex].Cells[4].Value) - Convert.ToDouble(numericUpDown2.Value))
                {
                    dataGridView1.Rows[e.RowIndex].Cells[3].Value = Convert.ToDouble(dataGridView1.Rows[e.RowIndex].Cells[4].Value) - Convert.ToDouble(numericUpDown2.Value) - 1;
                }
            }
            else if (e.ColumnIndex == 4)
            {
                if (dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString() == "")
                {
                    dataGridView1.Rows[e.RowIndex].Cells[4].Value = 0;
                }
               if (Convert.ToDouble(dataGridView1.Rows[e.RowIndex].Cells[4].Value) <= Convert.ToDouble(dataGridView1.Rows[e.RowIndex].Cells[3].Value) + Convert.ToDouble(numericUpDown2.Value))
                {
                    dataGridView1.Rows[e.RowIndex].Cells[4].Value = Convert.ToDouble(dataGridView1.Rows[e.RowIndex].Cells[3].Value) + Convert.ToDouble(numericUpDown2.Value) + 1;
                }
            }
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.ColumnIndex == 4)
            {
                MessageBox.Show("Моля въвеждайте само цифри!","Грешка",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
            }
        }

        private void dataGridView1_CellLeave(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 6 && !dataGridView1.Rows[e.RowIndex].Cells[6].ReadOnly)
                {
                    if (colorDialog1.ShowDialog() != DialogResult.Cancel)
                    {
                        dataSet1.Tables["sensors"].Rows[e.RowIndex]["color"] = "#" + string.Format("{0:X2}{1:X2}{2:X2}", colorDialog1.Color.R, colorDialog1.Color.G, colorDialog1.Color.B);
                        check_table();
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown3.Enabled = checkBox3.Checked;
            numericUpDown4.Enabled = checkBox3.Checked;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {

        }

        private void check_table()
        {
            loading = false;
            for (int row = 0; row <= 7; row++)
            {
                if ((int)dataGridView1.Rows[row].Cells[1].Value == 0)
                {
                    for (int i = 2; i <= 8; i++)
                    {
                        dataGridView1.Rows[row].Cells[i].Style.BackColor = Color.LightGray;
                        dataGridView1.Rows[row].Cells[i].ReadOnly = true;
                        dataGridView1.Rows[row].Cells[i].Style.ForeColor = (i == 6) ? Color.LightGray : Color.DarkGray;
                    }
                }
                else if ((int)dataGridView1.Rows[row].Cells[1].Value == 4)
                {
                    dataGridView1.Rows[row].Cells[2].ReadOnly = false;
                    dataGridView1.Rows[row].Cells[2].Style.BackColor = Color.White;
                    dataGridView1.Rows[row].Cells[2].Style.ForeColor = Color.Black;
                    for (int i = 3; i <= 5; i++)
                    {
                        dataGridView1.Rows[row].Cells[i].ReadOnly = true;
                        dataGridView1.Rows[row].Cells[i].Style.BackColor = Color.LightGray;
                        dataGridView1.Rows[row].Cells[i].Style.ForeColor = Color.DarkGray;
                    }
                    for (int i = 6; i <= 8; i++)
                    {
                        dataGridView1.Rows[row].Cells[i].ReadOnly = false;
                        dataGridView1.Rows[row].Cells[i].Style.BackColor = Color.White;
                        dataGridView1.Rows[row].Cells[i].Style.ForeColor = Color.Black;
                    }
                }
                else
                {
                    for (int i = 2; i <= 8; i++)
                    {
                        dataGridView1.Rows[row].Cells[i].ReadOnly = false;
                        dataGridView1.Rows[row].Cells[i].Style.BackColor = Color.White;
                        dataGridView1.Rows[row].Cells[i].Style.ForeColor = Color.Black;
                    }
                }
                if (Convert.ToInt32(dataGridView1.Rows[row].Cells[1].Value) != 0)
                {
                    try
                    {
                        Color bgcolor = System.Drawing.ColorTranslator.FromHtml(dataGridView1.Rows[row].Cells[6].Value.ToString());
                        Color fontcolor = ContrastColor(bgcolor);
                        dataGridView1.Rows[row].Cells[6].Style.BackColor = bgcolor;
                        dataGridView1.Rows[row].Cells[6].Style.ForeColor = bgcolor;
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        Color ContrastColor(Color color)
        {
            var c = new int[] { 255 - color.R, 255 - color.G, 255 - color.B };
            int d = 0;

            // Counting the perceptive luminance - human eye favors green color... 
            double a = 1 - (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;

            if (a < 0.5)
                d = 0; // bright colors - dark font
            else
                d = 255; // dark colors - white font

            return Color.FromArgb(d, d, d);
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewRowsAddedEventArgs e)
        {

        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Form1 f1 = new Form1();
            //f1.Show();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NCXCODE
{
    public partial class DebugWindow : Form
    {
        public DebugWindow()
        {
            InitializeComponent();
        }

        string outputcode;

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            outputcode = textBox2.Text;
        }

        public void OnOutput(object value)
        {
            textBox1.Text += value.ToString() + "\n\r";
        }

        public string OnInput(NXCODE.Virtual_Machine.vThread from)
        {
            return outputcode;
        }
    }
}

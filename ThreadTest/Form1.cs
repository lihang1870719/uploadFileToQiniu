using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace ThreadTest
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource _cts;
        public Form1()
        {
            InitializeComponent();
        }

        private void CountTo(int countTo, CancellationToken ct)
        {
            int sum = 0;
            for (; countTo > 0; countTo--)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }
                sum += countTo;
                //Invoke方法用于获得创建lbl_Status的线程所在的上下文
                this.Invoke(new Action(() => label1.Text = sum.ToString()));
                Thread.Sleep(200);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _cts = new CancellationTokenSource();
            ThreadPool.QueueUserWorkItem(state => CountTo(int.Parse(textBox1.Text), _cts.Token));
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (_cts != null)
                _cts.Cancel();
        }
    }
}

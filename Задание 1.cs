using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Compress_DeCompress
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string CompressM(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var sb = new StringBuilder();
            int count = 1;

            for (int i = 1; i <= input.Length; i++)
            {
                bool same = i < input.Length && input[i] == input[i - 1];

                if (same) count++;
                
                else
                {
                    sb.Append(input[i - 1]);
                    if (count > 1) sb.Append(count);
                    count = 1;
                }
            }
            return sb.ToString();
        }

        private string DecompressM(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var sb = new StringBuilder();
            int i = 0;

            while (i < input.Length)
            {
                char letter = input[i++];
                int numStart = i;

                while (i < input.Length && char.IsDigit(input[i])) i++;

                int count = 1;
                if (i > numStart)
                {
                    count = int.Parse(input.Substring(numStart, i - numStart));
                }

                sb.Append(letter, count);
            }

            return sb.ToString();
        }

        private void Compress_Click(object sender, EventArgs e)
        {
            CompressText.Text = CompressM(InputText.Text);
        }

        private void DeCompress_Click(object sender, EventArgs e)
        {
            DeCompressText.Text = DecompressM(CompressText.Text);
        }
    }
}

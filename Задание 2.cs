using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static class Server
        {
            private static int count = 0;
            private static readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

            private static int readErrorIssued = 0;

            public static void ResetReadErrorFlag()
            {
                Interlocked.Exchange(ref readErrorIssued, 0); 
            }

            public static int GetCount()
            {
                if (locker.TryEnterReadLock(50))
                {
                    try{ return count; }
                    
                    finally { locker.ExitReadLock(); }
                }
                else
                {
                    if (Interlocked.CompareExchange(ref readErrorIssued, 1, 0) == 0) 
                        throw new InvalidOperationException("Ошибка: сейчас происходит запись. Повторите позже.");
                    else
                        return -1; 
                }
            }

            public static void AddToCount(int value)
            {
                locker.EnterWriteLock();
                try
                {
                    Thread.Sleep(500);
                    count += value;
                }
                finally
                {
                    locker.ExitWriteLock();
                    ResetReadErrorFlag(); 
                }
            }
        }

        private void UpdateListBox(string message)
        {
            if (listBox1.InvokeRequired)
                listBox1.Invoke(new Action(() => listBox1.Items.Add(message)));
            
            else
                listBox1.Items.Add(message);
        }

        private void btnStartReaders_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                Task.Run(() =>
                {
                    try
                    {
                        int value = Server.GetCount();
                        if (value != -1)
                            UpdateListBox($"[Reader] Count = {value}");
                    }
                    catch (InvalidOperationException ex)
                    {
                        UpdateListBox($"[Reader] ❌ {ex.Message}");
                    }
                });
            }
        }

        private void btnStartWriters_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 5; i++)
            {
                Task.Run(() =>
                {
                    Server.AddToCount(1);
                    UpdateListBox("[Writer] Добавил 1");
                }
                );
            }
        }
    }
}

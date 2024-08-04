using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSV_Merge
{
    public partial class SelectCSV : Form
    {
        public string[] IgnoreItems { get; set; }
        public SelectCSV(string FolderPath, string outfile = "merge.csv")
        {
            InitializeComponent();
            outfile = Path.Combine(FolderPath, outfile);
            GetCSVs(FolderPath, outfile);
        }

        private void GetCSVs (string FolderPath, string outfile)
        {
            if (!Directory.Exists(FolderPath)) throw new DirectoryNotFoundException(FolderPath);

            var files = Directory.GetFiles(FolderPath, "*.csv");
            if (files.Length == 0) throw new FileNotFoundException("No CSV Files Found In The Selected Folder");

            checkedListBox1.Items.AddRange(files);
            if (File.Exists(outfile)) checkedListBox1.Items.Remove(outfile);
            for (int i = 0; i < checkedListBox1.Items.Count; i++) checkedListBox1.SetItemChecked(i, true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> items = new List<string>();
            foreach (var item in checkedListBox1.Items)
            {
                if (!checkedListBox1.CheckedItems.Contains(item))
                {
                    items.Add(item.ToString());
                    Console.WriteLine(item.ToString());
                }
            }

            IgnoreItems = items.ToArray();  
            DialogResult = DialogResult.OK;
        }
    }
}

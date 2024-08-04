using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSV_Merge
{
    public partial class Form1 : Form
    {
        private string SelectedFolderPath { get; set;}
        private string OutFile { get; set;}
        private string[] IgnoreItems { get; set;}

        public Form1()
        {
            InitializeComponent();
            textBoxFolder.Text = @"\\sm-vie-fs\IRIS\Export - SM_Invoices\";
            if (Directory.Exists(SelectedFolderPath))
                textBoxOutfile.Text = Path.Combine(SelectedFolderPath, "merge.csv");
        }

        private void SelectFolder()
        {
            using (var ofd = new FolderBrowserDialog())
            {
                var result = ofd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    SelectedFolderPath = ofd.SelectedPath;
                    textBoxFolder.Text = ofd.SelectedPath;
                    OutFile = Path.Combine(ofd.SelectedPath, "merge.csv");
                    textBoxOutfile.Text = OutFile;
                    GetCSVs();
                }
            }
        }

        private void selectFolderToolStripMenuItem_Click(object sender, EventArgs e) => SelectFolder();
        private void buttonSelectFolder_Click(object sender, EventArgs e) => SelectFolder();
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) => SaveCSV();

        private void GetCSVs ()
        {
            if (null == SelectedFolderPath) throw new Exception("Folder not selected");

            try
            {
                DataTable dataTable = new DataTable();
                var files = Directory.GetFiles(SelectedFolderPath, "*.csv");
                foreach (var file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (!fi.Exists) continue;
                    if (fi.Name == Path.GetFileName(OutFile)) continue;
                    if (IgnoreItems != null && IgnoreItems.Contains(fi.FullName)) continue;

                    DataTable dt = fillTable(fi.FullName);
                    Console.WriteLine($"\r\nFile: {fi.Name}\r\n Rows: {dt.Rows.Count} \r\n Cols: {dt.Columns.Count}");
                    dataTable.Merge(dt);

                    foreach (string c in new string[] { "BatchID", "Batch Name", "Original Filename", "Filename" })
                        dataTable.Columns[c].SetOrdinal(dataTable.Columns.Count - 1);
                }

                dataGridView1.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed To Get CSV Files", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable fillTable(string csvFile)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(csvFile))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string h in headers) dt.Columns.Add(h.Replace("\"", ""));

                while (!sr.EndOfStream)
                {
                    Regex parse = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                    String[] cols = parse.Split(sr.ReadLine());
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < cols.Length; i++) dr[i] = (Regex.Replace(cols[i], "(^\")|(\"$)", ""));
                    dt.Rows.Add(dr);
                }
            }
            return dt;
        }

        private void SaveCSV ()
        {
            if (File.Exists(OutFile))
            {
                var result =
                MessageBox.Show(
                    $"\"{OutFile}\" already exists, would you like to overwrite this file?"
                    , "Outfile Exists"
                    , MessageBoxButtons.YesNo
                    , MessageBoxIcon.Exclamation
                );

                if (result == DialogResult.No)
                {
                    MessageBox.Show("Save Canceled");
                    return;
                }

                File.Delete(OutFile);
            }

            try
            {
                DataTable dt = new DataTable();
                foreach (DataGridViewColumn col in dataGridView1.Columns) dt.Columns.Add(col.Name);
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    DataRow dr = dt.NewRow();
                    foreach (DataGridViewCell cell in row.Cells) dr[cell.ColumnIndex] = cell.Value;
                    dt.Rows.Add(dr);
                }

                StringBuilder sb = new StringBuilder();
                string[] csvCols = new string[dt.Columns.Count];
                for (int i = 0; i < dt.Columns.Count; i++) csvCols[i] = dt.Columns[i].ColumnName;
                sb.AppendLine(string.Join(",", csvCols));

                for (int r = 0; r < dt.Rows.Count; r++)
                {
                    List<string> csvRow = new List<string>();
                    for (int c = 0; c < dt.Columns.Count; c++)
                        csvRow.Add($"\"{dt.Rows[r][c]}\"");

                    sb.AppendLine(string.Join(",", csvRow));
                }

                Console.Write("Writing merged CSV...");
                File.WriteAllText(Path.Combine(OutFile), sb.ToString());
                Console.Write("Done");

                if (File.Exists(OutFile)) MessageBox.Show($"CSV file has been saved: \"{OutFile}\"");
                else MessageBox.Show("An unknown error occured while attempting to save the merged CSV file.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed To Save CSV", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBoxFolder_TextChanged(object sender, EventArgs e) => SelectedFolderPath = textBoxFolder.Text;
        private void textBoxOutfile_TextChanged(object sender, EventArgs e) => OutFile = textBoxOutfile.Text;
        private void buttonGetCSV_Click(object sender, EventArgs e) => GetCSVs();
        private void buttonSave_Click(object sender, EventArgs e) => SaveCSV();

        private void buttonSelectCSVs_Click(object sender, EventArgs e)
        {
            var select = new SelectCSV(SelectedFolderPath);
            var result = select.ShowDialog();
            if (result == DialogResult.OK) IgnoreItems = select.IgnoreItems;
            GetCSVs();
        }
    }
}

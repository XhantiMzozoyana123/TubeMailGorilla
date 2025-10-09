using CsvHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TubeMailGorilla.Entities;

namespace TubeMailGorilla.Forms
{
    public partial class Blocked_Email_Addresses : Form
    {
        private readonly ApplicationDbContext context = new ApplicationDbContext();
        public Blocked_Email_Addresses()
        {
            InitializeComponent();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                Blocker blockers = new Blocker();

                blockers.Email = txtEmail.Text;
                blockers.CreatedAt = DateTime.Now;
                blockers.UpdatedAt = DateTime.Now;

                context.Blockers.Add(blockers);
                context.SaveChanges();

                MessageBox.Show("Email successfully blocked...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnExportToCSV_Click(object sender, EventArgs e)
        {
            try
            {
                var query = context.Blockers.ToList();

                SaveFileDialog saveFileDialog = new SaveFileDialog()
                {
                    InitialDirectory = @"c:\",
                    DefaultExt = "csv",
                    Title = "",
                    CheckFileExists = false,
                    CheckPathExists = false,
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

                if (DialogResult.OK == saveFileDialog.ShowDialog())
                {
                    string path = saveFileDialog.FileName;

                    using (var writer = new StreamWriter(path))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(query);
                    }
                }

                MessageBox.Show("Data exported successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnImportToCSV_Click(object sender, EventArgs e)
        {
            try
            {
                var query = context.Blockers.ToList();

                OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    InitialDirectory = @"c:\",
                    DefaultExt = "csv",
                    Title = "",
                    CheckFileExists = false,
                    CheckPathExists = false,
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

                if (DialogResult.OK == openFileDialog.ShowDialog())
                {
                    string path = openFileDialog.FileName;

                    using (var reader = new StreamReader(path))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        var records = csv.GetRecords<Blocker>();

                        foreach (var item in records)
                        {
                            Blocker blockers = new Blocker();

                            blockers.Email = item.Email;
                            blockers.CreatedAt = item.CreatedAt;
                            blockers.UpdatedAt = item.UpdatedAt;

                            context.Blockers.Add(blockers);
                            context.SaveChanges();
                        }

                        dgvBlocker.DataSource = query;
                    }
                }

                MessageBox.Show("Data imported successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            try
            {
                var query = context.Blockers.ToList();

                foreach (var item in query)
                {
                    context.Remove(item);
                    context.SaveChanges();
                }

                MessageBox.Show("Records remove successfully...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                var query = context.Blockers.ToList();
                dgvBlocker.DataSource = query;
                
                context.SaveChanges();

                MessageBox.Show("Records updated successfully...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

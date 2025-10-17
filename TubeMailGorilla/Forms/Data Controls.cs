using CsvHelper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TubeMailGorilla.Constants;
using TubeMailGorilla.Entities;

namespace TubeMailGorilla.Forms
{
    public partial class Data_Controls : Form
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        public Data_Controls()
        {
            InitializeComponent();
        }

        private void btnExportToCSV_Click(object sender, EventArgs e)
        {
            try
            {
                var source = AppConstant.GetDataSources();

                if (cboSource.Text == source[0])
                {
                    var query = context.Emailers.ToList();

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
                }
                else
                {
                    var query = context.Commentors.ToList();

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
                }

                MessageBox.Show("Data exported successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnImportFromCSV_Click(object sender, EventArgs e)
        {
            var source = AppConstant.GetDataSources();
            var isEmailer = cboSource.Text == source[0];

            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    DefaultExt = "csv",
                    Title = "Select CSV File to Import",
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    RestoreDirectory = true
                })
                {
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        int importedCount = 0;

                        using (var reader = new StreamReader(openFileDialog.FileName))
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            if (isEmailer)
                            {
                                var records = csv.GetRecords<Emailer>().ToList();
                                foreach (var item in records)
                                {
                                    context.Emailers.Add(item);
                                    importedCount++;
                                }
                            }
                            else
                            {
                                var records = csv.GetRecords<Entities.Commentor>().ToList();
                                foreach (var item in records)
                                {
                                    context.Commentors.Add(item);
                                    importedCount++;
                                }
                            }
                            context.SaveChanges();
                        }

                        dgvEmailer.DataSource = isEmailer ? context.Emailers.ToList() : context.Commentors.ToList();

                        MessageBox.Show($"{importedCount} records imported successfully.",
                            "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Import failed:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdateRecords_Click(object sender, EventArgs e)
        {
            var source = AppConstant.GetDataSources();
            var isEmailer = cboSource.Text == source[0];

            try
            {
                dgvEmailer.DataSource = isEmailer ? context.Emailers.ToList() : context.Commentors.ToList();
                context.SaveChanges();

                MessageBox.Show("Records updated successfully.",
                    "Update Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteRecords_Click(object sender, EventArgs e)
        {
            try
            {
                var source = AppConstant.GetDataSources();
                var isEmailer = cboSource.Text == source[0];

                var confirm = MessageBox.Show(
                    "Are you sure you want to DELETE ALL records? This cannot be undone.",
                    "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm != DialogResult.Yes) return;

                int deleted = 0;

                // Always wrap in a transaction for safety
                using (var transaction = context.Database.BeginTransaction())
                {
                    if (isEmailer)
                    {
                        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Emailers");
                        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Captions");
                        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Icebreakers");
                        deleted = 1; // just an indicator that truncation succeeded
                    }
                    else
                    {
                        context.Database.ExecuteSqlRaw("TRUNCATE TABLE Commentors");
                        deleted = 1;
                    }

                    transaction.Commit();
                }

                // Refresh grid (will now be empty)
                dgvEmailer.DataSource = isEmailer ? context.Emailers.ToList() : context.Commentors.ToList();

                MessageBox.Show("All records deleted successfully.",
                    "Deletion Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Delete failed:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBlockedEmailAddresses_Click(object sender, EventArgs e)
        {
            Blocked_Email_Addresses blocked_Email_Addresses = new Blocked_Email_Addresses();
            blocked_Email_Addresses.Show();
        }

        private void cboSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var source = AppConstant.GetDataSources();
                var isEmailer = cboSource.Text == source[0];

                dgvEmailer.DataSource = isEmailer ? context.Emailers.ToList() : context.Commentors.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Data_Controls_Load(object sender, EventArgs e)
        {
            try
            {
                cboSource.Items.Clear();
                foreach (var item in AppConstant.GetDataSources())
                {
                    cboSource.Items.Add(item);
                }

                cboSource.SelectedIndex = 0;
                dgvEmailer.DataSource = context.Emailers.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAiAssistant_Click(object sender, EventArgs e)
        {
            AI_Assistant aI_Assistant = new AI_Assistant();
            aI_Assistant.Show();
        }
    }
}

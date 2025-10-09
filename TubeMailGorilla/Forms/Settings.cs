using CsvHelper;
using Microsoft.EntityFrameworkCore;
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
using TubeMailGorilla.Constants;
using TubeMailGorilla.Dtos;
using TubeMailGorilla.Entities;

namespace TubeMailGorilla.Forms
{
    public partial class Settings : Form
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        public Settings()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var query = await context.Settings.FirstOrDefaultAsync();

                if (query != null)
                {
                    query.ExtractType = cboPaging.Text;
                    query.YouTubeDataApiKey = txtYouTubeDataAPI.Text;
                    query.GoogleAiApiKey = txtGoogleAi.Text;
                    query.ApiUrl = txtDomainName.Text;
                    query.UpdatedAt = DateTime.Now;

                    await context.SaveChangesAsync();

                    MessageBox.Show("Settings updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    Entities.Settings settings = new Entities.Settings()
                    {
                        ExtractType = cboPaging.Text,
                        YouTubeDataApiKey = txtYouTubeDataAPI.Text,
                        GoogleAiApiKey = txtGoogleAi.Text,
                        ApiUrl = txtDomainName.Text,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    context.Settings.Add(settings);
                    await context.SaveChangesAsync();

                    MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSearchBatchFile_Click(object sender, EventArgs e)
        {
            try
            {

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

                saveFileDialog.ShowDialog();

                var path = saveFileDialog.FileName;

                List<BatchSearchDto> searchViewModels = new List<BatchSearchDto>()
                {
                    new BatchSearchDto()
                    {
                        Keywords  = null,
                        PageViewLimit = 0,
                        NumberofChannelsVideos = 0,
                        NumberofViewCount = 0,
                        NumberofLikeCount = 0,
                        AddressRecipientByEmailUserName = false,
                        ReturnGmailAccountOnly = false,
                        ReturnValidatedEmails = false,
                        AccelerateMode = false
                    }
                };

                using (var writer = new StreamWriter(path))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(searchViewModels);
                }

                MessageBox.Show("Batch search CSV file created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cboPaging_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Settings_Load(object sender, EventArgs e)
        {
            var query = AppConstant.GetPagingOptions();
            var settings = context.Settings.FirstOrDefault();

            if (settings != null)
            {
                txtYouTubeDataAPI.Text = settings.YouTubeDataApiKey;
                txtGoogleAi.Text = settings.GoogleAiApiKey;
                txtDomainName.Text = settings.ApiUrl;
                cboPaging.Text = settings.ExtractType;
            }

            foreach (var item in query)
            {
                cboPaging.Items.Add(item);
            }
        }

        private void btnBatchCommentSearch_Click(object sender, EventArgs e)
        {
            try
            {

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

                saveFileDialog.ShowDialog();

                var path = saveFileDialog.FileName;

                List<CommentorDto> searchViewModels = new List<CommentorDto>()
                {
                    new CommentorDto()
                    {
                        Keyword  = string.Empty,
                        MaxResults = 100,
                        HttpMode = false
                    }
                };

                using (var writer = new StreamWriter(path))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(searchViewModels);
                }

                MessageBox.Show("Batch comment search CSV file created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
}

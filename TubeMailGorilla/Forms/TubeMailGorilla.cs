using CsvHelper;
using System.Globalization;
using TubeMailGorillaApplication.Dtos;
using TubeMailGorillaApplication.Interfaces;
using TubeMailGorillaDomain.Entities;
using TubeMailGorillaDomain.Interfaces;

namespace TubeMailGorilla.Forms
{
    public partial class TubeMailGorilla : Form
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IYouTubeSearchService _youTubeSearchService;
        private readonly IEmailService _emailService;
        private readonly ICaptionService _captionService;
        private readonly IAIService _aiService;

        private Credientals? _credientals;
        private List<MessengerDto> _emailViewModelsList = new List<MessengerDto>();

        public TubeMailGorilla(
            IUnitOfWork unitOfWork,
            IYouTubeSearchService youTubeSearchService,
            IEmailService emailService,
            ICaptionService captionService,
            IAIService aiService)
        {
            _unitOfWork = unitOfWork;
            _youTubeSearchService = youTubeSearchService;
            _emailService = emailService;
            _captionService = captionService;
            _aiService = aiService;
            InitializeComponent();
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                var settings = await _unitOfWork.Settings.GetFirstAsync();
                if (settings == null)
                {
                    MessageBox.Show("No settings configured. Please configure settings first.");
                    return;
                }

                var options = new List<string> { "Page View", "Max Result" };

                var searchDto = new SearchDto
                {
                    Keywords = txtKeyword.Text,
                    PageViewLimit = Convert.ToInt32(txtPageViewLimit.Text),
                    NumberofChannelsVideos = Convert.ToInt32(txtNoVideoInChannel.Text),
                    NumberofViewCount = Convert.ToInt32(txtViewCount.Text),
                    NumberofLikeCount = Convert.ToInt32(txtLikeCount.Text),
                    AddressRecipientByEmailUserName = ckEmailUserName.Checked,
                    ReturnGmailAccountOnly = ckGmailAccountOnly.Checked,
                    ReturnValidatedEmails = ckValidateEmails.Checked,
                    AccelerateMode = ckAccelerateMode.Checked,
                    HttpMode = ckHTTPMode.Checked,
                    TestMode = ckInsights.Checked
                };

                if (settings.ExtractType == options[0])
                {
                    await _youTubeSearchService.PageViewExtractionAsync(
                        searchDto,
                        msg => lstResults.Items.Add(msg),
                        val => { if (val > 0) prgResult.Maximum = val; prgResult.Value = val; });
                }
                else if (settings.ExtractType == options[1])
                {
                    await _youTubeSearchService.MaxResultExtractionAsync(
                        searchDto,
                        msg => lstResults.Items.Add(msg),
                        val => { if (val > 0) prgResult.Maximum = val; prgResult.Value = val; });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void btnBatchSearch_Click(object sender, EventArgs e)
        {
            try
            {
                var settings = await _unitOfWork.Settings.GetFirstAsync();
                if (settings == null)
                {
                    MessageBox.Show("No settings configured. Please configure settings first.");
                    return;
                }

                var options = new List<string> { "Page View", "Max Result" };

                OpenFileDialog openFileDialog = new OpenFileDialog
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
                    using (var reader = new StreamReader(openFileDialog.FileName))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        var batchSearchRecords = csv.GetRecords<BatchSearchDto>();

                        foreach (var item in batchSearchRecords)
                        {
                            var searchDto = new SearchDto
                            {
                                Keywords = item.Keywords,
                                PageViewLimit = item.PageViewLimit,
                                NumberofChannelsVideos = item.NumberofChannelsVideos,
                                NumberofViewCount = item.NumberofViewCount,
                                NumberofLikeCount = item.NumberofLikeCount,
                                AddressRecipientByEmailUserName = item.AddressRecipientByEmailUserName,
                                ReturnGmailAccountOnly = item.ReturnGmailAccountOnly,
                                ReturnValidatedEmails = item.ReturnValidatedEmails,
                                AccelerateMode = item.AccelerateMode,
                                HttpMode = ckHTTPMode.Checked,
                                TestMode = ckInsights.Checked
                            };

                            if (settings.ExtractType == options[0])
                            {
                                await _youTubeSearchService.PageViewExtractionAsync(
                                    searchDto,
                                    msg => lstResults.Items.Add(msg),
                                    val => { if (val > 0) prgResult.Maximum = val; prgResult.Value = val; });
                            }
                            else if (settings.ExtractType == options[1])
                            {
                                await _youTubeSearchService.MaxResultExtractionAsync(
                                    searchDto,
                                    msg => lstResults.Items.Add(msg),
                                    val => { if (val > 0) prgResult.Maximum = val; prgResult.Value = val; });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnClearResults_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you would like to clear all your results?", "Clear Results", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                lstResults.Items.Clear();
            }
        }

        private void btnAddTest_Click(object sender, EventArgs e)
        {
            try
            {
                MessengerDto emailViewModels = new MessengerDto
                {
                    Id = _emailViewModelsList.Count(),
                    EmailFrom = txtEmail.Text,
                    Subject = txtSubject.Text,
                    Body = rtxtBody.Text,
                    Html = ckHtmlMode.Checked
                };

                _emailViewModelsList.Add(emailViewModels);
                cboABTest.Items.Add("Test " + _emailViewModelsList.Count().ToString());
                MessageBox.Show("Test " + _emailViewModelsList.Count().ToString() + " has been added successfully...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnRemoveTest_Click(object sender, EventArgs e)
        {
            _emailViewModelsList.Clear();
            cboABTest.Items.Clear();
            MessageBox.Show("All tests have been removed successfully...");
        }

        private async void TubeMailGorilla_Load(object sender, EventArgs e)
        {
            await GetEmailUpdates();
        }

        private async Task GetEmailUpdates()
        {
            try
            {
                var emailers = await _unitOfWork.Emailers.GetAllAsync();
                var inboxers = await _unitOfWork.Inboxers.GetAllAsync();

                int numberofUniqueEmails = emailers.Where(x => x.Checked == false).GroupBy(g => g.Email).Count();
                int emailsForwarded = emailers.Where(x => x.Checked == true).Count();
                int emailReplies = inboxers.Count();

                txtUniqueEmailsFound.Text = numberofUniqueEmails.ToString();
                txtEmailsForwarded.Text = emailsForwarded.ToString();
                txtEmailReplies.Text = emailReplies.ToString();

                cboEmailAddress.Items.Clear();
                foreach (var item in await _unitOfWork.Credientals.GetAllAsync())
                {
                    cboEmailAddress.Items.Add(item.Email);
                }

                cboTemplate.Items.Clear();
                foreach (var temp in await _unitOfWork.Templates.GetAllAsync())
                {
                    cboTemplate.Items.Add(temp.Name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void cboTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var template = await _unitOfWork.Templates.GetByNameAsync(cboTemplate.Text);
                if (template != null)
                {
                    txtSubject.Text = template.Subject;
                    rtxtBody.Text = template.Body;
                    ckHtmlMode.Checked = template.Html;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void btnSendEmail_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Procedure is now in process, please wait...");

            try
            {
                if (ckABTest.Checked)
                {
                    var emailers = await _unitOfWork.Emailers.GetAllAsync();
                    var uncheckedEmails = emailers.Where(x => x.Checked == false).ToList();
                    await AbTestMechanic(uncheckedEmails);
                }
                else
                {
                    var subject = txtSubject.Text;
                    var body = rtxtBody.Text;
                    var htmlMode = ckHtmlMode.Checked;

                    var emailers = await _unitOfWork.Emailers.GetAllAsync();
                    var uncheckedEmails = emailers.Where(x => x.Checked == false).ToList();

                    foreach (var item in uncheckedEmails)
                    {
                        var iceBreaker = await _unitOfWork.Icebreakers.GetByEmailerIdAsync(item.Id);

                        string icebreakerText = iceBreaker?.Text ?? "Hello!";

                        string newSubject = subject.Replace("[name]", item.Author)
                                                    .Replace("[title]", item.Title)
                                                    .Replace("[descr]", item.Description)
                                                    .Replace("[url]", item.Url)
                                                    .Replace("[icebreak]", icebreakerText);

                        string newBody = body.Replace("[name]", item.Author)
                                                .Replace("[title]", item.Title)
                                                .Replace("[descr]", item.Description)
                                                .Replace("[url]", item.Url)
                                                .Replace("[icebreak]", icebreakerText);

                        MessengerDto message = new MessengerDto
                        {
                            EmailTo = _emailService.EmailConditioner(item.Email, ckTestMode.Checked, _credientals?.Email ?? item.Email),
                            Subject = newSubject,
                            Body = newBody,
                            Html = htmlMode
                        };

                        await _emailService.SendEmailAsync(message);
                    }

                    MessageBox.Show("Emails successfully forwarded...");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task AbTestMechanic(List<Emailer> emailers)
        {
            foreach (var emailer in emailers)
            {
                Random random = new Random();
                int e = random.Next(_emailViewModelsList.Count);

                var testVm = _emailViewModelsList[e];
                var iceBreaker = await _unitOfWork.Icebreakers.GetByEmailerIdAsync(emailer.Id);
                string icebreakerText = iceBreaker?.Text ?? "Hello!";

                string newSubject = testVm.Subject
                    .Replace("[name]", emailer.Author)
                    .Replace("[title]", emailer.Title)
                    .Replace("[descr]", emailer.Description)
                    .Replace("[url]", emailer.Url)
                    .Replace("[icebreak]", icebreakerText);

                string newBody = testVm.Body
                    .Replace("[name]", emailer.Author)
                    .Replace("[title]", emailer.Title)
                    .Replace("[descr]", emailer.Description)
                    .Replace("[url]", emailer.Url)
                    .Replace("[icebreak]", icebreakerText);

                MessengerDto message = new MessengerDto
                {
                    EmailTo = _emailService.EmailConditioner(emailer.Email, ckTestMode.Checked, _credientals?.Email ?? emailer.Email),
                    Subject = newSubject,
                    Body = newBody,
                    Html = testVm.Html
                };

                await _emailService.SendEmailAsync(message);
            }
        }

        private void btnRefreshSummary_Click(object sender, EventArgs e)
        {
            _ = GetEmailUpdates();
        }

        private void btnDataControls_Click(object sender, EventArgs e)
        {
            Data_Controls data_Controls = new Data_Controls();
            data_Controls.Show();
        }

        private async void cboEmailAddress_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                _credientals = await _unitOfWork.Credientals.GetByEmailAsync(cboEmailAddress.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnFollowUp_Click(object sender, EventArgs e)
        {
            Follow_Up follow_Up = new Follow_Up();
            follow_Up.Show();
        }

        private void btnCommentor_Click(object sender, EventArgs e)
        {
            Commentor commentor = new Commentor();
            commentor.Show();
        }

        private void proxiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Proxies proxies = new Proxies();
            proxies.Show();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.Show();
        }

        private void templatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Templates templates = new Templates();
            templates.Show();
        }

        private void accountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Accounts accounts = new Accounts();
            accounts.Show();
        }

        private void addNewAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Add_Account add_Account = new Add_Account();
            add_Account.Show();
        }

        private void dataControlsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Data_Controls data_Controls = new Data_Controls();
            data_Controls.Show();
        }

        private void blockedEmailAddressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Blocked_Email_Addresses blocked = new Blocked_Email_Addresses();
            blocked.Show();
        }

        private void aIAssistantToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AI_Assistant aI_Assistant = new AI_Assistant();
            aI_Assistant.Show();
        }

        private void iceBreakersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Forms.Icebreakers icebreakers = new Forms.Icebreakers();
            icebreakers.Show();
        }

        private void ckABTest_CheckedChanged(object sender, EventArgs e)
        {
            cboABTest.Enabled = ckABTest.Checked;
            btnAddTest.Enabled = ckABTest.Checked;
            btnRemoveTest.Enabled = ckABTest.Checked;
        }
    }
}

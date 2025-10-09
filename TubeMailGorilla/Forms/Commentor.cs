using AngleSharp.Media;
using CsvHelper;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TubeMailGorilla.Api;
using TubeMailGorilla.Constants;
using TubeMailGorilla.Dtos;
using TubeMailGorilla.Entities;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YouTubeSearch;

namespace TubeMailGorilla.Forms
{
    public partial class Commentor : Form
    {
        private ApplicationDbContext context = new ApplicationDbContext();

        public Commentor()
        {
            InitializeComponent();
        }

        private void Commentor_Load(object sender, EventArgs e)
        {

        }

        private void ScrapeAllData(string videoUrl)
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("headless");
            chromeOptions.AddArgument("window-size=1920,1080");
            chromeOptions.AddArgument("--disable-gpu");
            chromeOptions.AddArgument("--no-sandbox");
            chromeOptions.AddArgument("log-level=3"); // Suppress non-critical logs

            IWebDriver driver = null;

            try
            {
                driver = new ChromeDriver(chromeOptions);
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(20);

                lstResults.Items.Add($"Navigating to video: {videoUrl}");
                driver.Navigate().GoToUrl(videoUrl);
                Thread.Sleep(5000);

                lstResults.Items.Add("Scrolling page to load comments...");
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                for (int i = 0; i < 5; i++)
                {
                    js.ExecuteScript("window.scrollTo(0, document.documentElement.scrollHeight);");
                    Thread.Sleep(2000);
                }

                var commentElements = driver.FindElements(By.CssSelector("ytd-comment-thread-renderer"));
                if (commentElements.Count == 0)
                {
                    lstResults.Items.Add("No comments found. Selectors may need updating.");
                    return;
                }

                lstResults.Items.Add($"\n--- Found {commentElements.Count} top-level comments. Starting scrape ---\n");

                var scrapedChannelVideos = new Dictionary<string, Dictionary<string, string>>();

                foreach (var commentElement in commentElements)
                {
                    try
                    {
                        var authorElement = commentElement.FindElement(By.CssSelector("#author-text"));
                        var commentTextElement = commentElement.FindElement(By.CssSelector("#content-text"));
                        string channelUrl = authorElement.GetAttribute("href");

                        lstResults.Items.Add($"Author: {authorElement.Text}");
                        lstResults.Items.Add($"Channel URL: {channelUrl}");
                        lstResults.Items.Add($"Comment: {commentTextElement.Text}");

                        Dictionary<string, string> videoInfo;

                        if (scrapedChannelVideos.ContainsKey(channelUrl))
                        {
                            lstResults.Items.Add("[Cache Hit] Retrieving videos from cache.");
                            videoInfo = scrapedChannelVideos[channelUrl];
                        }
                        else
                        {
                            lstResults.Items.Add("[Scraping] Getting videos from channel...");
                            videoInfo = ScrapeVideosFromChannel(channelUrl, driver);
                            scrapedChannelVideos[channelUrl] = videoInfo; // Add to cache
                        }

                        if (videoInfo.Any())
                        {
                            var channelDescr = ScrapeChannelDescription(channelUrl, driver);

                            lstResults.Items.Add("Commenter's Videos:");
                            foreach (var video in videoInfo)
                            {
                                var title = video.Key;
                                var url = video.Value;

                                var videoDescr = ScrapeVideoDescription(url, driver);
                                var payload = videoDescr + channelDescr;

                                var email = ExtractEmails(payload);

                                if (email != "")
                                {
                                    Entities.Commentor commentor = new Entities.Commentor()
                                    {
                                        Name = authorElement.Text,
                                        Email = email,
                                        VideoUrl = url,
                                        ChanneUrl = channelUrl,
                                        OriginVideoUrl = videoUrl,
                                        CreatedAt = DateTime.Now,
                                        UpdatedAt = DateTime.Now
                                    };

                                    context.Commentors.Add(commentor);
                                    context.SaveChanges();

                                    lstResults.Items.Add($"Commenter's Email Found: {email}");

                                    break;
                                }
                            }
                        }
                        else
                        {
                            lstResults.Items.Add("Commenter's Videos: None found or channel is private.");
                        }
                        lstResults.Items.Add("--------------------------------------------------\n");
                    }
                    catch (NoSuchElementException)
                    {
                        // Skip unexpected elements
                    }
                }
            }
            catch (Exception ex)
            {
                lstResults.Items.Add($"An error occurred: {ex.Message}");
            }
            finally
            {
                driver?.Quit();
            }
        }

        /// <summary>
        /// Scrapes the titles and URLs of the first few videos from a given channel URL.
        /// </summary>
        /// <returns>A dictionary where the key is the video title and the value is the video URL.</returns>
        private Dictionary<string, string> ScrapeVideosFromChannel(string channelUrl, IWebDriver driver)
        {
            var videoInfo = new Dictionary<string, string>();
            string originalTab = driver.CurrentWindowHandle;

            try
            {
                driver.SwitchTo().NewWindow(WindowType.Tab);
                driver.Navigate().GoToUrl(channelUrl + "/videos");
                Thread.Sleep(4000);

                var videoElements = driver.FindElements(By.CssSelector("a#video-title-link"));

                foreach (var videoElement in videoElements.Take(5))
                {
                    string title = videoElement.GetAttribute("title");
                    string url = videoElement.GetAttribute("href");

                    if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(url) && !videoInfo.ContainsKey(title))
                    {
                        videoInfo.Add(title, url);
                    }
                }
            }
            catch (Exception ex)
            {
                lstResults.Items.Add($"--> Could not scrape channel {channelUrl}. Error: {ex.Message}");
            }
            finally
            {
                driver.Close();
                driver.SwitchTo().Window(originalTab);
            }
            return videoInfo;
        }

        private string ScrapeChannelDescription(string channelUrl, IWebDriver driver)
        {
            string description = "";
            string originalTab = driver.CurrentWindowHandle;

            try
            {
                // Open About page in a new tab
                driver.SwitchTo().NewWindow(WindowType.Tab);
                driver.Navigate().GoToUrl(channelUrl + "/about");
                Thread.Sleep(5000);

                // Get channel description
                var descrElement = driver.FindElement(By.CssSelector("yt-attributed-string#description-container"));
                description = descrElement.Text;
            }
            catch (Exception ex)
            {
                lstResults.Items.Add($"[Error] Could not scrape description for {channelUrl}: {ex.Message}");
            }
            finally
            {
                driver.Close();
                driver.SwitchTo().Window(originalTab);
            }

            return description;
        }


        private string ExtractEmails(string textToScrape)
        {
            string value = "";
            try
            {
                Regex reg = new Regex(@"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,6}", RegexOptions.IgnoreCase);
                Match match;

                List<string> results = new List<string>();
                for (match = reg.Match(textToScrape); match.Success; match = match.NextMatch())
                {
                    if (!(results.Contains(match.Value)))
                        results.Add(match.Value);
                }

                value = results[0];
            }
            catch (Exception)
            {
                return "";
            }

            return value;
        }

        private string ScrapeVideoDescription(string videoUrl, IWebDriver driver)
        {
            string description = "";
            string originalTab = driver.CurrentWindowHandle;

            try
            {
                // Open video in a new tab
                driver.SwitchTo().NewWindow(WindowType.Tab);
                driver.Navigate().GoToUrl(videoUrl);
                Thread.Sleep(5000);

                // Click "...more" button to expand full description (if available)
                try
                {
                    var moreButton = driver.FindElement(By.CssSelector("tp-yt-paper-button#expand"));
                    if (moreButton.Displayed)
                    {
                        moreButton.Click();
                        Thread.Sleep(2000);
                    }
                }
                catch (NoSuchElementException)
                {
                    // No "...more" button, description is already short or not present
                }

                // Grab the description text
                var descrElement = driver.FindElement(By.CssSelector("ytd-text-inline-expander yt-attributed-string"));
                description = descrElement.Text.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Could not scrape description for {videoUrl}: {ex.Message}");
            }
            finally
            {
                driver.Close();
                driver.SwitchTo().Window(originalTab);
            }

            return description;
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (ckHtmlMode.Checked)
                {
                    CommentorDto commentorDto = new CommentorDto()
                    {
                        Keyword = txtKeyword.Text,
                        MaxResults = int.Parse(txtMaxResult.Text),
                        HttpMode = ckHTTPMode.Checked
                    };

                    CommentorApi.SingleExtractionApi(commentorDto);
                }
                else
                {

                    VideoSearch videoSearch = new VideoSearch();

                    var query = await videoSearch.GetVideos(txtKeyword.Text, 1);

                    foreach (var item in query)
                    {
                        // Replace this with the full URL of the YouTube video you want to scrape
                        ScrapeAllData(item.getUrl());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void btnSearchBatch_Click(object sender, EventArgs e)
        {
            try
            {
                var opt = AppConstant.GetPagingOptions();
                var settings = context.Settings.FirstOrDefault();

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
                        var records = csv.GetRecords<CommentorDto>();

                        //MessageBox.Show("Search is now in procedure, please wait for it complete...");

                        if (ckHTTPMode.Checked == true)
                        {
                            CommentorApi.BatchExtractionApi(records.ToList());
                            lstResults.Items.Add("Your procedure is now running as an http request and is being executed on the server. Please come back later to check your progress...");
                        }
                        else
                        {
                            foreach (var item in records)
                            {
                                VideoSearch videoSearch = new VideoSearch();
                                var query = await videoSearch.GetVideos(item.Keyword, item.MaxResults);

                                foreach (var video in query)
                                {
                                    // Replace this with the full URL of the YouTube video you want to scrape
                                    ScrapeAllData(video.getUrl());
                                }
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
    }
}

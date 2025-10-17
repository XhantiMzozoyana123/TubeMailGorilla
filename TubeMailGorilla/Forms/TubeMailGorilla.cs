using CsvHelper;
using DnsClient;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using TubeMailGorilla.Api;
using TubeMailGorilla.Constants;
using TubeMailGorilla.Dtos;
using TubeMailGorilla.Entities;
using YoutubeExplode;
using YouTubeSearch;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace TubeMailGorilla.Forms
{
    public partial class TubeMailGorilla : Form
    {
        private ApplicationDbContext appDbContext = new ApplicationDbContext();
        private Credientals credientals = new Credientals();
        private List<MessengerDto> emailViewModelsList = new List<MessengerDto>();
        private static readonly HttpClient _httpClient = new HttpClient();

        public TubeMailGorilla()
        {
            InitializeComponent();
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                var settings = appDbContext.Settings.FirstOrDefault();
                var options = AppConstant.GetPagingOptions();

                SearchDto searchViewModels = new SearchDto()
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
                    HttpMode = ckHTTPMode.Checked
                };

                if (settings.ExtractType == options[0])
                {
                    await PageViewingExtraction(searchViewModels);
                }

                if (settings.ExtractType == options[1])
                {
                    await MaxResultExtraction(searchViewModels);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static async Task<string> ExtractCaptions(string videoUrl)
        {
            var youtube = new YoutubeClient();

            // Get captions manifest
            var trackManifest = await youtube.Videos.ClosedCaptions.GetManifestAsync(videoUrl);
            if (trackManifest == null)
                return "❌ No captions available.";

            // Prefer English, fallback to first track
            var trackInfo = trackManifest.TryGetByLanguage("en") ?? trackManifest.Tracks[0];

            var track = await youtube.Videos.ClosedCaptions.GetAsync(trackInfo);

            var transcript = new StringBuilder();

            foreach (var caption in track.Captions)
            {
                // Just append text (no timestamps, no index)
                transcript.AppendLine(caption.Text);
            }

            return transcript.ToString();
        }

        private async Task SaveVideoCaptions(Emailer emailer)
        {
            Captions captions = new Captions()
            {
                EmailerId = emailer.Id,
                Text = await ExtractCaptions(emailer.Url),
                Url = emailer.Url
            };

            appDbContext.Captions.Add(captions);
            appDbContext.SaveChanges();
        }

        private async Task AddIceBreakers(Emailer emailer)
        {
            Icebreaker icebreaker = new Icebreaker()
            {
                EmailerId = emailer.Id,
                Text = string.Empty,
                CreatedAt = DateTime.Now
            };
            await appDbContext.Icebreakers.AddAsync(icebreaker);
            await appDbContext.SaveChangesAsync();
        }

        private async Task MaxResultExtraction(SearchDto searchDto)
        {
            try
            {
                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = appDbContext.Settings.FirstOrDefault().YouTubeDataApiKey,
                    ApplicationName = this.GetType().ToString()
                });

                var settings = appDbContext.Settings.FirstOrDefault();

                string keyword = searchDto.Keywords;
                int pageViewLimit = searchDto.PageViewLimit;
                int noVideosInChannel = searchDto.NumberofChannelsVideos;
                long noVideoView = searchDto.NumberofViewCount;
                long noVideoLikes = searchDto.NumberofLikeCount;

                bool emailUserName = searchDto.AddressRecipientByEmailUserName;
                bool gmailAccountOnly = searchDto.ReturnGmailAccountOnly;
                bool validEmail = searchDto.ReturnValidatedEmails;
                bool accelerateMode = searchDto.AccelerateMode;

                var httpMode = searchDto.HttpMode;
                var insights = searchDto.TestMode;

                if (insights == true)
                {
                    Stopwatch stopwatch = new Stopwatch();

                    stopwatch.Start();

                    var query = youtubeService.Search.List("snippet");
                    query.Q = keyword; // Replace with your search term.
                    query.MaxResults = pageViewLimit;

                    var searchListResponse = await query.ExecuteAsync();

                    lstResults.Items.Add(searchListResponse.Items.Count.ToString() + " videos found for keyword: " + keyword);
                    stopwatch.Stop();

                    lstResults.Items.Add("Delay time for keyword: " + keyword + " = " + Math.Round(stopwatch.Elapsed.TotalMinutes, 1, MidpointRounding.ToZero) + " min");
                    lstResults.Items.Add("Estimated extraction completion time for keyword: " + keyword + " = " + Math.Round(TimeSpan.FromSeconds(searchListResponse.Items.Count).TotalMinutes) + " min");
                    lstResults.Items.Add("===============PROCESS COMPLETE!===============");
                }
                else
                {
                    if (httpMode == true)
                    {
                        lstResults.Items.Add("Your procedure is now running as an http request and is being executed on the server. Please come back later to check your progress...");

                        EmailerApi.MaxResultSearch(new SearchDto
                        {
                            Keywords = txtKeyword.Text,
                            PageViewLimit = Convert.ToInt32(txtPageViewLimit.Text),
                            NumberofChannelsVideos = Convert.ToInt32(txtNoVideoInChannel.Text),
                            NumberofViewCount = Convert.ToInt32(txtViewCount.Text),
                            NumberofLikeCount = Convert.ToInt32(txtLikeCount.Text),
                            AddressRecipientByEmailUserName = ckEmailUserName.Checked,
                            ReturnGmailAccountOnly = ckGmailAccountOnly.Checked,
                            ReturnValidatedEmails = ckValidateEmails.Checked,
                            AccelerateMode = ckAccelerateMode.Checked
                        });

                        var emailer = appDbContext.Emailers.Where(x => x.Email != "").ToList();

                        lstResults.Items.Add("===============PROCESS COMPLETE!===============");
                        lstResults.Items.Add($"SUMMARY: {emailer.Count()} unique email address found...");
                    }
                    if (httpMode == false)
                    {
                        lstResults.Items.Add("Procedure is now in progress, please wait...");

                        var query = youtubeService.Search.List("snippet");
                        query.Q = keyword; // Replace with your search term.
                        query.MaxResults = pageViewLimit;

                        var searchListResponse = await query.ExecuteAsync();

                        lstResults.Items.Add(searchListResponse.Items.Count.ToString() + " videos found...");

                        string recipientUserName = null;

                        prgResult.Maximum = searchListResponse.Items.Count;

                        for (int i = 0; i < searchListResponse.Items.Count; i++)
                        {
                            string videoUrl = "https://www.youtube.com/watch?v=" + searchListResponse.Items[i].Id.VideoId;
                            string html = await GetYouTubeDataHtml(videoUrl);
                            var videoStats = await GetVideoData(videoUrl);

                            string emailFound = ExtractEmails(html);
                            int numberofVideosInChannel = 0;
                            bool gmailAccount = false;
                            long videoViewCount = 0;
                            long likeVideoCount = 0;
                            string videoDescription = searchListResponse.Items[i].Snippet.Description;

                            if (noVideoView != 0)
                            {
                                videoViewCount = Convert.ToInt64(videoStats.ViewCount);
                            }

                            if (noVideoLikes != 0)
                            {
                                likeVideoCount = Convert.ToInt64(videoStats.LikeCount);
                            }

                            if (appDbContext.Emailers.Where(x => x.Email == emailFound).FirstOrDefault() == null)
                            {
                                if (appDbContext.Blockers.Where(x => x.Email == emailFound).FirstOrDefault() == null)
                                {
                                    lstResults.Items.Add("Searching for the next video...");
                                    prgResult.Value = (i * 100) / 100;

                                    if (noVideosInChannel >= numberofVideosInChannel)
                                    {
                                        if (accelerateMode == false)
                                        {
                                            lstResults.Items.Add("Extracting data from video: (" + lstResults.Items.Count + "): " + videoUrl);

                                            if (noVideoView >= videoViewCount || videoViewCount == 0)
                                            {
                                                if (noVideoLikes >= likeVideoCount || likeVideoCount == 0)
                                                {
                                                    if (gmailAccountOnly == true)
                                                    {
                                                        if (gmailAccount == true)
                                                        {
                                                            if (validEmail == true)
                                                            {
                                                                var validateEmail = await EmailValidator(emailFound);

                                                                if (validateEmail == true)
                                                                {
                                                                    Emailer emailers = new Emailer();
                                                                    lstResults.Items.Add(i + ") Email adddress found - " + emailFound);

                                                                    if (emailUserName == true)
                                                                    {
                                                                        recipientUserName = emailFound.Split("@")[0];
                                                                    }
                                                                    else
                                                                    {
                                                                        recipientUserName = searchListResponse.Items[i].Snippet.ChannelTitle;
                                                                    }

                                                                    emailers.Author = recipientUserName;
                                                                    emailers.Title = searchListResponse.Items[i].Snippet.Title;
                                                                    emailers.Description = videoDescription;
                                                                    emailers.Url = videoUrl;
                                                                    emailers.Keyword = keyword;
                                                                    emailers.Checked = false;
                                                                    emailers.CreatedAt = DateTime.Now;
                                                                    emailers.UpdatedAt = DateTime.Now;

                                                                    appDbContext.Emailers.Add(emailers);
                                                                    appDbContext.SaveChanges();

                                                                    await SaveVideoCaptions(emailers);
                                                                    await AddIceBreakers(emailers);
                                                                }
                                                                else
                                                                {
                                                                    Emailer emailers = new Emailer();
                                                                    lstResults.Items.Add(i + ") Email adddress found - " + emailFound);

                                                                    if (emailUserName == true)
                                                                    {
                                                                        recipientUserName = emailFound.Split("@")[0];
                                                                    }
                                                                    else
                                                                    {
                                                                        recipientUserName = searchListResponse.Items[i].Snippet.ChannelTitle;
                                                                    }

                                                                    emailers.Author = recipientUserName;
                                                                    emailers.Title = searchListResponse.Items[i].Snippet.Title;
                                                                    emailers.Description = videoDescription;
                                                                    emailers.Url = videoUrl;
                                                                    emailers.Keyword = keyword;
                                                                    emailers.Checked = false;
                                                                    emailers.CreatedAt = DateTime.Now;
                                                                    emailers.UpdatedAt = DateTime.Now;

                                                                    appDbContext.Emailers.Add(emailers);
                                                                    appDbContext.SaveChanges();

                                                                    await SaveVideoCaptions(emailers);
                                                                    await AddIceBreakers(emailers);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (validEmail == true)
                                                        {
                                                            var validateEmail = await EmailValidator(emailFound);

                                                            if (validateEmail == true)
                                                            {
                                                                Emailer emailers = new Emailer();
                                                                lstResults.Items.Add(i + ") Email adddress found - " + emailFound);

                                                                if (emailUserName == true)
                                                                {
                                                                    recipientUserName = emailFound.Split("@")[0];
                                                                }
                                                                else
                                                                {
                                                                    recipientUserName = searchListResponse.Items[i].Snippet.ChannelTitle;
                                                                }

                                                                emailers.Author = recipientUserName;
                                                                emailers.Title = searchListResponse.Items[i].Snippet.Title;
                                                                emailers.Description = videoDescription;
                                                                emailers.Url = videoUrl;
                                                                emailers.Keyword = keyword;
                                                                emailers.Checked = false;
                                                                emailers.CreatedAt = DateTime.Now;
                                                                emailers.UpdatedAt = DateTime.Now;

                                                                appDbContext.Emailers.Add(emailers);
                                                                appDbContext.SaveChanges();

                                                                await SaveVideoCaptions(emailers);
                                                                await AddIceBreakers(emailers);
                                                            }
                                                            else
                                                            {
                                                                Emailer emailers = new Emailer();
                                                                lstResults.Items.Add(i + ") Email adddress found - " + emailFound);

                                                                if (emailUserName == true)
                                                                {
                                                                    recipientUserName = emailFound.Split("@")[0];
                                                                }
                                                                else
                                                                {
                                                                    recipientUserName = searchListResponse.Items[i].Snippet.ChannelTitle;
                                                                }

                                                                emailers.Author = recipientUserName;
                                                                emailers.Title = searchListResponse.Items[i].Snippet.Title;
                                                                emailers.Description = videoDescription;
                                                                emailers.Url = videoUrl;
                                                                emailers.Keyword = keyword;
                                                                emailers.Checked = false;
                                                                emailers.CreatedAt = DateTime.Now;
                                                                emailers.UpdatedAt = DateTime.Now;

                                                                appDbContext.Emailers.Add(emailers);
                                                                appDbContext.SaveChanges();

                                                                await SaveVideoCaptions(emailers);
                                                                await AddIceBreakers(emailers);
                                                            }
                                                        }
                                                    }

                                                    if (emailUserName == true)
                                                    {
                                                        recipientUserName = ExtractEmails(await GetYouTubeDataHtml(videoUrl)).Split("@")[0];
                                                    }
                                                    else
                                                    {
                                                        recipientUserName = searchListResponse.Items[i].Snippet.ChannelTitle;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            lstResults.Items.Add("Extracting data from video (" + lstResults.Items.Count + "): " + videoUrl);

                                            Emailer emailers = new Emailer();
                                            lstResults.Items.Add(i + ") Email adddress found - " + emailFound);

                                            recipientUserName = searchListResponse.Items[i].Snippet.ChannelTitle;

                                            emailers.Author = recipientUserName;
                                            emailers.Title = searchListResponse.Items[i].Snippet.Title;
                                            emailers.Description = videoDescription;
                                            emailers.Url = videoUrl;
                                            emailers.Keyword = keyword;
                                            emailers.Checked = false;
                                            emailers.CreatedAt = DateTime.Now;
                                            emailers.UpdatedAt = DateTime.Now;

                                            appDbContext.Emailers.Add(emailers);
                                            appDbContext.SaveChanges();

                                            await SaveVideoCaptions(emailers);
                                            await AddIceBreakers(emailers);
                                        }
                                    }
                                }
                            }
                        }

                        GetEmailUpdates();

                        var emailer = appDbContext.Emailers.Where(x => x.Email != "").ToList();

                        lstResults.Items.Add("===============PROCESS COMPLETE!===============");
                        lstResults.Items.Add($"SUMMARY: {emailer.Count()} unique email address found...");
                        //lstResults.SelectedIndex = int.MaxValue;
                    }
                }
            }
            catch (Exception ex)
            {
                lstResults.Items.Add($"Exception: {ex.Message}");
            }
        }

        private async Task PageViewingExtraction(SearchDto searchDto)
        {
            VideoSearch videoSearch = new VideoSearch();

            try
            {
                lstResults.Items.Add("Search is now in procedure, please wait for it complete...");

                string keyword = searchDto.Keywords;
                int pageViewLimit = searchDto.PageViewLimit;
                int noVideosInChannel = searchDto.NumberofChannelsVideos;
                long noVideoView = searchDto.NumberofViewCount;
                long noVideoLikes = searchDto.NumberofLikeCount;

                bool emailUserName = searchDto.AddressRecipientByEmailUserName;
                bool gmailAccountOnly = searchDto.ReturnGmailAccountOnly;
                bool validEmail = searchDto.ReturnValidatedEmails;
                bool accelerateMode = searchDto.AccelerateMode;

                var httpMode = searchDto.HttpMode;
                var insights = searchDto.TestMode;

                if (insights == true)
                {
                    Stopwatch stopwatch = new Stopwatch();

                    stopwatch.Start();
                    var query = await videoSearch.GetVideos(keyword, pageViewLimit);

                    lstResults.Items.Add(query.Count().ToString() + " videos found for keyword: " + keyword);
                    stopwatch.Stop();

                    lstResults.Items.Add("Delay time for keyword: " + keyword + " = " + Math.Round(stopwatch.Elapsed.TotalMinutes, 1, MidpointRounding.ToZero) + " min");
                    lstResults.Items.Add("Estimated extraction completion time for keyword: " + keyword + " = " + Math.Round(TimeSpan.FromSeconds(query.Count).TotalMinutes) + " min");
                    lstResults.Items.Add("===============PROCESS COMPLETE!===============");
                }
                else
                {
                    if (httpMode == true)
                    {
                        EmailerApi.PageViewSearch(new SearchDto
                        {
                            Keywords = txtKeyword.Text,
                            PageViewLimit = Convert.ToInt32(txtPageViewLimit.Text),
                            NumberofChannelsVideos = Convert.ToInt32(txtNoVideoInChannel.Text),
                            NumberofViewCount = Convert.ToInt32(txtViewCount.Text),
                            NumberofLikeCount = Convert.ToInt32(txtLikeCount.Text),
                            AddressRecipientByEmailUserName = ckEmailUserName.Checked,
                            ReturnGmailAccountOnly = ckGmailAccountOnly.Checked,
                            ReturnValidatedEmails = ckValidateEmails.Checked,
                            AccelerateMode = ckAccelerateMode.Checked
                        });

                        lstResults.Items.Add("Your procedure is now running as an http request and is being executed on the server. Please come back later to check your progress...");
                    }
                    else
                    {
                        lstResults.Items.Add("Procedure is now in progress, please wait...");

                        var query = await videoSearch.GetVideos(keyword, pageViewLimit);

                        lstResults.Items.Add(query.Count().ToString() + " videos found...");

                        string recipientUserName = null;

                        prgResult.Maximum = query.Count;

                        for (int i = 0; i < query.Count; i++)
                        {
                            try
                            {
                                string htmlData = await GetYouTubeDataHtml(query[i].getUrl());
                                var videoStats = await GetVideoData(query[i].getUrl());

                                string emailFound = ExtractEmails(htmlData);
                                int numberofVideosInChannel = 0;
                                bool gmailAccount = false;
                                long videoViewCount = 0;
                                long likeVideoCount = 0;
                                string videoDescription = "";

                                if (noVideoView != 0)
                                {
                                    videoViewCount = Convert.ToInt64(videoStats.ViewCount);
                                }

                                if (noVideoLikes != 0)
                                {
                                    likeVideoCount = Convert.ToInt64(videoStats.LikeCount);
                                }

                                if (appDbContext.Emailers.Where(x => x.Email == emailFound).FirstOrDefault() == null)
                                {
                                    if (appDbContext.Blockers.Where(x => x.Email == emailFound).FirstOrDefault() == null)
                                    {
                                        lstResults.Items.Add("Searching for the next video...");
                                        prgResult.Value = (i * 100) / 100;

                                        if (noVideosInChannel >= numberofVideosInChannel)
                                        {
                                            if (accelerateMode == false)
                                            {
                                                lstResults.Items.Add("Extracting data from video: (" + lstResults.Items.Count + "): " + query[i].getUrl());

                                                if (noVideoView <= videoViewCount || videoViewCount == 0)
                                                {
                                                    if (noVideoLikes <= likeVideoCount || likeVideoCount == 0)
                                                    {
                                                        if (gmailAccountOnly == true)
                                                        {
                                                            if (gmailAccount == true)
                                                            {
                                                                if (validEmail == true)
                                                                {
                                                                    var validateEmail = await EmailValidator(emailFound);

                                                                    if (validateEmail == true)
                                                                    {
                                                                        Emailer emailers = new Emailer();
                                                                        lstResults.Items.Add(i + ") Email adddress found - " + emailFound);

                                                                        if (emailUserName == true)
                                                                        {
                                                                            recipientUserName = emailFound.Split("@")[0];
                                                                        }
                                                                        else
                                                                        {
                                                                            recipientUserName = query[i].getAuthor();
                                                                        }

                                                                        emailers.Author = recipientUserName;
                                                                        emailers.Email = emailFound;
                                                                        emailers.Title = query[i].getTitle();
                                                                        emailers.Description = videoDescription;
                                                                        emailers.Url = query[i].getUrl();
                                                                        emailers.Keyword = keyword;
                                                                        emailers.Checked = false;
                                                                        emailers.CreatedAt = DateTime.Now;
                                                                        emailers.UpdatedAt = DateTime.Now;

                                                                        appDbContext.Emailers.Add(emailers);
                                                                        appDbContext.SaveChanges();

                                                                        await SaveVideoCaptions(emailers);
                                                                        await AddIceBreakers(emailers);
                                                                    }
                                                                    else
                                                                    {
                                                                        Emailer emailers = new Emailer();
                                                                        lstResults.Items.Add(i + ") Email adddress found - " + emailFound);

                                                                        if (emailUserName == true)
                                                                        {
                                                                            recipientUserName = emailFound.Split("@")[0];
                                                                        }
                                                                        else
                                                                        {
                                                                            recipientUserName = query[i].getAuthor();
                                                                        }

                                                                        emailers.Author = recipientUserName;
                                                                        emailers.Email = emailFound;
                                                                        emailers.Title = query[i].getTitle();
                                                                        emailers.Description = videoDescription;
                                                                        emailers.Url = query[i].getUrl();
                                                                        emailers.Keyword = keyword;
                                                                        emailers.Checked = false;
                                                                        emailers.CreatedAt = DateTime.Now;
                                                                        emailers.UpdatedAt = DateTime.Now;

                                                                        appDbContext.Emailers.Add(emailers);
                                                                        appDbContext.SaveChanges();

                                                                        await SaveVideoCaptions(emailers);
                                                                        await AddIceBreakers(emailers);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            if (validEmail == false)
                                                            {
                                                                var validateEmail = true;

                                                                if (validateEmail == true)
                                                                {
                                                                    //lstResults.SelectedIndex = i;

                                                                    Emailer emailers = new Emailer();
                                                                    lstResults.Items.Add(i + ") Email adddress found - " + emailFound);

                                                                    //lstResults.SelectedIndex = i;

                                                                    if (ckEmailUserName.Checked == true)
                                                                    {
                                                                        recipientUserName = emailFound.Split("@")[0];
                                                                    }
                                                                    else
                                                                    {
                                                                        recipientUserName = query[i].getAuthor();
                                                                    }

                                                                    emailers.Author = recipientUserName;
                                                                    emailers.Email = emailFound;
                                                                    emailers.Title = query[i].getTitle();
                                                                    emailers.Description = videoDescription;
                                                                    emailers.Url = query[i].getUrl();
                                                                    emailers.Keyword = keyword;
                                                                    emailers.Checked = false;
                                                                    emailers.CreatedAt = DateTime.Now;
                                                                    emailers.UpdatedAt = DateTime.Now;

                                                                    appDbContext.Emailers.Add(emailers);
                                                                    appDbContext.SaveChanges();

                                                                    await SaveVideoCaptions(emailers);
                                                                    await AddIceBreakers(emailers);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Emailer emailers = new Emailer();
                                                                lstResults.Items.Add(i + ") Email adddress found - " + emailFound);

                                                                if (ckEmailUserName.Checked == true)
                                                                {
                                                                    recipientUserName = emailFound.Split("@")[0];
                                                                }
                                                                else
                                                                {
                                                                    recipientUserName = query[i].getAuthor();
                                                                }

                                                                emailers.Author = recipientUserName;
                                                                emailers.Email = emailFound;
                                                                emailers.Title = query[i].getTitle();
                                                                emailers.Description = videoDescription;
                                                                emailers.Url = query[i].getUrl();
                                                                emailers.Keyword = keyword;
                                                                emailers.Checked = false;
                                                                emailers.CreatedAt = DateTime.Now;
                                                                emailers.UpdatedAt = DateTime.Now;

                                                                appDbContext.Emailers.Add(emailers);
                                                                appDbContext.SaveChanges();

                                                                await SaveVideoCaptions(emailers);
                                                                await AddIceBreakers(emailers);
                                                            }
                                                        }

                                                        if (emailUserName == true)
                                                        {
                                                            recipientUserName = ExtractEmails(await GetYouTubeDataHtml(query[i].getUrl())).Split("@")[0];
                                                        }
                                                        else
                                                        {
                                                            recipientUserName = query[i].getAuthor();
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                lstResults.Items.Add("Extracting data from video (" + lstResults.Items.Count + "): " + query[i].getUrl());

                                                Emailer emailers = new Emailer();
                                                lstResults.Items.Add(i + ") Email adddress found - " + emailFound);

                                                recipientUserName = query[i].getAuthor();

                                                emailers.Author = recipientUserName;
                                                emailers.Email = emailFound;
                                                emailers.Title = query[i].getTitle();
                                                emailers.Description = videoDescription;
                                                emailers.Url = query[i].getUrl();
                                                emailers.Keyword = keyword;
                                                emailers.Checked = false;
                                                emailers.CreatedAt = DateTime.Now;
                                                emailers.UpdatedAt = DateTime.Now;

                                                await appDbContext.Emailers.AddAsync(emailers);
                                                await appDbContext.SaveChangesAsync();

                                                await SaveVideoCaptions(emailers);
                                                await AddIceBreakers(emailers);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                lstResults.Items.Add($"Exception: {ex.Message}");
                            }
                        }

                        GetEmailUpdates();

                        var emailer = appDbContext.Emailers.Where(x => x.Email != "").ToList();

                        lstResults.Items.Add("===============PROCESS COMPLETE!===============");
                        lstResults.Items.Add($"SUMMARY: {emailer.Count()} unique email address found...");
                        //lstResults.SelectedIndex = int.MaxValue;
                    }
                }
            }
            catch (Exception ex)
            {
                lstResults.Items.Add($"Exception: {ex.Message}");
            }
        }

        private async Task<bool> EmailValidator(string emailFound)
        {
            try
            {
                // Step 1: Validate email format
                var addr = new MailAddress(emailFound);
                if (addr.Address != emailFound) return false;

                // Step 2: Check MX records
                string domain = emailFound.Split('@')[1];
                var lookup = new LookupClient();
                var result = await lookup.QueryAsync(domain, QueryType.MX);
                var mxRecords = result.Answers.MxRecords().Count();
                return mxRecords > 0;
            }
            catch
            {
                return false;
            }
        }

        private void GetEmailUpdates()
        {
            int numberofUniqueEmails = 0;
            int emailsForwarded = 0;
            int emailReplies = 0;

            try
            {
                var settings = appDbContext.Settings.FirstOrDefault();
                var inboxers = appDbContext.Inboxers.ToList();
                var emailers = appDbContext.Emailers.ToList();

                numberofUniqueEmails = emailers.Where(x => x.Checked == false).GroupBy(g => g.Email).ToList().Count();
                emailsForwarded = emailers.Where(x => x.Checked == true).Count();
                emailReplies = inboxers.Count();

                txtUniqueEmailsFound.Text = numberofUniqueEmails.ToString();
                txtEmailsForwarded.Text = emailsForwarded.ToString();
                txtEmailReplies.Text = emailReplies.ToString();

                foreach (var item in appDbContext.Credientals.ToList())
                {
                    cboEmailAddress.Items.Add(item.Email);
                }

                foreach (var temp in appDbContext.Templates.ToList())
                {
                    cboTemplate.Items.Add(temp.Name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async Task<long> GetNumberofVideoLikes(string htmlData)
        {
            // Load HTML
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlData);

            // Select the target <div> by class
            var divNode = doc.DocumentNode.SelectSingleNode("//div[@class='yt-spec-button-shape-next__button-text-content']");
            if (divNode == null) return 0;

            string text = divNode.InnerText.Trim().ToUpper(); // e.g. "2.4K"
            double multiplier = 1;

            if (text.EndsWith("K"))
            {
                multiplier = 1_000;
                text = text.Replace("K", "");
            }
            else if (text.EndsWith("M"))
            {
                multiplier = 1_000_000;
                text = text.Replace("M", "");
            }
            else if (text.EndsWith("B"))
            {
                multiplier = 1_000_000_000;
                text = text.Replace("B", "");
            }

            if (double.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double result))
            {
                return (long)(result * multiplier);
            }

            return 0;
        }

        private async Task<VideoStatistics> GetVideoData(string videoUrl)
        {
            string apiKey = appDbContext.Settings.FirstOrDefault().YouTubeDataApiKey;
            string videoId = videoUrl.Split("=")[1]; // e.g., dQw4w9WgXcQ

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = "YouTubeViewFetcher"
            });

            var videoRequest = youtubeService.Videos.List("statistics");
            videoRequest.Id = videoId;

            var response = await videoRequest.ExecuteAsync();

            if (response.Items.Count > 0)
            {
                var stats = response.Items[0].Statistics;

                return stats;
            }
            else
            {
                return null;
            }
        }

        private async Task<long> GetNumberofVideoView(string htmlData)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlData);

            // Select the FIRST <span> (which is always the views count on YouTube pages)
            var viewsNode = doc.DocumentNode.SelectSingleNode("//yt-formatted-string[@id='info']/span[1]");

            var viewsNumber = System.Text.RegularExpressions.Regex.Match(viewsNode.InnerText, @"[\d,]+").Value;

            return Convert.ToInt64(viewsNumber);
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

        private async Task<string> GetYouTubeDataHtml(string videoUrl)
        {
            try
            {

                // set a common User-Agent to avoid being blocked
                _httpClient.DefaultRequestHeaders.UserAgent.Clear();
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36");

                using var resp = await _httpClient.GetAsync(videoUrl);
                resp.EnsureSuccessStatusCode();
                var html = await resp.Content.ReadAsStringAsync();
                return html;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private async void btnBatchSearch_Click(object sender, EventArgs e)
        {
            try
            {
                var opt = AppConstant.GetPagingOptions();
                var settings = appDbContext.Settings.FirstOrDefault();

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
                        var records = csv.GetRecords<BatchSearchDto>();

                        //MessageBox.Show("Search is now in procedure, please wait for it complete...");

                        if (ckHTTPMode.Checked == true)
                        {
                            EmailerApi.BatchSearch(records.ToList());
                            lstResults.Items.Add("Your procedure is now running as an http request and is being executed on the server. Please come back later to check your progress...");
                        }
                        else
                        {
                            foreach (var item in records)
                            {
                                SearchDto searchViewModels = new SearchDto()
                                {
                                    Keywords = item.Keywords,
                                    PageViewLimit = item.PageViewLimit,
                                    NumberofChannelsVideos = item.NumberofChannelsVideos,
                                    NumberofViewCount = item.NumberofViewCount,
                                    NumberofLikeCount = item.NumberofLikeCount,
                                    AddressRecipientByEmailUserName = item.AddressRecipientByEmailUserName,
                                    ReturnGmailAccountOnly = item.ReturnGmailAccountOnly,
                                    ReturnValidatedEmails = item.ReturnValidatedEmails,
                                    AccelerateMode = item.AccelerateMode
                                };

                                if (settings.ExtractType == opt[0])
                                {
                                    await PageViewingExtraction(searchViewModels);
                                }

                                if (settings.ExtractType == opt[1])
                                {
                                    await MaxResultExtraction(searchViewModels);
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

        private void btnClearResults_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you would like to clear all your results?", "Clear Results", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    //do something
                    lstResults.Items.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnAddTest_Click(object sender, EventArgs e)
        {
            try
            {
                MessengerDto emailViewModels = new MessengerDto();

                emailViewModels.Id = emailViewModelsList.Count();
                emailViewModels.EmailFrom = txtEmail.Text;
                emailViewModels.Subject = txtSubject.Text;
                emailViewModels.Body = rtxtBody.Text;
                emailViewModels.Html = ckHtmlMode.Checked;

                emailViewModelsList.Add(emailViewModels);
                cboABTest.Items.Add("Test " + emailViewModelsList.Count().ToString());

                MessageBox.Show("Test " + emailViewModelsList.Count().ToString() + " " + "has been added successfully...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnRemoveTest_Click(object sender, EventArgs e)
        {
            try
            {
                emailViewModelsList.Clear();
                cboABTest.Items.Clear();

                MessageBox.Show("All tests have been removed successfully...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TubeMailGorilla_Load(object sender, EventArgs e)
        {
            GetEmailUpdates();
        }

        private void cboTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var query = appDbContext.Templates.Where(x => x.Name == cboTemplate.Text).FirstOrDefault();

                txtSubject.Text = query.Subject;
                rtxtBody.Text = query.Body;
                ckHtmlMode.Checked = query.Html;
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
                int abTestsNo = 0;
                var settings = appDbContext.Settings.FirstOrDefault();

                if (ckABTest.Checked == true)
                {
                    var emailer = appDbContext.Emailers.Where(x => x.Checked == false).ToList();

                    AbTestMerchanic(emailer, settings);
                }
                else
                {
                    var subject = txtSubject.Text;
                    var body = rtxtBody.Text;
                    var htmlMode = ckHtmlMode.Checked;

                    var query = appDbContext.Emailers.Where(x => x.Checked == false).ToList();


                    foreach (var item in query)
                    {
                        var emailerSplitter = appDbContext.Emailers.Where(x => x.Checked == false).ToList();
                        var iceBreaker = await appDbContext.Icebreakers.FirstOrDefaultAsync(x => x.EmailerId == item.Id);

                        if (iceBreaker == null)
                        {
                            iceBreaker = new Icebreaker()
                            {
                                Text = "Hello!",
                                EmailerId = item.Id
                            };
                        }

                        if (emailerSplitter != null)
                        {
                            string newSubject = subject.Replace("[name]", item.Author)
                                                        .Replace("[title]", item.Title)
                                                        .Replace("[descr]", item.Description)
                                                        .Replace("[url]", item.Url)
                                                        .Replace("[icebreak]", iceBreaker.Text);

                            string newBody = body.Replace("[name]", item.Author)
                                                        .Replace("[title]", item.Title)
                                                        .Replace("[descr]", item.Description)
                                                        .Replace("[url]", item.Url)
                                                        .Replace("[icebreak]", iceBreaker.Text);

                            MessengerDto emailViewModels = new MessengerDto();

                            emailViewModels.EmailTo = item.Email;
                            emailViewModels.Subject = newSubject;
                            emailViewModels.Body = newBody;
                            emailViewModels.Html = htmlMode;

                            await SendEmail(emailViewModels);
                        }
                    }
                }

                MessageBox.Show("Emails successfully forwarded...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void AbTestMerchanic(List<Emailer> emailer, Entities.Settings? settings)
        {
            for (int i = 0; i <= emailer.Count; i++)
            {
                Random random = new Random();
                int e = random.Next(emailViewModelsList.Count);

                var subject = emailViewModelsList[e].Subject;
                var body = emailViewModelsList[e].Body;
                var htmlMode = emailViewModelsList[e].Html;

                Icebreaker iceBreaker = new Icebreaker();

                iceBreaker = appDbContext.Icebreakers.FirstOrDefault(x => x.EmailerId == emailer[i].Id);
                if(iceBreaker == null)
                {
                    iceBreaker = new Icebreaker()
                    {
                        Text = "Hello!",
                        EmailerId = emailer[i].Id
                    };
                }

                string newSubject = subject.Replace("[name]", emailer[i].Author)
                                                        .Replace("[title]", emailer[i].Title)
                                                        .Replace("[descr]", emailer[i].Description)
                                                        .Replace("[url]", emailer[i].Url)
                                                        .Replace("[icebreak]", iceBreaker.Text);

                string newBody = body.Replace("[name]", emailer[i].Author)
                                            .Replace("[title]", emailer[i].Title)
                                            .Replace("[descr]", emailer[i].Description)
                                            .Replace("[url]", emailer[i].Url)
                                            .Replace("[icebreak]", iceBreaker.Text);

                MessengerDto emailViewModels = new MessengerDto();

                emailViewModels.EmailTo = emailer[i].Email;
                emailViewModels.Subject = newSubject;
                emailViewModels.Body = newBody;
                emailViewModels.Html = htmlMode;

                await SendEmail(emailViewModels);
            }
        }

        private async Task<bool> SendEmail(MessengerDto emailViewModels)
        {
            try
            {

                var blockers = appDbContext.Blockers.Where(x => x.Email == emailViewModels.EmailTo).FirstOrDefault();
                var credientals = appDbContext.Credientals.FirstOrDefault();

                if (blockers == null)
                {
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient(credientals.SmtpHost);

                    mail.From = new MailAddress(credientals.Email);
                    mail.To.Add(EmailConditioner(emailViewModels.EmailTo));
                    mail.Subject = emailViewModels.Subject;
                    mail.Body = emailViewModels.Body;
                    mail.IsBodyHtml = emailViewModels.Html;

                    SmtpServer.Port = credientals.SmtpPort;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(credientals.Email, credientals.Password);
                    SmtpServer.EnableSsl = true;

                    SmtpServer.Send(mail);
                    UpdateEmailer(emailViewModels.EmailTo);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void UpdateEmailer(string emailTo)
        {
            List<Emailer> query = new List<Emailer>();

            query = appDbContext.Emailers.Where(x => x.Email == emailTo).ToList();

            foreach (var item in query)
            {
                item.Checked = true;
                appDbContext.SaveChanges();
            }
        }

        private string EmailConditioner(string emailTo)
        {
            string result = null;

            try
            {
                if (ckTestMode.Checked == true)
                {
                    result = credientals.Email;
                }
                else
                {
                    result = emailTo;
                }
            }
            catch (Exception)
            {
                return null;
            }

            return result;
        }

        private void btnRefreshSummary_Click(object sender, EventArgs e)
        {
            GetEmailUpdates();
        }

        private void btnDataControls_Click(object sender, EventArgs e)
        {
            Data_Controls data_Controls = new Data_Controls();

            data_Controls.Show();
        }

        private void cboEmailAddress_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                credientals = appDbContext.Credientals.Where(x => x.Email == cboEmailAddress.Text).FirstOrDefault();
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
            Blocked_Email_Addresses blocked_Email_Addresses = new Blocked_Email_Addresses();
            blocked_Email_Addresses.Show();
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
            if (ckABTest.Checked == true)
            {
                cboABTest.Enabled = true;
                btnAddTest.Enabled = true;
                btnRemoveTest.Enabled = true;
            }
            else
            {
                cboABTest.Enabled = false;
                btnAddTest.Enabled = false;
                btnRemoveTest.Enabled = false;
            }
        }
    }
}

using System.Diagnostics;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using HtmlAgilityPack;
using Microsoft.Playwright;
using TubeMailGorillaApplication.Dtos;
using TubeMailGorillaApplication.Interfaces;
using TubeMailGorillaDomain.Entities;
using TubeMailGorillaDomain.Interfaces;
using YoutubeExplode;
using YouTubeSearch;

namespace TubeMailGorillaInfrastructure.Services
{
    public class YouTubeSearchService : IYouTubeSearchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ICaptionService _captionService;
        private static readonly HttpClient _httpClient = new HttpClient();

        public YouTubeSearchService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ICaptionService captionService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _captionService = captionService;
        }

        public async Task MaxResultExtractionAsync(SearchDto searchDto, Action<string> onLog, Action<int> onProgress, CancellationToken cancellationToken = default)
        {
            try
            {
                var settings = await _unitOfWork.Settings.GetFirstAsync();
                if (settings == null)
                {
                    onLog("Settings not found. Please configure settings first.");
                    return;
                }

                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = settings.YouTubeDataApiKey,
                    ApplicationName = GetType().ToString()
                });

                string keyword = searchDto.Keywords;
                int pageViewLimit = searchDto.PageViewLimit;
                bool emailUserName = searchDto.AddressRecipientByEmailUserName;
                bool gmailAccountOnly = searchDto.ReturnGmailAccountOnly;
                bool validEmail = searchDto.ReturnValidatedEmails;
                bool accelerateMode = searchDto.AccelerateMode;
                var insights = searchDto.TestMode;

                if (insights)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    var query = youtubeService.Search.List("snippet");
                    query.Q = keyword;
                    query.MaxResults = pageViewLimit;

                    var searchListResponse = await query.ExecuteAsync(cancellationToken);
                    onLog(searchListResponse.Items.Count + " videos found for: " + keyword);
                    stopwatch.Stop();
                    onLog("Delay time: " + Math.Round(stopwatch.Elapsed.TotalMinutes, 1) + " min");
                    onLog("===============PROCESS COMPLETE!===============");
                }
                else
                {
                    onLog("Launching Edge Browser session...");

                    using var playwright = await Playwright.CreateAsync();
                    await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = true,
                        Channel = "msedge"
                    });
                    var context = await browser.NewContextAsync(new BrowserNewContextOptions
                    {
                        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
                    });
                    var page = await context.NewPageAsync();

                    var query = youtubeService.Search.List("snippet");
                    query.Q = keyword;
                    query.MaxResults = pageViewLimit;
                    query.Type = "video";

                    var searchListResponse = await query.ExecuteAsync(cancellationToken);
                    onLog(searchListResponse.Items.Count + " videos found...");
                    onProgress(searchListResponse.Items.Count);

                    for (int i = 0; i < searchListResponse.Items.Count; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        string videoId = searchListResponse.Items[i].Id.VideoId;
                        string videoUrl = "https://www.youtube.com/watch?v=" + videoId;

                        string html = await GetYouTubeDataHtml(page, videoUrl);
                        string emailFound = _emailService.ExtractEmails(html);

                        onProgress(i + 1);

                        var exists = await _unitOfWork.Emailers.ExistsByEmailAsync(emailFound);
                        var blocked = await _unitOfWork.Blockers.ExistsByEmailAsync(emailFound);

                        if (!string.IsNullOrEmpty(emailFound) && !exists && !blocked)
                        {
                            onLog($"Found: {emailFound} from {videoUrl}");

                            bool isGmail = emailFound.ToLower().EndsWith("@gmail.com");
                            if (gmailAccountOnly && !isGmail) continue;

                            bool isValid = true;
                            if (validEmail)
                            {
                                isValid = await _emailService.ValidateEmailAsync(emailFound);
                            }

                            if (isValid || accelerateMode)
                            {
                                string recipientName = emailUserName ? emailFound.Split('@')[0] : searchListResponse.Items[i].Snippet.ChannelTitle;

                                Emailer newEntry = new Emailer
                                {
                                    Author = recipientName,
                                    Email = emailFound,
                                    Title = searchListResponse.Items[i].Snippet.Title,
                                    Description = html,
                                    Url = videoUrl,
                                    Keyword = keyword,
                                    Checked = false,
                                    CreatedAt = DateTime.Now,
                                    UpdatedAt = DateTime.Now
                                };

                                await _unitOfWork.Emailers.AddAsync(newEntry);
                                await _unitOfWork.CompleteAsync();

                                await SaveVideoCaptions(newEntry);
                                await AddIceBreakers(newEntry);
                            }
                        }
                        else
                        {
                            onLog($"Skipping video {i + 1}/{searchListResponse.Items.Count} (No email or duplicate)");
                        }
                    }

                    onLog("===============PROCESS COMPLETE!===============");
                }
            }
            catch (Exception ex)
            {
                onLog($"Exception: {ex.Message}");
            }
        }

        public async Task PageViewExtractionAsync(SearchDto searchDto, Action<string> onLog, Action<int> onProgress, CancellationToken cancellationToken = default)
        {
            VideoSearch videoSearch = new VideoSearch();

            try
            {
                onLog("Search is now in procedure, please wait for it complete...");

                string keyword = searchDto.Keywords;
                int pageViewLimit = searchDto.PageViewLimit;
                bool emailUserName = searchDto.AddressRecipientByEmailUserName;
                bool gmailAccountOnly = searchDto.ReturnGmailAccountOnly;
                bool validEmail = searchDto.ReturnValidatedEmails;
                bool accelerateMode = searchDto.AccelerateMode;
                var insights = searchDto.TestMode;

                onLog("Launching Edge Browser session...");

                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    Channel = "msedge"
                });
                var page = await browser.NewPageAsync();

                if (insights)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    var query = await videoSearch.GetVideos(keyword, pageViewLimit);
                    onLog(query.Count().ToString() + " videos found for keyword: " + keyword);
                    stopwatch.Stop();
                    onLog("Delay time for keyword: " + keyword + " = " + Math.Round(stopwatch.Elapsed.TotalMinutes, 1, MidpointRounding.ToZero) + " min");
                    onLog("Estimated extraction completion time for keyword: " + keyword + " = " + Math.Round(TimeSpan.FromSeconds(query.Count).TotalMinutes) + " min");
                    onLog("===============PROCESS COMPLETE!===============");
                }
                else
                {
                    onLog("Procedure is now in progress, please wait...");

                    var query = await videoSearch.GetVideos(keyword, pageViewLimit);
                    onLog(query.Count().ToString() + " videos found...");

                    onProgress(query.Count);

                    for (int i = 0; i < query.Count; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        try
                        {
                            string videoUrl = query[i].getUrl();
                            string descrData = await GetYouTubeVideoDescription(videoUrl);
                            string emailFound = _emailService.ExtractEmails(descrData);
                            string phoneFound = _emailService.ExtractPhoneNumbers(descrData);

                            onLog($"Extracting data from video ({i + 1}): {videoUrl}");

                            bool exists = await _unitOfWork.Emailers.ExistsByEmailAsync(emailFound);
                            bool blocked = await _unitOfWork.Blockers.ExistsByEmailAsync(emailFound);

                            if (!string.IsNullOrEmpty(emailFound) && !exists && !blocked)
                            {
                                if (gmailAccountOnly && !emailFound.ToLower().EndsWith("@gmail.com"))
                                    continue;

                                bool isValid = true;
                                if (validEmail)
                                {
                                    isValid = await _emailService.ValidateEmailAsync(emailFound);
                                }

                                if (isValid || accelerateMode)
                                {
                                    string recipientUserName = emailUserName ? emailFound.Split('@')[0] : query[i].getAuthor();

                                    Emailer emailers = new Emailer
                                    {
                                        Author = recipientUserName,
                                        Email = emailFound,
                                        Phone = phoneFound,
                                        Title = query[i].getTitle(),
                                        Description = descrData,
                                        Url = videoUrl,
                                        Keyword = keyword,
                                        Checked = false,
                                        CreatedAt = DateTime.Now,
                                        UpdatedAt = DateTime.Now
                                    };

                                    await _unitOfWork.Emailers.AddAsync(emailers);
                                    await _unitOfWork.CompleteAsync();

                                    await SaveVideoCaptions(emailers);
                                    await AddIceBreakers(emailers);

                                    onLog($"{i + 1}) Email address found - {emailFound}");
                                }
                            }
                            else
                            {
                                onLog($"Skipping video {i + 1}/{query.Count} (No email or duplicate)");
                            }
                        }
                        catch (Exception ex)
                        {
                            onLog($"Exception: {ex.Message}");
                        }

                        onProgress(i + 1);
                    }

                    var allEmailers = await _unitOfWork.Emailers.GetAllAsync();
                    onLog("===============PROCESS COMPLETE!===============");
                    onLog($"SUMMARY: {allEmailers.Count(e => !string.IsNullOrEmpty(e.Email))} unique email address found...");
                }
            }
            catch (Exception ex)
            {
                onLog($"Exception: {ex.Message}");
            }
        }

        private async Task SaveVideoCaptions(Emailer emailer)
        {
            var captionText = await _captionService.ExtractCaptionsAsync(emailer.Url);
            Captions captions = new Captions
            {
                EmailerId = emailer.Id,
                Text = captionText,
                Url = emailer.Url
            };
            await _unitOfWork.Captions.AddAsync(captions);
            await _unitOfWork.CompleteAsync();
        }

        private async Task AddIceBreakers(Emailer emailer)
        {
            Icebreaker icebreaker = new Icebreaker
            {
                EmailerId = emailer.Id,
                Text = string.Empty,
                CreatedAt = DateTime.Now
            };
            await _unitOfWork.Icebreakers.AddAsync(icebreaker);
            await _unitOfWork.CompleteAsync();
        }

        private static async Task<string> GetYouTubeDataHtml(IPage page, string videoUrl)
        {
            try
            {
                await page.GotoAsync(videoUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 30000
                });

                try
                {
                    await page.WaitForSelectorAsync("h1.ytd-video-primary-info-renderer", new PageWaitForSelectorOptions { Timeout = 3000 });
                }
                catch { }

                return await page.ContentAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Navigation Error: {ex.Message}");
                return string.Empty;
            }
        }

        private static async Task<string> GetYouTubeVideoDescription(string videoUrl)
        {
            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(videoUrl);
            return video.Description;
        }
    }
}


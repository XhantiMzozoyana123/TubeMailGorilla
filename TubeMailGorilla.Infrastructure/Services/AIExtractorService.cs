using TubeMailGorillaApplication.Interfaces;
using TubeMailGorillaDomain.Entities;
using TubeMailGorillaDomain.Interfaces;

namespace TubeMailGorillaInfrastructure.Services
{
    public class AIExtractorService : IAIExtractorService
    {
        private readonly IAIService _aiService;
        private readonly IUnitOfWork _unitOfWork;

        public AIExtractorService(IAIService aiService, IUnitOfWork unitOfWork)
        {
            _aiService = aiService;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GetCompanyAsync(string description, string subtitles)
        {
            try
            {
                var prompt = BuildNamePrompt(description + subtitles, "company");
                var result = await _aiService.GenerateTextAsync(prompt);
                return CleanResult(result);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<string> GetFirstNameAsync(string description, string subtitles)
        {
            try
            {
                var prompt = BuildNamePrompt(description + subtitles, "first name");
                var result = await _aiService.GenerateTextAsync(prompt);
                return CleanResult(result);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<string> GetLastNameAsync(string description, string subtitles)
        {
            try
            {
                var prompt = BuildNamePrompt(description + subtitles, "last name");
                var result = await _aiService.GenerateTextAsync(prompt);
                return CleanResult(result);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<string> GetJobTitleAsync(string description, string subtitles)
        {
            try
            {
                var prompt = BuildNamePrompt(description + subtitles, "job title");
                var result = await _aiService.GenerateTextAsync(prompt);
                return CleanResult(result);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<string> GetLocationAsync(string description, string subtitles)
        {
            try
            {
                var prompt = BuildNamePrompt(description + subtitles, "location");
                var result = await _aiService.GenerateTextAsync(prompt);
                return CleanResult(result);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task<string> GetIndustryAsync(string description, string subtitles)
        {
            try
            {
                var prompt = $@"
You are a classification system.

Classify the industry into ONE of these exact values:
Technology, Finance, Healthcare, Education, Retail, Manufacturing, Energy, Transportation, Entertainment, Hospitality, Other

Rules:
- Return ONLY one word
- No explanation
- No punctuation

Text:
{description} {subtitles}
";

                var result = await _aiService.GenerateTextAsync(prompt);
                var clean = CleanResult(result);
                return ParseIndustry(clean);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public async Task ExtractAllAsync(Emailer emailer)
        {
            var text = emailer.Description + emailer.Title + emailer.Author;

            var tasks = new[]
            {
                GetFirstNameAsync(emailer.Description, emailer.Title),
                GetLastNameAsync(emailer.Description, emailer.Title),
                GetCompanyAsync(emailer.Description, emailer.Title),
                GetJobTitleAsync(emailer.Description, emailer.Title),
                GetLocationAsync(emailer.Description, emailer.Title),
                GetIndustryAsync(emailer.Description, emailer.Title)
            };

            var results = await Task.WhenAll(tasks);

            emailer.FirstName = results[0];
            emailer.LastName = results[1];
            emailer.Company = results[2];
            emailer.JobTitle = results[3];
            emailer.Location = results[4];
            emailer.Industry = results[5];
        }

        private string BuildNamePrompt(string text, string target)
        {
            return $@"
You are a precise information extraction system.

Extract ONLY the person's {target} from the text below.

Rules:
- Return only the {target}
- No explanations
- No punctuation
- If not found return: UNKNOWN

Text:
{text}
";
        }

        private string CleanResult(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "UNKNOWN";

            return input.Trim()
                        .Replace("\n", "")
                        .Replace("\r", "");
        }

        private string ParseIndustry(string value)
        {
            var validIndustries = new[] { "Technology", "Finance", "Healthcare", "Education", "Retail", "Manufacturing", "Energy", "Transportation", "Entertainment", "Hospitality", "Other" };

            foreach (var industry in validIndustries)
            {
                if (value.Equals(industry, StringComparison.OrdinalIgnoreCase))
                    return industry;
            }

            return "Other";
        }
    }
}

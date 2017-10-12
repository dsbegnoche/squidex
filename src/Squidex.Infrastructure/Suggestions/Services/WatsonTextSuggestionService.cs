// ==========================================================================
//  WatsonTextSuggestionService.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using IBM.WatsonDeveloperCloud.NaturalLanguageUnderstanding.v1;
using IBM.WatsonDeveloperCloud.NaturalLanguageUnderstanding.v1.Model;

namespace Squidex.Infrastructure.Suggestions.Services
{
    public class WatsonTextSuggestionService : ISuggestionService
    {
        public string ResourceKey { get; set; }

        public string Username { get; set; }

        public string Endpoint { get; set; } = "https://gateway.watsonplatform.net/natural-language-understanding/api";

        public double MinimumTagConfidence { get; } = 0.5;

        public double MinimumCaptionConfidence { get; } = 0.3;

        public double MaxFileSize { get; } = Math.Pow(1024, 3); // 1gb

        public string TagKeyWord { get; } = "keywords";

        public NaturalLanguageUnderstandingService Service { get; set; }

        public void InitializeService()
        {
            Service = new NaturalLanguageUnderstandingService();
            Service.SetCredential(Username, ResourceKey);
            Service.VersionDate = NaturalLanguageUnderstandingService.NATURAL_LANGUAGE_UNDERSTANDING_VERSION_DATE_2017_02_27;
        }

        public async Task<object> Analyze(object content)
        {
            var parameters = new Parameters()
            {
                Text = (string)content,
                Features = new Features()
                {
                    Keywords = new KeywordsOptions
                    {
                        Limit = 4
                    },
                    Concepts = new ConceptsOptions
                    {
                        Limit = 1
                    }
                }
            };

            return Service.Analyze(parameters);
        }

        public string[] GetTags(object result)
        {
            return ((AnalysisResults)result)?.Keywords?.Select(tag => tag.Text).ToArray();
        }

        public string GetDescription(object result)
        {
            return ((AnalysisResults)result)?.Concepts?.FirstOrDefault()?.Text;
        }
    }
}

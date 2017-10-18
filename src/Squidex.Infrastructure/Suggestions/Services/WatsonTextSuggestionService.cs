// ==========================================================================
//  WatsonTextSuggestionService.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IBM.WatsonDeveloperCloud.NaturalLanguageUnderstanding.v1;
using IBM.WatsonDeveloperCloud.NaturalLanguageUnderstanding.v1.Model;

namespace Squidex.Infrastructure.Suggestions.Services
{
    public class WatsonTextSuggestionService : ITextSuggesionService
    {
        public string ResourceKey { get; set; }

        public string Username { get; set; }

        public string Endpoint { get; set; } = "https://gateway.watsonplatform.net/natural-language-understanding/api";

        public double MinimumTagConfidence { get; } = 0.5;

        public double MinimumCaptionConfidence { get; } = 0.5;

        public void InitializeService()
        {
            Service = new NaturalLanguageUnderstandingService();
            Service.SetCredential(Username, ResourceKey);
            Service.VersionDate = NaturalLanguageUnderstandingService.NATURAL_LANGUAGE_UNDERSTANDING_VERSION_DATE_2017_02_27;
        }

        public NaturalLanguageUnderstandingService Service { get; set; }

        public async Task<ServiceResults> Analyze(string content)
        {
            var parameters = new Parameters()
            {
                Text = content,
                Features = new Features()
                {
                    Keywords = new KeywordsOptions
                    {
                        Limit = 5
                    },
                    Concepts = new ConceptsOptions
                    {
                        Limit = 1
                    }
                }
            };

            var results = Service.Analyze(parameters);

            return new ServiceResults(GetTags(results.Keywords), GetDescription(results.Concepts), null);
        }

        private string GetDescription(List<ConceptsResult> results) =>
            results.Where(c => c.Relevance > MinimumCaptionConfidence)
                   .OrderBy(c => c.Relevance)
                   .Select(c => c.Text)
                   .FirstOrDefault();

        private List<string> GetTags(List<KeywordsResult> results) =>
            results.Where(k => k.Relevance > MinimumTagConfidence)
                   .OrderBy(k => k.Relevance)
                   .Select(k => k.Text)
                   .ToList();
    }
}

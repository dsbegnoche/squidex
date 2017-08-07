﻿// ==========================================================================
//  WebhookSender.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Squidex.Infrastructure.Http;

// ReSharper disable SuggestVarOrType_SimpleTypes

namespace Squidex.Domain.Apps.Read.Schemas
{
    public class WebhookSender
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

        public virtual async Task<(string Dump, WebhookResult Result, TimeSpan Elapsed)> SendAsync(WebhookJob job)
        {
            HttpRequestMessage request = BuildRequest(job);
            HttpResponseMessage response = null;

            var isTimeout = false;

            var watch = Stopwatch.StartNew();

            try
            {
                using (var client = new HttpClient { Timeout = Timeout })
                {
                    response = await client.SendAsync(request);
                }
            }
            catch (TimeoutException)
            {
                isTimeout = true;
            }
            catch (OperationCanceledException)
            {
                isTimeout = true;
            }
            finally
            {
                watch.Stop();
            }

            var responseString = string.Empty;

            if (response != null)
            {
                responseString = await response.Content.ReadAsStringAsync();
            }

            var dump = DumpFormatter.BuildDump(request, response, job.RequestBody, responseString, watch.Elapsed);

            var result = WebhookResult.Failed;

            if (isTimeout)
            {
                result = WebhookResult.Timeout;
            }
            else if (response?.IsSuccessStatusCode == true)
            {
                result = WebhookResult.Success;
            }

            return (dump, result, watch.Elapsed);
        }

        private static HttpRequestMessage BuildRequest(WebhookJob job)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, job.RequestUrl)
            {
                Content = new StringContent(job.RequestBody, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("X-Signature", job.RequestSignature);
            request.Headers.Add("User-Agent", "Squidex Webhook");

            return request;
        }
    }
}

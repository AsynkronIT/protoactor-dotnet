// -----------------------------------------------------------------------
// <copyright file="AwsHttpClient.cs" company="Asynkron AB">
//      Copyright (C) 2015-2021 Asynkron AB All rights reserved
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.IO;
using System.Net;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Proto.Cluster.AmazonECS
{
    public class AwsEcsContainerMetadataHttpClient
    {
        private readonly ILogger _logger = Log.CreateLogger<AwsEcsContainerMetadataHttpClient>();

        public AwsEcsContainerMetadataHttpClient()
        {
        }

        public Metadata GetContainerMetadata()
        {
            if (Uri.TryCreate(Environment.GetEnvironmentVariable("ECS_CONTAINER_METADATA_URI_V4"), UriKind.Absolute, out var containerMetadataUri))
            {
                var json = GetResponseString(containerMetadataUri);
                return JsonConvert.DeserializeObject<Metadata>(json);
            }

            return null;
        }
        
        
        //
        // public string GetHostPrivateIPv4Address() => GetResponseString(new Uri("http://169.254.169.254/latest/meta-data/local-ipv4"));
        //
        // public string GetHostPublicIPv4Address() => GetResponseString(new Uri("http://169.254.169.254/latest/meta-data/public-ipv4"));

        private string GetResponseString(Uri requestUri)
        {
            try
            {
                var request = WebRequest.Create(requestUri);

                using var response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("Failed to execute HTTP request. Request URI: {RequestUri}, Status code: {StatusCode}", requestUri, response.StatusCode);

                    return default;
                }

                using var stream = response.GetResponseStream();
                using var reader = new StreamReader(stream!);

                return reader.ReadToEnd();
            }
            catch (WebException ex) when (ex.Status == WebExceptionStatus.UnknownError)
            {
                // Network is unreachable
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get AWS metadata response");
            }

            return default;
        }
    }
}
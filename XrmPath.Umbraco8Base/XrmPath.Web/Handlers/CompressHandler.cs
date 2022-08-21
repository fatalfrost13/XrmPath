﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace XrmPath.UmbracoCore.Handlers
{
    using Umbraco.Core.Logging;
    using XrmPath.UmbracoCore.Helpers;
    using Task = Task;

    public class CompressHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken).ContinueWith<HttpResponseMessage>((responseToCompleteTask) =>
            {
                HttpResponseMessage response = responseToCompleteTask.Result;
                
                if (response?.RequestMessage?.Headers.AcceptEncoding != null && response.RequestMessage.Headers.AcceptEncoding.Any())
                {
                    var apiUrl = string.Empty;
                    try
                    {
                        apiUrl = response.RequestMessage?.RequestUri.OriginalString;
                        if (apiUrl.Contains("/api/") && !apiUrl.Contains("compression=0"))
                        {
                            var encodingType = response.RequestMessage.Headers.AcceptEncoding.First().Value;
                            if (response.Content != null && response.StatusCode == HttpStatusCode.OK && encodingType != null)
                            {
                                response.Content = new CompressedContent(response.Content, encodingType);
                            }
                        }

                    }
                    catch(Exception ex)
                    {
                        //Serilog.Log.Warning($"XrmPath.Web warning: Caught error on CompressHandler.CompressedContent() for Url ({apiUrl}). {ex.ToString()}");
                        LogHelper.Warning($"XrmPath.Web warning: Caught error on CompressHandler.CompressedContent() for Url ({apiUrl}). {ex.ToString()}");
                    }
                }

                return response;
            },
            TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }


    public class CompressedContent : HttpContent
    {
        private HttpContent originalContent;
        private string encodingType;

        public CompressedContent(HttpContent content, string encodingType)
        {
            if (content == null)
            {
                //throw new ArgumentNullException("content");
            }

            if (encodingType == null)
            {
                //throw new ArgumentNullException("encodingType");
            }

            originalContent = content;
            this.encodingType = encodingType.ToLowerInvariant();

            if (this.encodingType != "gzip" && this.encodingType != "deflate")
            {
                throw new InvalidOperationException(string.Format("Encoding '{0}' is not supported. Only supports gzip or deflate encoding.", this.encodingType));
            }

            // copy the headers from the original content
            foreach (KeyValuePair<string, IEnumerable<string>> header in originalContent.Headers)
            {
                this.Headers.Add(header.Key, header.Value);
            }

            this.Headers.ContentEncoding.Add(encodingType);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;

            return false;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            Stream compressedStream = null;

            if (encodingType == "gzip")
            {
                compressedStream = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true);
            }
            else if (encodingType == "deflate")
            {
                compressedStream = new DeflateStream(stream, CompressionMode.Compress, leaveOpen: true);
            }

            return originalContent.CopyToAsync(compressedStream).ContinueWith(tsk =>
            {
                if (compressedStream != null)
                {
                    compressedStream.Dispose();
                }
            });
        }
    }
}
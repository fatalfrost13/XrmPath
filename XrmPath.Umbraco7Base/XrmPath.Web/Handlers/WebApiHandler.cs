using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Umbraco.Core.Logging;
using Umbraco.Web.Models.ContentEditing;

public class WebApiHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri.AbsolutePath.ToLower() == "/umbraco/backoffice/umbracoapi/content/postsave")
        {
            return base.SendAsync(request, cancellationToken)
                .ContinueWith(task =>
                {
                    var response = task.Result;
                    try
                    {
                        var data = response.Content;
                        try
                        {
                            var content = ((ObjectContent)(data)).Value as ContentItemDisplay;
                            if (content == null)
                            {
                                return response;
                            }
                        }
                        catch (InvalidCastException)
                        {
                            //invalid cast exception
                            return response;
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error<WebApiHandler>("XrmPath.Web caught error on changing custom publishing cancelled message on WebApiHandler.SendAsyc().", ex);
                    }
                    return response;
                }, cancellationToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
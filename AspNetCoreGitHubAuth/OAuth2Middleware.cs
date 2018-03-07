using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using UofI.Passwords;

namespace AspNetCoreGitHubAuth
{
    public class AuthenticationMiddleware
    {
        public static void SetOAuth2Options(OAuthOptions options)
        {
            options.ClientId = "chartfield";
            options.ClientSecret = "gmXC+5ZfKFi0ED3UYt4AkjkrwB+0pL8VP7pNjoBJcqLpoiQZ1QchXLzMyAOx/RG2PUl7SC8wgu9d0251ZMq6zZFBWsM3pjiJfyMOMTo8Bt9KoLH+1Vdc+lKDolE0mGF01cpz0uHPn59V6nQlTVGsDEsSP4vSkjF0yqh+xfNDIH0="
                .Decrypt();
            options.AuthorizationEndpoint = "https://login.uiowa.edu/uip/auth.page";
            options.TokenEndpoint = "https://login.uiowa.edu/uip/token.page";
            //options.UserInformationEndpoint = "<given user information endpoint>";
            options.CallbackPath = new PathString("/Account/Login");

            // Set the scopes you want to request
            options.Scope.Add("user-read");
            options.Scope.Add("user-write");

            // Define how to map returned user data to claims
            options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "UnivId");
            options.ClaimActions.MapJsonKey(ClaimTypes.Name, "HawkId");

            // Register to events
            options.Events = new OAuthEvents
            {
                // After OAuth2 has authenticated the user
                OnCreatingTicket = async context =>
                {
                    // Create the request message to get user data via the backchannel
                    var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                    // Additional header if needed. Here's an example to go through Azure API Management 
                   // request.Headers.Add("Ocp-Apim-Subscription-Key", "<given key>");

                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Query for user data via backchannel
                    var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                    response.EnsureSuccessStatusCode();

                    // Parse user data into an object
                    var user = JObject.Parse(await response.Content.ReadAsStringAsync());

                    // Store the received authentication token somewhere. In a cookie for example
                    context.HttpContext.Response.Cookies.Append("token", context.AccessToken);

                    // Execute defined mapping action to create the claims from the received user object
                    context.RunClaimActions(JObject.FromObject(user));
                },
                OnRemoteFailure = context =>
                {
                    context.HandleResponse();
                    context.Response.Redirect("/Home/Error?message=" + context.Failure.Message);
                    return Task.FromResult(0);
                }
            };
        }
    }
}

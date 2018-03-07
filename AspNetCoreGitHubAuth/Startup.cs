using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

using UofI.Passwords;

namespace AspNetCoreGitHubAuth
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        { 
            
            services.AddMvc();
            var secret =
                "gmXC+5ZfKFi0ED3UYt4AkjkrwB+0pL8VP7pNjoBJcqLpoiQZ1QchXLzMyAOx/RG2PUl7SC8wgu9d0251ZMq6zZFBWsM3pjiJfyMOMTo8Bt9KoLH+1Vdc+lKDolE0mGF01cpz0uHPn59V6nQlTVGsDEsSP4vSkjF0yqh+xfNDIH0="
                    .Decrypt();
            services.AddAuthentication()
                .AddCookie(options=> options.LoginPath = new PathString("/Account/Login"))
                .AddOAuth("ITSOAUTH", options =>
                {
                    options.ClientId = "chartfield";
                    options.ClientSecret = secret;
                    options.CallbackPath = new PathString("/Account/Login");
                    options.Scope.Add("workflow.api.chartfield");
                    
                    options.AuthorizationEndpoint = "https://login.uiowa.edu/uip/auth.page";
                    options.TokenEndpoint = "https://login.uiowa.edu/uip/token.page";
                  //  options.UserInformationEndpoint = "https://api.github.com/user";
                    
                    options.SaveTokens = true;
                    

//                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "UnivId");
//                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "HawkId");

                    options.Events = new OAuthEvents
                    {
                        OnRedirectToAuthorizationEndpoint = async context =>
                        {
                            var a = 3 + 3;
                        },
                        OnRemoteFailure = async context  =>
                        {
                            var a = 3 + 3;
                        },
                        OnTicketReceived = async context =>
                        {
                            var a = 3 + 3;
                        },
                        OnCreatingTicket = async context =>
                        {
                            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                            response.EnsureSuccessStatusCode();

                            var user = JObject.Parse(await response.Content.ReadAsStringAsync());

                            context.RunClaimActions(user);
                        }
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
//            app.UseMiddleware <StackifyMiddleware.RequestTracerMiddleware
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseMiddleware<StackifyMiddleware.RequestTracerMiddleware>();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }
    }
}

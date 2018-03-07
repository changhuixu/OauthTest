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
                "abnmNTl5qOIi09uFFiKlWbFFhsOOyztvruJRRyENG4kMLpeitdcdJ2qey00/EIg7ILAA9qdrVh4wj6dBODJwzUb/EBu/PCYxk3OeHcOtY6fkwt2UfIM082y3PiNYfEeS8sQ0d+JKjrttJM2zOny6sSErkmy+hGEA6Wj+wnu9R9Q="
                    .Decrypt();

            services.AddAuthentication(options =>
                    {
                        options.DefaultChallengeScheme = "ITSOAUTH";
                        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    }
                )
                .AddCookie()
                .AddOAuth("ITSOAUTH", options =>
                {
                    options.ClientId = "fbis-fst";
                    options.ClientSecret = secret;
                    options.CallbackPath = new PathString("/FST/Login");
                    options.Scope.Add("workflow.api.fbis-fst");

                    options.AuthorizationEndpoint = "https://login.uiowa.edu/uip/auth.page";
                    options.TokenEndpoint = "https://login.uiowa.edu/uip/token.page";
                                //  options.UserInformationEndpoint = "https://api.github.com/user";

                                options.SaveTokens = true;


                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "uid");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "hawkid");

                    options.Events = new OAuthEvents
                    {
                       OnCreatingTicket = async context =>
                        {
                            //var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                            //request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                            //var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                            //response.EnsureSuccessStatusCode();

                            //var user = JObject.Parse(await response.Content.ReadAsStringAsync());
                            var user = context.TokenResponse.Response;
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

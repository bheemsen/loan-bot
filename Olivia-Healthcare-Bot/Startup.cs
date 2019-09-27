// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.3.0

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Olivia_Healthcare_Bot.Bots;
using Olivia_Healthcare_Bot.Bots.Services;
using Olivia_Healthcare_Bot.Dialogs;
using Olivia_Healthcare_Bot.Services;
//using Microsoft.Bot.Protocol.StreamingExtensions.NetCore; //added for DLS

namespace Olivia_Healthcare_Bot
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Create the credential provider to be used with the Bot Framework Adapter.
          //////  services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

            // Create the Bot Framework Adapter.
            services.AddSingleton<IBotFrameworkHttpAdapter, BotFrameworkHttpAdapter>(); //updated for DLS
                                                                                        //services.AddSingleton<IBotFrameworkHttpAdapter, WebSocketEnabledHttpAdapter>();

            // Configure Services
            services.AddSingleton<BotServices>();

            ConfigureState(services);

            ConfigureDialogs(services);

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, DialogBot<MainDialog>>();
        }

        public void ConfigureState(IServiceCollection services)
        {
            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.) 
            services.AddSingleton<IStorage, MemoryStorage>();

            //var storageAccount = "DefaultEndpointsProtocol=https;AccountName=oliviahealthcarestorage;AccountKey=mmyaDnDG9GpuMmmaq4HDjVBcJ5Lh547K1isp1JK71ut64Y5QVYKJwtO0q/7tTKw9jm2RMwBu7TJwWJU43NCZ6g==;EndpointSuffix=core.windows.net";
            //var storageContainer = "oliviahealthcarestorage";

            //services.AddSingleton<IStorage>(new AzureBlobStorage(storageAccount, storageContainer));


            // Create the User state. 
            services.AddSingleton<UserState>();

            // Create the Conversation state. 
            services.AddSingleton<ConversationState>();

            // Create an instanc of the state service 
            services.AddSingleton<BotStateService>();
        }

        public void ConfigureDialogs(IServiceCollection services)
        {
            services.AddSingleton<MainDialog>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            //app.UseHttpsRedirection();
            //app.UseWebSockets(); // added for DLS
            app.UseMvc();
        }
    }
}

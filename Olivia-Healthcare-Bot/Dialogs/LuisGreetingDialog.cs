using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Olivia_Healthcare_Bot.Bots.Services;
using Olivia_Healthcare_Bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olivia_Healthcare_Bot.Dialogs
{
    public class LuisGreetingDialog : ComponentDialog
    {
        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;
        public LuisGreetingDialog(string dialogId, BotStateService botStateService, BotServices botServices) : base(dialogId)
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));
            _botServices = botServices ?? throw new System.ArgumentNullException(nameof(botServices));
            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            // Create Waterfall Steps
            var waterfallSteps = new WaterfallStep[]
            {
                StartAsync,
                Respond,
                MessageReceivedAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(LuisGreetingDialog)}.mainFlow", waterfallSteps));
            AddDialog(new WaterfallDialog($"{nameof(LuisGreetingDialog)}.Start", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(LuisGreetingDialog)}.text"));
            AddDialog(new ConfirmPrompt($"{nameof(LuisGreetingDialog)}.confirm"));
            AddDialog(new NumberPrompt<Decimal>($"{nameof(LuisGreetingDialog)}.number"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(LuisGreetingDialog)}.mainFlow";
        }

        public async Task<DialogTurnResult> StartAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync("Hi, I am here to help you with your home loan realted queries.");
            return await stepContext.ContinueDialogAsync();
        }

        private async Task<DialogTurnResult> Respond(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            object userName;
            stepContext.Values.TryGetValue("Name", out userName);
            if (string.IsNullOrEmpty(Convert.ToString(userName)))
            {
                await stepContext.Context.SendActivityAsync("What is your name?");
                stepContext.Values["GetName"] = true;
                return await stepContext.PromptAsync($"{nameof(LuisGreetingDialog)}.text",
                   new PromptOptions
                   {
                       Prompt = MessageFactory.Text("What is your name?")
                   }, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(string.Format("Hi {0}, Welcome to Home loan advisor. Your one stop shop for home loan related queries.", userName));
                await stepContext.Context.SendActivityAsync(string.Format("We would like to know how may I assist you?"));
                return await stepContext.ContinueDialogAsync();
            }

        }


        public async Task<DialogTurnResult> MessageReceivedAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (string)stepContext.Result;
            object userName = String.Empty;
            object getName = false;

            stepContext.Values.TryGetValue("Name", out userName);
            stepContext.Values.TryGetValue("GetName", out getName);

            if (Convert.ToBoolean( getName))
            {
                userName = message;
                stepContext.Values["Name"]= userName.ToString();
                stepContext.Values["GetName"]= false;
            }
            return await stepContext.EndDialogAsync(message);
        }
    }
}

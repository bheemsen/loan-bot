using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Olivia_Healthcare_Bot.Bots.Models;
using Olivia_Healthcare_Bot.Bots.Services;
using Olivia_Healthcare_Bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olivia_Healthcare_Bot.Dialogs
{
    public class GreetingDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;
        #endregion  
        public GreetingDialog(string dialogId, BotStateService botStateService, BotServices botServices) : base(dialogId)
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
                Respond
                //FinalStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.name"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(GreetingDialog)}.mainFlow";
        }


        public async Task<DialogTurnResult> StartAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Hi, I am here to help you with your home loan realted queries.")), cancellationToken);
            return await AskForName(stepContext, cancellationToken);
            //  return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> AskForName(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.name",
              new PromptOptions
              {
                  Prompt = MessageFactory.Text("What is your name?")
              }, cancellationToken);

        }

        private async Task<DialogTurnResult> Respond(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            stepContext.Values["userName"] = (string)stepContext.Result;
            if (string.IsNullOrEmpty(Convert.ToString(stepContext.Values["userName"])))
            {
                await stepContext.Context.SendActivityAsync("What is your name?");
                stepContext.Values["userName"] = (string)stepContext.Result;
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(string.Format("Hi {0}, Welcome to Home loan advisor. Your one stop shop for home loan related queries.", stepContext.Values["userName"]));
                await stepContext.Context.SendActivityAsync(string.Format("We would like to know how may I assist you?"));

                return await stepContext.EndDialogAsync(new object());
            }
        }
    }
}

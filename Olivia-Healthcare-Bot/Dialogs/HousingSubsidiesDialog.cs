using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Olivia_Healthcare_Bot.Bots.Services;
using Olivia_Healthcare_Bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Olivia_Healthcare_Bot.Dialogs
{
    public class HousingSubsidiesDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;
        #endregion  
        public HousingSubsidiesDialog(string dialogId, BotStateService botStateService, BotServices botServices) : base(dialogId)
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
             MessageReceivedHousingSibsidiesQuery,
             MessageReceivedAsync
                //FinalStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(HousingSubsidiesDialog)}.mainFlow", waterfallSteps));
            AddDialog(new WaterfallDialog($"{nameof(HousingSubsidiesDialog)}.Start", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(HousingSubsidiesDialog)}.text"));
            AddDialog(new ConfirmPrompt($"{nameof(HousingSubsidiesDialog)}.confirm"));
            AddDialog(new NumberPrompt<Decimal>($"{nameof(HousingSubsidiesDialog)}.number"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(HousingSubsidiesDialog)}.mainFlow";
        }

        public async Task<DialogTurnResult> StartAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.PromptAsync($"{nameof(HousingSubsidiesDialog)}.confirm",
             new PromptOptions
             {
                 Prompt = MessageFactory.Text("Do you have amount for housing subsidies?")
             }, cancellationToken);

            return await stepContext.ContinueDialogAsync(cancellationToken);
        }
        private async Task<DialogTurnResult> MessageReceivedHousingSibsidiesQuery(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = (bool)stepContext.Result;
            stepContext.Values["result"] = result;
            if (result)
            {
                await stepContext.Context.SendActivityAsync("Please provide the amount for housing subsidies");
                return await stepContext.ContinueDialogAsync(cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync($"{nameof(HousingSubsidiesDialog)}.confirm",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("No Problem, do you want to determine its value?")
                }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> MessageReceivedAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = stepContext.Result;

            //message.Text = this.ownResources.ToString();

            message = "3000";

            return await stepContext.EndDialogAsync(3000);
        }
    }
}

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
    public class TestGreetingDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;
        #endregion  
        public TestGreetingDialog(string dialogId, BotStateService botStateService, BotServices botServices) : base(dialogId)
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
                InitialStepAsync//,
                //FinalStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.name"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(GreetingDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            //await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("Hello my name is Anna, your virtual health assistant. Based on your symptoms I can advice you whether you need to see a doctor, call 911 or get self care information.")), cancellationToken);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("So, how are you today?")), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);

            //return await stepContext.NextAsync(null, cancellationToken);
            //if (string.IsNullOrEmpty(userProfile.Name))
            //{
            //return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.name",
            //    new PromptOptions
            //    {
            //        Prompt = MessageFactory.Text("Hello my name is Anna, your virtual health assistant. Based on your symptoms I can advice you whether you need to see a doctor, call 911 or get self care information? Let's start what is your name?")
            //    }, cancellationToken);
            //}
            //else
            //{
            //    return await stepContext.NextAsync(null, cancellationToken);
            //}
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _botStateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            if (string.IsNullOrEmpty(userProfile.Name))
            {
                // Set the name
                userProfile.Name = (string)stepContext.Result;

                // Save any state changes that might have occured during the turn.
                await _botStateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(String.Format("How are you today?", userProfile.Name)), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}

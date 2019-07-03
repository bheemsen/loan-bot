using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Olivia_Healthcare_Bot.Bots.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olivia_Healthcare_Bot.Dialogs
{
    public class SymptomCheckerDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;
        #endregion  

        public SymptomCheckerDialog(string dialogId, BotStateService botStateService) : base(dialogId)
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            var waterfallSteps = new WaterfallStep[]
           {
                BasicSymptomsStepAsync,
                DurationStepAsync,
                ActionResultedToTheSymtomsStepAsync,
                StandardHealthParametersStepAsync
                //AppointmentStepAsync
           };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(SymptomCheckerDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(SymptomCheckerDialog)}.BasicSymptoms"));
            AddDialog(new TextPrompt($"{nameof(SymptomCheckerDialog)}.Duration"));
            AddDialog(new TextPrompt($"{nameof(SymptomCheckerDialog)}.ActionResultedToTheSymtoms"));
            AddDialog(new TextPrompt($"{nameof(SymptomCheckerDialog)}.StandardHealthParameters"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(SymptomCheckerDialog)}.mainFlow";
        }

        #region Waterfall Steps
        private async Task<DialogTurnResult> BasicSymptomsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(SymptomCheckerDialog)}.BasicSymptoms",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Sorry to hear that. What symptoms are bothering you?")                    
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> DurationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["basicSymptoms"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(SymptomCheckerDialog)}.Duration",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text($"How long (in hours) has this current {(string)stepContext.Values["basicSymptoms"]} been going on?"),
                    RetryPrompt = MessageFactory.Text("The value entered must be numeric."),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActionResultedToTheSymtomsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["duration"] = (string)stepContext.Result;

            return await stepContext.PromptAsync($"{nameof(SymptomCheckerDialog)}.ActionResultedToTheSymtoms",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Did you had coffee, energy drink or some sort of soda in last few hours"),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" }),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> StandardHealthParametersStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["ActionResultedToTheSymtoms"] = (string)stepContext.Result;
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Based on your registered FITBIT data, pulse and heart rate for last hours is fine. Based on your condtion will recomend you to have (PCM) Paracetamol"), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        
        #endregion
    }
}

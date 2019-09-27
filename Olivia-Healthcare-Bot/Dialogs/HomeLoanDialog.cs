using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Olivia_Healthcare_Bot.Bots.Services;
using Olivia_Healthcare_Bot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olivia_Healthcare_Bot.Dialogs
{
    public class HomeLoanDialog : ComponentDialog
    {
        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;

        private double finAmount = 0.0;
        private int numberOfYears = 0;

        public HomeLoanDialog(string dialogId, BotStateService botStateService, BotServices botServices) : base(dialogId)
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
                MessageReceivedStart,
                MessageReceivedProjectType
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(HomeLoanDialog)}.mainFlow", waterfallSteps));
            AddDialog(new WaterfallDialog($"{nameof(HomeLoanDialog)}.Start", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(HomeLoanDialog)}.text"));
            AddDialog(new ConfirmPrompt($"{nameof(HomeLoanDialog)}.confirm"));
            AddDialog(new NumberPrompt<Decimal>($"{nameof(HomeLoanDialog)}.number"));
            AddDialog(new ChoicePrompt($"{nameof(HomeLoanDialog)}.choice"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(HomeLoanDialog)}.mainFlow";
        }

        public async Task<DialogTurnResult> StartAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
         //   await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting");
            return await stepContext.ContinueDialogAsync();         
        }
        
        private async Task<DialogTurnResult> MessageReceivedStart(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            IList<string> projectTypesOptions = new List<string>() {
            "Construction", "Buy", "Renovation", "Miscellaneous"
            };

            var choices = new List<Choice>
            {
                new Choice { Value = "Construction" },
                new Choice { Value = "Buy" },
                new Choice { Value = "Renovation" },
                new Choice { Value = "Miscellaneous" }
            };

            return await stepContext.PromptAsync($"{nameof(HomeLoanDialog)}.choice",
               new PromptOptions
               {
                   Choices= choices,
                   Prompt = MessageFactory.Text("Please select project type from:")
                   
               }, cancellationToken);

        }

        private async Task<DialogTurnResult> MessageReceivedProjectType(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (FoundChoice)stepContext.Result;

            if (message.Value.ToLower().Equals("construction", StringComparison.InvariantCultureIgnoreCase))
            {               
                return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.constructiondialog");              
            }
            else if (message.Value.ToLower().Equals("buy", StringComparison.InvariantCultureIgnoreCase))
            {
                return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.buydialog");
                //need to implement
            }
            else if (message.Value.ToLower().Equals("renovation", StringComparison.InvariantCultureIgnoreCase))
            {
              return  await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.renovationdialog");
                //need to implement
            }
            else if (message.Value.ToLower().Equals("miscellaneous", StringComparison.InvariantCultureIgnoreCase))
            {
                return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.miscellaneousdialog");
                //need to implement
            }
            else
            {
              return await  stepContext.EndDialogAsync();
            }
        }
         
    }

    public enum ProjectTypes
    {
        Construction,
        Buy,
        Renovation,
        Others

    }
}

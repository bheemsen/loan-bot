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
    public class MainDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;

        private readonly BotServices _botServices;
        #endregion

        public MainDialog(BotStateService botStateService, BotServices botServices) : base(nameof(MainDialog))
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
                InitialStepAsync,
                FinalStepAsync
            };

            // Add Named Dialogs
            AddDialog(new MiscellaneousDialog($"{nameof(MainDialog)}.miscellaneousdialog", _botStateService, _botServices));
            AddDialog(new HomeLoanDialog($"{nameof(MainDialog)}.homeLoandialog", _botStateService, _botServices));
            AddDialog(new RenovationDialog($"{nameof(MainDialog)}.renovationdialog", _botStateService, _botServices));
            AddDialog(new ConstructionDialog($"{nameof(MainDialog)}.constructiondialog", _botStateService, _botServices));
            AddDialog(new ProjectCostDialog($"{nameof(MainDialog)}.projectCost", _botStateService, _botServices));
            AddDialog(new OwnResourcesDialog($"{nameof(MainDialog)}.ownResources", _botStateService, _botServices));
            AddDialog(new HousingSubsidiesDialog($"{nameof(MainDialog)}.housingSubsidiesDialog", _botStateService, _botServices));
            AddDialog(new BuyDialog($"{nameof(MainDialog)}.buydialog", _botStateService, _botServices));
            AddDialog(new RateCalcProcessDialog($"{nameof(MainDialog)}.ratecalcProcessdialog", _botStateService));
            AddDialog(new GreetingDialog($"{nameof(MainDialog)}.greeting", _botStateService, _botServices));
            //AddDialog(new SymptomCheckerDialog($"{nameof(MainDialog)}.symptomChecker", _botStateService));            
            //AddDialog(new SymptomDiagnoseDialog($"{nameof(MainDialog)}.symptomDiagnose", _botStateService, _botServices));
            //AddDialog(new NoSymptomeDialog($"{nameof(MainDialog)}.noSymptom", _botStateService, _botServices));

            AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainFlow", waterfallSteps));


            // Set the starting Dialog
            InitialDialogId = $"{nameof(MainDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            //if (Regex.Match(stepContext.Context.Activity.Text.ToLower(), "hi").Success)
            //{
            //    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting", null, cancellationToken);
            //}
            //else
            //{
            //    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.symptomChecker", null, cancellationToken);
            //}

            // First, we use the dispatch model to determine which cognitive service (LUIS or QnA) to use.
            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);

            // Top intent tell us which cognitive service to use.
            var topIntent = recognizerResult.GetTopScoringIntent();

            switch (topIntent.intent)
            {
                case "Greeting":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.greeting", null, cancellationToken);
                case "LoanCalculation":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.homeLoandialog", null, cancellationToken);
                case "Construction":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.constructiondialog", null, cancellationToken);
                case "Buy":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.buydialog", null, cancellationToken);
                case "Renovation":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.renovationdialog", null, cancellationToken);
                case "Miscellaneous":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.miscellaneousdialog", null, cancellationToken);
                case "RateCalculationProcess":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.ratecalcProcessdialog", null, cancellationToken);
                case "RateCalcInfo":
                    return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.ratecalcProcessdialog", null, cancellationToken);


                default:
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text($"I'm sorry I don't know what you mean."), cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

    }
}

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Olivia_Healthcare_Bot.Bots.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Olivia_Healthcare_Bot.Dialogs
{
    public class RateCalcProcessDialog : ComponentDialog
    {
        private readonly BotStateService _botStateService;
        public RateCalcProcessDialog(string dialogId, BotStateService botStateService) : base(dialogId)
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            var waterfallSteps = new WaterfallStep[]
           {
               StartAsync

           };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(RateCalcProcessDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(RateCalcProcessDialog)}.BasicSymptoms"));
            // Set the starting Dialog
            InitialDialogId = $"{nameof(RateCalcProcessDialog)}.mainFlow";
        }

        public async Task<DialogTurnResult> StartAsync(WaterfallStepContext context, CancellationToken cancellationToken)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("The calculation is based on your input, it serves as a first orientation and is not always guaranteed. The measured values are approximate figures.");
            stringBuilder.Append("FINANCING FORM");
            stringBuilder.Append("savings loans with mortgage");
            stringBuilder.Append("Bausparkasse Austrian Sparkasse Aktiengesellschaft (s Bausparkasse) ");

            stringBuilder.Append("\r\n");

            stringBuilder.Append("INTEREST");
            stringBuilder.Append("The calculated rate results from an assumed variable interest rate of 1.50% per annum for the remaining period after an initial fixed interest period. ");
            stringBuilder.Append("The fixed interest period with 1.35% per annum of the loan amount is valid for the first 72 months, gives a monthly launch rate of 40 Euro. ");
            stringBuilder.Append("Number of installments: 71 monthly installments in the Fix-interest phase and 169 monthly installments in the variable phase. ");

            stringBuilder.Append("\r\n");

            stringBuilder.Append("FEES AND EXPENSES(in the rate already included)");
            stringBuilder.Append("- Booking fees: 3% of the loan amount");
            stringBuilder.Append("- account maintenance fee: 12 Euro pa ");
            stringBuilder.Append("- capital raising fee: 0.5% of the loan amount ");

            stringBuilder.Append("\r\n");

            stringBuilder.Append("The registration fee of 1.44% of the loan we have considered NOT due to a possible exemption ,");

            stringBuilder.Append("\r\n");

            stringBuilder.Append("The granting of a building society loan mortgage-backed collateral is the existence or conclusion of a savings agreement, as well as a fire insurance.");

            stringBuilder.Append("\r\n");

            stringBuilder.Append("PAYABLE TOTAL");
            stringBuilder.Append("9,796 euros");

            stringBuilder.Append("\r\n");

            stringBuilder.Append("EFFECTIVE INTEREST RATE");
            stringBuilder.Append("2.1% pa");

            stringBuilder.Append("\r\n");

            stringBuilder.Append("STAND");
            stringBuilder.Append("01.01.2017");

            await context.Context.SendActivityAsync(stringBuilder.ToString());


            return await context.EndDialogAsync(null, cancellationToken);
        }
    }
}

using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Olivia_Healthcare_Bot.Bots.Services;
using Olivia_Healthcare_Bot.Services;
using System.Threading;
using Microsoft.Bot.Builder;
//using HomeLoanCalculator.Common;

namespace Olivia_Healthcare_Bot.Dialogs
{
    public class BuyDialog : ComponentDialog
    {
        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;

        private double finAmount = 0.0;
        private double projectCost = 0.0;
        private double ownResources = 0.0;
        private double housingSubsidies = 0.0;
        private int numberOfYears = 0;
        private double totalMonthlyIncome = 0.0;
        private double EMI = 0.0;
        private readonly double totalExpenditure = 0.0;
        public BuyDialog(string dialogId, BotStateService botStateService, BotServices botServices) : base(dialogId)
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
                MessageReceivedFinancingAmountQuery,
                MessageReceivedFinancingAmount,
                MessageReceivedDuration,
                MessageReceivedReEvaluate,
                MessageReceivedSavingAfterFinancing,
                MessageReceivedTotalMonthlyIncome,
                MessageReceivedTotalMonthlyExpenditure
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(BuyDialog)}.mainFlow", waterfallSteps));
            AddDialog(new WaterfallDialog($"{nameof(BuyDialog)}.Start", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(BuyDialog)}.text"));
            AddDialog(new ConfirmPrompt($"{nameof(BuyDialog)}.confirm"));
            AddDialog(new NumberPrompt<Decimal>($"{nameof(BuyDialog)}.number"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(BuyDialog)}.mainFlow";
        }

        public async Task<DialogTurnResult> StartAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(BuyDialog)}.confirm",
               new PromptOptions
               {
                   Prompt = MessageFactory.Text("Do you know your financing amount?")
               }, cancellationToken);

        }

        private async Task<DialogTurnResult> MessageReceivedFinancingAmountQuery(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            bool result = (bool)stepContext.Result;
            if (result)
            {
                stepContext.Values["isProjectcosts"] = false;
                return await stepContext.PromptAsync($"{nameof(BuyDialog)}.number",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please provide the financing amount")
                }, cancellationToken);
            }
            else
            {
                stepContext.Values["isProjectcosts"] = true;
                return await stepContext.PromptAsync($"{nameof(BuyDialog)}.confirm",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("No Problem, do you know the project costs?")
                }, cancellationToken);

            }
        }

        private async Task<DialogTurnResult> MessageReceivedFinancingAmount(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (Convert.ToBoolean(stepContext.Values["isProjectcosts"]))
            {
                bool result = (bool)stepContext.Result;
                if (result)
                {
                    return await stepContext.PromptAsync($"{nameof(BuyDialog)}.number",
                       new PromptOptions
                       {
                           Prompt = MessageFactory.Text("Please provide the project cost:"),
                           RetryPrompt = MessageFactory.Text("Please provide the project cost in numbers")
                       }, cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync("Thanks for connecting with us");
                    return await stepContext.EndDialogAsync();
                }
            }
            else
            {
                var message = (decimal)stepContext.Result;
                double.TryParse(message.ToString(), out finAmount);
                return await stepContext.PromptAsync($"{nameof(BuyDialog)}.number",
                   new PromptOptions
                   {
                       Prompt = MessageFactory.Text("Please provide the duration in years"),
                       RetryPrompt = MessageFactory.Text("Please provide the duration in numbers")
                   }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> MessageReceivedDuration(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (Convert.ToBoolean(stepContext.Values["isProjectcosts"]))
            {
                var message = (decimal)stepContext.Result;
                if (!string.IsNullOrEmpty(message.ToString()))
                {
                    double.TryParse(message.ToString(), out projectCost);

                    return await stepContext.PromptAsync($"{nameof(BuyDialog)}.number",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Please provide the amount invested by you (Own Resources):"),
                        RetryPrompt = MessageFactory.Text("PleasePlease provide the amount invested by you (Own Resources): in numbers")
                    }, cancellationToken);

                }
                else
                {
                    this.projectCost = 0;
                    return await stepContext.ContinueDialogAsync();
                }
            }
            else
            {
                var message = (decimal)stepContext.Result;
                int.TryParse(message.ToString(), out numberOfYears);

                var emi = Calculations.CalculateEMI(finAmount, numberOfYears * 12);
                this.EMI = emi;
                await stepContext.Context.SendActivityAsync($"Your monthly installment is: {emi} €");

                return await stepContext.PromptAsync($"{nameof(BuyDialog)}.confirm",
                   new PromptOptions
                   {
                       Prompt = MessageFactory.Text("Do you wish to re-evaluate your installments?")
                   }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> MessageReceivedReEvaluate(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (Convert.ToBoolean(stepContext.Values["isProjectcosts"]))
            {
                var message = (decimal)stepContext.Result;
                if (!string.IsNullOrEmpty(message.ToString()))
                {
                    double.TryParse(message.ToString(), out this.ownResources);
                    return await stepContext.PromptAsync($"{nameof(BuyDialog)}.number",
                      new PromptOptions
                      {
                          Prompt = MessageFactory.Text("Please provide the amount of housing subsidies:")
                      }, cancellationToken);
                }
                else
                {
                    this.ownResources = 0;
                    return await stepContext.ContinueDialogAsync();
                }
            }
            else
            {
                bool result = (bool)stepContext.Result;
                if (result)
                {
                    return await StartAsync(stepContext, cancellationToken);
                }
                else
                {
                    return await stepContext.PromptAsync($"{nameof(BuyDialog)}.confirm",
                       new PromptOptions
                       {
                           Prompt = MessageFactory.Text("Do you wish to calculate your saving after fanancing every month?")
                       }, cancellationToken);
                }
            }
        }

        private async Task<DialogTurnResult> MessageReceivedSavingAfterFinancing(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (Convert.ToBoolean(stepContext.Values["isProjectcosts"]))
            {
                var message = (string)stepContext.Result;

                if (!string.IsNullOrEmpty(message))
                {
                    double.TryParse(message, out this.housingSubsidies);

                    var financingAmount = this.projectCost - (this.ownResources + this.housingSubsidies);
                    this.finAmount = financingAmount;
                    await stepContext.Context.SendActivityAsync($"Financing Amount for your project is: {financingAmount}  €");
                    return await stepContext.PromptAsync($"{nameof(BuyDialog)}.confirm",
                     new PromptOptions
                     {
                         Prompt = MessageFactory.Text("Do you wish to continue?")
                     }, cancellationToken);
                }
                else
                {
                    this.housingSubsidies = 0;
                    return await stepContext.ContinueDialogAsync();
                }
            }
            else
            {
                bool result = (bool)stepContext.Result;
                if (result)
                {
                    return await stepContext.PromptAsync($"{nameof(BuyDialog)}.number",
                      new PromptOptions
                      {
                          Prompt = MessageFactory.Text("Please provide your total monthly income:")
                      }, cancellationToken);

                }
                else
                {
                    return await stepContext.ContinueDialogAsync();
                }
            }
        }

        private async Task<DialogTurnResult> MessageReceivedTotalMonthlyIncome(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (decimal)stepContext.Result;
            double.TryParse(message.ToString(), out totalMonthlyIncome);

            return await stepContext.PromptAsync($"{nameof(BuyDialog)}.number",
                 new PromptOptions
                 {
                     Prompt = MessageFactory.Text("Please provide your monthly expenditure:")
                 }, cancellationToken);

        }

        private async Task<DialogTurnResult> MessageReceivedTotalMonthlyExpenditure(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (decimal)stepContext.Result;
            double.TryParse(message.ToString(), out totalMonthlyIncome);

            await stepContext.Context.SendActivityAsync($"Month amount available: {this.totalMonthlyIncome - this.totalExpenditure}");
            await stepContext.Context.SendActivityAsync($"After Financing: {this.totalMonthlyIncome - (this.totalExpenditure + this.EMI)}");
            return await stepContext.EndDialogAsync();

        }
    }
}

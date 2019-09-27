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
    public class MiscellaneousDialog : ComponentDialog
    {
        private double finAmount = 0.0;
        private double projectCost = 0.0;
        private double ownResources = 0.0;
        private double housingSubsidies = 0.0;
        private int numberOfYears = 0;
        private double totalMonthlyIncome = 0.0;
        private double EMI = 0.0;
        private readonly double totalExpenditure = 0.0;

        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;
        public MiscellaneousDialog(string dialogId, BotStateService botStateService, BotServices botServices) : base(dialogId)
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
              MessageReceivedTotalMonthlyExpenditure,
              CalculateSavingAfterMonth
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(MiscellaneousDialog)}.mainFlow", waterfallSteps));
            AddDialog(new WaterfallDialog($"{nameof(MiscellaneousDialog)}.Start", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(MiscellaneousDialog)}.text"));
            AddDialog(new ConfirmPrompt($"{nameof(MiscellaneousDialog)}.confirm"));
            AddDialog(new NumberPrompt<Decimal>($"{nameof(MiscellaneousDialog)}.number"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(MiscellaneousDialog)}.mainFlow";
        }

        public async Task<DialogTurnResult> StartAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(MiscellaneousDialog)}.confirm",
               new PromptOptions
               {
                   Prompt = MessageFactory.Text("Do you know your financing amount?")
               }, cancellationToken);

        }

        private async Task<DialogTurnResult> MessageReceivedFinancingAmountQuery(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {           
            bool result = (bool)stepContext.Result;
            stepContext.Values["projectCost"] = false;
            if (result)
            {
                return await stepContext.PromptAsync($"{nameof(MiscellaneousDialog)}.number",
                 new PromptOptions
                 {
                     Prompt = MessageFactory.Text("Please provide the financing amount")
                 }, cancellationToken);
            }
            else
            {
                stepContext.Values["projectCost"] = true;
                return await stepContext.PromptAsync($"{nameof(MiscellaneousDialog)}.confirm",
                   new PromptOptions
                   {
                       Prompt = MessageFactory.Text("No Problem, do you know the project costs?")
                   }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> MessageReceivedFinancingAmount(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (Convert.ToBoolean(stepContext.Values["projectCost"]))
            {
                return await stepContext.PromptAsync($"{nameof(MiscellaneousDialog)}.number",
                   new PromptOptions
                   {
                       Prompt = MessageFactory.Text("Please provide the duration in years")
                   }, cancellationToken);
            }
            else
            {
                var message = (decimal)stepContext.Result;
                double.TryParse(message.ToString(), out finAmount);
                return await stepContext.PromptAsync($"{nameof(MiscellaneousDialog)}.number",
                   new PromptOptions
                   {
                       Prompt = MessageFactory.Text("Please provide the duration in years")
                   }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> MessageReceivedDuration(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (Convert.ToBoolean(stepContext.Values["projectCost"]))
            {
                var message = (decimal)stepContext.Result;
                if (!string.IsNullOrEmpty(message.ToString()))
                {
                    double.TryParse(message.ToString(), out projectCost);
                    return await stepContext.PromptAsync($"{nameof(MiscellaneousDialog)}.number",
                     new PromptOptions
                     {
                         Prompt = MessageFactory.Text("Please provide the amount invested by you (Own Resources):")
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

                return await stepContext.PromptAsync($"{nameof(MiscellaneousDialog)}.confirm",
                              new PromptOptions
                              {
                                  Prompt = MessageFactory.Text("Do you wish to re-evaluate your installments?")
                              }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> MessageReceivedReEvaluate(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (Convert.ToBoolean(stepContext.Values["projectCost"]))
            {
                var message = (decimal)stepContext.Result;
                if (!string.IsNullOrEmpty(message.ToString()))
                {
                    double.TryParse(message.ToString(), out this.ownResources);
                    return await stepContext.PromptAsync($"{nameof(MiscellaneousDialog)}.number",
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
                stepContext.Values["fanancingEveryMonth"] = false;
                var result = (bool)stepContext.Result;
                if (result)
                {
                    return await StartAsync(stepContext, cancellationToken);
                }
                else
                {
                    stepContext.Values["fanancingEveryMonth"] = true;
                    return await stepContext.PromptAsync($"{nameof(MiscellaneousDialog)}.confirm",
                               new PromptOptions
                               {
                                   Prompt = MessageFactory.Text("Do you wish to calculate your saving after fanancing every month?")
                               }, cancellationToken);
                }
            }
        }

        private async Task<DialogTurnResult> MessageReceivedSavingAfterFinancing(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (Convert.ToBoolean(stepContext.Values["projectCost"]))
            {
                var message = (decimal)stepContext.Result;

                if (!string.IsNullOrEmpty(message.ToString()))
                {
                    stepContext.Values["wishToContinue"] = true;
                    double.TryParse(message.ToString(), out this.housingSubsidies);
                    var financingAmount = this.projectCost - (this.ownResources + this.housingSubsidies);
                    this.finAmount = financingAmount;
                    await stepContext.Context.SendActivityAsync($"Financing Amount for your project is: {financingAmount}  €");                   
                    return await stepContext.PromptAsync($"{nameof(MiscellaneousDialog)}.confirm",
                               new PromptOptions
                               {
                                   Prompt = MessageFactory.Text("Do you wish to continue?")
                               }, cancellationToken);

                }
                else
                {
                    this.housingSubsidies = 0;
                    stepContext.Values["wishToContinue"] = false;
                    return await stepContext.ContinueDialogAsync();
                }
            }
            else
            {
                bool result = (bool)stepContext.Result;
                if (result)
                {
                    return await stepContext.PromptAsync($"{nameof(MiscellaneousDialog)}.number",
                              new PromptOptions
                              {
                                  Prompt = MessageFactory.Text("Please provide your total monthly income:")
                              }, cancellationToken);
                }
                else
                {
                    return await stepContext.EndDialogAsync();
                }
            }
        }

        private async Task<DialogTurnResult> MessageReceivedTotalMonthlyIncome(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (Convert.ToBoolean(stepContext.Values["projectCost"]))
            {
                if (Convert.ToBoolean(stepContext.Values["wishToContinue"]))
                {
                    return await stepContext.PromptAsync($"{nameof(MiscellaneousDialog)}.number",
                             new PromptOptions
                             {
                                 Prompt = MessageFactory.Text("Please provide the duration in years")
                             }, cancellationToken);
                }
                else
                {
                    return await stepContext.EndDialogAsync();
                }
            }
            else
            {
                var message = (decimal)stepContext.Result;
                double.TryParse(message.ToString(), out totalMonthlyIncome);
                return await stepContext.PromptAsync($"{nameof(MiscellaneousDialog)}.number",
                            new PromptOptions
                            {
                                Prompt = MessageFactory.Text("Please provide your monthly expenditure:")
                            }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> MessageReceivedTotalMonthlyExpenditure(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (Convert.ToBoolean(stepContext.Values["projectCost"]))
            {
                var message = (decimal)stepContext.Result;
                int.TryParse(message.ToString(), out numberOfYears);

                var emi = Calculations.CalculateEMI(finAmount, numberOfYears * 12);
                this.EMI = emi;
                await stepContext.Context.SendActivityAsync($"Your monthly installment is: {emi} €");
                return await stepContext.PromptAsync($"{nameof(MiscellaneousDialog)}.confirm",
                           new PromptOptions
                           {
                               Prompt = MessageFactory.Text("Do you wish to re-evaluate your installments")
                           }, cancellationToken);
            }
            else
            {
                var message = (decimal)stepContext.Result;
                double.TryParse(message.ToString(), out totalMonthlyIncome);

                await stepContext.Context.SendActivityAsync($"Month amount available: {this.totalMonthlyIncome - this.totalExpenditure}");
                await stepContext.Context.SendActivityAsync($"After Financing: {this.totalMonthlyIncome - (this.totalExpenditure + this.EMI)}");

                return await stepContext.EndDialogAsync();
            }
        }

        //private async Task<DialogTurnResult> MessageReceivedReEvaluate(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    var result = (bool)stepContext.Result;
        //    if ( result)
        //    {
        //        await StartAsync(stepContext,cancellationToken);
        //    }
        //    else
        //    {
        //        return await stepContext.PromptAsync($"{nameof(MiscellaneousDialog)}.confirm",
        //                   new PromptOptions
        //                   {
        //                       Prompt = MessageFactory.Text("Do you wish to calculate your saving after fanancing every month? ")
        //                   }, cancellationToken);
        //    }
        //}

        private async Task<DialogTurnResult> CalculateSavingAfterMonth(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = (bool)stepContext.Result;
            if(result)
            {
                return await MessageReceivedSavingAfterFinancing(stepContext, cancellationToken);
            }
            else
            {
                return await stepContext.EndDialogAsync();
            }
        }
    }
}

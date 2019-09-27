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
    public class RenovationDialog : ComponentDialog
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

        public RenovationDialog(string dialogId, BotStateService botStateService, BotServices botServices) : base(dialogId)
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
                ReEvaluate
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(RenovationDialog)}.mainFlow", waterfallSteps));
            AddDialog(new WaterfallDialog($"{nameof(RenovationDialog)}.Start", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(RenovationDialog)}.text"));
            AddDialog(new ConfirmPrompt($"{nameof(RenovationDialog)}.confirm"));
            AddDialog(new NumberPrompt<Decimal>($"{nameof(RenovationDialog)}.number"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(RenovationDialog)}.mainFlow";
        }

        public async Task<DialogTurnResult> StartAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(RenovationDialog)}.confirm",
              new PromptOptions
              {
                  Prompt = MessageFactory.Text("Do you know your financing amount?")
              }, cancellationToken);
        }

        private async Task<DialogTurnResult> MessageReceivedFinancingAmountQuery(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            bool result = (bool)stepContext.Result;
            stepContext.Values["projectCosts"] = !result;
            if (result)
            {
                return await stepContext.PromptAsync($"{nameof(RenovationDialog)}.text",
                  new PromptOptions
                  {
                      Prompt = MessageFactory.Text("Please provide the financing amount")
                  }, cancellationToken);
            }
            else
            {
                return await stepContext.PromptAsync($"{nameof(RenovationDialog)}.confirm",
                   new PromptOptions
                   {
                       Prompt = MessageFactory.Text("No Problem, do you know the project costs?")
                   }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> MessageReceivedFinancingAmount(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (Convert.ToBoolean(stepContext.Values["projectCosts"]))
            {
                bool result = (bool)stepContext.Result;
                if (result)
                {
                    return await stepContext.PromptAsync($"{nameof(RenovationDialog)}.number",
                           new PromptOptions
                           {
                               Prompt = MessageFactory.Text("Please provide the project cost:")
                           }, cancellationToken);
                }
                else
                {
                    return await stepContext.EndDialogAsync();
                }
            }
            else
            {
                var message = (string)stepContext.Result;
                double.TryParse(message, out finAmount);
                return await stepContext.PromptAsync($"{nameof(RenovationDialog)}.number",
                       new PromptOptions
                       {
                           Prompt = MessageFactory.Text("Please provide the duration in years")
                       }, cancellationToken);
            }
        }
        private async Task<DialogTurnResult> MessageReceivedDuration(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            object projectCosts;
            stepContext.Values.TryGetValue("projectCosts", out projectCosts);
            if (Convert.ToBoolean(projectCosts))
            {
                var message = (decimal)stepContext.Result;
                if (!string.IsNullOrEmpty(message.ToString()))
                {
                    double.TryParse(message.ToString(), out projectCost);
                    return await stepContext.PromptAsync($"{nameof(RenovationDialog)}.number",
                                         new PromptOptions
                                         {
                                             Prompt = MessageFactory.Text("Please provide the amount invested by you (Own Resources):")
                                         }, cancellationToken);
                }
                else
                {
                    this.projectCost = 0;
                    return await stepContext.EndDialogAsync();
                }
            }
            else
            {
                var message = (decimal)stepContext.Result;

                numberOfYears = Convert.ToInt32(message);//  int.Parse(message, out numberOfYears);


                var emi = Calculations.CalculateEMI(finAmount, numberOfYears * 12);
                this.EMI = emi;
                await stepContext.Context.SendActivityAsync($"Your monthly installment is: {emi} €");

                return await stepContext.PromptAsync($"{nameof(RenovationDialog)}.confirm",
                       new PromptOptions
                       {
                           Prompt = MessageFactory.Text("Do you wish to re-evaluate your installments?")
                       }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> MessageReceivedReEvaluate(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            object projectCosts;
            stepContext.Values.TryGetValue("projectCosts", out projectCosts);
            if (Convert.ToBoolean(projectCosts))
            {
                var message = (decimal)stepContext.Result;
                if (!string.IsNullOrEmpty(message.ToString()))
                {
                    double.TryParse(message.ToString(), out this.ownResources);
                    return await stepContext.PromptAsync($"{nameof(RenovationDialog)}.text",
                        new PromptOptions
                        {
                            Prompt = MessageFactory.Text("Please provide the amount of housing subsidies:")
                        }, cancellationToken);
                }
                else
                {
                    this.ownResources = 0;
                    return await stepContext.EndDialogAsync();
                }
            }
            else
            {
                var result = (bool)stepContext.Result;
                if (result)
                {
                    return await StartAsync(stepContext, cancellationToken);
                }
                else
                {
                    return await stepContext.PromptAsync($"{nameof(RenovationDialog)}.confirm",
                         new PromptOptions
                         {
                             Prompt = MessageFactory.Text("Do you wish to calculate your saving after fanancing every month?")
                         }, cancellationToken);
                }
            }
        }

        private async Task<DialogTurnResult> MessageReceivedSavingAfterFinancing(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            object projectCosts;
            stepContext.Values.TryGetValue("projectCosts", out projectCosts);
            if (Convert.ToBoolean(projectCosts))
            {
                var message = (string)stepContext.Result;

                if (!string.IsNullOrEmpty(message))
                {
                    double.TryParse(message, out this.housingSubsidies);

                    var financingAmount = this.projectCost - (this.ownResources + this.housingSubsidies);
                    this.finAmount = financingAmount;
                    await stepContext.Context.SendActivityAsync($"Financing Amount for your project is: {financingAmount}  €");
                    stepContext.Values["financingAmount"] = true;
                    return await stepContext.PromptAsync($"{nameof(RenovationDialog)}.confirm",
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
                var result = (bool)stepContext.Result;
                if (result)
                {
                    return await stepContext.PromptAsync($"{nameof(RenovationDialog)}.text",
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
            object financingAmount;
            stepContext.Values.TryGetValue("financingAmount",out financingAmount);
            if (Convert.ToBoolean(financingAmount))
            {
                bool result = (bool)stepContext.Result;
                if (result)
                {
                    return await stepContext.PromptAsync($"{nameof(RenovationDialog)}.number",
                        new PromptOptions
                        {
                            Prompt = MessageFactory.Text("Please provide the duration in years")
                        }, cancellationToken);
                }
                else
                {
                    return await stepContext.ContinueDialogAsync();
                }
            }
            else
            {
                var message = (string)stepContext.Result;
                double.TryParse(message, out totalMonthlyIncome);
                return await stepContext.PromptAsync($"{nameof(RenovationDialog)}.text",
                         new PromptOptions
                         {
                             Prompt = MessageFactory.Text("Please provide your total monthly income:")
                         }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> MessageReceivedTotalMonthlyExpenditure(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            object financingAmount;
            stepContext.Values.TryGetValue("financingAmount", out financingAmount);
            if (Convert.ToBoolean(financingAmount))
            {
                var message = (decimal)stepContext.Result;
                int.TryParse(message.ToString(), out numberOfYears);

                var emi = Calculations.CalculateEMI(finAmount, numberOfYears * 12);
                this.EMI = emi;
                await stepContext.Context.SendActivityAsync($"Your monthly installment is: {emi} €");
                return await stepContext.PromptAsync($"{nameof(RenovationDialog)}.confirm",
                        new PromptOptions
                        {
                            Prompt = MessageFactory.Text("Do you wish to re-evaluate your installments?")
                        }, cancellationToken);
            }
            else
            {
                var message = (string)stepContext.Result;
                double.TryParse(message, out totalMonthlyIncome);

                await stepContext.Context.SendActivityAsync($"Month amount available: {this.totalMonthlyIncome - this.totalExpenditure}");
                await stepContext.Context.SendActivityAsync($"After Financing: {this.totalMonthlyIncome - (this.totalExpenditure + this.EMI)}");
                return await stepContext.EndDialogAsync();
            }
        }

        private async Task<DialogTurnResult> ReEvaluate(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = (bool)stepContext.Result;
            if (result)
            {
                return await StartAsync(stepContext, cancellationToken);
            }
            else
            {
                return await stepContext.EndDialogAsync();
            }
        }
    }
}
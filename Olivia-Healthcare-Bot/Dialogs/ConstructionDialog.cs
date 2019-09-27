using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
//using HomeLoanCalculator.Common;
using HomeLoanCalculator.Models;
using System.Text;
using Olivia_Healthcare_Bot.Bots.Services;
using Olivia_Healthcare_Bot.Services;
using System.Threading;
using Microsoft.Bot.Builder;

namespace Olivia_Healthcare_Bot.Dialogs
{
    public class ConstructionDialog : ComponentDialog
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

        //EMIModel emiModel = new EMIModel();
        //AmountAvailable amountAvail = new AmountAvailable();
        //ProjectCosts projCost = new ProjectCosts();

        private double totalExpenditure = 0.0;

        public double purchasePriceOfLand = 0;
        public double buildingCost = 0;
        public double completionCosts = 0;
        public double otherCosts = 0;
        public ConstructionDialog(string dialogId, BotStateService botStateService, BotServices botServices) : base(dialogId)
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
            AddDialog(new WaterfallDialog($"{nameof(ConstructionDialog)}.mainFlow", waterfallSteps));
            AddDialog(new WaterfallDialog($"{nameof(ConstructionDialog)}.Start", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(ConstructionDialog)}.text"));
            AddDialog(new ConfirmPrompt($"{nameof(ConstructionDialog)}.confirm"));
            AddDialog(new NumberPrompt<Decimal>($"{nameof(ConstructionDialog)}.number"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(ConstructionDialog)}.mainFlow";
        }
        
        public async Task<DialogTurnResult> StartAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(ConstructionDialog)}.confirm",
              new PromptOptions
              {
                  Prompt = MessageFactory.Text("Do you know your financing amount?")
              }, cancellationToken);           
        }

        private async Task<DialogTurnResult> MessageReceivedFinancingAmountQuery(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = (bool)stepContext.Result;
            if ( result)
            {
                return await stepContext.PromptAsync($"{nameof(ConstructionDialog)}.number",
                   new PromptOptions
                   {
                       Prompt = MessageFactory.Text("Please provide the financing amount")
                   }, cancellationToken);                
            }
            else
            {
                //project cost dialog start
                return await stepContext.BeginDialogAsync($"{nameof(MainDialog)}.projectCost");
            }
        }

        private async Task<DialogTurnResult> MessageReceivedFinancingAmount(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (decimal)stepContext.Result;
            double finAmount = 0.0;
            double.TryParse(message.ToString(), out finAmount);
            //emiModel.financingAmount = finAmount;
            this.finAmount = finAmount;            
            return await stepContext.PromptAsync($"{nameof(ConstructionDialog)}.number",
                  new PromptOptions
                  {
                      Prompt = MessageFactory.Text("Please provide the duration in years")
                  }, cancellationToken);
        
        }

        private async Task<DialogTurnResult> MessageReceivedDuration(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (decimal)stepContext.Result;
            int numberOfYears = 0;
            int.TryParse(message.ToString(), out numberOfYears);
            //emiModel.NumberOfYears = numberOfYears;
            this.numberOfYears = numberOfYears;

            //var emi = Calculations.CalculateEMI(emiModel.financingAmount, numberOfYears * 12);
            var emi = Calculations.CalculateEMI(this.finAmount, numberOfYears * 12);
            //this.emiModel.MonthlyInstallment = emi;
            this.EMI = emi;
            await stepContext.Context.SendActivityAsync($"Your monthly installment is: {emi} €");

            stepContext.Values["reEvaluateInstallments"] = true;

            return await stepContext.PromptAsync($"{nameof(ConstructionDialog)}.confirm",
                  new PromptOptions
                  {
                      Prompt = MessageFactory.Text("Do you wish to re-evaluate your installments?")
                  }, cancellationToken);

        }

        private async Task<DialogTurnResult> MessageReceivedReEvaluate(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = (bool)stepContext.Result;
            if (result)
            {
             return  await StartAsync(stepContext,cancellationToken);
            }
            else
            {
                stepContext.Values["savingEveryMonth"] = true;
                return await stepContext.PromptAsync($"{nameof(ConstructionDialog)}.confirm",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Do you wish to calculate your saving after fanancing every month?")
                }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> MessageReceivedSavingAfterFinancing(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = (bool)stepContext.Result;
            if (result)
            {
                return await stepContext.PromptAsync($"{nameof(ConstructionDialog)}.number",
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

        private async Task<DialogTurnResult> MessageReceivedTotalMonthlyIncome(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = (decimal)stepContext.Result;
            var message =  result;
            var totalMonthlyIncome = 0.0;
            double.TryParse(message.ToString(), out totalMonthlyIncome);
            this.totalMonthlyIncome = totalMonthlyIncome;
            return await stepContext.PromptAsync($"{nameof(ConstructionDialog)}.number",
                   new PromptOptions
                   {
                       Prompt = MessageFactory.Text("Please provide your monthly expenditure:")
                   }, cancellationToken);
        }

        private async Task<DialogTurnResult> MessageReceivedTotalMonthlyExpenditure(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (decimal)stepContext.Result;
            var totalMonthlyExpenditure = 0.0;
            double.TryParse(message.ToString(), out totalMonthlyExpenditure);
            //this.amountAvail.TotalIncome = totalMonthlyIncome;
            this.totalExpenditure = totalMonthlyExpenditure;

            //await context.PostAsync($"Month amount available: {this.amountAvail.TotalIncome- this.amountAvail.TotalExpenditure}");
            await stepContext.Context.SendActivityAsync($"Month amount available: {this.totalMonthlyIncome - this.totalExpenditure}");
            //await context.PostAsync($"After Financing: {this.amountAvail.TotalIncome - (this.amountAvail.TotalExpenditure + this.emiModel.MonthlyInstallment)}");
            await stepContext.Context.SendActivityAsync($"After Financing: {this.totalMonthlyIncome - (this.totalExpenditure + this.EMI)}");
            return await stepContext.EndDialogAsync();
        }

       
    }
}

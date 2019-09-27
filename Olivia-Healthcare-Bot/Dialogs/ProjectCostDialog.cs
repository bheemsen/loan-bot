using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
//using HomeLoanCalculator.Common;
using System.Text;
using Olivia_Healthcare_Bot.Bots.Services;
using Olivia_Healthcare_Bot.Services;
using Microsoft.Bot.Builder;
using System.Threading;

namespace Olivia_Healthcare_Bot.Dialogs
{
    public class ProjectCostDialog : ComponentDialog
    {
        private double purchasePriceOfLand = 0;
        private double buildingCost = 0;
        private double completionCosts = 0;
        private double otherCosts = 0;
        private double projectCost = 0.0;

        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;
        public ProjectCostDialog(string dialogId, BotStateService botStateService, BotServices botServices) : base(dialogId)
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
                MessageReceivedProjectCostQuery,
                MessageReceivedProjectCost,
                MessageReceivedPurchasePriceOfLand,
                MessageReceivedBuildingcost,
                MessageReceivedCompletioncosts,
                MessageReceivedOthercosts
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(ProjectCostDialog)}.mainFlow", waterfallSteps));
            AddDialog(new WaterfallDialog($"{nameof(ProjectCostDialog)}.Start", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(ProjectCostDialog)}.text"));
            AddDialog(new ConfirmPrompt($"{nameof(ProjectCostDialog)}.confirm"));
            AddDialog(new NumberPrompt<Decimal>($"{nameof(ProjectCostDialog)}.number"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(ProjectCostDialog)}.mainFlow";
        }

        public async Task<DialogTurnResult> StartAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(ProjectCostDialog)}.confirm",
              new PromptOptions
              {
                  Prompt = MessageFactory.Text("No Problem, do you know the project costs?")
              }, cancellationToken);
        }

        private async Task<DialogTurnResult> MessageReceivedProjectCostQuery(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = (bool)stepContext.Result;
            if (result)
            {
                stepContext.Values["determineValue"] = false;
                return await stepContext.PromptAsync($"{nameof(ProjectCostDialog)}.number",
                 new PromptOptions
                 {
                     Prompt = MessageFactory.Text("Please provide Project cost:")
                 }, cancellationToken);
            }
            else
            {
                stepContext.Values["determineValue"] = true;
                return await stepContext.PromptAsync($"{nameof(ProjectCostDialog)}.confirm",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("No Problem, do you want to determine its value?")
                    }, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> MessageReceivedProjectCost(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (Convert.ToBoolean(stepContext.Values["determineValue"]))
            {
                var result = (bool)stepContext.Result;
                if (result)
                {
                    await stepContext.Context.SendActivityAsync("Enter your project cost here:");
                    return await stepContext.PromptAsync($"{nameof(ProjectCostDialog)}.number",
                   new PromptOptions
                   {
                       Prompt = MessageFactory.Text("Please provide purchase price of the land.")
                   }, cancellationToken);
                }
                else
                {
                    return await stepContext.ContinueDialogAsync();
                }
            }
            else
            {
                var message = (decimal)stepContext.Result;
                if (!string.IsNullOrEmpty(message.ToString()))
                {
                    double projectCost = 0.0;
                    double.TryParse(message.ToString(), out projectCost);
                    //emiModel.ProjectCosts = projectCost;
                    this.projectCost = projectCost;
                }
                else
                {
                    this.projectCost = 0;
                }
                return await stepContext.EndDialogAsync(this.projectCost.ToString());
            }
        }

        private async Task<DialogTurnResult> MessageReceivedPurchasePriceOfLand(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (decimal)stepContext.Result;
            double purchasePriceOfLand = 0.0;
            double.TryParse(message.ToString(), out purchasePriceOfLand);
            //this.projCost.PurchasePriceOfLand = purchasePriceOfLand;
            this.purchasePriceOfLand = purchasePriceOfLand;
            return await stepContext.PromptAsync($"{nameof(ProjectCostDialog)}.number",
                   new PromptOptions
                   {
                       Prompt = MessageFactory.Text("Please provide building costs?")
                   }, cancellationToken);

        }

        private async Task<DialogTurnResult> MessageReceivedBuildingcost(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (decimal)stepContext.Result;
            var buildingCost = 0.0;
            double.TryParse(message.ToString(), out buildingCost);
            //this.projCost.BuildingCosts = buildingCost;
            this.buildingCost = buildingCost;
            return await stepContext.PromptAsync($"{nameof(ProjectCostDialog)}.number",
                  new PromptOptions
                  {
                      Prompt = MessageFactory.Text("Please provide completion costs?")
                  }, cancellationToken);

        }

        private async Task<DialogTurnResult> MessageReceivedCompletioncosts(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (decimal)stepContext.Result;
            var completionCosts = 0.0;
            double.TryParse(message.ToString(), out completionCosts);
            this.completionCosts = completionCosts;
            return await stepContext.PromptAsync($"{nameof(ProjectCostDialog)}.number",
                 new PromptOptions
                 {
                     Prompt = MessageFactory.Text("Please provide other costs")
                 }, cancellationToken);

        }

        private async Task<DialogTurnResult> MessageReceivedOthercosts(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (decimal)stepContext.Result;
            var otherCosts = 0.0;
            double.TryParse(message.ToString(), out otherCosts);
            //this.projCost.OthrCosts = otherCosts;

            this.otherCosts = otherCosts;

            double purchaseCost = Calculations.CalculatePurchaseCost(this.purchasePriceOfLand);
            double realEstateTax = Calculations.CalculateRealEstateTax(this.purchasePriceOfLand);
            double entryPropertyRights = Calculations.CalculateEntryPropertyRights(this.purchasePriceOfLand);
            double brokerageCommission = Calculations.CalculateBrockerageCommission(this.purchasePriceOfLand);
            StringBuilder sb = new StringBuilder();
            sb.Append($"Cost dependent on purchase price: {System.Environment.NewLine} Purchase costs: {purchaseCost} {System.Environment.NewLine}");
            sb.Append($"Real Estate Tax: { realEstateTax} {System.Environment.NewLine}");
            sb.Append($"Entry Property Rights: { entryPropertyRights} {System.Environment.NewLine}");
            sb.Append($"BrokerageCommision: { brokerageCommission } {System.Environment.NewLine}");

            this.projectCost = this.purchasePriceOfLand + this.buildingCost + this.completionCosts + this.otherCosts + purchaseCost + realEstateTax + entryPropertyRights + brokerageCommission;
            //sb.Append($"Your Project Cost is: {this.projectCost}");
            //var purchaseCost = Calculations.CalculatePurchaseCost(this.projCost.PurchasePriceOfLand);
            //var realEstateTax = cal
            await stepContext.Context.SendActivityAsync(sb.ToString());
            await stepContext.Context.SendActivityAsync($"Your Project Cost is: { this.projectCost}");
            return await stepContext.EndDialogAsync(this.projectCost);
        }
    }
}

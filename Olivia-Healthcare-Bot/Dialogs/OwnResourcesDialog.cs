using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Olivia_Healthcare_Bot.Bots.Services;
using Olivia_Healthcare_Bot.Services;
using Microsoft.Bot.Builder;
using System.Threading;

namespace Olivia_Healthcare_Bot.Dialogs
{
    public class OwnResourcesDialog : ComponentDialog
    {
        private double savings;
        private double securities;
        private double proceedsFromLIC;
        private double proceedsFromPropeertySale;
        private double repaymentSubsidizedConstructionDeposits;
        private double others;
        private double ownResources;

        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;
        private WaterfallStep[] waterfallSteps;
        public OwnResourcesDialog(string dialogId, BotStateService botStateService, BotServices botServices) : base(dialogId)
        {
            _botStateService = botStateService ?? throw new System.ArgumentNullException(nameof(botStateService));
            _botServices = botServices ?? throw new System.ArgumentNullException(nameof(botServices));
            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            // Create Waterfall Steps
            this.waterfallSteps = new WaterfallStep[]
            {
                StartAsync,
                MessageReceivedOwnResourceQuery,
                MessageReceivedOwnResources,
                MessageReceivedSavings,
                MessageReceivedSecurities,
                MessageReceivedProceedsFromLIC,
                MessageReceivedProceedsFromPropertySale,
                MessageReceivedRepaymentSubsidizedConstructionDeposit,
                MessageReceivedOtherExpense
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(OwnResourcesDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(OwnResourcesDialog)}.text"));
            AddDialog(new ConfirmPrompt($"{nameof(OwnResourcesDialog)}.confirm"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(OwnResourcesDialog)}.mainFlow";
        }


        private async Task<DialogTurnResult> StartAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.PromptAsync($"{nameof(OwnResourcesDialog)}.confirm",
              new PromptOptions
              {
                  Prompt = MessageFactory.Text("Do you have amount for Own Resources?")
              }, cancellationToken);

            return await stepContext.ContinueDialogAsync(cancellationToken);
        }
        private async Task<DialogTurnResult> MessageReceivedOwnResourceQuery(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            bool haveOwnResources = (bool)stepContext.Result;
            stepContext.Values["haveOwnResources"] = haveOwnResources;

            if (haveOwnResources)
            {
                stepContext.Values["determineValues"] = false;
                return await stepContext.PromptAsync($"{nameof(OwnResourcesDialog)}.text",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Please provide the amount invested by you (Own Resources):")
                    }, cancellationToken);

            }
            else
            {
              
                stepContext.Values["determineValues"] = true;
                return await stepContext.PromptAsync($"{nameof(OwnResourcesDialog)}.confirm",
                      new PromptOptions
                      {
                          Prompt = MessageFactory.Text("No Problem, do you want to determine its value?:")
                      }, cancellationToken);

            }
        }
        private async Task<DialogTurnResult> MessageReceivedOwnResources(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (Convert.ToBoolean(stepContext.Values["determineValues"]) == false)
            {
                var message = (string)stepContext.Result;
                if (!string.IsNullOrEmpty(message))
                {
                    double _ownResource = 0.0;
                    double.TryParse(message, out _ownResource);
                    //emiModel.OwnResources = _ownResource;
                    this.ownResources = _ownResource;
                }
                else
                {
                    this.ownResources = 0;
                }
                return await stepContext.EndDialogAsync(this.ownResources);
            }
            else
            {
                var result = (bool)stepContext.Result;
                if (result)
                {
                    return await stepContext.PromptAsync($"{nameof(OwnResourcesDialog)}.text",
                    new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Enter your own resource here:")
                    }, cancellationToken);

                    //await stepContext.Context.SendActivityAsync("Please provide Savings");
                    //return await stepContext.ContinueDialogAsync(cancellationToken);
                }
                else
                {
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }
            }
        }
        private async Task<DialogTurnResult> MessageReceivedSavings(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (string)stepContext.Result;
            double savings;
            double.TryParse(message, out savings);

            this.savings = savings;
            
            return await stepContext.PromptAsync($"{nameof(OwnResourcesDialog)}.text",
                   new PromptOptions
                   {
                       Prompt = MessageFactory.Text("Please provide Securities")
                   }, cancellationToken);

        }

        private async Task<DialogTurnResult> MessageReceivedSecurities(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (string)stepContext.Result;
            double securites;
            double.TryParse(message, out securites);
            //this.projCost.PurchasePriceOfLand = savings;
            this.securities = securites;
            return await stepContext.PromptAsync($"{nameof(OwnResourcesDialog)}.text",
                 new PromptOptions
                 {
                     Prompt = MessageFactory.Text("Please provide Proceeds from LIC")
                 }, cancellationToken);
        }

        private async Task<DialogTurnResult> MessageReceivedProceedsFromLIC(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (string)stepContext.Result;
            double proceedsFromLIC;
            double.TryParse(message, out proceedsFromLIC);
            //this.projCost.PurchasePriceOfLand = savings;
            this.proceedsFromLIC = proceedsFromLIC;

            return await stepContext.PromptAsync($"{nameof(OwnResourcesDialog)}.text",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please provide Proceeds from Property Sale")
                }, cancellationToken);

        }

        private async Task<DialogTurnResult> MessageReceivedProceedsFromPropertySale(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (string)stepContext.Result;
            double proceedsFromPropertySale;
            double.TryParse(message, out proceedsFromPropertySale);
            //this.projCost.PurchasePriceOfLand = savings;
            this.proceedsFromPropeertySale = proceedsFromPropertySale;
            return await stepContext.PromptAsync($"{nameof(OwnResourcesDialog)}.text",
               new PromptOptions
               {
                   Prompt = MessageFactory.Text("Please provide Repayment subsidized construction deposit")
               }, cancellationToken);

        }
        private async Task<DialogTurnResult> MessageReceivedRepaymentSubsidizedConstructionDeposit(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (string)stepContext.Result;
            double repaymentSubsidizedConstructionDeposit;
            double.TryParse(message, out repaymentSubsidizedConstructionDeposit);
            //this.projCost.PurchasePriceOfLand = savings;
            this.repaymentSubsidizedConstructionDeposits = repaymentSubsidizedConstructionDeposit;
            return await stepContext.PromptAsync($"{nameof(OwnResourcesDialog)}.text",
              new PromptOptions
              {
                  Prompt = MessageFactory.Text("Please provide other expense")
              }, cancellationToken);
           
        }
        private async Task<DialogTurnResult> MessageReceivedOtherExpense(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var message = (string)stepContext.Result;
            double others;
            double.TryParse(message, out others);
            //this.projCost.PurchasePriceOfLand = savings;
            this.others = others;
            this.ownResources = this.savings + this.securities + this.proceedsFromLIC + this.proceedsFromPropeertySale + this.repaymentSubsidizedConstructionDeposits + this.others;
            await stepContext.Context.SendActivityAsync($"Your Own Resource is: { this.ownResources}");
            return await stepContext.EndDialogAsync(cancellationToken);
        }
    }
}

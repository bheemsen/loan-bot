
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
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
    public class SymptomDiagnoseDialog : ComponentDialog
    {
        #region Variables
        private readonly BotStateService _botStateService;
        private readonly BotServices _botServices;
        #endregion

        public SymptomDiagnoseDialog(string dialogId, BotStateService botStateService, BotServices botServices) : base(dialogId)
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
                BasicSymptomsStepAsync,
                DurationStepAsync,
                ActionResultedToTheSymtomsStepAsync,
                StandardHealthParametersStepAsync,
                FinalStepAsync
                //AppointmentStepAsync
            };

            // Add Named Dialogs
            AddDialog(new WaterfallDialog($"{nameof(SymptomDiagnoseDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(SymptomDiagnoseDialog)}.BasicSymptoms"));
            AddDialog(new TextPrompt($"{nameof(SymptomDiagnoseDialog)}.Duration"));
            AddDialog(new TextPrompt($"{nameof(SymptomDiagnoseDialog)}.ActionResultedToTheSymtoms"));
            AddDialog(new TextPrompt($"{nameof(SymptomDiagnoseDialog)}.StandardHealthParameters"));

            // Set the starting Dialog
            InitialDialogId = $"{nameof(SymptomDiagnoseDialog)}.mainFlow";
        }

        #region Waterfall Steps
        private async Task<DialogTurnResult> BasicSymptomsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(SymptomDiagnoseDialog)}.BasicSymptoms",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Sorry to hear that. What symptoms are bothering you?")
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> DurationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            //return await stepContext.PromptAsync($"{nameof(SymptomDiagnoseDialog)}.Duration",
            //    new PromptOptions
            //    {
            //        Prompt = MessageFactory.Text($"How long (in hours) has this current {(string)stepContext.Values["basicSymptoms"]} been going on?"),
            //        RetryPrompt = MessageFactory.Text("The value entered must be numeric."),
            //    }, cancellationToken);

            var recognizerResult = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            var luisResult = recognizerResult.Properties["luisResult"] as LuisResult;
            var entities = luisResult.Entities;

            // Top intent tell us which cognitive service to use.
            var topIntent = recognizerResult.GetTopScoringIntent();

            var strEntities = string.Empty;

            if (topIntent.intent == "SymptomDiagnose")
            {
                foreach (var entity in entities)
                {
                    strEntities = strEntities == "" ? entity.Entity.ToString() : $"{strEntities}, {entity.Entity.ToString()}";                    
                }                
            }

            stepContext.Values["basicSymptoms"] = strEntities;

            return await stepContext.PromptAsync($"{nameof(SymptomDiagnoseDialog)}.Duration",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text($"How long has this current {strEntities} been going on?"),
                    RetryPrompt = MessageFactory.Text("The value entered must be numeric."),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActionResultedToTheSymtomsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["duration"] = (string)stepContext.Result;
            var promptMessage = string.Empty;

            switch ((string)stepContext.Values["basicSymptoms"])
            {
                case "headache":
                    promptMessage = "Did you had coffee, energy drink or some sort of soda in last few hours";
                    break;
                case "cold":
                    promptMessage = "Did you had cold drink, ice cream or some sort of cold eatables in last few hours";
                    break;
                case "cold, fever":
                    promptMessage = "Did you had cold drink, ice cream or some sort of cold eatables in last few hours";
                    break;
                default:
                    break;
            }
            

            return await stepContext.PromptAsync($"{nameof(SymptomDiagnoseDialog)}.ActionResultedToTheSymtoms",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text(promptMessage),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Yes", "No" }),
                }, cancellationToken);
        }

        private async Task<DialogTurnResult> StandardHealthParametersStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["ActionResultedToTheSymtoms"] = (string)stepContext.Result;
            var promptMessage = string.Empty;
            try
            {
                switch ((string)stepContext.Values["basicSymptoms"])
                {
                    case "headache":
                        if ((string)stepContext.Result.ToString().ToLower() == "yes")
                        {
                            promptMessage = "Allright, nothing much to worry about. Based on the information received from your registered device and facial expression you looks fine. You can have a dose of Panadol Advance 500 mg tablet";
                        }
                        else
                        {
                            promptMessage = "Allright, nothing much to worry about. Based on the information received from your registered device and facial expression you looks fine. You can take some rest for a while";
                        }

                        //await stepContext.Context.SendActivityAsync(MessageFactory.Text(promptMessage), cancellationToken);
                        //return await stepContext.NextAsync(null, cancellationToken);
                        break;
                    case "cold":
                        if ((string)stepContext.Result == "yes")
                        {
                            promptMessage = "Allright, nothing much to worry about. Based on the information received from your registered device and facial expression you looks fine. You can have a dose of Theraflu tablet and take proper rest";
                        }
                        else
                        {
                            promptMessage = "Allright, based on the information received from your registered device and facial expression you looks fine. You can take steam twice a day and a dose of Theraflu";
                        }

                        break;
                    case "cold, fever":
                        if ((string)stepContext.Result == "yes")
                        {
                            promptMessage = "Ok, Based on the information received from your registered device and facial expression I would recomend you to visit a doctor. Would you like me to book an appointment with your regular doctor at Medanta Hospital for today?";
                        }
                        else
                        {
                            promptMessage = "Allright, Based on the information received from your registered device and facial expression I would recomend you to visit a doctor. Would you like me to book an appointment with your regular doctor at Medanta Hospital for today?";
                        }
                        break;
                    default:
                        break;
                }
            }
            catch(Exception ex)
            {

            }
            return await stepContext.PromptAsync($"{nameof(SymptomDiagnoseDialog)}.Duration",
                new PromptOptions
                {
                    Prompt = MessageFactory.Text(promptMessage)
                    
                }, cancellationToken);
        }
        #endregion

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //stepContext.Values["StandardHealthParameters"] = (string)stepContext.Result;
            if ((string)stepContext.Values["basicSymptoms"] == "headache" || (string)stepContext.Values["basicSymptoms"] == "cold")
            {
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                if ((string)stepContext.Result == "yes")
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("sure, your appointment with Dr. Nitin at 4 PM today. Get well soon."), cancellationToken);
                }
                else
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("No problem, Take care and get well soon."), cancellationToken);
                }

                return await stepContext.EndDialogAsync(null, cancellationToken);

            }
            
        }
    }
}

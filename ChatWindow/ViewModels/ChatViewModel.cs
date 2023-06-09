﻿using ChatWindow.Helper;
using ChatWindow.Models;
using ChatWindow.Models.Translation;
using ChatWindow.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.Media.SpeechRecognition;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using WinRT;
using static ChatWindow.Helper.ApiHelper;

namespace ChatWindow.ViewModels {
    public partial class ChatViewModel : ObservableObject {
        public ChatViewModel() {
            this.dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            this.speechRecognizer = new SpeechRecognizer();
        }
        private SpeechRecognizer speechRecognizer;
        private StringBuilder dictatedTextBuilder;
        private Microsoft.UI.Dispatching.DispatcherQueue dispatcher;

        [ObservableProperty]
        private ObservableCollection<UIMessage> messagesList = new ObservableCollection<UIMessage>();
        [ObservableProperty]
        private string userText;
        [RelayCommand]
        private async void OnMessageSend() {
            OnSendAsync();

        }

     

        [RelayCommand]
        private void OnMessageEnterSend(KeyRoutedEventArgs args) {
            if (args.Key == Windows.System.VirtualKey.Enter) {
                OnSendAsync();
            }
        }
        private ApowersoftResultData lastMessage = null;

        private async Task OnSendAsync() {
            if (!string.IsNullOrEmpty(userText)) {
                var text = userText;
                UserText = "";
                ApiHelper apiHelper = new ApiHelper();
                UIMessage myMessage = new UIMessage();
                myMessage.Message = text;
                myMessage.From = MessageType.Input;
                MessagesList.Add(myMessage);
                myMessage.ProgressIsVisibility = Visibility.Visible;
                UIMessage AiMessage = new UIMessage();
                AiMessage.From = MessageType.AiInput;
                MessagesList.Add(AiMessage);


                var result = await apiHelper.GetStreamResult(text, (appendText) => {
                    dispatcher.TryEnqueue(() => {
                        AiMessage.Message += appendText;
                    });
                }, lastData: lastMessage);
                myMessage.ProgressIsVisibility = Visibility.Collapsed;
                //myMessage.ProgressIsActive = false;

                AiMessage.Message = result.text;
                lastMessage = result;
            }
        }

        [RelayCommand]
        private async Task SpeechRecognitionAsync() {
            string Result = "";
            try {
                #region 第一种语音输入
                //using (SpeechRecognizer recognizer = new SpeechRecognizer()) {
                //    SpeechRecognitionCompilationResult compilationResult = await recognizer.CompileConstraintsAsync();
                //    recognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
                //    if (compilationResult.Status == SpeechRecognitionResultStatus.Success) {

                //        recognizer.UIOptions.IsReadBackEnabled = false;
                //        recognizer.UIOptions.ShowConfirmation = false;
                //        recognizer.UIOptions.AudiblePrompt = "我在听,请说...";
                //        SpeechRecognitionResult recognitionResult = await recognizer.RecognizeWithUIAsync();
                //        //SpeechRecognitionResult recognitionResult = await recognizer.RecognizeAsync();
                //        if (recognitionResult.Status == SpeechRecognitionResultStatus.Success) {
                //            Result = recognitionResult.Text;
                //        }
                //    }

                //}
                #endregion

                #region 第二种方式
                SpeechRecognitionCompilationResult result =
                await speechRecognizer.CompileConstraintsAsync();
                speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
                speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
                if (speechRecognizer.State == SpeechRecognizerState.Idle) {
                    await speechRecognizer.ContinuousRecognitionSession.StartAsync();

                }

                #endregion
            } catch (Exception ex) {
                Result = ex.Message;
            }
            UserText = Result;
        }

        private void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args) {
            throw new NotImplementedException();
        }

        private void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args) {
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium || args.Result.Confidence == SpeechRecognitionConfidence.High) {
                dictatedTextBuilder.Append(args.Result.Text + " ");
                UserText = dictatedTextBuilder.ToString();
                //btnClearText.IsEnabled = true;
            } else {
                UserText = dictatedTextBuilder.ToString();

            }
        }



    }
}

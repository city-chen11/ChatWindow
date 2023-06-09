﻿using ChatWindow.Helper;
using ChatWindow.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatWindow.ViewModels {
    public partial class HomeViewModel : ObservableObject {
        [ObservableProperty]
        private ObservableCollection<UIMessage> messagesList = new ObservableCollection<UIMessage>();
        [ObservableProperty]
        private string userText;
        [RelayCommand]
        private async void OnMessageSend() {
            var text = userText;
            UserText = "";
            ApiHelper apiHelper = new ApiHelper();
            UIMessage myMessage = new UIMessage();
            myMessage.Message = text;
            myMessage.From = MessageType.Input;
            MessagesList.Add(myMessage);
            UIMessage AiMessage = new UIMessage();
            AiMessage.From = MessageType.AiInput;
            MessagesList.Add(AiMessage);
            var result = await apiHelper.GetStreamResult(text, new Action<string>((e) => { AiMessage.Message += e; }));
            AiMessage.Message = result.text;
        }
    }
}

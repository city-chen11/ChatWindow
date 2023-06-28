﻿using Microsoft.UI.Xaml.Shapes;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatWindow.Helper {
    public class ApiHelper {
        public class ApowersoftTaskData {
            public string task_id { get; set; }
        }

        public class ApowersoftTask {
            public int status { get; set; }
            public string message { get; set; }
            public ApowersoftTaskData data { get; set; }
        }
        public class ApowersoftResult {
            public int status { get; set; }
            public ApowersoftResultData data { get; set; }
        }
        public class ApowersoftResultData {
            public string text { get; set; }
            public string conversation_id { get; set; }
            public string msg_id { get; set; }
            public int state { get; set; }
            public string task_id { get; set; }
        }


        public ApiHelper() {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
        }

        private string url = "https://aw.aoscdn.com/tech/tasks/gpt/completion/";
        private string apiKey = "wxo20k7zpw293rjbr";
        private HttpClient client;

        public async Task<ApowersoftResultData> GetResults(string inputText, ApowersoftResultData lastData = null) {
            var result = new ApowersoftResultData();

            var form = new MultipartFormDataContent();
            form.Add(new StringContent(inputText), "prompt");
            form.Add(new StringContent("1"), "sync");
            if (lastData != null) {
                form.Add(new StringContent(lastData.conversation_id), "conversation_id");
                form.Add(new StringContent(lastData.msg_id), "prev_msg_id");
            }
            try {
                var response = await client.PostAsync(url, form);
                var resultResponse = await response.Content.ReadFromJsonAsync<ApowersoftResult>();
                if (resultResponse?.data != null) {
                    result = resultResponse.data;
                }
            } catch (System.Exception) {
                result.text = "暂时没有思绪呢~ ε=ε=ε=ε=ε=ε=┌(;￣◇￣)┘  (｡￫‿￩｡) （๑￫‿ฺ￩๑）（=ˇωˇ=）（⺻▽⺻ ）<(￣︶￣)>(•‾̑⌣‾̑•)✧˖° (๑˘ ˘๑)  ♥(｡￫v￩｡)♥";
            }
            return result;
        }

        public async Task<ApowersoftResultData> GetStreamResult(string inputText, Action<string> append, ApowersoftResultData lastData = null) {
            var result = new ApowersoftResultData();

            var taskId = await CreateTask(inputText, lastData);
            if (string.IsNullOrEmpty(taskId)) {
                result.text = "暂时没有思绪呢~ ε=ε=ε=ε=ε=ε=┌(;￣◇￣)┘  (｡￫‿￩｡) （๑￫‿ฺ￩๑）（=ˇωˇ=）（⺻▽⺻ ）<(￣︶￣)>(•‾̑⌣‾̑•)✧˖° (๑˘ ˘๑)  ♥(｡￫v￩｡)♥";
            }
            var streamReader = await QueryTask(taskId);

            while (true && streamReader != null) {
                var line = await streamReader.ReadLineAsync();
                if (line == null) break;

                // 解析 SSE 消息内容
                if (line.StartsWith("data: ")) {
                    var data = line.Substring("data: ".Length).Trim();
                    if (data.EndsWith("}")) {
                        result = JsonSerializer.Deserialize<ApowersoftResultData>(data);
                    } else {
                        append?.Invoke(data);
                    }
                }
                await Task.Delay(10);
            }
            return result;
        }

        private async Task<string> CreateTask(string inputText, ApowersoftResultData lastData = null) {
            var form = new MultipartFormDataContent();
            form.Add(new StringContent(inputText), "prompt");
            form.Add(new StringContent("1"), "response_type");
            if (lastData != null) {
                form.Add(new StringContent(lastData.conversation_id), "conversation_id");
                form.Add(new StringContent(lastData.msg_id), "prev_msg_id");
            }
            try {
                var response = await client.PostAsync(url, form);
                var result = await response.Content.ReadFromJsonAsync<ApowersoftTask>();
                return result.data.task_id;
            } catch (System.Exception) {
                return string.Empty;
            }
        }

        private async Task<StreamReader> QueryTask(string id) {
            var request = new HttpRequestMessage(HttpMethod.Get, url + id) {
                Headers = {
                    {"Accept", "text/event-stream" }
                }
            };
            try {
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                var stream = response.Content.ReadAsStream();
                var reader = new StreamReader(stream);
                return reader;
            } catch (Exception ex) {
                return null;
            }
        }
    }
}

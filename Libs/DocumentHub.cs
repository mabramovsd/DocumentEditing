using DocumentEditing.Models.Messages;
using DocumentEditing.Repositories;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace DocumentEditing.Libs
{
    /// <summary>
    /// Class for WebSocket connection
    /// </summary>
    public class DocumentHub : Hub
    {
        private readonly IDocumentSessionRepository _repository;
        public DocumentHub(IDocumentSessionRepository repository) 
        {
            _repository = repository;
        }

        public async Task Send(string message)
        {
            try
            {
                // Case insensitive paramaters (fileName = FileName)
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                //Unserialize incoming message
                var incomingMsg = JsonSerializer.Deserialize<IncomingMessage>(message, options);

                if (incomingMsg != null && !string.IsNullOrEmpty(incomingMsg.FileName) && !string.IsNullOrEmpty(incomingMsg.User))
                {
                    _repository.AddEditor(incomingMsg.FileName, incomingMsg.User);

                    // 3. Создаем объект для отправки на клиент
                    var outgoingMsg = new OutgoingMessage
                    {
                        FileName = incomingMsg.FileName,
                        User = incomingMsg.User,
                        ActiveEditors = _repository.GetEditors(incomingMsg.FileName)
                    };

                    string updatedMessage = JsonSerializer.Serialize(outgoingMsg);

                    //Send to all clients
                    await this.Clients.All.SendAsync("Receive", updatedMessage);
                }
                else
                {
                    Console.WriteLine("Входящее сообщение некорректно или пусто.");
                    // Опционально: отправить исходное сообщение без изменений
                    await this.Clients.All.SendAsync("Receive", message);
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine("Ошибка при разборе JSON: " + ex.Message);
                // В случае ошибки парсинга можно отправить исходную строку или заглушку
                await this.Clients.All.SendAsync("Receive", message);
            }
        }
    }
}
//ToDo Подумать про удаление людей если чел закрыл вкладку браузера (тут, блин, надо по всему словарю бегать...но это аутофскоуп)
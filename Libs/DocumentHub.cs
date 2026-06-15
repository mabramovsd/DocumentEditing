using DocumentEditing.Models.Messages;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace DocumentEditing.Libs
{
    /// <summary>
    /// Class for WebSocket connection
    /// </summary>
    public class DocumentHub : Hub
    {
        public async Task Send(string message)
        {
            try
            {
                // Создаем экземпляр настроек
                var options = new JsonSerializerOptions
                {
                    // Делаем десериализацию нечувствительной к регистру
                    PropertyNameCaseInsensitive = true
                };

                // Используем эти настройки при вызове Deserialize
                // 1. Десериализуем входящее сообщение в объект
                var incomingMsg = JsonSerializer.Deserialize<IncomingMessage>(message, options);

                if (incomingMsg != null && !string.IsNullOrEmpty(incomingMsg.FileName) && !string.IsNullOrEmpty(incomingMsg.User))
                {
                    // 2. Обновляем Global.ActiveDocuments (ваш текущий код)
                    if (!Global.ActiveDocuments.ContainsKey(incomingMsg.FileName))
                    {
                        Global.ActiveDocuments.Add(incomingMsg.FileName, new List<string> { incomingMsg.User });
                    }
                    else if (!Global.ActiveDocuments[incomingMsg.FileName].Contains(incomingMsg.User))
                    {
                        Global.ActiveDocuments[incomingMsg.FileName].Add(incomingMsg.User);
                    }

                    // 3. Создаем объект для отправки на клиент
                    var outgoingMsg = new OutgoingMessage
                    {
                        FileName = incomingMsg.FileName,
                        User = incomingMsg.User,
                        // Передаем актуальное состояние словаря
                        ActiveDocuments = Global.ActiveDocuments
                    };

                    // 4. Сериализуем объект в JSON-строку
                    string updatedMessage = JsonSerializer.Serialize(outgoingMsg);

                    // 5. Отправляем новую строку всем клиентам
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
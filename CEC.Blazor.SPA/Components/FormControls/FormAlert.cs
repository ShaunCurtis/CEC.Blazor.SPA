using System;
using CEC.Blazor.SPA.Components.UIControls;
using CEC.Blazor.Data;

namespace CEC.Blazor.SPA.Components.Forms
{
    public class FormAlert
    {
        public string Message { get; set; } = string.Empty;

        public bool IsAlert => !string.IsNullOrWhiteSpace(this.Message);

        public MessageType FormMessageType { get; set; } = MessageType.None;

        public void SetAlert(string message, MessageType type)
        {
            this.Message = message;
            this.FormMessageType = type;
        }

        public void SetAlert(DbTaskResult result)
        {
            this.Message = result.Message;
            this.FormMessageType = result.Type;
        }

        public void ClearAlert()
        {
            this.Message = string.Empty;
            this.FormMessageType = MessageType.None;
        }

        public string GetCode()
            => (FormMessageType switch
            {
                MessageType.None => "alert-info",
                MessageType.NotImplemented => "alert-info",
                _ => $"alert-{(Enum.GetName(typeof(MessageType), FormMessageType)).ToLower()}"
            }
        );
          
    }
}

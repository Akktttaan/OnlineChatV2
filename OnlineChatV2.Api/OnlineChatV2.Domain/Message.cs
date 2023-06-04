using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using OnlineChatV2.Domain.Enums;

namespace OnlineChatV2.Domain;

[Index(nameof(ChatId), Name = "IX_Chats")]
[Index(nameof(ToUserId), Name = "IX_PrivateChats")]
[Index(nameof(FromUserId), Name = "IX_Sender")]
public class Message
{

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    /// <summary>
    /// Из какого чата сообщение - null если сообщение из лички
    /// </summary>
    public long? ChatId { get; set; }
    public Chat Chat { get; set; }
    
    /// <summary>
    /// Для кого сообщение - null если сообщение из чата
    /// </summary>
    public long? ToUserId { get; set; }
    [InverseProperty("IncomingMessages")]
    public User ToUser { get; set; }
    
    /// <summary>
    /// От кого сообщение
    /// </summary>
    public long FromUserId { get; set; }
    [InverseProperty("OutgoingMessages")]
    public User FromUser { get; set; }
    
    /// <summary>
    /// Кем прочитано сообщение
    /// </summary>
    public ICollection<ReadMessage> ReadBy { get; set; }

    /// <summary>
    /// Текст сообщения
    /// </summary>
    public string MessageText { get; set; }
    
    /// <summary>
    /// Время отправки
    /// </summary>
    public DateTime MessageDate { get; set; }
    //todo все остальное

    /// <summary>
    /// Тип сообщения (на основе enum Message Type)
    /// </summary>
    public int MessageTypeId { get; set; }

    [EnumDataType(typeof(MessageType))]
    [NotMapped]
    public MessageType MessageType
    {
        get => (MessageType)MessageTypeId;
        set => MessageTypeId = (int)value;
    }
}
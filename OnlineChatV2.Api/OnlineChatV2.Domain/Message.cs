using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineChatV2.Domain;

public class Message
{
    [Key]
    public ulong Id { get; set; }
    
    /// <summary>
    /// Из какого чата сообщение - null если сообщение из лички
    /// </summary>
    public long ChatId { get; set; }
    public Chat Chat { get; set; }
    
    /// <summary>
    /// Для кого сообщение - null если сообщение из чата
    /// </summary>
    public long ToUserId { get; set; }
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
    
    
    //todo все остальное
}
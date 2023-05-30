using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OnlineChatV2.Domain;

public class User
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    
    [JsonIgnore]
    public string Password { get; set; }
    
    [JsonIgnore]
    public ICollection<UserRole> UserRoles { get; set; }
    [JsonIgnore]
    public ICollection<ChatUser> Chats { get; set; }
    [JsonIgnore]
    public ICollection<Message> OutgoingMessages { get; set; }
    [JsonIgnore]
    public ICollection<Message> IncomingMessages { get; set; }
    [JsonIgnore]
    public ICollection<ReadMessage> ReadMessages { get; set; }
    
    /// <summary>
    /// Список контактов юзера
    /// </summary>
    [JsonIgnore]
    [InverseProperty("ContactOwner")]
    public ICollection<UserContact> Contacts { get; set; }
    
    /// <summary>
    /// У кого данный юзер находится в контактах
    /// </summary>
    [JsonIgnore]
    [InverseProperty("Contact")]
    public ICollection<UserContact> InContacts { get; set; }
    
}
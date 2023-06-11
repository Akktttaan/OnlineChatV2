using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OnlineChatV2.Domain.Enums;

namespace OnlineChatV2.Domain;

public class MessageContent
{
    public long Id { get; set; }
    public long MessageId { get; set; }
    public string ContentPath { get; set; }
    public Message Message { get; set; }
    
    /// <summary>
    /// Тип контента
    /// </summary>
    public int ContentTypeId { get; set; }

    [EnumDataType(typeof(ContentType))]
    [NotMapped]
    public ContentType ContentType
    {
        get => (ContentType)ContentTypeId;
        set => ContentTypeId = (int)value;
    }
}
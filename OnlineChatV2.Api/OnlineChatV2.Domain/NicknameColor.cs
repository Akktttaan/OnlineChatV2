using System.ComponentModel.DataAnnotations;

namespace OnlineChatV2.Domain;

public class NicknameColor
{
    [Key]
    public int Id { get; set; }
    public string Hex { get; set; }
}
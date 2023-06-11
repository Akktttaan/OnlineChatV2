namespace OnlineChatV2.WebApi.Models;

public class UploadPhotoDto
{
    public long UserId { get; set; }
    public FileModel Photo { get; set; }
}
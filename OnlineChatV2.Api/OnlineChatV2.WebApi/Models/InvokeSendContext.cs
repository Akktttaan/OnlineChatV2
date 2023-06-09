﻿namespace OnlineChatV2.WebApi.Models;

public class InvokeSendContext
{
    public ChatHistoryModel Message { get; set; }
    public long FromId { get; set; }
    public long ToChatId { get; set; }
    public string GroupId { get; set; }
    public string? AvatarUrl { get; set; }
    public bool NotifyInvoker { get; set; } = false;
}
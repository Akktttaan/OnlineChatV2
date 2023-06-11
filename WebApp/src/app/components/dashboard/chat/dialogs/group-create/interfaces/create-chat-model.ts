import {FormFile} from "./form-file";

export class CreateChatModel{
  createdById: string
  chatUserIds: Array<number>
  chatName: string
  description: string
  avatar: FormFile
}

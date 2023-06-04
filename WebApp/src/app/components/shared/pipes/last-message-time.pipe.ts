import {Pipe} from "@angular/core";

@Pipe({
  name: 'lastMessageTime'
})
export class LastMessageTimePipe {
  transform(value: string | Date): string {
    const date = value instanceof Date ? value : new Date(value);
    const currentDate = new Date((new Date()).getDate() - 24 * 60 * 60 * 1000);
    const lastWeekDate = new Date(currentDate.getTime() - 7 * 24 * 60 * 60 * 1000);

    if (date >= currentDate) {
      return this.formatTime(date);
    } else if (date >= lastWeekDate) {
      return this.formatWeek(date);
    } else {
      return this.formatDate(date);
    }
  }

  private formatTime(date: Date): string {
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${hours}:${minutes}`;
  }

  private formatWeek(date: Date): string {
    const weekdays = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    const dayOfWeek = weekdays[date.getDay()];
    return dayOfWeek;
  }

  private formatDate(date: Date): string {
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();
    return `${day}.${month}.${year}`;
  }
}

import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'duration'
})
export class DurationPipe implements PipeTransform {

  transform(value: number, seconds: boolean): unknown {
    return this.humanizeDuration(value, seconds);
  }

  humanizeDuration(sec_num: number, showSeconds: boolean): string {
    if (sec_num === undefined || sec_num === 0) {
      return '';
    }

    const hours   = Math.floor(sec_num / 3600);
    const minutes = Math.floor((sec_num - (hours * 3600)) / 60);
    const seconds = Math.floor(sec_num - (hours * 3600) - (minutes * 60));

    let hours_s = hours.toString();
    let minutes_s = minutes.toString();
    let seconds_s = seconds.toString();

    if (hours   < 10) { hours_s   = '0' + hours; }
    if (minutes < 10) { minutes_s = '0' + minutes; }
    if (seconds < 10) { seconds_s = '0' + seconds; }
    return hours_s + 'h ' + minutes_s + 'm ' + (showSeconds ? seconds_s + 's' : '');
  }

}

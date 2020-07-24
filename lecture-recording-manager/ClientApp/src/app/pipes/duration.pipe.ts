import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'duration'
})
export class DurationPipe implements PipeTransform {

  transform(value: number, seconds: boolean): unknown {
    return this.humanizeDuration(value, seconds);
  }

  humanizeDuration(secondAsNumber: number, showSeconds: boolean): string {
    if (secondAsNumber === undefined || secondAsNumber === 0) {
      return '';
    }

    const hours   = Math.floor(secondAsNumber / 3600);
    const minutes = Math.floor((secondAsNumber - (hours * 3600)) / 60);
    const seconds = Math.floor(secondAsNumber - (hours * 3600) - (minutes * 60));

    let hoursAsString = hours.toString();
    let minutesAsString = minutes.toString();
    let secondsAsString = seconds.toString();

    if (hours   < 10) { hoursAsString   = '0' + hours; }
    if (minutes < 10) { minutesAsString = '0' + minutes; }
    if (seconds < 10) { secondsAsString = '0' + seconds; }
    return hoursAsString + 'h ' + minutesAsString + 'm ' + (showSeconds ? secondsAsString + 's' : '');
  }

}

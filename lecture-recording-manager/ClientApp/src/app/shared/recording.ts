import { RecordingOutput } from './recording-output';

export class Recording {
  public id?: number;
  public lectureId?: number;

  public type: RecordingType;
  public title: string;
  public description?: string;

  public duration: number;
  public published: boolean;

  public uploadDate?: Date;
  public publishDate?: Date;

  public sorting?: number;

  public filePath: string;

  public outputs: RecordingOutput[];
}

export enum RecordingType {
  UNKNOWN = 0,
  GREEN_SCREEN_RECORDING = 1,
  SIMPLE_RECORDING = 2,
  ZOOM_RECORDING = 3
}

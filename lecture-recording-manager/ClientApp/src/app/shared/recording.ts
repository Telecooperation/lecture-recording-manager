export class Recording {
  public id?: number;
  public lectureId?: number;
  public type: RecordingType;
  public title: string;
  public description?: string;
  public duration: number;
  public published: boolean;
  public status?: RecordingStatus;
  public statusText?: string;
  public uploadDate?: Date;
  public publishDate?: Date;
  public sorting?: number;
  public filePath: string;
}

export enum RecordingStatus {
  UPLOADED = 0,
  PROCESSING = 1,
  PROCESSED = 2,
  SCHEDULED = 3,
  ERROR = -1
}

export enum RecordingType {
  UNKNOWN = 0,
  GREEN_SCREEN_RECORDING = 1
}
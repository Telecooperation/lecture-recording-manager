export class RecordingOutput {
    id: number;
    recordingId: number;
    jobType: string;
    jobConfiguration: string;
    status: RecordingStatus;
    processed: boolean;
    jobError: string;
    dateStarted: Date;
    dateFinished: Date;
}

export enum RecordingStatus {
    UPLOADED = 0,
    PROCESSING = 1,
    PROCESSED = 2,
    SCHEDULED = 3,
    ERROR = -1
}

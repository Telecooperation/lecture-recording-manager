import { Semester } from './semester';

export class Lecture {
  public id: number;
  public shortHand: string;
  public title: string;
  public description?: string;

  public semesterId?: number;
  public semester?: Semester;

  public publish: boolean;
  public active: boolean;
  public renderFullHd: boolean;
  public lastSynchronized?: Date;

  public publishPath: string;
  public sourcePath: string;
  public convertedPath: string;
}

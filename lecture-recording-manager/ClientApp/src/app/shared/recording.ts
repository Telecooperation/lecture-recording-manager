export class Recording {
  public id?: number;
  public title: string;
  public description?: string;
  public duration: number;
  public published: boolean;
  public status?: string;
  public statusText?: string;
  public uploadDate?: Date;
  public publishDate?: Date;
  public sorting?: number;
}

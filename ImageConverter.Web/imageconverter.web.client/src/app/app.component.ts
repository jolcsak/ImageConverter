import { HttpClient } from '@angular/common/http';
import { MatTabsModule } from '@angular/material/tabs';
import { Component, OnInit } from '@angular/core';
import { Data } from '@angular/router';
import { Subscription, interval } from 'rxjs';

interface ImageConverterSummary {
  inputBytes: number;
  outputBytes: number;
  convertedImageCount: number;
  deletedFileCount: number;
  ignoredFileCount: number;
  errorCount: number;
  sumDeleteFileSize: number;
  lastStarted: Date;
  lastFinished: Date;
  nextFire: Date;
  state: string;
  lastStartDate: Date;
  lastFinishDate: Date;
  nextFireDate: Date;
  currentDate: Date;
  jobCount: number;
}

interface JobSummary {
  jobStarted: Date;
  jobFinished: Date;
  inputBytes: number;
  outputBytes: number;
  convertedImageCount: number;
  errorCount: number;
  deletedFileCount: number;
  ignoredFileCount: number;
  sumDeletedFileSize: number;
  state: string;
}

enum ProcessingQueueItemState {
  Compressing,
  Recompressing,
  Deleted
}

interface ProcessingQueueItem {
  state: ProcessingQueueItemState,
  queueItem: QueueItem;
}

interface QueueItem {
  id: number;
  baseDirectory: string;
  fullPath: string;
  state: number;
}

interface Settings {
  serverTime: Date;
  threadCount: number;
  queueLength: number;
}

interface LogMessage {
  type?: string;
  timeStamp?: string;
  message?: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {

  public settings: Settings = {
    serverTime: new Date(),
    threadCount: 0,
    queueLength: 0
  }

  public sum: ImageConverterSummary = {
      inputBytes: 0,
      outputBytes: 0,
      convertedImageCount: 0,
      deletedFileCount: 0,
      ignoredFileCount: 0,
      errorCount: 0,
      sumDeleteFileSize: 0,
      lastStarted: new Date(),
      lastFinished: new Date(),
      nextFire: new Date(),
      state: "not connected",
      lastStartDate: new Date(),
      lastFinishDate: new Date(),
      nextFireDate: new Date(),
      currentDate: new Date(),
      jobCount: 0,
  };

  public jobSum: JobSummary = {
    jobStarted: new Date(),
    jobFinished: new Date(),
    inputBytes: 0,
    outputBytes: 0,
    convertedImageCount: 0,
    errorCount: 0,
    deletedFileCount: 0,
    ignoredFileCount: 0,
    sumDeletedFileSize: 0,
    state: "not connected"
  };

  public isImageConverterJobRunning: boolean = false;
  public isNextFirePresent: boolean = false;
  public processingQueueItems: ProcessingQueueItem[] = [];
  public logMessages: LogMessage[] = [];
  public jobSummaries: JobSummary[] = [];

  private subscription: Subscription = new Subscription();
  
  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.refreshContent();
    this.startPolling();
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  startPolling(): void {
    this.getLogMessages();

    const logPolling = interval(1000).subscribe(() => {
      this.refreshContent();
    });

    this.subscription.add(logPolling);
  }

  private refreshContent() {
    this.getIsImageConverterJobRunning();
    this.getSettings();
    this.getSummaries();
    if (this.isImageConverterJobRunning) {
      this.getLogMessages();
      this.getProcessingQueueItems();
    }

    this.getJobSummaries();
  }

  start() {
    this.http.get<string>('/ImageConverter/StartJob').subscribe(
      (result) => {

      },
      (error) => {
        console.error(error);
      }
    );
  }

  stop() {
    this.http.get<string>('/ImageConverter/StopJob').subscribe(
      (result) => {
       
      },
      (error) => {
        console.error(error);
      }
    );
  }

  getQueueState(state: ProcessingQueueItemState) : string | undefined {
    return ProcessingQueueItemState[state];
  }

  getSettings() {
    this.http.get<Settings>('/ImageConverter/GetSettings').subscribe(
      (result) => {
        this.settings = result;
      },
      (error) => {
        console.error(error);
      }
    );
  }

  getIsImageConverterJobRunning() {
    this.http.get<boolean>('/ImageConverter/IsImageConverterJobRunning').subscribe(
      (result) => {
        this.isImageConverterJobRunning = result;
      },
      (error) => {
        console.error(error);
      }
    );
  }

  getSummaries() {
    this.http.get<ImageConverterSummary>('/ImageConverter/GetImageConverterSummary').subscribe(
      (result) => {
        this.sum = result;
        this.sum.lastStartDate = result.lastStarted;
        this.sum.lastFinishDate = result.lastFinished;
        this.sum.nextFireDate = result.nextFire;
        this.sum.currentDate = new Date();
        this.isNextFirePresent = result.nextFire != null;
      },
      (error) => {
        console.error(error);
      }
    );

    this.http.get<JobSummary>('/ImageConverter/GetJobSummary').subscribe(
      (result) => {
        this.jobSum = result;
      },
      (error) => {
        console.error(error);
      }
    );

  }

  getLogMessages() {
    this.http.get<LogMessage[]>('/ImageConverter/GetLogs').subscribe(
      (result) => {
        this.logMessages = result;
      },
      (error) => {
        console.error(error);
      }
    );
  }

  getProcessingQueueItems() {
    this.http.get<ProcessingQueueItem[]>('/ImageConverter/GetProcessingQueue').subscribe(
      (result) => {
        this.processingQueueItems = result;
      },
      (error) => {
        console.error(error);
      }
    );
  }


  getJobSummaries() {
    this.http.get<JobSummary[]>('/ImageConverter/GetJobSummaries').subscribe(
      (result) => {
        this.jobSummaries = result;
      },
      (error) => {
        console.error(error);
      }
    );
  }

  title = 'ImageConverter Web Client';
}

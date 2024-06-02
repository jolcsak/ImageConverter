import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Data } from '@angular/router';
import { Subscription, interval } from 'rxjs';

interface SumStorage {
  processedBytes: number;
  sumSavedBytes: number;
  convertedImageCount: number;
  deletedFileCount: number;
  sumDeleteFileSize: number;
  lastStarted: number;
  lastFinished: number;
  nextFire: number;
  state: string;
  lastStartDate: Date;
  lastFinishDate: Date;
  nextFireDate: Date;
  currentDate: Date;
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
  public sum: SumStorage = {
      processedBytes: 0,
      sumSavedBytes: 0,
      convertedImageCount: 0,
      deletedFileCount: 0,
      sumDeleteFileSize: 0,
      lastStarted: 0,
      lastFinished: 0,
      nextFire: 0,
      state: "not connected",
      lastStartDate: new Date(),
      lastFinishDate: new Date(),
      nextFireDate: new Date(),
      currentDate: new Date(),
  };

  public isImageConverterJobRunning: boolean = false;
  public isNextFirePresent: boolean = false;
  public logMessages: LogMessage[] = [];

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
    const logPolling = interval(1000).subscribe(() => {
      this.refreshContent();
    });

    this.subscription.add(logPolling);
  }

  private refreshContent() {
    this.getIsImageConverterJobRunning();
    this.getSummaries();
    if (this.isImageConverterJobRunning) {
      this.getLogMessages();
    }
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
    this.http.get<SumStorage>('/ImageConverter/GetSummaries').subscribe(
      (result) => {
        this.sum = result;
        this.sum.lastStartDate = this.netTicksToDate(result.lastStarted);
        this.sum.lastFinishDate = this.netTicksToDate(result.lastFinished);
        this.sum.nextFireDate = this.netTicksToDate(result.nextFire);
        this.sum.currentDate = new Date();
        this.isNextFirePresent = result.nextFire > 0;
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

  netTicksToDate(ticks: number): Date {
    const ticksSinceUnixEpoch = ticks - 621355968000000000;
    const milliseconds = ticksSinceUnixEpoch / 10000;
    return new Date(milliseconds);
  }
    
  title = 'ImageConverter Web Client';
}

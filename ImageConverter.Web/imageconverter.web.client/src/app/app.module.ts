import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { NgxFilesizeModule } from 'ngx-filesize';
import { AppComponent } from './app.component';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule, HttpClientModule, NgxFilesizeModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }

import { Component, inject, OnInit } from '@angular/core';
import { AuthService } from './services/auth.service';
import { ThisReceiver } from '@angular/compiler';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'SZRST.UI';
  

  ngOnInit() {
  }

  }

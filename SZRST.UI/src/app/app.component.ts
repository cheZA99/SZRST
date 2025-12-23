import { Component, inject, OnInit } from '@angular/core';
import { AuthService } from './services/auth.service';
import { ThisReceiver } from '@angular/compiler';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  protected router = inject(Router);
  title = 'SZRST.UI';
  

  ngOnInit() {
  }

  }

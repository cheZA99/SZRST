import { Component, inject, OnInit } from '@angular/core';
import { AuthService } from './services/auth.service';
import { Router } from '@angular/router';
import { animate, style, transition, trigger } from '@angular/animations';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  animations: [
  trigger('routeAnimations', [
    transition('* <=> *', [
      style({ opacity: 0 }),
      animate('200ms ease-in', style({ opacity: 1 }))
    ])
  ])
]
})
export class AppComponent implements OnInit {
  protected router = inject(Router);
  title = 'SZRST.UI';
  

  ngOnInit() {
  }

  }

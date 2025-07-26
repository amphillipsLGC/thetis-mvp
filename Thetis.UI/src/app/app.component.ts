import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  showMenu = false;
  constructor(public auth: AuthService, private router: Router) {}

  logout() {
    this.auth.logout().subscribe(success => {
      if (success) {
        this.router.navigate(['/login']);
      }
      this.showMenu = false;
    });  
  }
}

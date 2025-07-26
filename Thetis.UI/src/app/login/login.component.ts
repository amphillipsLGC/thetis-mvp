
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  username = '';
  password = '';
  error = '';

  constructor(private auth: AuthService, private router: Router) {}

  ngOnInit() {
    this.auth.getUserDetails().subscribe(userSuccess => {
      if (userSuccess) {
        this.router.navigate(['/dashboard']);
      }
    });
  }

  onSubmit() {
    this.auth.login(this.username, this.password).subscribe(success => {
      if (success) {
        this.auth.getUserDetails().subscribe(userSuccess => {
          if (userSuccess) {
            this.router.navigate(['/dashboard']);
          } else {
            this.error = 'Could not load user details.';
          }
        });
      } else {
        this.error = 'Invalid username or password';
      }
    });
  }
}

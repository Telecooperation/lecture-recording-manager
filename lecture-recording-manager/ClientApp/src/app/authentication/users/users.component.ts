import { Component, OnInit } from '@angular/core';
import { AuthenticationService } from 'src/app/services/authentication.service';

@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.scss']
})
export class UsersComponent implements OnInit {
  public users: any[];

  constructor(private authService: AuthenticationService) { }

  ngOnInit(): void {
    this.authService.list().subscribe(x => this.users = x);
  }

  doDelete(user: any): void {
    this.authService.delete(user.userName).subscribe(x => {
      this.ngOnInit();
    });
  }

}

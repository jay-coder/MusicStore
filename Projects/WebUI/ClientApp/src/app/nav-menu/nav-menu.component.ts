import { Component } from '@angular/core';
import { UserProfileService } from '../userprofile/userprofile.service';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent {
  isExpanded = false;
  firstName: string = '';
  lastName: string = '';
  constructor(
    private _userProfileService: UserProfileService
  ) {
    var that = this;
    this._userProfileService.identityClaimsReady.subscribe(function (claims) {
      if (claims) {
        that.firstName = claims["FirstName"];
        that.lastName = claims["LastName"];
      }
    });
  }

  collapse() {
    this.isExpanded = false;
  }

  toggle() {
    this.isExpanded = !this.isExpanded;
  }
}

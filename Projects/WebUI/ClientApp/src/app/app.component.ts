import { Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

import { authConfig } from '../config/auth.config';
import { environment } from '../environments/environment';

import { OAuthService, JwksValidationHandler } from 'angular-oauth2-oidc';
import { UserProfileService } from './userprofile/userprofile.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'app';
  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private _oauthService: OAuthService,
    private _userProfileService: UserProfileService
  ) {
    if (isPlatformBrowser(this.platformId)) {
      this._oauthService.configure(authConfig);
      this._oauthService.tokenValidationHandler = new JwksValidationHandler();
      
      if (!environment.production) {
        this._oauthService.events.subscribe(e => {
          console.log("oauth/oidc event", e);
        });
      }
    }
  }
  /**
     * On init
     */
  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this._oauthService.loadDiscoveryDocumentAndTryLogin().then(_ => {
        if (!this._oauthService.hasValidIdToken() || !this._oauthService.hasValidAccessToken()) {
          this._oauthService.initImplicitFlow();
        } else {
          this._userProfileService.onIdentityClaimsReadyChanged(this._oauthService.getIdentityClaims());
        }
      });
    }
  }
}

import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

import { OAuthService, JwksValidationHandler } from 'angular-oauth2-oidc';
import { authConfig } from '../config/auth.config';
import { environment } from '../environments/environment';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'app';
  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private _oauthService: OAuthService
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

        }
      });
    }
  }
}

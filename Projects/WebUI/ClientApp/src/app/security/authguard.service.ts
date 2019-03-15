import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';

@Injectable()
export class AuthGuardService implements CanActivate {
    constructor(
        private _oauthService: OAuthService,
        private _router: Router
    )
    {
    }

    canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
        var isRoleValid = this._oauthService.hasValidIdToken();
        if (isRoleValid)
        {
            var expectedRole = route.data.expectedRole;
            var claims = this._oauthService.getIdentityClaims();
            if (claims && expectedRole) {
                if (expectedRole.indexOf(",") >= 0) {
                    var roleArray = expectedRole.split(",");
                    isRoleValid = roleArray.indexOf(claims["UserType"]) >= 0
                }
                else {
                    isRoleValid = claims["UserType"] == expectedRole;
                }
            }
        }
        if (!isRoleValid) {
          alert("Sorry, you don't have access to this module!");
          this._router.navigate(['/']);
        }
        return isRoleValid;
    }
}

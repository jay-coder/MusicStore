import { Injectable, Injector } from "@angular/core";
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from "@angular/common/http";

import { Observable } from "rxjs";
import { OAuthService } from "angular-oauth2-oidc";

@Injectable()
export class TokenInterceptor implements HttpInterceptor {

    private _authService: OAuthService;

    // Would like to inject authService directly but it causes a cyclic dependency error
    // see https://github.com/angular/angular/issues/18224
    constructor(private _injector: Injector) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        if (this.getAuthService().hasValidAccessToken()) {
            request = request.clone({
                // Remove cache in IE
                headers: request.headers.set('Cache-Control', 'no-cache')
                    .set('Pragma', 'no-cache')
                    .set('Expires', 'Sat, 01 Jan 1900 00:00:00 GMT')
                    .set('Authorization', 'Bearer ' + this.getAuthService().getAccessToken())
            });
        }
        return next.handle(request);
    }

    getAuthService(): OAuthService {
        if (typeof this._authService === 'undefined') {
            this._authService = this._injector.get(OAuthService);
        }
        return this._authService;
    }
}

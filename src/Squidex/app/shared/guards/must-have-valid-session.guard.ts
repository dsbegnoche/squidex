// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { AuthService } from './../services/auth.service';

@Injectable()
export class MustHaveValidSessionGuard implements CanActivate {
    constructor(
        private readonly authService: AuthService
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
        return this.authService.isValidSession()
            .do(valid => {
                if (!valid) {
                    this.authService.loginRedirect();
                }
            }).catch(error => {
                this.authService.loginRedirect();
                return Observable.of(null);
            });
    }
}
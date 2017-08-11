/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { AuthService } from './../services/auth.service';

@Injectable()
export class MustBeAuthenticatedGuard implements CanActivate {
    constructor(
        private readonly authService: AuthService
    ) {
    }

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
        return this.authService.userChanges.first()
            .do(user => {
                if (!user) {
                    this.authService.loginRedirect();
                }
            })
            .map(user => !!user);
    }
}
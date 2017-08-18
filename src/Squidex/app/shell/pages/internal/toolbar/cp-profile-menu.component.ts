/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import {
    ApiUrlConfig,
    AuthService,
    fadeAnimation,
    ModalView
} from 'shared';

@Component({
    selector: 'cp-profile-menu',
    templateUrl: './cp-profile-menu.component.html',
    animations: [
        fadeAnimation
    ]
})
export class CpProfileMenuComponent implements OnDestroy, OnInit {
    private authenticationSubscription: Subscription;

    public modalMenu = new ModalView(false, true);

    public profileDisplayName = '';
    public profileId = '';

    public isAdmin = false;

    public profileUrl = this.apiUrl.buildUrl('/identity-server/account/profile');

    constructor(
        private readonly authService: AuthService,
        private readonly apiUrl: ApiUrlConfig
    ) {
    }

    public ngOnDestroy() {
        this.authenticationSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.authenticationSubscription =
            this.authService.userChanges.filter(user => !!user)
            .subscribe(user => {
                console.log(user);
                    this.profileId = user.id;
                    this.profileDisplayName = user.displayName;

                    this.isAdmin = user.isAdmin;
                });
    }

    public logout() {
        this.authService.logoutRedirect();
    }
}
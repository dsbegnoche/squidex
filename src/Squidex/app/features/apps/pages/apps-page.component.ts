/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import {
    AppsStoreService,
    AuthService,
    fadeAnimation,
    ModalView
} from 'shared';

@Component({
    selector: 'sqx-apps-page',
    styleUrls: ['./apps-page.component.scss'],
    templateUrl: './apps-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class AppsPageComponent implements OnInit {
    private authenticationSubscription: Subscription;
    public addAppDialog = new ModalView();

    public apps = this.appsStore.apps;
    public isAdmin = false;

    constructor(
        public readonly appsStore: AppsStoreService,
        private readonly authService: AuthService
    ) {
    }

    public ngOnInit() {
        this.appsStore.selectApp(null);
        this.authenticationSubscription =
            this.authService.userChanges.filter(user => !!user)
            .subscribe(user => {
                this.isAdmin = user.isAdmin;
            });
    }

    public deleteApp(appName: string) {
        this.appsStore.deleteApp(appName)
            .subscribe(() => {
                    console.log(appName);
                },
                error => {
                    console.log(error);
                });
    }
}

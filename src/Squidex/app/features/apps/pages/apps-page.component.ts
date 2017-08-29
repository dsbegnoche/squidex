/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import {
    ComponentBase,
    AppsStoreService,
    AuthService,
    DialogService,
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
export class AppsPageComponent extends ComponentBase implements OnInit {
    private authenticationSubscription: Subscription;
    public addAppDialog = new ModalView();

    public apps = this.appsStore.apps;
    public isAdmin = false;

    constructor(dialogs: DialogService,
        private readonly appsStore: AppsStoreService,
        private readonly authService: AuthService
    ) {
        super(dialogs);
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
                    this.apps = this.appsStore.apps;
                },
                error => {
                    this.notifyError(error);
                });
    }
}

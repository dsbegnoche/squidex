/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import {
    ComponentBase,
    AppsStoreService,
    AuthService,
    DialogService,
    fadeAnimation,
    ModalView,
    OnboardingService
} from 'shared';

@Component({
    selector: 'sqx-apps-page',
    styleUrls: ['./apps-page.component.scss'],
    templateUrl: './apps-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class AppsPageComponent extends ComponentBase implements OnDestroy, OnInit {
    private onboardingAppsSubscription: Subscription;
    private authenticationSubscription: Subscription;

    public addAppDialog = new ModalView();
    public apps = this.appsStore.apps;
    public isAdmin = false;

    public onboardingModal = new ModalView();

    constructor(dialogs: DialogService,
        private readonly appsStore: AppsStoreService,
        private readonly onboardingService: OnboardingService,
        private readonly authService: AuthService
    ) {
        super(dialogs);
    }

    public ngOnDestroy() {
        this.onboardingAppsSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.appsStore.selectApp(null);

        this.onboardingAppsSubscription =
            this.appsStore.apps
                .subscribe(apps => {
                    if (apps.length === 0 && this.onboardingService.shouldShow('dialog')) {
                        this.onboardingService.disable('dialog');
                        this.onboardingModal.show();
                    }
                });

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

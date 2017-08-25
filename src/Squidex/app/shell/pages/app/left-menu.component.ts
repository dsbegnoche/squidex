/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';

import { AppsStoreService, PermissionEnum } from 'shared';

@Component({
    selector: 'sqx-left-menu',
    styleUrls: ['./left-menu.component.scss'],
    templateUrl: './left-menu.component.html'
})
export class LeftMenuComponent implements OnDestroy, OnInit {
    private appSubscription: Subscription;

    public permission: number | null = -1;
    public PermissionEnum = PermissionEnum;

    constructor(
        private readonly appsStore: AppsStoreService
    ) {
    }

    public ngOnDestroy() {
        this.appSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.appSubscription =
            this.appsStore.selectedApp.subscribe(app => {
                if (app) {
                    this.permission = isNaN(Number(app.permission))
                        ? Number(PermissionEnum[app.permission])
                        : app.permission;
                }
            });
    }
}
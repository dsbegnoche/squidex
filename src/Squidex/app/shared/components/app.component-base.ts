/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Observable } from 'rxjs';

import { AppsStoreService, DialogService } from './../declarations-base';

import { ComponentBase } from './component-base';
import { PermissionEnum } from 'shared';

export abstract class AppComponentBase extends ComponentBase {
    private appName$: Observable<string>;
    private appPermission$: Observable<PermissionEnum>;

    constructor(dialogs: DialogService,
        private readonly appsStore: AppsStoreService
    ) {
        super(dialogs);

        this.appName$ = this.appsStore.selectedApp.filter(a => !!a).map(a => a!.name);
        this.appPermission$ = this.appsStore.selectedApp.filter(a => !!a).map(a => a!.permission);
    }

    public appName(): Observable<string> {
        return this.appName$;
    }

    public appNameOnce(): Observable<string> {
        return this.appName$.first();
    }

    private appPermission(): PermissionEnum {
        let permission = PermissionEnum.Reader;
        this.appPermission$.subscribe(x => permission = x).unsubscribe();
        return permission;
    }

    public isAppEditor(): boolean {
        return this.appPermission() < PermissionEnum.Author;
    }

    public isAppAuthor(): boolean {
        return this.appPermission() === PermissionEnum.Author;
    }
}
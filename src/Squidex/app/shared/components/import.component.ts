/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AppComponentBase } from './app.component-base';
import { ActivatedRoute } from '@angular/router';

import {
    AppsStoreService,
    allParams,
    AssetDto,
    AssetsService,
    ApiUrlConfig,
    AuthService,
    DateTime,
    DialogService,
    fadeAnimation,
    Notification
} from './../declarations-base';

@Component({
    selector: 'sqx-import',
    styleUrls: ['./import.component.scss'],
    templateUrl: './import.component.html',
    animations: [
        fadeAnimation
    ]
})
export class ImportComponent extends AppComponentBase implements OnInit {
    public notifications: Notification[] = [];

    private schemaName: string | null;
    private maxFileSize = Math.pow(1024, 2) * 500;

    @Input()
    public initFile: File;

    @Input()
    public publish = false;

    @Output()
    public loaded = new EventEmitter<AssetDto>();

    @Output()
    public failed = new EventEmitter();

    public progress = 0;

    constructor(apps: AppsStoreService,
                dialogs: DialogService,
                authService: AuthService,
                private readonly apiUrl: ApiUrlConfig,
                private readonly assetsService: AssetsService,
                private readonly route: ActivatedRoute
    ) {
        super(dialogs, apps, authService);
    }

    public ngOnInit() {
        const initFile = this.initFile;
        const publish = this.publish;
        const params = allParams(this.route);
        this.schemaName = params['schemaName'];

        if (initFile) {
            if (initFile.size > this.maxFileSize) {
                this.notifyError('Files must be smaller than 500 MB.');
            } else if (this.getExtension(initFile.name) !== 'csv') {
                this.notifyError('File must be a .csv.');
                this.failed.emit();
            } else {
                this.appNameOnce()
                    .switchMap(app => {
                        const url = this.apiUrl.buildUrl(`api/content/${app}/${this.schemaName}/import?publish=${publish}`);
                        return this.assetsService.uploadFile(app,
                            initFile,
                            this.authService.user!.token,
                            DateTime.now(),
                            url);
                    })
                    .subscribe(dto => {
                            if (dto instanceof AssetDto) {
                                this.emitLoaded(dto);
                            } else {
                                this.progress = dto;
                            }
                        },
                        error => {
                            this.notifyError(error);
                            this.emitFailed(error);
                        });
            }
        }
    }

    private getExtension(file: string) {
        return file.substring(file.lastIndexOf('.') + 1).toLowerCase();
    }

    private emitFailed(error: any) {
        this.failed.emit(error);
    }

    private emitLoaded(asset: AssetDto) {
        this.loaded.emit(asset);
    }
}

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
    fadeAnimation
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
        if (initFile) {
            if (initFile.size >= this.maxFileSize) {
                this.notifyError('Files must be smaller than 500 MB.');
                this.failed.emit();
            } else if (this.getExtension(initFile.name) !== 'csv') {
                this.notifyError('File must be a .csv.');
                this.failed.emit();
            } else {
                this.appNameOnce()
                    .switchMap(app => {
                        const url = this.apiUrl.buildUrl(
                            `api/content/${app}/${allParams(this.route)['schemaName']}/import?publish=${this.publish}`);
                        return this.assetsService.importFile(app,
                            initFile,
                            this.authService.user!.token,
                            DateTime.now(),
                            url);
                    })
                    .subscribe(dto => {
                            if (dto === null) {
                                this.loaded.emit();
                            } else if (dto instanceof Array) {
                                this.notifyError('Some records failed to create.');
                                this.failed.emit();
                            } else if (typeof dto === 'number') {
                                this.progress = dto;
                            }
                        },
                        error => {
                            this.notifyError(error);
                            this.failed.emit(error);
                        });
            }
        }
    }

    private getExtension(file: string) {
        return file.substring(file.lastIndexOf('.') + 1).toLowerCase();
    }
}

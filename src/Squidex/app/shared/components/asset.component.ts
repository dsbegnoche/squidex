/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import { AppComponentBase } from './app.component-base';

import {
    AppsStoreService,
    AssetDto,
    AssetsService,
    AuthService,
    DateTime,
    DialogService,
    fadeAnimation,
    ModalView,
    UpdateAssetDto,
    Version,
    Versioned
} from './../declarations-base';

@Component({
    selector: 'sqx-asset',
    styleUrls: ['./asset.component.scss'],
    templateUrl: './asset.component.html',
    animations: [
        fadeAnimation
    ]
})
export class AssetComponent extends AppComponentBase implements OnInit {
    private assetVersion: Version;
    private maxFileSize = Math.pow(1024, 2) * 500;

    @Input()
    public initFile: File;

    @Input()
    public asset: AssetDto;

    @Input()
    public closeMode = false;

    @Output()
    public loaded = new EventEmitter<AssetDto>();

    @Output()
    public closing = new EventEmitter<AssetDto>();

    @Output()
    public updated = new EventEmitter<AssetDto>();

    @Output()
    public deleting = new EventEmitter<AssetDto>();

    @Output()
    public failed = new EventEmitter();

    public renameDialog = new ModalView();
    public renameFormSubmitted = false;
    public renameForm =
        this.formBuilder.group({
            name: ['',
                [
                    Validators.required
            ]],
            briefDescription: [''],
            tags: ['']
        });

    public progress = 0;
    public renameFormError = '';

    constructor(apps: AppsStoreService, dialogs: DialogService, authService: AuthService,
        private readonly formBuilder: FormBuilder,
        private readonly assetsService: AssetsService
    ) {
        super(dialogs, apps, authService);
    }

    public ngOnInit() {
        const initFile = this.initFile;

        if (initFile) {
            if (initFile.size < this.maxFileSize) {
                this.appNameOnce()
                    .switchMap(app => this.assetsService.uploadFile(app,
                        initFile,
                        this.authService.user!.token,
                        DateTime.now()))
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
            } else {
                this.notifyError('Files must be smaller than 500 MB.');
            }
        } else {
            this.updateAsset(this.asset, false);
        }
    }


    public updateFile(files: FileList) {
        if (files.length === 1) {
            this.appNameOnce()
                .switchMap(app => this.assetsService.replaceFile(app, this.asset.id, files[0], this.assetVersion))
                .subscribe(dto => {
                    if (dto instanceof Versioned) {
                        this.updateAsset(this.asset.update(dto.payload, this.userToken, dto.version), true);
                    } else {
                        this.setProgress(dto);
                    }
                }, error => {
                    this.notifyError(error);
                    this.setProgress();
                });
        }
    }

    public renameAsset() {
        this.renameFormSubmitted = true;

        if (this.renameForm.valid) {
            this.renameForm.disable();

            const requestDto =
                new UpdateAssetDto(
                    this.renameForm.controls['name'].value,
                    this.renameForm.controls['briefDescription'].value ? this.renameForm.controls['briefDescription'].value : '',
                    this.renameForm.controls['tags'].value ? this.renameForm.controls['tags'].value : []
                );

            this.appNameOnce()
                .switchMap(app => this.assetsService.putAsset(app, this.asset.id, requestDto, this.assetVersion))
                .subscribe(dto => {
                    this.updateAsset(
                        this.asset.rename(
                            requestDto.fileName,
                            this.authService.user!.token,
                            dto.version,
                            requestDto.briefDescription,
                            requestDto.tags), true);
                    this.resetRenameForm();
                }, error => {
                    this.enableRenameForm(error.displayMessage);
                });
        }
    }

    public cancelRenameAsset() {
        this.resetRenameForm();
    }

    private setProgress(progress = 0) {
        this.progress = progress;
    }

    private emitFailed(error: any) {
        this.failed.emit(error);
    }

    private emitLoaded(asset: AssetDto) {
        this.loaded.emit(asset);
    }

    private emitUpdated(asset: AssetDto) {
        this.updated.emit(asset);
    }

    private enableRenameForm(message: string) {
        this.renameForm.enable();
        this.renameFormError = message;
    }

    private resetRenameForm() {
        this.renameFormError = '';
        this.renameForm.enable();
        this.renameForm.controls['name'].setValue(this.asset.fileName);
        this.renameForm.controls['briefDescription'].setValue(this.asset.briefDescription);
        this.renameForm.controls['tags'].setValue(this.asset.tags);
        this.renameFormSubmitted = false;
        this.renameDialog.hide();
    }

    private updateAsset(asset: AssetDto, emitEvent: boolean) {
        this.asset = asset;
        this.assetVersion = asset.version;
        this.progress = 0;

        if (emitEvent) {
            this.emitUpdated(asset);
        }

        this.resetRenameForm();
    }
}

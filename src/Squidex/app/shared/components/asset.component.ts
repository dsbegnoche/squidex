/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

import { AppContext } from './app-context';

import {
    AssetDto,
    AssetsService,
    AssetDragged,
    DateTime,
    fadeAnimation,
    Version,
    Versioned,
    MessageBus
} from './../declarations-base';

@Component({
    selector: 'sqx-asset',
    styleUrls: ['./asset.component.scss'],
    templateUrl: './asset.component.html',
    providers: [
        AppContext
    ],
    animations: [
        fadeAnimation
    ]
})
export class AssetComponent implements OnInit {
    private assetVersion: Version;

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
    public clicked = new EventEmitter<AssetDto>();

    @Output()
    public failed = new EventEmitter();

    public progress = 0;

    constructor(public readonly ctx: AppContext,
        private readonly assetsService: AssetsService,
        private readonly messageBus: MessageBus
    ) {
    }

    public ngOnInit() {
        const initFile = this.initFile;

        if (initFile) {
            this.assetsService.uploadFile(this.ctx.appName, initFile, this.ctx.userToken, DateTime.now())
                .subscribe(dto => {
                    if (dto instanceof AssetDto) {
                        this.emitLoaded(dto);
                    } else {
                        this.progress = dto;
                    }
                }, error => {
                    this.ctx.notifyError(error);

                    this.emitFailed(error);
                });
        } else {
            this.updateAsset(this.asset, false);
        }
    }

    public updateFile(files: FileList) {
        if (files.length === 1) {
            this.assetsService.replaceFile(this.ctx.appName, this.asset.id, files[0], this.assetVersion)
                .subscribe(dto => {
                    if (dto instanceof Versioned) {
                        this.updateAsset(this.asset.update(dto.payload, this.ctx.userToken, dto.version), true);
                    } else {
                        this.setProgress(dto);
                    }
                }, error => {
                    this.ctx.notifyError(error);

                    this.setProgress();
                });
        }
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

    private updateAsset(asset: AssetDto, emitEvent: boolean) {
        this.asset = asset;
        this.assetVersion = asset.version;
        this.progress = 0;

        if (emitEvent) {
            this.emitUpdated(asset);
        }
    }

    public onAssetDragStart(event: any) {
        this.messageBus.emit(new AssetDragged(event.dragData, AssetDragged.DRAG_START, this));
    }

    public onAssetDragEnd(event: any) {
        this.messageBus.emit(new AssetDragged(event.dragData, AssetDragged.DRAG_END, this));
    }
}
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';

import {
    AppContext,
    AssetDto,
    AssetsService,
    AssetUpdated,
    CanComponentDeactivate,
    UpdateAssetDto,
    Version
} from 'shared';

@Component({
    selector: 'sqx-asset-page',
    styleUrls: ['./asset-page.component.scss'],
    templateUrl: './asset-page.component.html',
    providers: [
        AppContext
    ]
})
export class AssetPageComponent implements CanComponentDeactivate, OnInit {
    constructor(public readonly ctx: AppContext,
        private readonly assetsService: AssetsService,
        private readonly formBuilder: FormBuilder,
        private readonly router: Router
    ) {
    }

    private assetVersion: Version;
    public asset: AssetDto;
    public assetFormSubmitted = false;

    public assetForm = this.formBuilder.group({
        name: [
            '',
            [
                Validators.required
            ]
        ]
    });

    public ngOnInit() {
        this.ctx.route.data.map(d => d.asset)
            .subscribe((asset: AssetDto) => {
                this.reloadAssetForm(asset);
            });
    }

    public canDeactivate(): Observable<boolean> {
        if (!this.assetForm.dirty) {
            return Observable.of(true);
        } else {
            return this.ctx.confirmUnsavedChanges();
        }
    }

    private reloadAssetForm(asset: AssetDto) {
        this.asset = asset;
        this.assetForm.controls['name'].setValue(this.asset.fileName);
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.ctx.route, replaceUrl: true });
    }

    public saveAsset() {
        this.assetFormSubmitted = true;

        if (this.assetForm.valid) {
            this.disableAssetForm();

            const requestDto = new UpdateAssetDto(this.assetForm.controls['name'].value);

            this.assetsService.putAsset(this.ctx.appName, this.asset.id, requestDto, this.assetVersion)
                .subscribe(dto => {
                    this.updateAsset(this.asset.rename(requestDto.fileName, this.ctx.userToken, dto.version));
                    this.enableAssetForm();
                    this.back();
                }, error => {
                    this.ctx.notifyError(error);
                    this.enableAssetForm();
                });
        }
    }

    private updateAsset(asset: AssetDto) {
        this.asset = asset;
        this.assetVersion = asset.version;
        this.emitUpdated(asset);
    }

    private emitUpdated(asset: AssetDto) {
        this.ctx.bus.emit(new AssetUpdated(asset, this));
    }

    private disableAssetForm() {
        this.assetForm.disable();
    }

    private enableAssetForm() {
        this.assetForm.markAsPristine();
    }
}
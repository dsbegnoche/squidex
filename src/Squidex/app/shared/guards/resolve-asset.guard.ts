/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Resolve, Router, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';

import { allParams } from 'framework';

import { AssetDto, AssetsService } from './../services/assets.service';

@Injectable()
export class ResolveAssetGuard implements Resolve<AssetDto | null> {
    constructor(
        private readonly assetsService: AssetsService,
        private readonly router: Router
    ) {
    }

    public resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<AssetDto | null> {
        const params = allParams(route);
        const appName = params['appName'];

        if (!appName) {
            throw 'Route must contain app name.';
        }

        const assetId = params['assetId'];

        if (!assetId) {
            throw 'Route must contain asset id.';
        }

        const result =
            this.assetsService.getAsset(appName, assetId)
                .do(dto => {
                    if (!dto) {
                        this.router.navigate(['/404']);
                    }
                })
                .catch(error => {
                    this.router.navigate(['/404']);

                    return Observable.of(null);
                });

        return result;
    }
}
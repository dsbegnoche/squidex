/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DndModule } from 'ng2-dnd';

import {
    CanDeactivateGuard,
    HistoryComponent,
    ResolveAssetGuard,
    SqxFrameworkModule,
    SqxSharedModule
} from 'shared';

import {
    AssetPageComponent,
    AssetsPageComponent
} from './declarations';

const routes: Routes = [
    {
        path: '',
        component: AssetsPageComponent,
        children: [
            {
                path: ''
            },
            {
                path: ':assetId',
                component: AssetPageComponent,
                canDeactivate: [CanDeactivateGuard],
                resolve: {
                    asset: ResolveAssetGuard
                },
                children: [
                    {
                       path: 'history',
                       component: HistoryComponent,
                       data: {
                           channel: 'assets.{assetId}'
                       }
                   }
                ]
            }
        ]
    }
];

@NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule,
        DndModule,
        RouterModule.forChild(routes)
    ],
    declarations: [
        AssetPageComponent,
        AssetsPageComponent
    ]
})

export class SqxFeatureAssetsModule { }
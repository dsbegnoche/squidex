﻿/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ModuleWithProviders } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import {
    AppAreaComponent,
    HomePageComponent,
    InternalAreaComponent,
    LoginPageComponent,
    LogoutPageComponent,
    NotFoundPageComponent
} from './shell';

import {
    AppMustExistGuard,
    MustBeAuthenticatedGuard,
    MustBeNotAuthenticatedGuard,
    MustHaveValidSessionGuard,
    ResolvePublishedSchemaGuard
} from './shared';

import {
    ContentImportComponent
    } from './features/content/declarations';

export const routes: Routes = [
    {
        path: '',
        component: HomePageComponent,
        canActivate: [MustBeNotAuthenticatedGuard]
    },
    {
        path: 'app/:appName/content/:schemaName/import',
        component: ContentImportComponent,
        canActivate: [
            MustHaveValidSessionGuard,
            MustBeAuthenticatedGuard,
            AppMustExistGuard
        ],
        resolve: {
            schema: ResolvePublishedSchemaGuard
        }
    },
    {
        path: 'app',
        component: InternalAreaComponent,
        canActivate: [
            MustHaveValidSessionGuard,
            MustBeAuthenticatedGuard
        ],
        children: [
            {
                path: '',
                loadChildren: './features/apps/module#SqxFeatureAppsModule'
            },
            {
                path: 'administration',
                loadChildren: './features/administration/module#SqxFeatureAdministrationModule'
            },
            {
                path: ':appName',
                component: AppAreaComponent,
                canActivate: [AppMustExistGuard],
                children: [
                    {
                        path: '',
                        loadChildren: './features/dashboard/module#SqxFeatureDashboardModule'
                    },
                    {
                        path: 'content',
                        loadChildren: './features/content/module#SqxFeatureContentModule'
                    },
                    {
                        path: 'schemas',
                        loadChildren: './features/schemas/module#SqxFeatureSchemasModule'
                    },
                    {
                        path: 'assets',
                        loadChildren: './features/assets/module#SqxFeatureAssetsModule'
                    },
                    {
                        path: 'webhooks',
                        loadChildren: './features/webhooks/module#SqxFeatureWebhooksModule'
                    },
                    {
                        path: 'settings',
                        loadChildren: './features/settings/module#SqxFeatureSettingsModule'
                    },
                    {
                        path: 'api',
                        loadChildren: './features/api/module#SqxFeatureApiModule'
                    }
                ]
            }
        ]
    },
    {
        path: 'logout',
        component: LogoutPageComponent
    },
    {
        path: 'login',
        component: LoginPageComponent
    },
    {
        path: '**',
        component: NotFoundPageComponent
    }
];

export const routing: ModuleWithProviders = RouterModule.forRoot(routes, { useHash: false });
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { NgModule } from '@angular/core';

import { SqxFrameworkModule, SqxSharedModule } from 'shared';

import {
    AppAreaComponent,
    AppsMenuComponent,
    CpHelpLinksMenuComponent,
    CpProductsMenuComponent,
    HomePageComponent,
    InternalAreaComponent,
    LeftMenuComponent,
    LoginPageComponent,
    LogoutPageComponent,
    NotFoundPageComponent,
    CpProfileMenuComponent,
    ProfileMenuComponent
} from './declarations';

import {
    ContentImportComponent
} from '../features/content/declarations'

@NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule
    ],
    exports: [
        AppAreaComponent,
        HomePageComponent,
        InternalAreaComponent,
        NotFoundPageComponent,
        ContentImportComponent
    ],
    declarations: [
        AppAreaComponent,
        AppsMenuComponent,
        CpHelpLinksMenuComponent,
        CpProductsMenuComponent,
        HomePageComponent,
        InternalAreaComponent,
        LeftMenuComponent,
        LoginPageComponent,
        LogoutPageComponent,
        NotFoundPageComponent,
        CpProfileMenuComponent,
        ProfileMenuComponent,
        ContentImportComponent
    ]
})
export class SqxShellModule { }
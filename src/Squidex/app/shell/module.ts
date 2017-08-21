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

@NgModule({
    imports: [
        SqxFrameworkModule,
        SqxSharedModule
    ],
    exports: [
        AppAreaComponent,
        HomePageComponent,
        InternalAreaComponent,
        NotFoundPageComponent
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
        ProfileMenuComponent
    ]
})
export class SqxShellModule { }
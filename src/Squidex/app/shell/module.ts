﻿/*
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
    CpProductsMenuComponent,
    HomePageComponent,
    InternalAreaComponent,
    LeftMenuComponent,
    LoginPageComponent,
    LogoutPageComponent,
    NotFoundPageComponent,
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
        CpProductsMenuComponent,
        HomePageComponent,
        InternalAreaComponent,
        LeftMenuComponent,
        LoginPageComponent,
        LogoutPageComponent,
        NotFoundPageComponent,
        ProfileMenuComponent
    ]
})
export class SqxShellModule { }
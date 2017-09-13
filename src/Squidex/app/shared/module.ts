/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { ModuleWithProviders, NgModule } from '@angular/core';
import { DndModule } from 'ng2-dnd';

import { SqxFrameworkModule } from 'framework';

import {
    AppFormComponent,
    AppClientsService,
    AppContributorsService,
    AppLanguagesService,
    AppsStoreService,
    AppsService,
    AppMustExistGuard,
    AssetComponent,
    AssetPreviewUrlPipe,
    AssetsService,
    AssetUrlPipe,
    AuthInterceptor,
    AuthService,
    ContentsService,
    CpHelpLinksService,
    CpProductsService,
    EventConsumersService,
    FileIconPipe,
    GraphQlService,
    HelpComponent,
    HelpService,
    HistoryComponent,
    HistoryService,
    LanguageSelectorComponent,
    LanguagesService,
    MustBeAuthenticatedGuard,
    MustBeNotAuthenticatedGuard,
    PlansService,
    ResolveAppLanguagesGuard,
    ResolveContentGuard,
    ResolvePublishedSchemaGuard,
    ResolveSchemaGuard,
    ResolveUserGuard,
    SchemasService,
    TextAnalyticsService,
    UIService,
    UsagesService,
    UserDtoPicture,
    UserEmailPipe,
    UserEmailRefPipe,
    UserNamePipe,
    UserNameRefPipe,
    UserIdPicturePipe,
    UserPicturePipe,
    UserPictureRefPipe,
    UserManagementService,
    UsersProviderService,
    UsersService,
    WebhooksService
} from './declarations';

@NgModule({
    imports: [
        DndModule,
        SqxFrameworkModule
    ],
    declarations: [
        AppFormComponent,
        AssetComponent,
        AssetPreviewUrlPipe,
        AssetUrlPipe,
        FileIconPipe,
        HelpComponent,
        HistoryComponent,
        LanguageSelectorComponent,
        UserDtoPicture,
        UserEmailPipe,
        UserEmailRefPipe,
        UserIdPicturePipe,
        UserNamePipe,
        UserNameRefPipe,
        UserPicturePipe,
        UserPictureRefPipe
    ],
    exports: [
        AppFormComponent,
        AssetComponent,
        AssetPreviewUrlPipe,
        AssetUrlPipe,
        FileIconPipe,
        HelpComponent,
        HistoryComponent,
        LanguageSelectorComponent,
        UserDtoPicture,
        UserEmailPipe,
        UserEmailRefPipe,
        UserIdPicturePipe,
        UserNamePipe,
        UserNameRefPipe,
        UserPicturePipe,
        UserPictureRefPipe
    ]
})
export class SqxSharedModule {
    public static forRoot(): ModuleWithProviders {
        return {
            ngModule: SqxSharedModule,
            providers: [
                AppClientsService,
                AppContributorsService,
                AppLanguagesService,
                AppsStoreService,
                AppsService,
                AppMustExistGuard,
                AssetsService,
                AuthService,
                ContentsService,
                CpHelpLinksService,
                CpProductsService,
                EventConsumersService,
                GraphQlService,
                HelpService,
                HistoryService,
                LanguagesService,
                MustBeAuthenticatedGuard,
                MustBeNotAuthenticatedGuard,
                PlansService,
                ResolveAppLanguagesGuard,
                ResolveContentGuard,
                ResolvePublishedSchemaGuard,
                ResolveSchemaGuard,
                ResolveUserGuard,
                SchemasService,
                TextAnalyticsService,
                UIService,
                UsagesService,
                UserManagementService,
                UsersProviderService,
                UsersService,
                WebhooksService,
                {
                    provide: HTTP_INTERCEPTORS,
                    useClass: AuthInterceptor,
                    multi: true
                }
            ]
        };
    }
}
﻿/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { CommonModule } from '@angular/common';
import { ModuleWithProviders, NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import {
    AnalyticsService,
    AutocompleteComponent,
    CanDeactivateGuard,
    ClipboardService,
    ConfirmClickDirective,
    ControlErrorsComponent,
    CopyDirective,
    DateTimeEditorComponent,
    DayOfWeekPipe,
    DayPipe,
    DialogService,
    DialogRendererComponent,
    DisplayNamePipe,
    DropdownComponent,
    DurationPipe,
    FileDropDirective,
    FileSizePipe,
    FocusOnInitDirective,
    FromNowPipe,
    GeolocationEditorComponent,
    ImageSourceDirective,
    IndeterminateValueDirective,
    JscriptEditorComponent,
    JsonEditorComponent,
    KNumberPipe,
    LocalCacheService,
    LocalStoreService,
    LowerCaseInputDirective,
    MarkdownEditorComponent,
    MessageBus,
    ModalTargetDirective,
    ModalViewDirective,
    MoneyPipe,
    MonthPipe,
    PanelContainerDirective,
    PanelComponent,
    ParentLinkDirective,
    PopupLinkDirective,
    ProgressBarComponent,
    ResourceLoaderService,
    RichEditorComponent,
    RootViewDirective,
    RootViewService,
    ScrollActiveDirective,
    ShortcutComponent,
    ShortcutService,
    ShortDatePipe,
    ShortTimePipe,
    SliderComponent,
    SortedDirective,
    StarsComponent,
    TagEditorComponent,
    TemplateWrapperDirective,
    TitleService,
    TitleComponent,
    ToggleComponent,
    UserReportComponent
} from './declarations';

@NgModule({
    imports: [
        HttpClientModule,
        FormsModule,
        CommonModule,
        RouterModule,
        ReactiveFormsModule
    ],
    declarations: [
        AutocompleteComponent,
        ConfirmClickDirective,
        ControlErrorsComponent,
        CopyDirective,
        DateTimeEditorComponent,
        DayOfWeekPipe,
        DayPipe,
        DialogRendererComponent,
        DisplayNamePipe,
        DropdownComponent,
        DurationPipe,
        FileDropDirective,
        FileSizePipe,
        FocusOnInitDirective,
        FromNowPipe,
        GeolocationEditorComponent,
        ImageSourceDirective,
        IndeterminateValueDirective,
        JscriptEditorComponent,
        JsonEditorComponent,
        KNumberPipe,
        LowerCaseInputDirective,
        MarkdownEditorComponent,
        ModalTargetDirective,
        ModalViewDirective,
        MoneyPipe,
        MonthPipe,
        PanelContainerDirective,
        PanelComponent,
        ParentLinkDirective,
        PopupLinkDirective,
        ProgressBarComponent,
        RichEditorComponent,
        RootViewDirective,
        ScrollActiveDirective,
        ShortcutComponent,
        ShortDatePipe,
        ShortTimePipe,
        SliderComponent,
        SortedDirective,
        StarsComponent,
        TagEditorComponent,
        TemplateWrapperDirective,
        TitleComponent,
        ToggleComponent,
        UserReportComponent
    ],
    exports: [
        AutocompleteComponent,
        ConfirmClickDirective,
        ControlErrorsComponent,
        CopyDirective,
        DateTimeEditorComponent,
        DayOfWeekPipe,
        DayPipe,
        DialogRendererComponent,
        DisplayNamePipe,
        DropdownComponent,
        DurationPipe,
        FileDropDirective,
        FileSizePipe,
        FocusOnInitDirective,
        FromNowPipe,
        GeolocationEditorComponent,
        ImageSourceDirective,
        IndeterminateValueDirective,
        JscriptEditorComponent,
        JsonEditorComponent,
        KNumberPipe,
        LowerCaseInputDirective,
        MarkdownEditorComponent,
        ModalTargetDirective,
        ModalViewDirective,
        MoneyPipe,
        MonthPipe,
        PanelContainerDirective,
        PanelComponent,
        ParentLinkDirective,
        PopupLinkDirective,
        ProgressBarComponent,
        RichEditorComponent,
        RootViewDirective,
        ScrollActiveDirective,
        ShortcutComponent,
        ShortDatePipe,
        ShortTimePipe,
        SliderComponent,
        SortedDirective,
        StarsComponent,
        TagEditorComponent,
        TemplateWrapperDirective,
        TitleComponent,
        ToggleComponent,
        UserReportComponent,
        HttpClientModule,
        FormsModule,
        CommonModule,
        RouterModule,
        ReactiveFormsModule
    ]
})
export class SqxFrameworkModule {
    public static forRoot(): ModuleWithProviders {
        return {
            ngModule: SqxFrameworkModule,
            providers: [
                AnalyticsService,
                CanDeactivateGuard,
                ClipboardService,
                DialogService,
                LocalCacheService,
                LocalStoreService,
                MessageBus,
                ResourceLoaderService,
                RootViewService,
                ShortcutService,
                TitleService
            ]
        };
    }
 }
/*
 * CivicPlus implementation of Squidex HCMS
 */

import { Component } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AppComponentBase,
    AppsStoreService,
    AuthService,
    DialogService,
    ImmutableArray
    } from 'shared';

@Component({
    selector: 'sqx-content-import',
    styleUrls: ['./content-import.component.scss'],
    templateUrl: './content-import.component.html'
})
export class ContentImportComponent extends AppComponentBase {
    public files = ImmutableArray.empty<File>();

    public importForm =
        this.formBuilder.group({
            publish: [false]
        });

    constructor(apps: AppsStoreService,
                dialogs: DialogService,
                authService: AuthService,
                private readonly formBuilder: FormBuilder) {
        super(dialogs, apps, authService);
    }

    public addFile(fileList: FileList) {
        if (fileList.length > 0) {
            this.files = this.files.pushFront(fileList[0]);
        }
    }

    public onImportLoaded(file: File) {
        this.files = this.files.remove(file);
    }

    public onImportFailed(file: File) {
        this.files = this.files.remove(file);
        this.notifyError('error occurred');
    }
}
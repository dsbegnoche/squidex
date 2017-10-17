/*
 * CivicPlus implementation of Squidex HCMS
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';

import {
    AppComponentBase,
    AppsStoreService,
    AuthService,
    DialogService
    } from 'shared';

@Component({
    selector: 'sqx-content-import',
    styleUrls: ['./content-import.component.scss'],
    templateUrl: './content-import.component.html'
})
export class ContentImportComponent extends AppComponentBase implements OnInit {
    public publishOrSubmit: string;
    public file: File | null = null;

    public importForm =
        this.formBuilder.group({
            publish: [true]
        });

    constructor(apps: AppsStoreService,
                dialogs: DialogService,
                authService: AuthService,
                private readonly formBuilder: FormBuilder) {
        super(dialogs, apps, authService);
    }

    public ngOnInit() {
        this.publishOrSubmit = this.isAppAuthor() ? 'Submit' : 'Publish';
    }

    public addFile(fileList: FileList) {
        if (fileList.length > 0) {
            this.file = fileList[0];
        }
    }

    public onImportLoaded(file: File) {
        setTimeout(() => {
            this.file = null;
        });
        this.notifyInfo('Import Succeeded');
    }

    public onImportFailed(file: File) {
        setTimeout(() => {
            this.file = null;
        });
    }
}
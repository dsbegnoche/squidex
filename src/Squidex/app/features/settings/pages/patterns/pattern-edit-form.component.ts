/*
 * CivicPlus implementation of Squidex Headless CMS
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    ComponentBase,
    DialogService,
    UIRegexSuggestionDto,
    Version
} from 'shared';

@Component({
    selector: 'sqx-pattern-edit-form',
    styleUrls: ['./pattern-edit-form.component.scss'],
    templateUrl: './pattern-edit-form.component.html'
})
export class PatternEditFormComponent extends ComponentBase implements OnInit {
    @Input()
    public name: string;

    @Input()
    public pattern: UIRegexSuggestionDto;

    @Input()
    public version: Version;

    @Input()
    public appName: string;

    public editFormSubmitted = false;
    public editForm =
    this.formBuilder.group({
        name: [
            '',
            [
                Validators.maxLength(100)
            ]
        ],
        pattern: [
            '',
            [
                Validators.maxLength(1000)
            ]
        ],
        message: [
            '',
            [
                Validators.maxLength(1000)
            ]
        ]
    });

    constructor(dialogs: DialogService,
        private readonly formBuilder: FormBuilder
    ) {
        super(dialogs);
    }

    public ngOnInit() {
        this.editForm.patchValue(this.pattern);
    }

    public cancel() {
        this.resetEditForm();
    }

    private resetEditForm() {
        this.editForm.reset(this.pattern);
        this.editForm.enable();
        this.editFormSubmitted = false;
    }

    public savePattern() {
    }
}
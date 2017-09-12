/*
 * CivicPlus implementation of Squidex Headless CMS
 */

import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    fadeAnimation,
    UIRegexSuggestionDto
} from 'shared';

@Component({
    selector: 'sqx-pattern',
    styleUrls: ['./pattern.component.scss'],
    templateUrl: './pattern.component.html',
    animations: [
        fadeAnimation
    ]
})
export class PatternComponent implements OnInit {
    @Input()
    public pattern: UIRegexSuggestionDto;

    public isEditing = false;
    public selectedTab = 0;

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

    constructor(
        private readonly formBuilder: FormBuilder
    ) {
    }

    public ngOnInit() {
        this.resetEditForm();
    }

    public toggleEditing() {
        this.isEditing = !this.isEditing;
    }

    public selectTab(tab: number) {
        this.selectedTab = tab;
    }

    public cancel() {
        this.resetEditForm();
    }

    private resetEditForm() {
        this.editFormSubmitted = false;
        this.editForm.reset(this.pattern);

        this.isEditing = false;
    }
}


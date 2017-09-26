/*
 * CivicPlus implementation of Squidex Headless CMS
 */

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    AppComponentBase,
    AppPatternsService,
    AppPatternsSuggestionDto,
    AppsStoreService,
    AuthService,
    DialogService,
    fadeAnimation,
    ValidatorsEx,
    Version
} from 'shared';

@Component({
    selector: 'sqx-pattern',
    styleUrls: ['./pattern.component.scss'],
    templateUrl: './pattern.component.html',
    animations: [
        fadeAnimation
    ]
})
export class PatternComponent extends AppComponentBase implements OnInit {
    @Input()
    public pattern: AppPatternsSuggestionDto;

    @Input()
    public isNew: boolean;

    @Output()
    public removing = new EventEmitter<AppPatternsSuggestionDto>();

    @Output()
    public created = new EventEmitter<AppPatternsSuggestionDto>();

    @Output()
    public updated = new EventEmitter<AppPatternsSuggestionDto>();

    public isEditing = false;
    public originalPattern: AppPatternsSuggestionDto = null;

    private version = new Version();
    public editFormSubmitted = false;

    public editForm =
    this.formBuilder.group({
        name: [
            '',
            [
                Validators.required,
                Validators.maxLength(100),
                ValidatorsEx.pattern('[A-z0-9]+[A-z0-9\- ]*[A-z0-9]', 'Name can only contain letters, numbers, dashes and spaces.')
            ]
        ],
        pattern: [
            '',
            [
                Validators.required
            ]
        ],
        defaultMessage: [
            '',
            [
                Validators.maxLength(1000)
            ]
        ]
    });

    constructor(apps: AppsStoreService, dialogs: DialogService, authService: AuthService,
        private readonly patternService: AppPatternsService,
        private readonly formBuilder: FormBuilder
    ) {
        super(dialogs, apps, authService);
    }

    public ngOnInit() {
        this.resetEditForm();
    }

    public toggleEditing() {
        this.isEditing = !this.isEditing;
        this.originalPattern = this.pattern;
    }

    public cancel() {
        this.resetEditForm();
    }

    public save() {
        this.editFormSubmitted = true;

        if (this.editForm.valid) {
            let requestDto: AppPatternsSuggestionDto = new AppPatternsSuggestionDto(
                this.editForm.controls['name'].value,
                this.editForm.controls['pattern'].value,
                this.editForm.controls['defaultMessage'].value);

            if (this.isNew) {
                this.appNameOnce()
                    .switchMap(app => this.patternService.postPattern(app, requestDto, this.version))
                    .subscribe(dto => {
                            this.created.emit(dto);
                        },
                        error => {
                            this.notifyError(error);
                        },
                        () => {
                            this.resetEditForm();
                        });
            } else {
                this.appNameOnce()
                    .switchMap(app => this.patternService.updatePattern(app,
                        this.originalPattern.name,
                        requestDto,
                        this.version))
                    .subscribe(() => {
                            this.updated.emit(this.originalPattern);
                            this.created.emit(requestDto);
                        },
                        error => {
                            this.notifyError(error);
                        },
                        () => {
                            this.resetEditForm();
                        });
            }
        }
    }

    public resetEditForm() {
        this.editFormSubmitted = false;
        this.editForm.reset(this.pattern);

        this.isEditing = false;
        this.originalPattern = null;
    }
}


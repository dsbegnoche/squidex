/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';

import {
    AppComponentBase,
    AppsStoreService,
    DialogService,
    ModalView,
    StringFieldPropertiesDto,
    UIRegexSuggestionDto,
    UIService
} from 'shared';

@Component({
    selector: 'sqx-string-validation',
    styleUrls: ['string-validation.component.scss'],
    templateUrl: 'string-validation.component.html'
})
export class StringValidationComponent extends AppComponentBase implements OnDestroy, OnInit {
    private patternSubscription: Subscription;
    private uiSettingsSubscription: Subscription;

    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: StringFieldPropertiesDto;

    public showDefaultValue: Observable<boolean>;
    public showPatternMessage: boolean;
    public showPatternSuggestions: Observable<boolean>;
    public patternName: string;

    public regexSuggestions: UIRegexSuggestionDto[] = [];

    public regexSuggestionsModal = new ModalView(false, false);

    constructor(dialogs: DialogService, apps: AppsStoreService,
        private readonly uiService: UIService
    ) {
        super(dialogs, apps);
    }

    public ngOnDestroy() {
        this.patternSubscription.unsubscribe();
        this.uiSettingsSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.editForm.setControl('maxLength',
            new FormControl(this.properties.maxLength));

        this.editForm.setControl('minLength',
            new FormControl(this.properties.minLength));

        this.editForm.setControl('pattern',
            new FormControl(this.properties.pattern));

        this.editForm.setControl('patternMessage',
            new FormControl(this.properties.patternMessage));

        this.editForm.setControl('defaultValue',
            new FormControl(this.properties.defaultValue));

        this.showDefaultValue =
            this.editForm.controls['isRequired'].valueChanges
            .startWith(this.properties.isRequired)
            .map(x => !x);

        this.showPatternMessage =
            this.editForm.controls['pattern'].value && this.editForm.controls['pattern'].value.trim().length > 0;
        this.editForm.controls['pattern'].valueChanges.subscribe(() =>
            this.showPatternMessage = this.editForm.controls['pattern'] &&
            this.editForm.controls['pattern'].value.trim() !== '');

        this.showPatternSuggestions =
            this.editForm.controls['pattern'].valueChanges
            .startWith('')
            .map(x => !x || x.trim().length === 0);

        this.uiSettingsSubscription =
            this.appNameOnce()
            .switchMap(app =>
                this.uiService.getSettings(app))
            .subscribe(settings => {
                this.regexSuggestions = settings.regexSuggestions;
                this.setPatternName();
            });

        this.patternSubscription =
            this.editForm.controls['pattern'].valueChanges
            .subscribe((value: string) => {
                if (!value || value.length === 0) {
                    this.editForm.controls['patternMessage'].setValue(undefined);
                }
                this.setPatternName();
            });
    }

    public setPattern(pattern: UIRegexSuggestionDto) {
        this.patternName = pattern.name;
        this.editForm.controls['pattern'].setValue(pattern.pattern);
        this.editForm.controls['patternMessage'].setValue(pattern.message);
        this.showPatternMessage = true;
    }

    private setPatternName() {
        let matchingPattern = this.regexSuggestions.find(x => x.pattern === this.editForm.controls['pattern'].value);
        if (matchingPattern) {
            this.patternName = matchingPattern.name;
        } else if (this.editForm.controls['pattern'].value && this.editForm.controls['pattern'].value.trim() !== '') {
            this.patternName = 'Advanced';
        } else {
            this.patternName = undefined;
        }
    }
}
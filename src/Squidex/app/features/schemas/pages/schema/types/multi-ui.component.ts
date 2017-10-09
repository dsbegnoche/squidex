// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';

import { StringFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-multi-ui',
    styleUrls: ['multi-ui.component.scss'],
    templateUrl: 'multi-ui.component.html'
})
export class MultiUIComponent implements OnDestroy, OnInit {
    private editorSubscription: Subscription;

    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: StringFieldPropertiesDto;

    public hideAllowedValues: Observable<boolean>;

    public ngOnDestroy() {
        this.editorSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.editForm.setControl('editor',
            new FormControl(this.properties.editor, [
                Validators.required
            ]));

        this.editForm.setControl('allowedValues',
            new FormControl(this.properties.allowedValues));

        this.editorSubscription =
            this.hideAllowedValues
                .subscribe(isSelection => {
                    if (isSelection) {
                        this.editForm.controls['allowedValues'].setValue(undefined);
                    }
                });
    }
}

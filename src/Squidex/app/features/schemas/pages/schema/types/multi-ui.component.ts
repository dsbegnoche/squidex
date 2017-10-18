// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

import { MultiFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-multi-ui',
    styleUrls: ['multi-ui.component.scss'],
    templateUrl: 'multi-ui.component.html'
})
export class MultiUIComponent implements OnInit {

    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: MultiFieldPropertiesDto;

    public allowedValuesHolder: string[];

    public ngOnInit() {
        this.editForm.setControl('editor',
            new FormControl(this.properties.editor, [
                Validators.required
            ]));

        this.editForm.setControl('allowedValues',
            new FormControl(this.properties.allowedValues));

        this.editForm.setControl('defaultValue',
            new FormControl(this.properties.defaultValue));

        this.allowedValuesHolder = this.properties.allowedValues;
    }

    public SyncAllowed() {
        let allowedValues = this.editForm.get('allowedValues').value;
        this.allowedValuesHolder = allowedValues;
    }
}

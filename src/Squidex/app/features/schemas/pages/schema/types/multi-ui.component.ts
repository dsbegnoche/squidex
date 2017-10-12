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

    public ngOnInit() {
        this.editForm.setControl('editor',
            new FormControl(this.properties.editor, [
                Validators.required
            ]));

        this.editForm.setControl('allowedValues',
            new FormControl(this.properties.allowedValues));

        this.editForm.setControl('defaultValues',
            new FormControl(this.properties.defaultValues));
    }
}

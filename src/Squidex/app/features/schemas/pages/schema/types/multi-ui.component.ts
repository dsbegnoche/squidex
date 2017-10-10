// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

import { StringFieldPropertiesDto } from 'shared';

@Component({
    selector: 'sqx-multi-ui',
    styleUrls: ['multi-ui.component.scss'],
    templateUrl: 'multi-ui.component.html'
})
export class MultiUIComponent implements OnInit {

    @Input()
    public editForm: FormGroup;

    @Input()
    public properties: StringFieldPropertiesDto;

    public ngOnInit() {
        this.editForm.setControl('editor',
            new FormControl(this.properties.editor, [
                Validators.required
            ]));

        this.editForm.setControl('allowedValues',
            new FormControl(this.properties.allowedValues));
    }
}

/*
 * CivicPlus implementation of Squidex Headless CMS
 */

import { Component, Input, forwardRef } from '@angular/core';
import { ControlValueAccessor, FormGroup, NG_VALUE_ACCESSOR } from '@angular/forms';

import { Types } from './../utils/types';

const KEY_ENTER = 13;

export const SQX_MULTI_EDITOR_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => MultiEditorComponent), multi: true
};

@Component({
    selector: 'sqx-multi-editor',
    styleUrls: ['./multi-editor.component.scss'],
    templateUrl: './multi-editor.component.html',
    providers: [SQX_MULTI_EDITOR_CONTROL_VALUE_ACCESSOR]
})
export class MultiEditorComponent implements ControlValueAccessor {
    private callChange = (v: any) => { /* NOOP */ };
    private callTouched = () => { /* NOOP */ };

    @Input()
    public useDefaultValue = true;

    @Input()
    public inputName = 'multi-editor';

    public items: string[] = [];

    public form = new FormGroup();

    public writeValue(value: any[]) {
        this.resetForm();
        this.items = value;
    }

    public setDisabledState(isDisabled: boolean): void {
        /*
        if (isDisabled) {
            this.addInput.disable();
        } else {
            this.addInput.enable();
        }
        */
    }

    public registerOnChange(fn: any) {
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
    }

    public remove(index: number) {
        this.updateItems([...this.items.slice(0, index), ...this.items.splice(index + 1)]);
    }

    public markTouched() {
        this.callTouched();
    }

    private resetForm() {
        this.addInput.reset();
    }

    public onSelect(event: KeyboardEvent) {
        const value = <string>this.addInput.value.trim();

        const converted = this.converter.convert(value);

        this.updateItems([...this.items, converted]);
        this.resetForm();
        return false;
    }

    private updateItems(items: string[]) {
        this.items = items;

        if (items.length === 0 && this.useDefaultValue) {
            this.callChange(undefined);
        } else {
            this.callChange(this.items);
        }
    }
}

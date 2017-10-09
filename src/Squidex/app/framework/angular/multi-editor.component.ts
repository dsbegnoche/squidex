/*
 * CivicPlus implementation of Squidex Headless CMS
 */

import { Component, Input, forwardRef } from '@angular/core';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR } from '@angular/forms';

import { Converter, StringConverter } from './tag-editor.component';
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
    public converter: Converter = new StringConverter();

    @Input()
    public useDefaultValue = true;

    @Input()
    public inputName = 'multi-editor';

    public items: any[] = [];

    public addInput = new FormControl();

    public writeValue(value: any[]) {
        this.resetForm();

        if (this.converter && Types.isArrayOf(value, v => this.converter.isValidValue(v))) {
            this.items = value;
        } else {
            this.items = [];
        }
    }

    public setDisabledState(isDisabled: boolean): void {
        if (isDisabled) {
            this.addInput.disable();
        } else {
            this.addInput.enable();
        }
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

    public onKeyDown(event: KeyboardEvent) {
        if (event.keyCode === KEY_ENTER) {
            const value = <string>this.addInput.value.trim();

            if (this.converter.isValidInput(value) && !this.items.includes(value)) {
                const converted = this.converter.convert(value);

                this.updateItems([...this.items, converted]);
                this.resetForm();
                return false;
            }

            this.addInput.reset();
            return false;
        }

        return true;
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

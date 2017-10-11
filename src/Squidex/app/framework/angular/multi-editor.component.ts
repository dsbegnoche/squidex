/*
 * CivicPlus implementation of Squidex Headless CMS
 */

import { Component, Input, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormControl } from '@angular/forms';

// import { Types } from './../utils/types';

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
    public inputName = 'multi-editor';

    public checkInput = new FormControl();

    @Input()
    public items: string[] = [];

    // @Input()
    public selectedItems: string[] = [];

    public writeValue(value: any[]) {
        this.items = value;
    }

    public setDisabledState(isDisabled: boolean): void {
        // noop
    }

    public registerOnChange(fn: any) {
        this.callChange = fn;
    }

    public registerOnTouched(fn: any) {
        this.callTouched = fn;
    }

    public remove(value: string) {
        let index = this.items.indexOf(value);
        this.updateItems([...this.items.slice(0, index), ...this.items.splice(index + 1)]);
    }

    public markTouched() {
        this.callTouched();
    }

    public debug() {
        console.log(this.items);
        console.log(this.selectedItems);
    }


    public toggle(value: string, toggle: boolean) {
        if (toggle) {
            this.updateItems([...this.items, value]);
        } else {
            this.remove(value);
        }
    }

    private updateItems(items: string[]) {
        this.items = items;

        if (items.length === 0) {
            this.callChange(undefined);
        } else {
            this.callChange(this.items);
        }
    }
}

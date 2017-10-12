/*
 * CivicPlus implementation of Squidex Headless CMS
 */

import { Component, Output, Input, forwardRef } from '@angular/core';
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
    public items: any[] = [];

    @Output()
    public selectedItems: string[] = [];

    public writeValue(value: any[]) {
        console.log('writevalue!');
        console.log(value);
        this.selectedItems = value;
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

    public markTouched() {
        this.callTouched();
    }

    public debug() {
        console.log(this.items);
        console.log(this.selectedItems);
    }

    public toggle(value: string, toggle: boolean) {
        console.log('toggle: ' + value + ' - ' + toggle);
        if (toggle) {
            this.updateItems([...this.selectedItems, value]);
        } else {
            let index = this.selectedItems.indexOf(value);
            this.updateItems([...this.selectedItems.slice(0, index), ...this.selectedItems.splice(index + 1)]);
        }
    }

    private updateItems(items: string[]) {
        this.selectedItems = items;

        if (items.length === 0) {
            this.callChange(undefined);
        } else {
            this.callChange(this.selectedItems);
        }
    }
}

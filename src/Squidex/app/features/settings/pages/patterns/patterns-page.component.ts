/*
 * CivicPlus implementation of Squidex Headless CMS
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    AppComponentBase,
    AppsStoreService,
    DialogService,
    HistoryChannelUpdated,
    ImmutableArray,
    MessageBus,
    UIService,
    UIRegexSuggestionDto
} from 'shared';

@Component({
    selector: 'sqx-patterns-page',
    styleUrls: ['./patterns-page.component.scss'],
    templateUrl: './patterns-page.component.html'
})
export class PatternsPageComponent extends AppComponentBase implements OnInit {
    public appPatterns = ImmutableArray.empty<UIRegexSuggestionDto>();

    public addPatternForm =
    this.formBuilder.group({
        pattern: [null,
            Validators.required
        ]
    });

    constructor(apps: AppsStoreService, dialogs: DialogService,
        private readonly messageBus: MessageBus,
        private readonly formBuilder: FormBuilder,
        private readonly uiService: UIService
    ) {
        super(dialogs, apps);
    }

    public ngOnInit() {
        this.load();
    }

    public load() {
        this.appNameOnce()
            .switchMap(app => this.uiService.getSettings(app).retry(2))
            .subscribe(dtos => {
                this.updatePatterns(ImmutableArray.of(dtos.regexSuggestions));
            }, error => {
                this.notifyError(error);
            });
    }

    private updatePatterns(patterns: ImmutableArray<UIRegexSuggestionDto>) {
        this.addPatternForm.reset();

        this.appPatterns =
            patterns.map(p => {
                return new UIRegexSuggestionDto(
                    p.name,
                    p.pattern
                );
            });

        this.messageBus.emit(new HistoryChannelUpdated());
    }
}


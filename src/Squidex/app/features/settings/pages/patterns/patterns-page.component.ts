/*
 * CivicPlus implementation of Squidex Headless CMS
 */

import { Component, OnInit } from '@angular/core';

import {
    AppComponentBase,
    AppsStoreService,
    AuthService,
    DialogService,
    HistoryChannelUpdated,
    ImmutableArray,
    MessageBus,
    AppPatternsService,
    AppPatternsSuggestionDto,
    Version
} from 'shared';

@Component({
    selector: 'sqx-patterns-page',
    styleUrls: ['./patterns-page.component.scss'],
    templateUrl: './patterns-page.component.html'
})
export class PatternsPageComponent extends AppComponentBase implements OnInit {
    private version = new Version();
    public appPatterns = ImmutableArray.empty<AppPatternsSuggestionDto>();

    constructor(apps: AppsStoreService, dialogs: DialogService, authService: AuthService,
        private readonly messageBus: MessageBus,
        private readonly patternService: AppPatternsService
    ) {
        super(dialogs, apps, authService);
    }

    public ngOnInit() {
        this.load();
    }

    public load() {
        this.appNameOnce()
            .switchMap(app => this.patternService.getPatterns(app).retry(2))
            .subscribe(dtos => {
                this.updatePatterns(ImmutableArray.of(dtos));
            }, error => {
                this.notifyError(error);
            });
    }

    public patternAdded(requestDto: AppPatternsSuggestionDto) {
        this.updatePatterns(this.appPatterns.push(requestDto));
    }

    public removePattern(pattern: AppPatternsSuggestionDto) {
        this.appNameOnce()
            .switchMap(app => this.patternService.deletePattern(app, pattern.name, this.version))
            .subscribe(() => {
                this.updatePatterns(this.appPatterns.remove(pattern));
            }, error => {
                this.notifyError(error);
            });
    }

    private updatePatterns(patterns: ImmutableArray<AppPatternsSuggestionDto>) {
        this.appPatterns =
            patterns.map(p => {
            return new AppPatternsSuggestionDto(
                    p.name,
                    p.pattern,
                    p.defaultMessage
                );
            });

        this.messageBus.emit(new HistoryChannelUpdated());
    }
}
/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { AbstractControl, FormControl, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';

import {
    ContentCreated,
    ContentRemoved,
    ContentUpdated,
    ContentVersionSelected
} from './../messages';

import {
    AppComponentBase,
    AppLanguageDto,
    AppsStoreService,
    allData,
    AuthService,
    CanComponentDeactivate,
    ContentDto,
    ContentsService,
    DialogService,
    DateTime,
    MessageBus,
    SchemaDetailsDto,
    Version,
    TextAnalyticsService
} from 'shared';

import {
    Status
} from 'framework';

@Component({
    selector: 'sqx-content-page',
    styleUrls: ['./content-page.component.scss'],
    templateUrl: './content-page.component.html'
})
export class ContentPageComponent extends AppComponentBase implements CanComponentDeactivate, OnDestroy, OnInit {
    private contentDeletedSubscription: Subscription;
    private contentVersionSelectedSubscription: Subscription;
    private isCopy = false;

    public schema: SchemaDetailsDto;

    public content: ContentDto;
    public contentFormSubmitted = false;
    public contentForm: FormGroup;

    public isNewMode = true;
    public isViewOnly = false;

    public Status: typeof Status = Status;

    public languages: AppLanguageDto[] = [];

    public textAnalyticsBody: string[];
    public recommendedTags: string[];
    public allTags: string[];

    constructor(apps: AppsStoreService,
        dialogs: DialogService,
        private readonly authService: AuthService,
        private readonly contentsService: ContentsService,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly messageBus: MessageBus,
        private readonly textAnalyticsService: TextAnalyticsService
    ) {
        super(dialogs, apps);
    }

    public ngOnDestroy() {
        this.contentVersionSelectedSubscription.unsubscribe();
        this.contentDeletedSubscription.unsubscribe();
    }

    public ngOnInit() {
        const routeData = allData(this.route);

        this.languages = routeData['appLanguages'];

        this.contentVersionSelectedSubscription =
            this.messageBus.of(ContentVersionSelected)
                .subscribe(message => {
                    this.loadVersion(message.version);
                });

        this.contentDeletedSubscription =
            this.messageBus.of(ContentRemoved)
                .subscribe(message => {
                    if (this.content && message.content.id === this.content.id) {
                        this.router.navigate(['../'], { relativeTo: this.route });
                    }
                });

        this.setupContentForm(routeData['schema']);

        this.route.data
            .map(p => p['isCopy'] || false)
            .subscribe((isCopy: boolean) => {
                this.isCopy = isCopy;
            });

        this.route.data
            .map(p => p['content'])
            .subscribe((content: ContentDto) => {
                this.content = content;

                this.populateContentForm();
            });

        this.textAnalyticsBody = new Array<string>(this.schema.fields.length);
    }

    public canDeactivate(): Observable<boolean> {
        if (!this.contentForm.dirty || this.isNewMode) {
            return Observable.of(true);
        } else {
            return this.dialogs.confirmUnsavedChanges();
        }
    }

    public saveAndPerformAction() {
        if (this.isAppEditor()) {
            this.saveAndPublish();
        } else if (this.isAppAuthor()) {
            this.saveAndSubmit();
        } else {
            this.saveAsDraft();
        }
    }

    public saveAndSubmit() {
        this.saveContent(Status.Submitted);
    }

    public saveAndPublish() {
        this.saveContent(Status.Published);
    }

    public saveAsDraft() {
        this.saveContent(Status.Draft);
    }

    private saveContent(status: Status) {
        this.contentFormSubmitted = true;

        if (this.contentForm.valid) {
            this.disableContentForm();

            const requestDto = this.contentForm.value;

            if (this.isNewMode) {
                this.appNameOnce()
                    .switchMap(app => this.contentsService.postContent(app,
                        this.schema.name,
                        requestDto,
                        status))
                    .subscribe(dto => {
                        this.content = dto;

                        this.emitContentCreated(this.content);
                        this.notifyInfo('Content created successfully.');
                        this.back();
                    },
                    error => {
                        this.notifyError(error);
                        this.enableContentForm();
                    });
            } else {
                this.appNameOnce()
                    .switchMap(app => this.contentsService.putContent(app, this.schema.name, this.content.id, requestDto, this.content.version))
                    .subscribe(dto => {
                        this.content = this.content.update(dto, this.authService.user!.token);

                        this.emitContentUpdated(this.content);
                        this.notifyInfo('Content saved successfully.');
                        this.enableContentForm();
                    },
                    error => {
                        this.notifyError(error);
                        this.enableContentForm();
                    });
            }
        } else {
            this.notifyError(
                'Content element not valid, please check the field with the red bar on the left in all languages (if localizable).');
        }
    }

    private loadVersion(version: number) {
        if (!this.isNewMode && this.content) {
            this.appNameOnce()
                .switchMap(app => this.contentsService.getVersionData(app, this.schema.name, this.content.id, new Version(version.toString())))
                .subscribe(dto => {
                    this.content = this.content.setData(dto);

                    this.emitContentUpdated(this.content);
                    this.notifyInfo('Content version loaded successfully.');
                    this.populateContentForm();
                }, error => {
                    this.notifyError(error);
                });
        }
    }

    private back() {
        let command = '../';


        if (this.isCopy) {
            command = '../../';
        }

        this.router.navigate([command], { relativeTo: this.route, replaceUrl: true });
    }

    private emitContentCreated(content: ContentDto) {
        this.messageBus.emit(new ContentCreated(content));
    }

    private emitContentUpdated(content: ContentDto) {
        this.messageBus.emit(new ContentUpdated(content));
    }

    private disableContentForm() {
        this.contentForm.disable();
    }

    private enableContentForm() {
        this.contentForm.markAsPristine();

        for (const field of this.schema.fields.filter(f => !f.isDisabled)) {
            this.contentForm.controls[field.name].enable();
        }
    }

    private setupContentForm(schema: SchemaDetailsDto) {
        this.schema = schema;

        const controls: { [key: string]: AbstractControl } = {};

        for (const field of schema.fields) {
            const group = new FormGroup({});

            if (field.partitioning === 'language') {
                for (let language of this.languages) {
                    group.addControl(language.iso2Code,
                        new FormControl(undefined, field.createValidators(language.isOptional)));
                }
            } else {
                group.addControl('iv', new FormControl(undefined, field.createValidators(false)));
            }

            controls[field.name] = group;
        }

        this.contentForm = new FormGroup(controls);
    }

    private populateContentForm() {
        this.contentForm.markAsPristine();

        this.isNewMode = !this.content;

        if (this.isCopy) {
            this.content = new ContentDto(null,
                Status.Draft,
                this.authService.user!.id,
                this.authService.user!.id,
                DateTime.now(),
                DateTime.now(),
                this.content.data,
                null);
            this.isNewMode = true;
        } else if (!this.isNewMode) {
            this.isViewOnly = (this.content.status === Status.Published && this.isAppAuthor());
        }

        if (!this.isNewMode || this.isCopy) {
            for (const field of this.schema.fields) {
                const fieldValue = this.content.data[field.name] || {};
                const fieldForm = <FormGroup>this.contentForm.get(field.name);

                if (field.partitioning === 'language') {
                    for (let language of this.languages) {
                        fieldForm.controls[language.iso2Code].setValue(fieldValue[language.iso2Code]);
                    }
                } else {
                    fieldForm.controls['iv'].setValue(fieldValue['iv']);
                }
                if (this.isViewOnly || this.content.status === Status.Archived) {
                    this.contentForm.disable();
                } else {
                    this.contentForm.enable();
                }
            }
        }
    }

    public analyzeForTags($event: any) {
        if ($event.text!.trim().length > 0) {
            this.textAnalyticsBody[$event.id] = $event.text;
        }

        if (this.textAnalyticsBody[$event.id] && this.textAnalyticsBody[$event.id]!.trim().length > 0 && this.textAnalyticsBody.join(' ').length > 0) {
            this.textAnalyticsService.getKeyPhrases(this.textAnalyticsBody.join(' ')).then((x: string[]) => {
                    this.recommendedTags = x;

                    let tagField = this.schema.fields[0];
                    const tagFieldForm = <FormGroup>this.contentForm.get(tagField.name);

                    if (tagField.partitioning === 'language') {
                        for (let language of this.languages) {
                            this.allTags = tagFieldForm.controls[language.iso2Code].value;
                            let formattedTags = this.updateTags();
                            tagFieldForm.controls[language.iso2Code].setValue(formattedTags);
                        }
                    } else {
                        tagFieldForm.controls['iv'].setValue(this.recommendedTags);
                    }
                }
            );
        }
    }

    private updateTags() {
        if (this.allTags && this.allTags.length > 0) {
            let missing = this.recommendedTags.filter(tag => this.allTags.indexOf(tag) < 0);
            missing.forEach(m => this.allTags.push(m));
        } else {
            this.allTags = this.recommendedTags;
        }
        return this.schema.fields[0].properties.formatValue(this.allTags);
    }
}

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
    ContentDeleted,
    ContentUpdated
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
    private version = new Version('');
    private content: ContentDto;
    private isCopy = false;

    public schema: SchemaDetailsDto;

    public contentFormSubmitted = false;
    public contentForm: FormGroup;
    public contentData: any = null;
    public contentId: string | null = null;

    public isNewMode = true;
    public isViewOnly = false;

    public languages: AppLanguageDto[] = [];

    public textAnalyticsBody: string[];

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
        this.contentDeletedSubscription.unsubscribe();
    }

    public ngOnInit() {
        const routeData = allData(this.route);

        this.languages = routeData['appLanguages'];

        this.contentDeletedSubscription =
            this.messageBus.of(ContentDeleted)
                .subscribe(message => {
                    if (message.content.id === this.contentId) {
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
            this.saveAndSubmit()
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
                        status,
                        this.version))
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
                    .switchMap(app => this.contentsService.putContent(app,
                        this.schema.name,
                        this.contentId!,
                        requestDto,
                        this.version))
                    .subscribe(() => {
                        this.content = this.content.update(requestDto, this.authService.user.token);

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

        if (!this.content) {
            this.contentData = null;
            this.contentId = null;
            this.isNewMode = true;
            return;
        }

        if (this.isCopy) {
            this.contentData = this.content.data;
            this.contentId = null;
            this.isNewMode = true;
        } else {
            this.contentData = this.content.data;
            this.contentId = this.content.id;
            this.version = this.content.version;
            this.isNewMode = false;
            this.isViewOnly = (this.content.status === Status.Published && this.isAppAuthor());
        }

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
            if (this.isViewOnly) {
                fieldForm.disable();
            } else {
                fieldForm.enable();
            }
        }
    }

    public analyzeForTags($event: any) {
        if ($event.text!.trim().length > 0) {
            this.textAnalyticsBody[$event.id] = $event.text;
        }
        console.log(this.textAnalyticsBody.join(' '));

        if (this.textAnalyticsBody[$event.id] && this.textAnalyticsBody[$event.id]!.trim().length > 0 && this.textAnalyticsBody.join(' ').length > 0) {
            this.textAnalyticsService.getKeyPhrases(this.textAnalyticsBody.join(' ')).subscribe(x => console.log(x));
        }
    }
}

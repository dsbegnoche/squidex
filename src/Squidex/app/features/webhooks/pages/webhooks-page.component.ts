/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    AppComponentBase,
    AppsStoreService,
    CreateWebhookDto,
    DialogService,
    ImmutableArray,
    SchemaDto,
    SchemasService,
    Version,
    WebhookDto,
    WebhooksService
} from 'shared';

interface WebhookWithSchema { webhook: WebhookDto; schema: SchemaDto; };

@Component({
    selector: 'sqx-webhooks-page',
    styleUrls: ['./webhooks-page.component.scss'],
    templateUrl: './webhooks-page.component.html'
})
export class WebhooksPageComponent extends AppComponentBase implements OnInit {
    private readonly version = new Version();

    public webhooks: ImmutableArray<WebhookWithSchema>;

    public schemas: SchemaDto[];

    public addWebhookFormSubmitted = false;
    public addWebhookForm =
        this.formBuilder.group({
            schemaId: ['',
                [
                    Validators.required
                ]],
            url: ['',
                [
                    Validators.required
                ]]
        });

    public get hasUrl() {
        return this.addWebhookForm.controls['url'].value && this.addWebhookForm.controls['url'].value.length > 0;
    }

    constructor(apps: AppsStoreService, dialogs: DialogService,
        private readonly schemasService: SchemasService,
        private readonly webhooksService: WebhooksService,
        private readonly formBuilder: FormBuilder
    ) {
        super(dialogs, apps);
    }

    public ngOnInit() {
        this.load();
    }

    public load(showInfo = false) {
        this.appNameOnce()
            .switchMap(app =>
                this.schemasService.getSchemas(app)
                    .combineLatest(this.webhooksService.getWebhooks(app),
                        (s, w) => { return { webhooks: w, schemas: s }; }))
            .subscribe(dtos => {
                this.schemas = dtos.schemas;

                this.webhooks =
                    ImmutableArray.of(
                        dtos.webhooks.map(w => {
                            return { webhook: w, schema: dtos.schemas.find(s => s.id === w.schemaId), showDetails: false };
                        }).filter(w => !!w.schema));

                this.addWebhookForm.controls['schemaId'].setValue(this.schemas.map(x => x.id)[0]);

                if (showInfo) {
                    this.notifyInfo('Webhooks reloaded.');
                }
            }, error => {
                this.notifyError(error);
            });
    }

    public deleteWebhook(webhook: WebhookWithSchema) {
        this.appNameOnce()
            .switchMap(app => this.webhooksService.deleteWebhook(app, webhook.schema.name, webhook.webhook.id, this.version))
            .subscribe(dto => {
                this.webhooks = this.webhooks.remove(webhook);
            }, error => {
                this.notifyError(error);
            });
    }

    public addWebhook() {
        this.addWebhookFormSubmitted = true;

        if (this.addWebhookForm.valid) {
            this.addWebhookForm.disable();

            const requestDto = new CreateWebhookDto(this.addWebhookForm.controls['url'].value);

            const schemaId = this.addWebhookForm.controls['schemaId'].value;
            const schema = this.schemas.find(s => s.id === schemaId);

            this.appNameOnce()
                .switchMap(app => this.webhooksService.postWebhook(app, schema.name, requestDto, this.version))
                .subscribe(dto => {
                    this.webhooks = this.webhooks.push({ webhook: dto, schema: schema });

                    this.resetWebhookForm();
                }, error => {
                    this.notifyError(error);
                    this.enableWebhookForm();
                });
        }
    }

    public cancelAddWebhook() {
        this.resetWebhookForm();
    }

    private enableWebhookForm() {
        this.addWebhookForm.enable();
    }

    private resetWebhookForm() {
        this.addWebhookFormSubmitted = false;
        this.addWebhookForm.enable();
        this.addWebhookForm.reset();
    }
}

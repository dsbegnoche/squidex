/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormControl } from '@angular/forms';
import { Subscription } from 'rxjs';

import {
    AppComponentBase,
    AppsStoreService,
    DialogService,
    fadeAnimation,
    ImmutableArray,
    MessageBus,
    ModalView,
    SchemaDto,
    SchemasService
} from 'shared';

import { SchemaDeleted, SchemaUpdated, SchemaCopied } from './../messages';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class SchemasPageComponent extends AppComponentBase implements OnDestroy, OnInit {
    private schemaCopiedSubscription: Subscription;
    private schemaUpdatedSubscription: Subscription;
    private schemaDeletedSubscription: Subscription;

    public addSchemaDialog = new ModalView();

    public schemas = ImmutableArray.empty<SchemaDto>();
    public schemaQuery: string;
    public schemasFilter = new FormControl();
    public schemasFiltered = ImmutableArray.empty<SchemaDto>();

    constructor(apps: AppsStoreService, dialogs: DialogService,
        private readonly schemasService: SchemasService,
        private readonly messageBus: MessageBus,
        private readonly route: ActivatedRoute,
        private readonly router: Router
    ) {
        super(dialogs, apps);
    }

    public ngOnDestroy() {
        this.schemaCopiedSubscription.unsubscribe();
        this.schemaUpdatedSubscription.unsubscribe();
        this.schemaDeletedSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.schemasFilter.valueChanges
            .distinctUntilChanged()
            .debounceTime(100)
            .subscribe(q => {
                this.updateSchemas(this.schemas, this.schemaQuery = q);
            });

        this.route.params.map(q => q['showDialog'])
            .subscribe(showDialog => {
                if (showDialog) {
                    this.addSchemaDialog.show();
                }
            });

        this.schemaCopiedSubscription =
            this.messageBus.of(SchemaCopied)
                .subscribe(m => {
                    this.updateSchemas(this.schemas.push(m.schema));
                    this.goToSchema(m.schema);
                });

        this.schemaUpdatedSubscription =
            this.messageBus.of(SchemaUpdated)
                .subscribe(m => {
                    this.updateSchemas(this.schemas.replaceBy('id', m.schema));
                });

        this.schemaDeletedSubscription =
            this.messageBus.of(SchemaDeleted)
                .subscribe(m => {
                    this.updateSchemas(this.schemas.filter(s => s.id !== m.schema.id));
                });

        this.load();
    }

    private load() {
        this.appNameOnce()
            .switchMap(app => this.schemasService.getSchemas(app).retry(2))
            .subscribe(dtos => {
                this.updateSchemas(ImmutableArray.of(dtos));
            }, error => {
                this.notifyError(error);
            });
    }

    public onSchemaCreated(dto: SchemaDto) {
        this.updateSchemas(this.schemas.push(dto), this.schemaQuery);

        this.addSchemaDialog.hide();
        this.goToSchema(dto);
    }

    private updateSchemas(schemas: ImmutableArray<SchemaDto>, query?: string) {
        this.schemas = schemas;

        query = query || this.schemaQuery;

        if (query && query.length > 0) {
            schemas = schemas.filter(t => t.name.indexOf(query!) >= 0);
        }

        this.schemasFiltered = schemas.sortByStringAsc(x => x.name);
    }

    private goToSchema(dto: SchemaDto) {
        this.router.navigate([dto.name], { relativeTo: this.route, replaceUrl: true });
    }
}


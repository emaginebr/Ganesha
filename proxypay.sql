-- ProxyPay Database Creation Script
-- PostgreSQL
-- Generated from EF Core Code First model

CREATE TABLE proxypay_stores (
    store_id BIGSERIAL NOT NULL,
    slug VARCHAR(120) NOT NULL,
    name VARCHAR(120) NOT NULL,
    owner_id BIGINT NOT NULL,
    logo VARCHAR(150),
    status INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT proxypay_stores_pkey PRIMARY KEY (store_id)
);

CREATE UNIQUE INDEX ix_proxypay_stores_slug ON proxypay_stores (slug);

CREATE TABLE proxypay_invoices (
    invoice_id BIGSERIAL NOT NULL,
    user_id BIGINT NOT NULL,
    invoice_number VARCHAR(50) NOT NULL,
    notes TEXT,
    status INTEGER NOT NULL DEFAULT 1,
    sub_total DOUBLE PRECISION NOT NULL,
    discount DOUBLE PRECISION NOT NULL DEFAULT 0,
    tax DOUBLE PRECISION NOT NULL DEFAULT 0,
    total DOUBLE PRECISION NOT NULL,
    due_date TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    paid_at TIMESTAMP WITHOUT TIME ZONE,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    CONSTRAINT proxypay_invoices_pkey PRIMARY KEY (invoice_id)
);

CREATE UNIQUE INDEX ix_proxypay_invoices_number ON proxypay_invoices (invoice_number);

CREATE TABLE proxypay_invoice_items (
    invoice_item_id BIGSERIAL NOT NULL,
    invoice_id BIGINT NOT NULL,
    description VARCHAR(500) NOT NULL,
    quantity INTEGER NOT NULL,
    unit_price DOUBLE PRECISION NOT NULL,
    discount DOUBLE PRECISION NOT NULL DEFAULT 0,
    total DOUBLE PRECISION NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    CONSTRAINT proxypay_invoice_items_pkey PRIMARY KEY (invoice_item_id),
    CONSTRAINT fk_proxypay_invoice_item_invoice FOREIGN KEY (invoice_id) REFERENCES proxypay_invoices (invoice_id) ON DELETE CASCADE
);

CREATE TABLE proxypay_transactions (
    transaction_id BIGSERIAL NOT NULL,
    user_id BIGINT NOT NULL,
    invoice_id BIGINT,
    type INTEGER NOT NULL,
    category INTEGER NOT NULL,
    description VARCHAR(500) NOT NULL,
    amount DOUBLE PRECISION NOT NULL,
    balance DOUBLE PRECISION NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    CONSTRAINT proxypay_transactions_pkey PRIMARY KEY (transaction_id),
    CONSTRAINT fk_proxypay_transaction_invoice FOREIGN KEY (invoice_id) REFERENCES proxypay_invoices (invoice_id)
);

CREATE INDEX ix_proxypay_transactions_user_id ON proxypay_transactions (user_id);

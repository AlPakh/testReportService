-- public.checkouts определение

-- Drop table

-- DROP TABLE public.checkouts;

CREATE TABLE public.checkouts (
	id text NOT NULL,
	"name" text NOT NULL,
	is_active bool NOT NULL,
	CONSTRAINT checkouts_pkey PRIMARY KEY (id)
);


-- public.processing_batches определение

-- Drop table

-- DROP TABLE public.processing_batches;

CREATE TABLE public.processing_batches (
	id uuid NOT NULL,
	status int2 NOT NULL,
	items_count int4 NOT NULL,
	started_at_utc timestamptz NOT NULL,
	completed_at_utc timestamptz NULL,
	CONSTRAINT processing_batches_pkey PRIMARY KEY (id)
);


-- public.products определение

-- Drop table

-- DROP TABLE public.products;

CREATE TABLE public.products (
	id text NOT NULL,
	"name" text NOT NULL,
	is_active bool NOT NULL,
	CONSTRAINT products_pkey PRIMARY KEY (id)
);


-- public.report_requests определение

-- Drop table

-- DROP TABLE public.report_requests;

CREATE TABLE public.report_requests (
	id uuid NOT NULL,
	external_message_id text NULL,
	product_id text NOT NULL,
	checkout_id text NOT NULL,
	period_from date NOT NULL,
	period_to date NOT NULL,
	status int2 NOT NULL,
	batch_id uuid NULL,
	error_message text NULL,
	created_at_utc timestamptz NOT NULL,
	updated_at_utc timestamptz NOT NULL,
	CONSTRAINT report_requests_external_message_id_key UNIQUE (external_message_id),
	CONSTRAINT report_requests_pkey PRIMARY KEY (id)
);
CREATE INDEX ix_report_requests_created_at_utc ON public.report_requests USING btree (created_at_utc);
CREATE INDEX ix_report_requests_status ON public.report_requests USING btree (status);


-- public.conversion_facts определение

-- Drop table

-- DROP TABLE public.conversion_facts;

CREATE TABLE public.conversion_facts (
	fact_date date NOT NULL,
	product_id text NOT NULL,
	checkout_id text NOT NULL,
	views_count int4 NOT NULL,
	payments_count int4 NOT NULL,
	CONSTRAINT conversion_facts_pkey PRIMARY KEY (fact_date, product_id, checkout_id),
	CONSTRAINT fk_conversion_facts_checkout FOREIGN KEY (checkout_id) REFERENCES public.checkouts(id) ON DELETE RESTRICT,
	CONSTRAINT fk_conversion_facts_product FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE RESTRICT
);
CREATE INDEX ix_conversion_facts_lookup ON public.conversion_facts USING btree (product_id, checkout_id, fact_date);


-- public.product_checkouts определение

-- Drop table

-- DROP TABLE public.product_checkouts;

CREATE TABLE public.product_checkouts (
	product_id text NOT NULL,
	checkout_id text NOT NULL,
	CONSTRAINT product_checkouts_pkey PRIMARY KEY (product_id, checkout_id),
	CONSTRAINT fk_product_checkouts_checkout FOREIGN KEY (checkout_id) REFERENCES public.checkouts(id) ON DELETE CASCADE,
	CONSTRAINT fk_product_checkouts_product FOREIGN KEY (product_id) REFERENCES public.products(id) ON DELETE CASCADE
);


-- public.report_results определение

-- Drop table

-- DROP TABLE public.report_results;

CREATE TABLE public.report_results (
	report_request_id uuid NOT NULL,
	views_count int4 NOT NULL,
	payments_count int4 NOT NULL,
	ratio numeric(18, 6) NULL,
	generated_at_utc timestamptz NOT NULL,
	CONSTRAINT report_results_pkey PRIMARY KEY (report_request_id),
	CONSTRAINT fk_report_results_request FOREIGN KEY (report_request_id) REFERENCES public.report_requests(id) ON DELETE CASCADE
);
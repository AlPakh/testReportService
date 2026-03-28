INSERT INTO public.checkouts (id,"name",is_active) VALUES
	 ('CHK-STD','Standard',true),
	 ('CHK-FAST','Fast',true),
	 ('CHK-MOBILE','Mobile',true);
INSERT INTO public.conversion_facts (fact_date,product_id,checkout_id,views_count,payments_count) VALUES
	 ('2026-03-01','PROD-001','CHK-STD',100,5),
	 ('2026-03-02','PROD-001','CHK-STD',80,4),
	 ('2026-03-03','PROD-001','CHK-STD',120,6),
	 ('2026-03-01','PROD-001','CHK-MOBILE',150,9),
	 ('2026-03-02','PROD-001','CHK-MOBILE',170,10),
	 ('2026-03-01','PROD-002','CHK-FAST',200,8),
	 ('2026-03-02','PROD-002','CHK-FAST',220,7),
	 ('2026-03-01','PROD-003','CHK-STD',90,3);
INSERT INTO public.product_checkouts (product_id,checkout_id) VALUES
	 ('PROD-001','CHK-STD'),
	 ('PROD-001','CHK-MOBILE'),
	 ('PROD-002','CHK-FAST'),
	 ('PROD-003','CHK-STD');
INSERT INTO public.products (id,"name",is_active) VALUES
	 ('PROD-001','Credit Card',true),
	 ('PROD-002','Loan',true),
	 ('PROD-003','Insurance',true);

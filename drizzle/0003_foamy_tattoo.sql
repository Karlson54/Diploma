PRAGMA foreign_keys=OFF;--> statement-breakpoint
CREATE TABLE `__new_reports` (
	`id` integer PRIMARY KEY AUTOINCREMENT NOT NULL,
	`employee_id` integer,
	`date` text NOT NULL,
	`market` text,
	`contracting_agency` text,
	`client` text,
	`project_brand` text,
	`media` text,
	`job_type` text,
	`comments` text,
	`hours` real NOT NULL,
	FOREIGN KEY (`employee_id`) REFERENCES `employees`(`id`) ON UPDATE no action ON DELETE cascade
);
--> statement-breakpoint
INSERT INTO `__new_reports`("id", "employee_id", "date", "market", "contracting_agency", "client", "project_brand", "media", "job_type", "comments", "hours") SELECT "id", "employee_id", "date", "market", "contracting_agency", "client", "project_brand", "media", "job_type", "comments", "hours" FROM `reports`;--> statement-breakpoint
DROP TABLE `reports`;--> statement-breakpoint
ALTER TABLE `__new_reports` RENAME TO `reports`;--> statement-breakpoint
PRAGMA foreign_keys=ON;--> statement-breakpoint
ALTER TABLE `employees` ADD `agency` text;
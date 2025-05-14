PRAGMA foreign_keys=OFF;
--> statement-breakpoint
CREATE TABLE `__new_reports` (
	`id` integer PRIMARY KEY AUTOINCREMENT NOT NULL,
	`employee_id` integer,
	`date` text NOT NULL,
	`market` text,
	`contracting_agency` text,
	`client` integer,
	`project_brand` text,
	`media` text,
	`job_type` text,
	`comments` text,
	`hours` real NOT NULL,
	FOREIGN KEY (`employee_id`) REFERENCES `employees`(`id`) ON UPDATE no action ON DELETE cascade,
	FOREIGN KEY (`client`) REFERENCES `clients`(`id`) ON UPDATE no action ON DELETE set null
);
--> statement-breakpoint
-- Copy data to the new table, converting client names to client IDs where possible
INSERT INTO `__new_reports` (`id`, `employee_id`, `date`, `market`, `contracting_agency`, `client`, `project_brand`, `media`, `job_type`, `comments`, `hours`)
SELECT r.`id`, r.`employee_id`, r.`date`, r.`market`, r.`contracting_agency`, 
    (SELECT c.`id` FROM `clients` c WHERE c.`name` = r.`client` LIMIT 1), 
    r.`project_brand`, r.`media`, r.`job_type`, r.`comments`, r.`hours`
FROM `reports` r;
--> statement-breakpoint
DROP TABLE `reports`;
--> statement-breakpoint
ALTER TABLE `__new_reports` RENAME TO `reports`;
--> statement-breakpoint
PRAGMA foreign_keys=ON; 
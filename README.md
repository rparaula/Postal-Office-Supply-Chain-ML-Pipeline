README

Team 4 - Postal Office Database Web applications
---------------------------------------------------------
Get Started:

To access the Web application: Click on the link [https://postalofficeportal-frgkd9fqd8a3cfc6.centralus-01.azurewebsites.net](https://post-office-bi-system-aefsgadccra3cffk.centralus-01.azurewebsites.net/Login) to access the Web application

To access the Database: open 'COSC3380_UH4.sql' in your preferred SQL editor

To access the code: Select the "New MAin" branch, open your preferred code editor, and click on "clone repository". If this did not work,
download the repository and open the COSCPFWA.sln file

---------------------------------------------------------
Permissions:

Customer view: customer@email.com
Password: customerpassword

Employee view: employee@email.com
Password: employeepassword

Manager view: manager@email.com
Password: adminpassword


---------------------------------------------------------


<h1 align="center">📥MySQL Database📥</h1>

Overview
--------

This project uses a MySQL OLTP database schema to simulate the day-to-day operations of a postal company. The schema is designed to model the operational side of a postal business before the data is later transformed, summarized, or visualized for business intelligence purposes.

The database is intentionally designed as an OLTP system rather than a pure OLAP/reporting schema. This means the tables focus on recording real business events as they happen: customers creating package records, employees handling packages, facilities processing packages, lockers being assigned, inventory being sold, incidents being reported, and package movement events being tracked over time.

The main goal of this schema is to provide a realistic operational data source that can later feed dashboards, reports, analytics views, and machine learning workflows.


Why an OLTP Design Was Used
---------------------------

The database was designed as an OLTP schema because the project simulates live postal operations. In a real postal company, operational systems need to support frequent inserts, updates, and status changes. For example:

- A customer creates or drops off a package.
- An employee processes the package at a facility.
- The package moves from one facility to another.
- A package is assigned to a smart locker.
- A customer retrieves a package.
- Inventory is purchased or restocked.
- Incidents are reported for lost, damaged, delayed, or problematic packages.

These are transactional events. Because of that, the schema prioritizes normalized tables, foreign key relationships, data integrity, lookup tables, stored procedures, and views that organize operational data for later reporting.


Main Design Philosophy
----------------------

The schema was designed around the idea that a postal company is not just one local post office. Instead, the company contains multiple facility types that perform different roles in the delivery network.

Earlier versions of the design focused mostly on customer-facing postal offices. The current schema improves that by using a generalized facility model. This allows the system to represent retail offices, processing centers, distribution centers, and local delivery centers without needing completely separate schemas for each type of location.

This facility-based approach makes the schema more flexible and realistic. A package can now be received at one facility, processed at another, transferred between facilities, delayed at a facility, sent out for delivery, and eventually delivered or placed in a locker.


Core Entity Groups
------------------

The schema is organized around several major groups of entities:

1. User and Role Tables

The user and role tables support application login behavior for demo users, customers, employees, and managers. These tables allow the web application to distinguish between different types of users while still connecting each login to the appropriate customer or employee record.

Important tables include:

- user_logins
- user_roles
- customer
- employee

The customer table stores customer identity, contact information, address information, account linkage, and preferred facility information.

The employee table stores employee identity, job role, department, manager relationship, salary, hours worked, and account linkage.

This allows the database to support customer-facing, employee-facing, and manager-facing parts of the web application.


2. Facility and Facility Type Tables

The facility table is one of the most important design changes in the schema. Instead of modeling only a single postal office concept, the schema uses a generalized facility table.

Important tables include:

- facility
- facility_type
- departments
- works_on

The facility_type table defines what kind of facility each facility is. Example facility types include:

- Retail Office
- Regional Processing and Distribution Center
- Local Processing Center

Each facility type has flags describing what that type of facility can do, such as:

- Whether it is customer-facing.
- Whether it handles retail activity.
- Whether it handles package processing.
- Whether it handles distribution.
- Whether it handles local delivery.

This makes the schema more realistic because not every postal facility performs the same business function. A retail office may handle customer drop-offs and pickups, while a processing and distribution center may focus on sorting and package transfers.


3. Package and Package Status Tables

The package table represents the core mail/package object being tracked by the postal company.

Important tables include:

- package
- package_status
- service_type
- shipping_cost

The package table stores package ownership, package dimensions, package contents, received date, service type, package status, and employee handling information.

The package_status table normalizes status values instead of storing inconsistent text directly in the package table. This avoids problems such as different spellings or formats like "Received", "received", "InTransit", or "In Transit".

The service_type table separates package service options from the package itself. This allows the package to reference a controlled list of service types such as standard delivery, smart locker delivery, pickup, or other future service options.

The shipping_cost table separates cost information from the package table. This keeps the package table focused on the physical and operational package record, while shipping cost can be handled as its own related business record.


4. Package Movement Tables

The package_movement table is the main table used to simulate the movement of packages through the postal network.

Important tables include:

- package_movement
- package_movement_event_type
- package_status
- facility
- vehicle
- employee

Instead of only storing one current package status, the schema records package movement events over time. This allows the system to answer questions such as:

- Where was the package received?
- Which facility processed it?
- When did it leave a facility?
- When did it arrive at another facility?
- Was it delayed?
- How long did it stay at a facility?
- Which employee processed the event?
- Which vehicle was involved?
- Has the package reached a final status?

The package_movement_event_type table defines the kinds of movement events that can occur. Examples include:

- Received At Facility
- Sent To Facility
- Arrived At Facility
- Sorted At Facility
- Departed Facility
- Out For Delivery
- Delivered
- Delayed At Facility
- Held For Pickup
- Returned To Sender
- Placed In Locker
- Picked Up By Customer

Each event type includes flags that describe whether it is an entry event, exit event, processing event, delay event, or final event.

This design is important because it allows package tracking to behave more like a real postal logistics system. Instead of overwriting history, the database keeps an event timeline.


5. Smart Locker Tables

The schema supports smart locker delivery as a separate operational workflow.

Important tables include:

- smartlocker
- lockerlocation
- lockerassignment
- package_to_locker
- notifications

Smart lockers are modeled separately from packages because a locker is a physical resource that can be available, occupied, or unavailable. A package can be assigned to a locker, linked through package_to_locker, and later retrieved by the customer.

The schema includes stored procedures for assigning packages to lockers and retrieving packages from lockers. This prevents the application from manually updating multiple tables in an unsafe or inconsistent order.

For example, assigning a package to a locker may require:

- Checking that the package exists.
- Checking that the package belongs to the customer.
- Checking that the package uses the SmartLocker service type.
- Checking that the locker exists.
- Checking that the locker is available.
- Creating a locker assignment.
- Linking the package to the locker assignment.
- Updating the locker status to Occupied.
- Creating a customer notification.

This logic is handled transactionally so that either the full operation succeeds or the database rolls it back.


6. Inventory and Retail Sales Tables

The schema also models the retail side of a postal facility.

Important tables include:

- inventory
- transaction
- transaction_item
- payment
- facility

Inventory records represent items sold by the postal company, such as packaging supplies or mailing materials. Each inventory item belongs to a facility, which allows each location to have its own stock levels.

Transaction and transaction item tables allow the schema to represent retail sales. This is useful because a real customer-facing postal office does more than accept packages. It may also sell envelopes, boxes, stamps, labels, or other shipping materials.

The payment table separates payment details from transactions. This keeps the sales transaction structure cleaner and makes it easier to analyze revenue, transaction volume, and payment methods later.


7. Incident Management Tables

The schema includes incident tracking so the postal company can simulate operational problems.

Important tables include:

- incident
- incident_type
- incident_severity
- incident_status

Incidents can be connected to packages, customers, employees, facilities, and package movement events. This allows the database to record problems such as:

- Damaged packages
- Lost packages
- Delayed deliveries
- Customer complaints
- Employee accidents
- Tracking errors
- Facility issues

Incident lookup tables normalize severity, status, and type values. This makes incident reporting easier and keeps the data consistent for dashboards.


8. Vehicle and Route-Related Tables

The schema includes vehicle-related fields to support package movement and delivery simulation.

Important tables include:

- vehicle
- package_movement

Vehicles can be associated with package movement events. This allows the database to model packages being transferred or delivered by a specific vehicle. Even if the route simulation is not fully developed yet, the schema has the structure needed to support future logistics analysis.


Lookup Tables
-------------

The schema uses lookup tables for values that should be controlled and consistent. Examples include:

- package_status
- package_movement_event_type
- service_type
- facility_type
- incident_type
- incident_severity
- incident_status
- user_roles

Lookup tables are used instead of free-text fields because they prevent inconsistent values and make the database easier to query.

For example, package status should not be typed manually into every package record. Instead, the package table references package_status. This allows statuses to be grouped, ordered, marked as final, and reused across the system.

This design also makes Power BI reporting easier because lookup tables can later become dimension-like reference tables in an analytical model.


Foreign Keys and Referential Integrity
--------------------------------------

The schema uses foreign keys to enforce relationships between business entities.

For example:

- A package belongs to a customer.
- A package can be handled by an employee.
- A customer can have a preferred facility.
- A facility belongs to a facility type.
- A department belongs to a facility.
- An employee belongs to a department.
- A package movement belongs to a package.
- A package movement can reference a facility, employee, vehicle, status, and event type.
- An incident can reference a package, customer, employee, facility, or package movement event.

Foreign keys are important because they prevent orphaned or invalid data. For example, the database should not allow a package movement record for a package that does not exist. It should also not allow a facility to reference a facility type that does not exist.

This is one of the main reasons the schema is structured relationally instead of storing everything in one large table.


Constraints and Data Validation
-------------------------------

The schema uses several forms of validation:

- NOT NULL constraints for required fields.
- UNIQUE constraints for values such as email addresses.
- CHECK constraints for valid numeric ranges and simple formatting rules.
- Foreign key constraints for valid relationships.
- Triggers for rule enforcement.
- Stored procedures for multi-step business operations.

Examples of validation include:

- Customer state codes must be two characters.
- ZIP codes must be either 5 or 10 characters.
- Employee salary cannot be negative.
- Hours worked cannot be negative.
- Inventory quantity cannot be negative.
- Inventory unit price cannot be negative.
- Package movement delay minutes cannot be negative.
- Facility type flags must be valid boolean values.

These constraints help keep demo data realistic and reduce the chance of invalid records entering the database.


Triggers
--------

The schema uses triggers for business rules that should happen automatically when certain records are inserted or updated.

Examples include:

- Ensuring an employee does not have a salary greater than their manager.
- Validating package movement rules before inserting movement records.
- Updating related package state when movement events occur.
- Supporting automatic cost calculation or cost synchronization depending on the current schema version.

Triggers are useful when the rule should always be enforced at the database level, regardless of whether the data came from the web application, a SQL script, or a future import process.

However, the schema does not rely on triggers for every workflow. More complicated business operations are better handled through stored procedures.


Stored Procedures
-----------------

Stored procedures are used for multi-step operations that should happen transactionally.

Important procedures include:

- CreatePackageWithInitialMovement
- TransferPackageBetweenFacilities
- AssignPackageToLocker
- RetrievePackageFromLocker
- SelectWrongSalaries

Stored procedures help keep the application logic cleaner because the web application can call one procedure instead of manually running several SQL statements.

For example, creating a package with an initial movement is not just a simple package insert. It may require:

- Validating the customer.
- Finding the customer's preferred facility.
- Validating the service type.
- Finding the correct starting package status.
- Finding the correct movement event type.
- Inserting the package.
- Inserting the initial movement event.
- Returning the new package ID and movement ID.

Wrapping this in a stored procedure makes the operation safer and easier to reuse.


Views
-----

The schema uses views as a reporting and semantic layer over the OLTP tables.

Important views include:

- vw_customer_accounts
- vw_employee_accounts
- vw_facility_directory
- vw_facility_type_summary
- vw_inventory_status
- vw_locker_occupancy
- vw_package_overview
- vw_package_tracking_history
- vw_package_delay_summary
- vw_package_facility_stays
- vw_incident_summary
- vw_store_sales_summary

Views are useful because the normalized OLTP tables are good for data integrity but not always convenient for dashboards or application display pages.

For example, a package dashboard should not require the application to manually join package, customer, package_status, facility, employee, and service_type every time. A view can present a cleaner package overview with readable names and calculated values.

The views act as a bridge between the normalized transactional schema and the future BI/dashboard layer.


Why the Schema Uses Facilities Instead of Only Post Offices
-----------------------------------------------------------

A key design decision was replacing the older "postal office only" design with a generalized facility design.

This was done because a postal company has different kinds of operational locations:

- Retail offices interact directly with customers.
- Processing centers sort and process packages.
- Distribution centers transfer packages across regions.
- Local processing centers prepare packages for final-mile delivery.
- Warehouses or storage-like facilities may hold inventory or packages.

If the schema only used a postal_office table, then every package, employee, department, locker, inventory record, and movement record would be forced into a customer-facing post office model.

The facility model is more flexible. It allows the same table structure to support many facility types while still preserving the unique role of each facility through facility_type.


Why Package Movement Is Separate From Package
---------------------------------------------

The package table stores the current package object. The package_movement table stores the history of what happened to the package.

This separation is important because a package can have many events over time. If all movement information were stored directly in the package table, the database would only know the current state and would lose the historical path.

By separating package_movement, the system can answer operational questions such as:

- How many packages passed through a facility today?
- Which packages are currently delayed?
- How long do packages stay at each facility?
- Which facilities have the most package volume?
- Which employees processed the most packages?
- Which movement event caused a package to become delayed?
- What is the full tracking history for a package?

This is one of the most important pieces of the schema for business intelligence because movement history creates time-series operational data.


Why Shipping Cost Is Separated
------------------------------

Shipping cost is separated from the main package record to avoid overloading the package table with financial logic.

The package table should describe the package itself: customer, dimensions, weight, contents, status, service type, and employee handling.

Shipping cost is a related business value that may be calculated, adjusted, charged, refunded, or reported separately. By keeping cost information separate, the schema is more flexible and easier to expand in the future.

This also allows the application or reporting layer to distinguish between:

- The physical package.
- The estimated shipping cost.
- The actual shipping charge.
- The payment transaction.
- Revenue reporting.

For a BI system, this separation is useful because cost and revenue can later be modeled more cleanly in reporting views or analytical fact tables.


How the OLTP Schema Supports BI
-------------------------------

Although this is an OLTP schema, it was designed with future BI use in mind.

The OLTP tables collect operational data. The views then organize that data into more readable forms. Later, Power BI or another dashboarding layer can connect to the views or to a separate OLAP schema built from this data.

The intended flow is:

MySQL OLTP Tables
    -> MySQL Views / Semantic Layer
    -> Optional ETL or ELT Process
    -> Fact and Dimension Tables or Power BI Model
    -> Dashboards and Business Insights

Possible BI questions supported by the schema include:

- How many packages were processed per facility?
- Which facilities have the highest package volume?
- How many packages are delayed?
- What are the most common delay reasons?
- How long do packages stay at each facility?
- Which package statuses are most common?
- How much retail revenue was generated?
- Which inventory items are low in stock?
- How many incidents occurred by severity?
- Which facilities report the most incidents?
- How many packages were assigned to smart lockers?
- How many locker assignments expired or were retrieved?


Normalization Strategy
----------------------

The schema is mostly normalized to reduce duplication and maintain consistency.

For example:

- Facility type information is stored once in facility_type.
- Package status values are stored once in package_status.
- Incident severity values are stored once in incident_severity.
- Incident status values are stored once in incident_status.
- Package movement event definitions are stored once in package_movement_event_type.

This reduces repeated text fields and makes updates easier. If a status label or facility type description changes, it only needs to be updated in one lookup table.


Use of Audit Fields
-------------------

Most operational tables include created_at and updated_at fields.

These fields are useful because they show when records were created and when they were last modified. This supports debugging, auditing, and future BI analysis.

For a simulated postal company, timestamps are especially important because operational performance depends heavily on time. Package movement, facility dwell time, delay tracking, locker assignment expiration, and transaction history all require reliable date and time data.


Safe Delete and Update Behavior
-------------------------------

The schema uses different foreign key behaviors depending on the relationship.

Examples:

- Some relationships use ON DELETE RESTRICT to prevent deleting important records that are still referenced.
- Some relationships use ON DELETE SET NULL when historical records should remain even if an optional related record is removed.
- Some relationship tables use ON DELETE CASCADE where dependent records should disappear with the parent.

This was done to preserve important operational history while still allowing cleanup where appropriate.

For example, deleting a package may delete its package movement history because the movement records depend directly on the package. However, deleting an employee should not necessarily delete historical incident records, so some relationships use SET NULL instead.


Demo Data Design
----------------

The schema contains demo users and generated data to support testing the web application and BI reports.

The demo users represent:

- A customer account.
- An employee account.
- A manager account.

The customer records are designed to simulate a larger postal customer base around the Houston area. Facilities are also modeled as Houston-area postal facilities. This gives the database enough realistic structure to support demos, dashboard screenshots, and portfolio explanations without requiring private or sensitive real-world data.


Strengths of the Current Design
-------------------------------

The current schema has several strengths:

1. It models a postal company as a network of facilities rather than a single post office.

2. It separates package identity from package movement history.

3. It uses lookup tables to avoid inconsistent text values.

4. It includes realistic business areas: customers, employees, facilities, packages, lockers, inventory, transactions, incidents, and notifications.

5. It uses foreign keys and constraints to protect data integrity.

6. It uses stored procedures for multi-step workflows.

7. It uses views to make normalized data easier to consume by dashboards and the web application.

8. It supports future BI work because operational events are timestamped and tied to facilities, employees, packages, and customers.


Limitations
-----------

This schema is designed for a demo and portfolio project, not for a production postal company.

Some limitations include:

- Address data is simulated.
- Package tracking data is generated rather than connected to live carrier systems.
- Facility operations are simplified.
- Routing and transportation logic are not fully modeled.
- Real payment processing is not implemented.
- Some business rules are simplified for demonstration purposes.
- The schema is still primarily OLTP and would need additional modeling for a full OLAP warehouse.

These limitations are acceptable for the purpose of the project because the goal is to demonstrate database design, web application integration, operational simulation, and BI readiness.


Future Improvements
-------------------

Possible future improvements include:

- Building a separate OLAP schema with fact and dimension tables.
- Creating fact_package_movement, fact_sales, fact_incidents, and fact_locker_usage tables.
- Adding route and delivery zone tables.
- Adding more advanced vehicle tracking.
- Adding service-level agreement calculations.
- Adding estimated delivery time logic.
- Adding more detailed shipping cost and billing tables.
- Adding inventory restocking workflows.
- Adding scheduled ETL or ELT jobs.
- Connecting Power BI directly to curated reporting views.
- Creating historical snapshots for long-term trend analysis.


Conclusion
----------

The MySQL schema was designed to simulate the operational database of a postal company. It uses an OLTP structure because the system needs to record real-time business activity such as package creation, package movement, facility processing, locker assignment, retail transactions, employee actions, and incident reporting.

The most important design decision was shifting from a single post office model to a generalized facility model. This allows the system to represent retail offices, processing centers, distribution centers, and local processing centers in one consistent structure.

The schema is normalized for data integrity, supported by lookup tables for consistency, protected by constraints and foreign keys, and extended with procedures, triggers, and views for business logic and reporting. This makes it a strong operational foundation for a postal company BI system.
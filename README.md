# Charge Amps Reminder
Get a reminder if you haven't remembered to connect your charge amps charger cable to your car at a specified time

## Setup
This solution requires that you have a charge amps charger and an account from them. Additionally you need an ApiKey, you can get that from their support team at "support @ charge-amps.com" without the spaces.

You also need a gmail account and an application password for it. That is explained here: https://support.google.com/accounts/answer/185833?hl=en

In order to run this azure function, a couple of settings must be supplied. They are retrieved through the IConfiguration interface so settings can be supplied from environmental variables, app settings (either in local.settings.json Values section if running locally, or in the Application Settings blade for the azure function in the azure portal) or from User Secrets.

* charge_amp_user_name: your charge amps account username e.g.: "david.s.svedberg@gmail.com"
* charge_amp_password: your charge amps account password
* charge_amp_apiKey: provided upon request from the charge amps support team.
* gmail_user_name: your gmail account username e.g.: "david.s.svedberg@gmail.com"
* email_to_notify: the email to notify if "david.s.svedberg@gmail.com",
* gmail_user_password: application password for your gmail account (see above),
* charge_amp_chargePointId: this is the id of your charger, you can find it on the profile tab on https://my.charge.space in the group box "My chargers"
* schedule_expression: (n)cron expression for when the function will be run e.g. "0 0 19 * * *". It follows UTC.
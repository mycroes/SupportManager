# SupportManager
Manage call forwarding for a support team

## What is it for?
The SupportManager allows you and your team to setup forwarding of phone calls from a shared number to individual phone numbers.
Currently only AT commands over serial port are supported to read and set the forwarding status.
It's possible to manually pick a team member to forward to, or to schedule forwarding at a later time.
It will even detect if you manually change the forwarding state by polling the device for it's current status.

## Who is it for?
Someone with .NET experience. Right now it will only suit developers and even then it probably won't work out of the box.

## What doesn't it do?
A lot. Authentication is not enforced, authorization is not implemented (although some minor preparations have been made).
Theoretically it also supports multiple teams, however the web interface lacks team separation in some places.

## But it does actually work?
Yes! We've been using it for over a year with scheduled and manual forwarding.
It works so well that development has somewhat stalled and that's why the code is here in it's current state.

## What's next?
* Try to (self-)host the web project in the service
* Enforcing authentication
* Implementing authorization
* Implementing a public status page
* User registration

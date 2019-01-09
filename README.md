# SupportManager
Manage call forwarding for a support team

## What is it for?
The SupportManager allows you and your team to setup forwarding of phone calls from a shared number to individual phone numbers.
Currently only AT commands over serial port are supported to read and set the forwarding status.
It's possible to manually pick a team member to forward to, or to schedule forwarding at a later time.
It will even detect if you manually change the forwarding state by polling the device for it's current status.
There's also Telegram support in the form of a bot that allows to read and set the current forward and the scheduled forwards.

## Who is it for?
Someone with .NET experience. Right now it will only suit developers and even then it probably won't work out of the box.

## What doesn't it do?
A lot. Authentication is not enforced everywhere, authorization is partially implemented.
Theoretically it also supports multiple teams, however the web interface lacks team separation in some places.

## But it does actually work?
Yes! It's currently in use for three support teams.
It works really well, although setting up a team is the thing that requires the most effort.

## What's next?
* Enforcing authentication everywhere
* Authorize team membership
* Implementing a public status page
* User registration

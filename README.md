# SupportManager
Manage cell phone forwarding for a support team

## What is it for?
The SupportManager allows you and your team to setup forwarding of phone calls from one support phone attached to the computer running the SupportManager to individual phone numbers. It's possible to manually pick a team member to forward to, or to schedule forwarding at a later time. It will even detect if you manually change the forwarding state on the phone (polled once a minute).

# Who is it for?
Someone with .NET experience. Right now it will only suit developers and even then it probably won't work out of the box.

## What doesn't it do?
A lot. Authentication is not enforced, authorization is not implemented (although some minor preparations have been made). Theoretically it also supports multiple teams, however the web interface lacks team separation in some places.

## But it does actually work?
Yes! We've been using it for a while, mainly with scheduled forwarding. It works so well that development has somewhat stalled and that's why the code is here in it's current state.

## What's next?
* Changing the web project to ASP.NET Core to allow self-hosting
* Enforcing authentication
* Implementing authorization
* Implementing a public status page
* User registration

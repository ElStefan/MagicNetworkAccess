# MagicNetworkAccess

##Wake network devices on samba access over LAN

The service is syncing the hosts arp table and wakes all devices where an outgoing tcp package is found and directed to port 445.

Useful if you've saved your network drives in explorer and want to access them by simple double click, but don't want your devices keep running all time.
It may not work on the first try, but the accessed computer should wake up if it is configured right.

All wakes are logged in a file in the service executables folder.
After a wake was executed it won't send the magic package again for the next 10 minutes to avoid traffic overhead.

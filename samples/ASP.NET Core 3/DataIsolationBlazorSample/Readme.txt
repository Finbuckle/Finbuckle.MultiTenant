The example uses multitenant strategies: host and route.

It can be set in the appsettings.json
"TenantStrategy": {
    "RouteStrategy": false,
    "DefaultRoute": "{__tenant__=}/{controller=Home}/{action=Index}",
    "HostTemplate": "__tenant__.*"
  },

To test host strategy use one of ways:
1. Edit hosts file
	127.0.0.1 megacorp
	127.0.0.1 finbuckle
	127.0.0.1 initech

2. Use xip.io
	https://megacorp.127.0.0.1.xip.io:5001
	https://finbuckle.127.0.0.1.xip.io:5001
	https://initech.127.0.0.1.xip.io:5001

-----------------
https://github.com/Oleg26Dev
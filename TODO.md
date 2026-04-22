Cloudflare ddns:

- create a method to get the machines current ip - how would you do it without relying on external services? - who do i trust - how do i do it securely? - implemented using cloudflare trace. needs redoing.
- read config file json for api key and what domain / subdomain a records to update.
- send get request to the correct Zone id to get the current ip for the wanted record
- check if the ip i got returned from the ip check is valid.
- if the ip is valid and the ip is different from the cloudflare a record run update method - use update call not overwrite
- create update method to send post request to update the correct subdomain for the correct zone
- make this run in a infinite loop with a configurable delay between ip change checks.

- Read listDns and find Id from the correct record. ✅
- make a method that maps a list DnsRecord models from listDns to the correct record. ✅
- make a method that checks if the ip is different from the current record ip, and only then run update method. ✅
- make sure that i dont send a extra listdns request for each record.✅

CURRENT TODO:

- make a list of A records to update in the config file, and loop through them to update each one if needed. Done ish

- check if the ip from ipfy or other ip provider is valid. DONE - blacklist cloudflare ips... etc

- add config option to choose which ip provider to use. DONE

- decide if i want to get the ip from multiple providers.

- add error handling DONE ISH
- add error handling if network is down. DONE ISH
- if the network is down and all outgoing requests are timing out. wait X minutes and try again. DONE ISH

- maybe use polly?
- add rate limiting

- actually support pagination if there is more than 200 records.

- multiple zone support. https://developers.cloudflare.com/api/resources/zones/methods/list DONE
  derive zone id from the domain name in the config DONE

- look at batch api requests to limit the amount of requests. https://developers.cloudflare.com/api/resources/dns/subresources/records/methods/batch

LATER:

- add containerize the program with docker. done
- add ENV VARIABLES done. maybe redo for better env variable names
- docker secrets?

{"id":"EXAMPLE","name":"EXAMPLE.COM","type":"A","content":"0.0.0.0","proxiable":true,"proxied":true,"ttl":1,"settings":{},"meta":{},"comment":null,"tags":[],"created_on":"DATE","modified_on":"DATE"}

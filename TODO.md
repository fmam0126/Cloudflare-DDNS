Cloudflare ddns:

- create a method to get the machines current ip - how would you do it without relying on external services? - who do i trust - how do i do it securely? - implemented using cloudflare trace. needs redoing.
- read config file json for api key and what domain / subdomain a records to update.
- send get request to the correct Zone id to get the current ip for the wanted record
- check if the ip i got returned from the ip check is valid.
- if the ip is valid and the ip is different from the cloudflare a record run update method - use update call not overwrite 
- create update method to send post request to update the correct subdomain for the correct zone
- make this run in a infinite loop with a configurable delay between ip change checks.




CURRENT TODO:
- make a list of A records to update in the config file, and loop through them to update each one if needed.

- Read listDns and find Id from the correct record.
- make a method that maps a list DnsRecord models from listDns to the correct record.
- make a method that checks if the ip is different from the current record ip, and only then run update method.
- make sure that i dont send a extra listdns request for each record.



{"id":"EXAMPLE","name":"EXAMPLE.COM","type":"A","content":"0.0.0.0","proxiable":true,"proxied":true,"ttl":1,"settings":{},"meta":{},"comment":null,"tags":[],"created_on":"DATE","modified_on":"DATE"}